using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CTCClassSchedule
{
    public class AutomatedFootnotesConfig
    {
        protected static Dictionary<string, AutomatedFootnoteElement> _footnoteInstances;

        static AutomatedFootnotesConfig()
        {
            AutomatedFootnotesSection sec = (AutomatedFootnotesSection)System.Configuration.ConfigurationManager.GetSection("ctcAutomatedFootnoteSettings");

            _footnoteInstances = GetFootnoteInstances(sec.FootnoteInstances);
        }



        public static AutomatedFootnoteElement Footnotes(string footnoteName)
        {
            return _footnoteInstances[footnoteName];
        }

        public static string getAutomatedFootnotesText(SectionWithSeats section)
        {
            string footnoteTextResult = string.Empty;
            string dateParam = "{DATE}";
            string dateText;
            AutomatedFootnoteElement footnote;
            string wSpace = section.Footnotes.Count() == 0 ? string.Empty : " ";

            // If the section has a late start
            if (section.IsLateStart)
            {
                footnote = Footnotes("lateStart");
                dateText = section.StartDate.GetValueOrDefault(DateTime.Now).ToString(footnote.StringFormat);
                footnoteTextResult += wSpace + Footnotes("lateStart").Text.Replace(dateParam, dateText);
            }

            // If the section has a different end date than usual
            if (section.IsDifferentEndDate)
            {
                footnote = Footnotes("lateStart");
                dateText = section.EndDate.GetValueOrDefault(DateTime.Now).ToString(footnote.StringFormat);
                footnoteTextResult += wSpace + Footnotes("endDate").Text.Replace(dateParam, dateText);
            }

            // If the section is a hybrid section
            if (section.IsHybrid)
            {
                footnoteTextResult += wSpace + Footnotes("hybrid").Text;
            }

            // If the section is a continuous enrollment section
            if (section.IsContinuousEnrollment)
            {
                footnote = Footnotes("continuousEnrollment");
                dateText = section.EndDate.GetValueOrDefault(DateTime.Now).ToString(footnote.StringFormat);
                footnoteTextResult += wSpace + Footnotes("continuousEnrollment").Text.Replace(dateParam, dateText);
            }

            return footnoteTextResult;
        }


        private static Dictionary<string, AutomatedFootnoteElement> GetFootnoteInstances(AutomatedFootnoteCollection collection)
        {
            Dictionary<string, AutomatedFootnoteElement> instances = new Dictionary<string, AutomatedFootnoteElement>();

            foreach (AutomatedFootnoteElement i in collection)
            {
                instances.Add(i.Name, i);
            }

            return instances;
        }
    }
}