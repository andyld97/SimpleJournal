using Newtonsoft.Json;
using SimpleJournal.Common;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        /// The updates hash integrity
        /// </summary>
        public string SHA256Hash { get; set; }  

        /// <summary>
        /// The timestamp when the cache was written
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now; 

        public UpdateInfo(UpdateResult result, Version version, string hash, DateTime lastUpdated)
        {
            Result = result;
            Version = version;
            SHA256Hash = hash;  
            LastUpdated = lastUpdated;
        }

        public UpdateInfo()
        { }
    }

    public class VersionInfo
    {
        [JsonProperty("current")]
        public CurrentVersion Current { get; set; }

        [JsonProperty("beta")]
        public string[] Beta { get; set; } = null;

        [JsonProperty("versions")]
        public List<LongVersionInfo> Versions { get; set; }
    }

    public class CurrentVersion
    {
        [JsonProperty("normal")]
        public string Normal { get; set; }

        [JsonProperty("store")]
        public string Store { get; set; }
    }

    public class LongVersionInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("beta")]
        public int Beta { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }    

        [JsonProperty("release")]
        public VersionDate Release { get; set; }

        [JsonProperty("last_patch")]
        public VersionDate LastPatch { get; set; }

        public override string ToString()
        {
            return Version;
        }
    }

    public class VersionDate
    {
        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        public DateTime AsDateTime => new DateTime(Year, Month, Day);

        public override string ToString()
        {
            return AsDateTime.ToString("s");
        }
    }
}