using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace CTCClassSchedule
{
    public class AutomatedFootnotesSection : ConfigurationSection
    {
        /// <summary>
        ///
        /// </summary>
        [ConfigurationProperty("", IsRequired = false, IsDefaultCollection = true)]
        public AutomatedFootnoteCollection FootnoteInstances
        {
            get { return (AutomatedFootnoteCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class AutomatedFootnoteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AutomatedFootnoteElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AutomatedFootnoteElement)element).Name;
        }
    }

    public class AutomatedFootnoteElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("text", IsRequired = true)]
        public string Text
        {
            get { return (string)base["text"]; }
            set { base["text"] = value; }
        }

        [ConfigurationProperty("stringFormat", IsRequired = false)]
        public string StringFormat
        {
            get { return (string)base["stringFormat"]; }
            set { base["stringFormat"] = value; }
        }
    }
}