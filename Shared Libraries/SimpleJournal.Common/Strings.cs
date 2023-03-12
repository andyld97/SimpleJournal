﻿using System;

namespace SimpleJournal.Common
{
    public static class Strings
    {
        public const string Version = "0.6.0.0";
        public const string StoreVersion = "1.600.0.0";
        public const string Copyright = "Copyright © 2018-2023";

#if UWPNEW || NORMAL
        public static readonly DateTime ReleaseDate = new DateTime(2023, 3, 12, 9, 30, 0);
#else 
        public static readonly DateTime ReleaseDate = DateTime.Now;
#endif
    }
}