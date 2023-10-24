using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.6.4.0";
        public const string StoreVersion = "1.640.0.0";
        public const string Copyright = "Copyright © 2018-2023";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2023, 8, 5, 12, 0, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}