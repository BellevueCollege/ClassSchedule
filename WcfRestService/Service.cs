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

                // Set web response format
                SetResponseFormat(queryParams["format"]);

                // Build facet list from query strings
                string paramValue;
                foreach (string param in queryParams.AllKeys)
                {
                    paramValue = queryParams[param];
                    switch (param)
                    {
                        case "availability":
                            addAvailabilityFacet(paramValue);
                            break;
                        case "days":
                            addDaysFacet(paramValue);
                            break;
                        case "modality":
                            addModalityFacet(paramValue);
                            break;
                        case "quarters":
                            addQuartersFacet(paramValue);
                            break;
                        case "time":
                            addTimeFacet(paramValue); // TODO: Implement time facets. Determine best wayt o pass times with querystrings
                            break;
                    }
                }
            }

        }
        #endregion



        #region GetCourse Members
        IList<Course> IService.GetCoursesByCourseID(string courseID)
		{
            using (OdsRepository repository = new OdsRepository())
            {

                IList<Course> courses = repository.GetCourses(CourseID.FromString(courseID));
                return courses;
            }
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

        #region AddFacet Members
        private void addAvailabilityFacet(string options)
        {
            if (options.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                incomingFacets.Add(new AvailabilityFacet(AvailabilityFacet.Options.Open));
            }
        }
        private void addDaysFacet(string options)
        {

            options = options.ToUpper();
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
                            return;
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
                incomingFacets.Add(new DaysFacet(daysToSelect));
            }
            else
            {
                return;
            }

        }
        private void addModalityFacet(string options)
        {
            options = options.ToUpper();
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
                            return;
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
                incomingFacets.Add(new ModalityFacet(modalitiesToSelect));
            }
            else
            {
                return;
            }
        }
        private void addQuartersFacet(string options)
        {
            int quarterCount;
            try
            {
                quarterCount = Convert.ToInt32(options);
            }
            catch
            {
                throw new WebFaultException<string>(string.Format("The value '{0}' is not a valid integer.", options), HttpStatusCode.BadRequest);
            }

            incomingFacets.Add(new RegistrationQuartersFacet(quarterCount));
        }
        private void addTimeFacet(string options)
        {
            // TODO: Determine how the user should specify TIME

        }
        #endregion
    }
}
