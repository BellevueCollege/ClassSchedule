using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace CTCClassSchedule
{
    /// <summary>
    /// Manages the automated footnote section in the web.config.
    /// </summary>
    public class AutomatedFootnotesSection : ConfigurationSection
    {
        /// <summary>
        /// Grabs the root of the settings not in the web.config file, and converts it to
        /// a collection of elements.
        /// </summary>
        [ConfigurationProperty("", IsRequired = false, IsDefaultCollection = true)]
        public AutomatedFootnoteCollection FootnoteInstances
        {
            get { return (AutomatedFootnoteCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    /// <summary>
    /// Reprents a collection of automated footnote messages.
    /// </summary>
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

    /// <summary>
    /// Represents a single automated message from within a collection
    /// </summary>
    public class AutomatedFootnoteElement : ConfigurationElement
    {
        /// <summary>
        /// The name of the automated footnote property.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        /// <summary>
        /// The text template that the footnote should display.
        /// </summary>
        [ConfigurationProperty("text", IsRequired = true)]
        public string Text
        {
            get { return (string)base["text"]; }
            set { base["text"] = value; }
        }

        /// <summary>
        /// If a special string format should be used, it will be defined
        /// within this property (ex. date format: "MM/dd/YYYY").
        /// </summary>
        [ConfigurationProperty("stringFormat", IsRequired = false)]
        public string StringFormat
        {
            get { return (string)base["stringFormat"]; }
            set { base["stringFormat"] = value; }
        }
    }
}