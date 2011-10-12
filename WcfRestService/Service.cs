using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;
using System.Net;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace Ctc.Wcf
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class Service : IService
	{
        // TODO: Add detailed member documentation

        private IList<ISectionFacet> _incomingFacets;

        #region Properties
        private IList<ISectionFacet> incomingFacets
        {
            get
            {
                if (_incomingFacets == null)
                {
                    _incomingFacets = new List<ISectionFacet>();
                }
                return _incomingFacets;
            }
        }
        #endregion

        #region Constructors
        public Service()
        {
            // If there is an incoming request
            NameValueCollection queryParams;
            if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
            {
                queryParams = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                // Build facet list from query strings, and handle other applicable parameters
                string paramValue;
                ISectionFacet tempFacet;
                foreach (string param in queryParams.AllKeys)
                {
                    paramValue = queryParams[param];
                    tempFacet = null;
                    switch (param)
                    {
                        case "availability":
                            tempFacet = createAvailabilityFacet(paramValue);
                            break;
                        case "days":
                            tempFacet = createDaysFacet(paramValue);
                            break;
                        case "modality":
                            tempFacet = createModalityFacet(paramValue);
                            break;
                        case "quarters":
                            tempFacet = createQuartersFacet(paramValue);
                            break;
                        case "time":
                            tempFacet = createTimeFacet(paramValue); // TODO: Implement time facets. Determine best way to pass times with querystrings
                            break;
                        case "format":
                            SetResponseFormat(queryParams["format"]);
                            break;
                    }

                    if (tempFacet != null)
                    {
                        incomingFacets.Add(tempFacet);
                    }
                }
            }

        }
        #endregion



        #region GetCourse Members
        IList<Course> IService.GetAllCourses()
        {
            IList<Course> courses;
            using (OdsRepository repository = new OdsRepository())
            {
                courses = repository.GetCourses(incomingFacets);
            }

            return courses;
        }

        IList<Course> IService.GetCoursesByCourseID(string courseID)
		{
            IList<Course> courses;
            using (OdsRepository repository = new OdsRepository())
            {
                courses = repository.GetCourses(CourseID.FromString(courseID), incomingFacets);
            }

            return courses;
		}

        IList<Course> IService.GetCoursesBySubject(string subject)
        {
            IList<Course> courses;
            using (OdsRepository repository = new OdsRepository())
            {
                courses = repository.GetCourses(subject, incomingFacets);
            }

            return courses;
        }

        IList<CourseDescription> IService.GetCourseDescription(string courseID, string yearQuarterID)
        {
            IList<CourseDescription> descriptions;
            using (OdsRepository repository = new OdsRepository())
            {
                if (yearQuarterID != null)
                {
                    try
                    {
                        descriptions = repository.GetCourseDescription(CourseID.FromString(courseID), YearQuarter.FromString(yearQuarterID));
                    }
                    catch (ArgumentOutOfRangeException er)
                    {
                        // Invalid YearQuarterID
                        throw new WebFaultException<string>(string.Format("The value '{0}' is not a valid YearQuarterID.", yearQuarterID), HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    descriptions = repository.GetCourseDescription(CourseID.FromString(courseID));
                }
            }

            return descriptions;
        }
        #endregion


        #region GetRegistrationQuarters Members
        IList<YearQuarter> IService.GetRegistrationQuarters(string count)
        {
            IList<YearQuarter> quarters;
            int quarterCount = 0;
            using (OdsRepository repository = new OdsRepository())
            {
                // Try to convert the COUNT parameter
                try
                {
                    quarterCount = Convert.ToInt32(count);
                }
                catch (FormatException er)
                {
                    throw new WebFaultException<string>(string.Format("The value '{0}' is not a valid integer.", count), HttpStatusCode.BadRequest);
                }

                // Throw an exception if the COUNT is negative
                if (quarterCount < 0)
                {
                    throw new WebFaultException<string>(string.Format("The value '{0}' is not a valid quarter count. Count must be non-negative.", count), HttpStatusCode.BadRequest);
                }

                quarters = repository.GetRegistrationQuarters(quarterCount);
            }

            return quarters;
        }

        IList<YearQuarter> IService.GetCurrentRegistrationQuarters()
        {
            IList<YearQuarter> quarters;
            using (OdsRepository repository = new OdsRepository())
            {
                quarters = repository.GetRegistrationQuarters();
            }

            return quarters;
        }

        YearQuarter IService.GetCurrentQuarter()
        {
            YearQuarter quarter;
            using (OdsRepository repository = new OdsRepository())
            {
                quarter = repository.CurrentYearQuarter;
            }

            return quarter;
        }
        #endregion


        #region GetSection Members

        #endregion


        /// <summary>
        /// Checks for a 'format' query string for a format value (e.g. xml, json). If supported, the web response format is set to the requested format
        /// This code is based heavily on <a href="http://msdn.microsoft.com/en-us/library/ee476510.aspx">this MSDN article</a>
        /// </summary>
        /// <param name="format">The name of the response format requested</param>
        private void SetResponseFormat(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                if (format.Equals("xml", System.StringComparison.OrdinalIgnoreCase))
                {
                    WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Xml;
                }
                else if (format.Equals("json", System.StringComparison.OrdinalIgnoreCase))
                {
                    WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
                }
                else
                {
                    throw new WebFaultException<string>(string.Format("Unsupported format '{0}'", format), HttpStatusCode.BadRequest);
                }
            }
        }


        #region CreateFacet Members
        /// <summary>
        /// Creates an instance of an AvailabilityFacet from a single string of parameters. Meant to ease the creation of a facet from a single querystring.
        /// </summary>
        /// <param name="options">String of option(s) that define the facet. The accepted values are case insensitive. <see cref="AvailabilityFacet.Options"/>.<br />
        /// Accepted values: <ul><li>options = "all"; accepted, but will return a null instance. Filtering by all is the same as not filtering.</li><li>options = "open"</li></ul>
        /// </param>
        /// <returns>An instance of AvailabilityFacet that reflects the options passed to the method. Returns a null instance if no applicable options were found.</returns>
        private AvailabilityFacet createAvailabilityFacet(string options)
        {
            AvailabilityFacet resultFacet = null;
            if (options.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                resultFacet = new AvailabilityFacet(AvailabilityFacet.Options.Open);
            }

            return resultFacet;
        }

        /// <summary>
        /// Creates an instance of a DaysFacet from a single string of parameters. Meant to ease the creation of a facet from a single querystring.
        /// </summary>
        /// <param name="options">String of concatenated option(s) that define the facet. The accepted values are case insensitive. <see cref="DaysFacet.Options"/>.<br />
        /// Sample values: <ul><li>options = "MW"</li><li>options = "TTh"</li><li>options = "MWF"</li><li>options = "All"; accepted, but will return a null instance. Filtering by all is the same as not filtering.</li></ul>
        /// </param>
        /// <returns>An instance of DaysFacet that reflects the options passed to the method. Returns a null instance if no applicable options were found.</returns>
        private DaysFacet createDaysFacet(string options)
        {
            options = options.ToUpper();
            DaysFacet resultFacet = null;
            List<string> days = new List<string>("ALL,TH,SA,SU,M,T,W,F".Split(','));
            DaysFacet.Options daysToSelect = DaysFacet.Options.All;
            foreach (string day in days)
            {
                if (options.Contains(day))
                {
                    options = options.Replace(day, String.Empty);
                    switch (day)
                    {
                        case "ALL":
                            daysToSelect = daysToSelect | DaysFacet.Options.All;
                            break;
                        case "TH":
                            daysToSelect = daysToSelect | DaysFacet.Options.Thursday;
                            break;
                        case "SA":
                            daysToSelect = daysToSelect | DaysFacet.Options.Saturday;
                            break;
                        case "SU":
                            daysToSelect = daysToSelect | DaysFacet.Options.Sunday;
                            break;
                        case "M":
                            daysToSelect = daysToSelect | DaysFacet.Options.Monday;
                            break;
                        case "T":
                            daysToSelect = daysToSelect | DaysFacet.Options.Tuesday;
                            break;
                        case "W":
                            daysToSelect = daysToSelect | DaysFacet.Options.Wednesday;
                            break;
                        case "F":
                            daysToSelect = daysToSelect | DaysFacet.Options.Friday;
                            break;
                    }

                    // Don't process any more than we have to
                    if ((options == String.Empty) || (daysToSelect == DaysFacet.Options.All)) { break; }
                }
            }

            // If all days were selected, don't filter by days
            if (daysToSelect != DaysFacet.Options.All)
            {
                resultFacet = new DaysFacet(daysToSelect);
            }

            return resultFacet;
        }

        /// <summary>
        /// Creates an instance of a ModalityFacet from a single string of parameters. Meant to ease the creation of a facet from a single querystring.
        /// </summary>
        /// <param name="options">String of concatenated option(s) that define the facet. The accepted values are case insensitive. <see cref="ModalityFacet.Options"/>.<br />
        /// Sample values: <ul><li>options = "Online"</li><li>options = "OnCampus"</li><li>options = "OnlineTelecourseOnCampus"</li><li>options = "Online,OnCampus"</li><li>options = "All"; accepted, but will return a null instance. Filtering by all is the same as not filtering.</li></ul>
        /// </param>
        /// <returns>An instance of ModalityFacet that reflects the options passed to the method. Returns a null instance if no applicable options were found.</returns>
        private ModalityFacet createModalityFacet(string options)
        {
            options = options.ToUpper();
            ModalityFacet resultFacet = null;
            List<string> modalities = new List<string>("ALL,ONLINE,HYBRID,TELECOURSE,ONCAMPUS".Split(','));
            ModalityFacet.Options modalitiesToSelect = ModalityFacet.Options.All;
            foreach (string modality in modalities)
            {
                if (options.Contains(modality))
                {
                    options = options.Replace(modality, String.Empty);
                    switch (modality)
                    {
                        case "ALL":
                            modalitiesToSelect = modalitiesToSelect | ModalityFacet.Options.All;
                            break;
                        case "ONLINE":
                            modalitiesToSelect = modalitiesToSelect | ModalityFacet.Options.Online;
                            break;
                        case "HYBRID":
                            modalitiesToSelect = modalitiesToSelect | ModalityFacet.Options.Hybrid;
                            break;
                        case "TELECOURSE":
                            modalitiesToSelect = modalitiesToSelect | ModalityFacet.Options.Telecourse;
                            break;
                        case "ONCAMPUS":
                            modalitiesToSelect = modalitiesToSelect | ModalityFacet.Options.OnCampus;
                            break;
                    }

                    // Don't process any more than we have to
                    if ((options == String.Empty) || (modalitiesToSelect == ModalityFacet.Options.All)) { break; }
                }
            }

            // If all days were selected, don't filter by days
            if (modalitiesToSelect != ModalityFacet.Options.All)
            {
                resultFacet = new ModalityFacet(modalitiesToSelect);
            }

            return resultFacet;
        }

        /// <summary>
        /// Creates an instance of a RegistrationQuartersFacet from a single string of parameters. Meant to ease the creation of a facet from a single querystring.
        /// </summary>
        /// <param name="options">A number that represents how many quarters from the current quarter you wish to look into the future (positive number) or past (negative), where 1 looks at the current quarter.</param>
        /// <returns>An instance of RegistrationQuartersFacet that reflects the options passed to the method. Returns a null instance if no applicable options were found.</returns>
        private RegistrationQuartersFacet createQuartersFacet(string options)
        {
            int quarterCount;
            RegistrationQuartersFacet resultFacet = null;
            try
            {
                quarterCount = Convert.ToInt32(options);
                resultFacet = new RegistrationQuartersFacet(quarterCount);
            }
            catch(FormatException er)
            {
                throw new WebFaultException<string>(string.Format("The value '{0}' is not a valid integer.", options), HttpStatusCode.BadRequest);
            }

            return resultFacet;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private TimeFacet createTimeFacet(string options)
        {
            // TODO: Determine how the user should specify TIME
            TimeFacet resultFacet = null;
            return resultFacet;
        }
        #endregion
    }
}
