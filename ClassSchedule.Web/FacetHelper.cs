/*
This file is part of CtcClassSchedule.

CtcClassSchedule is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

CtcClassSchedule is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CtcClassSchedule.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Logging;
using Ctc.Ods;
using CTCClassSchedule.Common;
using Microsoft.Security.Application;

namespace CTCClassSchedule
{
  public class FacetHelper : IFacetData
  {
    private const int CREDITS_ANY = -1; // actual credits should always be positive (or zero), so we can use -1 as a flag

    private const string DEFAULT_START_TIME = "00:00 AM";
    private const string DEFAULT_END_TIME = "23:59 PM";

    readonly IList<GeneralFacetInfo> _modality = new List<GeneralFacetInfo>(4);
    readonly IList<GeneralFacetInfo> _days = new List<GeneralFacetInfo>(7);
    readonly IDictionary<string, object> _linkParameters;
    private string _timeEnd;
    private string _timeStart;
    private string _availability;

    readonly IList<ISectionFacet> _sectionFacets = new List<ISectionFacet>();
    private readonly ILog _log = LogManager.GetCurrentClassLogger();
    private string _lateStart;
    private bool _isLateStart;
    private string _credits;
    private int _creditsNumber = CREDITS_ANY;
    private readonly IList<GeneralFacetInfo> _facets;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ignoreKeys"></param>
    public FacetHelper(HttpRequestBase request, params string[] ignoreKeys) : this()
    {
      IList<string> requestParameters = request.QueryString.AllKeys.Union(request.Form.AllKeys).ToList();
      _linkParameters = new Dictionary<string, object>(requestParameters.Count);

      foreach (string key in requestParameters)
      {
        // X-Requested-With is appended for AJAX calls.
        if (key != null && key != "X-Requested-With" && !ignoreKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
        {
          string value = request.QueryString.AllKeys.Contains(key) ? request.QueryString[key] : request.Form[key];

          if (!String.IsNullOrWhiteSpace(value))
          {
            if (_linkParameters.ContainsKey(key))
            {
              _linkParameters[key] = value;
            }
            else
            {
              _linkParameters.Add(key, value);
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public FacetHelper()
    {
      _facets = new List<GeneralFacetInfo>();
      _modality = InitializeFacetList(ModalityListSchema);
      _days = InitializeFacetList(DaysListSchema);

      if (_linkParameters == null)
      {
        _linkParameters = new Dictionary<string, object>();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TimeStart
    {
      get {return _timeStart = _timeStart ?? string.Empty;}
      set
      {
        if (Helpers.IsValidTimeString(value))
        {
          _timeStart = value;

          UpdateFacetList("timestart", "Start", _timeStart);
        }
        else
        {
          _log.Warn(m =>m("Ignoring invalid start time value provided by the user: (encoded): [{0}]", Encoder.HtmlEncode(value)));
          _timeStart = string.Empty;

          DropFacetFromList("timestart");
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TimeEnd
    {
      get
      {
        return _timeEnd = _timeEnd ?? string.Empty;
      }
      set
      {
        if (Helpers.IsValidTimeString(value))
        {
          _timeEnd = value;

          UpdateFacetList("timeend", "End", _timeEnd);
        }
        else
        {
          _log.Warn(m => m("Ignoring invalid end time value provided by the user: (encoded): [{0}]", Encoder.HtmlEncode(value)));
          _timeEnd = string.Empty;

          DropFacetFromList("timeend");
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Availability
    {
      get {return _availability = _availability ?? string.Empty;}
      set
      {
        _availability = value ?? string.Empty;
        UpdateFacetList("avail", "Availability", _availability);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string LateStart
    {
      get {return _lateStart = _lateStart ?? string.Empty;}
      set
      {
        bool isLateStart;
        if (bool.TryParse(value, out isLateStart))
        {
          _lateStart = value;
          _isLateStart = isLateStart;

          UpdateFacetList("latestart", "Late Start", _lateStart);
        }
        else
        {
          _log.Warn(m => m("Ignoring invalid late start value entered by user: (encoded) [{0}]", Encoder.HtmlEncode(value)));
          _lateStart = string.Empty;
          _isLateStart = false;

          DropFacetFromList("latestart");
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Credits
    {
      get {return _credits = _credits ?? string.Empty;}
      set
      {
        if (!String.IsNullOrWhiteSpace(value))
        {
          int credits;

          if (int.TryParse(value, out credits))
          {
            _creditsNumber = credits;
            _credits = value;

            UpdateFacetList("numcredits", "Number of Credits", _credits);
          }
          else
          {
            if (value.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
              _credits = value;
              UpdateFacetList("numcredits", "Number of Credits", _credits);
            }
            else
            {
              _log.Warn(m => m("Ignoring invalid credits value entered by user: (encoded) [{0}]", Encoder.HtmlEncode(value)));
              _credits = string.Empty;
            }
            _creditsNumber = CREDITS_ANY;

            DropFacetFromList("numcredits");
          }
        }
        else
        {
          _credits = string.Empty;
          _creditsNumber = CREDITS_ANY;

          DropFacetFromList("numcredits");
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public IList<GeneralFacetInfo> Days
    {
      get {return _days;}
    }

    /// <summary>
    /// 
    /// </summary>
    public IList<GeneralFacetInfo> Modality
    {
      get {return _modality;}
    }

    /// <summary>
    /// 
    /// </summary>
    public IDictionary<string, object> LinkParameters
    {
      get {return _linkParameters;}
    }

    /// <summary>
    /// 
    /// </summary>
    public int CreditsAny
    {
      // TODO: Allow CreditsAny value to be set in config
      get {return CREDITS_ANY;}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fOncampus"></param>
    /// <param name="fOnline"></param>
    /// <param name="fHybrid"></param>
    public void SetModalities(string fOncampus = "", string fOnline = "", string fHybrid = "")
    {
      ModalityFacet.Options modality = ModalityFacet.Options.All;	// default
      
      if (SetFacetValue(_modality, "f_oncampus", fOncampus))
      {
        modality = (modality | ModalityFacet.Options.OnCampus);
      }
      if (SetFacetValue(_modality, "f_online", fOnline))
      {
        modality = (modality | ModalityFacet.Options.OnCampus);
      }
      if (SetFacetValue(_modality, "f_hybrid", fHybrid))
      {
        modality = (modality | ModalityFacet.Options.OnCampus);
      }

      _sectionFacets.Add(new ModalityFacet(modality));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="daySu"></param>
    /// <param name="dayM"></param>
    /// <param name="dayT"></param>
    /// <param name="dayW"></param>
    /// <param name="dayTh"></param>
    /// <param name="dayF"></param>
    /// <param name="dayS"></param>
    public void SetDays(string daySu = "", string dayM = "", string dayT = "", string dayW = "", string dayTh = "", string dayF = "", string dayS = "")
    {
      DaysFacet.Options days = DaysFacet.Options.All;	// default

      if (SetFacetValue(_days, "day_su", daySu))
      {
        days = (days | DaysFacet.Options.Sunday);
      }
      if (SetFacetValue(_days, "day_m", dayM))
      {
        days = (days | DaysFacet.Options.Monday);
      }
      if (SetFacetValue(_days, "day_t", dayT))
      {
        days = (days | DaysFacet.Options.Tuesday);
      }
      if (SetFacetValue(_days, "day_w", dayW))
      {
        days = (days | DaysFacet.Options.Wednesday);
      }
      if (SetFacetValue(_days, "day_th", dayTh))
      {
        days = (days | DaysFacet.Options.Thursday);
      }
      if (SetFacetValue(_days, "day_f", dayF))
      {
        days = (days | DaysFacet.Options.Friday);
      }
      if (SetFacetValue(_days, "day_s", dayS))
      {
        days = (days | DaysFacet.Options.Saturday);
      }

      _sectionFacets.Add(new DaysFacet(days));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IList<ISectionFacet> CreateSectionFacets()
    {
      /*
       * Time facet
       */
      if (!string.IsNullOrWhiteSpace(TimeStart) || !string.IsNullOrWhiteSpace(TimeEnd))
      {
        TimeSpan startTime = ToTime(TimeStart);
        TimeSpan endTime = ToTime(TimeEnd, DEFAULT_END_TIME);

        _sectionFacets.Add(new TimeFacet(startTime, endTime));
      }

      /*
       * Availability facet
       */
      switch (Availability.ToUpper())
      {
        case "OPEN":
          _sectionFacets.Add(new AvailabilityFacet(AvailabilityFacet.Options.Open));
          break;
        case "ALL":
          _sectionFacets.Add(new AvailabilityFacet(AvailabilityFacet.Options.All));
          break;
      }

      /*
       * Late start facet
       */
      if (_isLateStart) // <= string property is backed by bool variable
      {
        _sectionFacets.Add(new LateStartFacet());
      }

      /*
       * Number of credits facet
       */
      if (_creditsNumber != CREDITS_ANY && !string.IsNullOrWhiteSpace(Credits))
      {
        _sectionFacets.Add(new CreditsFacet(_creditsNumber));
      }

      return _sectionFacets;
    }

    #region Private methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="listSchema"></param>
    /// <returns></returns>
    private IList<GeneralFacetInfo> InitializeFacetList(IDictionary<string, string> listSchema)
    {
      IList<GeneralFacetInfo> list;

      if (listSchema.Any())
      {
        list = new List<GeneralFacetInfo>(listSchema.Count);

        foreach (KeyValuePair<string, string> item in listSchema)
        {
          list.Add(new GeneralFacetInfo
          {
            ID = item.Key,
            Title = item.Value,
            Value = string.Empty
          });
        }
      }
      else
      {
        list = new List<GeneralFacetInfo>();
      }

      return list;
    }

    /// <summary>
    /// 
    /// </summary>
    private IDictionary<string, string> DaysListSchema
    {
      get
      {
        // TODO: allow specifying modality schema in config
        IDictionary<string, string> listSchema = new Dictionary<string, string>
                                                 {
                                                   {"day_su", "S"},
                                                   {"day_m", "M"},
                                                   {"day_t", "T"},
                                                   {"day_w", "W"},
                                                   {"day_th", "T"},
                                                   {"day_f", "F"},
                                                   {"day_s", "S"}
                                                 };


        return listSchema;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private IDictionary<string, string> ModalityListSchema
    {
      get
      {
        // TODO: allow specifying modality schema in config
        IDictionary<string, string> listSchema = new Dictionary<string, string>
                                                 {
                                                   {"f_oncampus", "On Campus"},
                                                   {"f_online", "Online"},
                                                   {"f_hybrid", "Hybrid"}
                                                 };


        return listSchema;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="id"></param>
    /// <param name="value"></param>
    private bool SetFacetValue(IList<GeneralFacetInfo> list, string id, string value)
    {
      bool valueBool = false;

      if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out valueBool))
      {
        list.First(m => m.ID == id).Value = valueBool.ToString();
      }

      return valueBool;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="title"></param>
    /// <param name="value"></param>
    private void UpdateFacetList(string id, string title, string value)
    {
      DropFacetFromList(id);
      _facets.Add(new GeneralFacetInfo
      {
        ID = id,
        Title = title,
        Value = value
      });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="facetID"></param>
    private void DropFacetFromList(string facetID)
    {
      if (_facets.Any(f => f.ID == facetID))
      {
        bool removed = _facets.Remove(_facets.First(f => f.ID == facetID));

        if (!removed)
        {
          _log.Warn(m => m("Failed to remove '{0}' facet from collection.", facetID));
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static TimeSpan ToTime(int hour, int minute, int seconds = 0)
    {
      return new TimeSpan(hour, minute, seconds);
    }

    // TODO: move ToTime() to API as an extension method of String
    /// <summary>
    /// Takes a string that represents a time (ie "6:45pm") and outputs the parsed hour and minute integers
    /// based on a 24 hour clock. If a time cannot be parsed, the output defaults to either 12am or 11:59pm
    /// depending on the time default parameter passed
    /// </summary>
    /// <param name="time">String representing a time value</param>
    /// <param name="defaultTime"></param>
    public static TimeSpan ToTime(string time, string defaultTime = DEFAULT_START_TIME)
    {
      if (string.IsNullOrWhiteSpace(time))
      {
        time = defaultTime;
      }

      // Determine integer values for time hours and minutes
      string timeTrimmed = time.Trim();
      bool isPM = (timeTrimmed.Length > 2 ? timeTrimmed.Substring(timeTrimmed.Length - 2) : timeTrimmed).Equals("PM", StringComparison.OrdinalIgnoreCase);

      // Adjust the conversion to integers if the user leaves off a leading 0
      // (possible by using tab instead of mouseoff on the time selector)
      int hour;
      short minute = 0;

      if (time.IndexOf(':') == 2)
      {
        hour = Convert.ToInt16(time.Substring(0, 2)) + (isPM ? 12 : 0);
        if (time.IndexOf(':') != -1)
        {
          minute = Convert.ToInt16(time.Substring(3, 2));
        }
      }
      else
      {
        hour = Convert.ToInt16(time.Substring(0, 1)) + (isPM ? 12 : 0);
        if (time.IndexOf(':') != -1)
        {
          minute = Convert.ToInt16(time.Substring(2, 2));
        }
      }

      return ToTime(hour, minute);
    }

    #endregion
  }
}