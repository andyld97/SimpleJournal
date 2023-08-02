using SimpleJournal.Common;
using System;

namespace Data
{
    public class UpdateInfo
    {
        /// <summary>
        /// Info whether there is a new update or not
        /// </summary>
        public UpdateResult Result { get; set; }

        /// <summary>
        /// The newest available version (if any, otherwise null)
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The timestamp when the cache was written
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now; 

        public UpdateInfo(UpdateResult result, Version version)
        {
            Result = result;
            Version = version;
        }

        public UpdateInfo()
        { }
    }
}