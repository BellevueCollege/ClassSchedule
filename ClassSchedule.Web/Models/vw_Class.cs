//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CTCClassSchedule.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class vw_Class
    {
        public string ClassID { get; set; }
        public string YearQuarterID { get; set; }
        public string ItemNumber { get; set; }
        public string CourseID { get; set; }
        public string Department { get; set; }
        public string CourseNumber { get; set; }
        public Nullable<int> ClassCapacity { get; set; }
        public Nullable<int> StudentsEnrolled { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
    }
}
