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