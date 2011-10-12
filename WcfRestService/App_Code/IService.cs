using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using Ctc.Ods;
using Ctc.Ods.Data;
using Ctc.Ods.Types;

namespace Ctc.Wcf
{
    [ServiceContract]
    public interface IService
    {
        #region GetCourse Members
        [WebGet(UriTemplate = "Courses/")]
        [OperationContract]
        IList<Course> GetAllCourses();

        [WebGet(UriTemplate = "Courses/{courseID}")]
        [OperationContract]
        IList<Course> GetCoursesByCourseID(string courseID);

        [WebGet(UriTemplate = "Courses/subjects/{subject}")]
        [OperationContract]
        IList<Course> GetCoursesBySubject(string subject);

        [WebGet(UriTemplate = "Courses/{courseID}/description?yearquarterid={yearQuarterID}")]
        [OperationContract]
        IList<CourseDescription> GetCourseDescription(string courseID, string yearQuarterID);
        #endregion

        #region GetRegistration Members
        [WebGet(UriTemplate = "RegistrationQuarters/{count}/")]
        [OperationContract]
        IList<YearQuarter> GetRegistrationQuarters(string count);

        [WebGet(UriTemplate = "RegistrationQuarters/")]
        [OperationContract]
        IList<YearQuarter> GetCurrentRegistrationQuarters();

        [WebGet(UriTemplate = "RegistrationQuarters/current")]
        [OperationContract]
        YearQuarter GetCurrentQuarter();
        #endregion

        #region GetSections Members

        #endregion
    }
}
