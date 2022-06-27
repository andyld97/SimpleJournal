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

        public UpdateInfo(UpdateResult result, Version version)
        {
            Result = result;
            Version = version;
        }
    }
}
