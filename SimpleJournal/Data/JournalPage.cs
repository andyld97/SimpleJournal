using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SimpleJournal.Data
{
    public class JournalPage
    {
        private bool cacheIsValid = false;
        private byte[] cache = null;

        [XmlIgnore]
        public byte[] Data
        {
            get
            {
                if (cacheIsValid)
                    return cache;
                else
                {
                    if (string.IsNullOrEmpty(Base64Data))
                        return null;
                    else
                    {
                        cache = Convert.FromBase64String(Base64Data);
                        cacheIsValid = true;
                        return cache;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if there are any resources to display
        /// </summary>
        public bool HasAdditionalResources => JournalResources.Count != 0;

        /// <summary>
        /// Represents stroke collection of InkCanvas as base64
        /// </summary>
        public string Base64Data = string.Empty;

        /// <summary>
        /// Additonal data e.g images, text
        /// </summary>
        public List<JournalResource> JournalResources = new List<JournalResource>();

        /// <summary>
        /// The format of the page (currently only A4 is possible)
        /// </summary>
        public Format PageFormat { get; set; } = Format.A4;

        /// <summary>
        /// The pattern of the page (i.e. chequered)
        /// </summary>
        public PaperType PaperPattern { get; set; } = PaperType.Chequeued;

        public void SetData(byte[] data)
        {
            Base64Data = Convert.ToBase64String(data);
            cache = data;
            cacheIsValid = true;
        }
    }
}
