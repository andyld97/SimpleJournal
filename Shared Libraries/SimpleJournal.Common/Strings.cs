using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.7.1.0";
        public const string StoreVersion = "1.710.0.0";
        public const string Copyright = "Copyright © 2018-2025";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2025, 12, 20, 12, 0, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}