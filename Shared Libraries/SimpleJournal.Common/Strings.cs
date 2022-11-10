using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.5.9.0";
        public const string StoreVersion = "1.590.0.0";
        public const string Copyright = "Copyright © 2018-2022";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2022, 9, 2, 11, 0, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}