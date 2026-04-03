using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.7.2.0";
        public const string StoreVersion = "1.720.0.0";
        public const string Copyright = "Copyright © 2018-2026";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2026, 04, 03, 10, 41, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}