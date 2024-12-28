using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.6.7.0";
        public const string StoreVersion = "1.670.0.0";
        public const string Copyright = "Copyright © 2018-2025";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2024, 12, 28, 15, 10, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}