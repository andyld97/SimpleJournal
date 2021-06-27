using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SimpleJournal.Helper
{
    public static class TouchHelper
    {
        public static void SetTouchState(bool state)
        {
            try
            {
#if !UWP
                //Consts.TouchExecutable
                string path = Consts.TouchExecutable; // @"C:\Users\andre\source\repos\andyld97\SimpleJournal\Touch\bin\Debug\Touch.exe";
                System.Diagnostics.Process.Start(path, state ? Consts.SetTouchOn : Consts.SetTouchOff);
#endif
            }
            catch (Exception)
            {
                
            }
        }
    }
}
