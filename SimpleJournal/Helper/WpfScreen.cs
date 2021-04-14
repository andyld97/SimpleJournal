using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace SimpleJournal.Helper
{
    public class WpfScreen
    {
        private readonly Screen screen;

        #region Properties

        public static WpfScreen Primary => new WpfScreen(System.Windows.Forms.Screen.PrimaryScreen);

        public Rect DeviceBounds => this.GetRect(this.screen.Bounds);

        public Rect WorkingArea => this.GetRect(this.screen.WorkingArea);

        public bool IsPrimary => this.screen.Primary;

        public string DeviceName => this.screen.DeviceName;

        #endregion

        internal WpfScreen(System.Windows.Forms.Screen screen)
        {
            this.screen = screen;
        }

        public static IEnumerable<WpfScreen> AllScreens()
        {
            foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                yield return new WpfScreen(screen);
            }
        }

        public static WpfScreen GetScreenFrom(Window window)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            Screen screen = System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
            WpfScreen wpfScreen = new WpfScreen(screen);
            return wpfScreen;
        }

        public static WpfScreen GetScreenFrom(System.Windows.Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            // are x,y device-independent-pixels ??
            System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
            Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
            WpfScreen wpfScreen = new WpfScreen(screen);

            return wpfScreen;
        }

        private Rect GetRect(Rectangle value)
        {
            // should x, y, width, height be device-independent-pixels ??
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        } 
    }
}
