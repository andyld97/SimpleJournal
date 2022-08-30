using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.5.8.0";
        public const string StoreVersion = "1.580.0.0";
        public const string Copyright = "Copyright © 2018-2022";

#if RELEASE
        public static readonly DateTime ReleaseDate = new DateTime(2022, 8, 27, 16, 37, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}