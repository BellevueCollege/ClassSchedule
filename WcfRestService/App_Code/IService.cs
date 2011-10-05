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
        [WebGet(UriTemplate = "Course/{courseID}")]
        [OperationContract]
        IList<Course> GetCoursesByCourseID(string courseID);
    }
}
