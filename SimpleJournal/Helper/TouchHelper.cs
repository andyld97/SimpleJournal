using System;

namespace SimpleJournal.Helper
{
    public static class TouchHelper
    {
        public static void SetTouchState(bool state)
        {
            try
            {
#if !UWP
                string path = Consts.TouchExecutable;
                System.Diagnostics.Process.Start(path, state ? Consts.SetTouchOn : Consts.SetTouchOff);
#endif
            }
            catch (Exception)
            {

            }
        }
    }
}