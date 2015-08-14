using System;
using SwinGameSDK;
using System.Collections.Generic;
using Arcadia.Emulator;

namespace Arcadia
{
    public class GameMain
    {         
        public static void Main()
        {
            Loader load = new Loader();
            load.Start();
        }
    }

    public class Loader
    {
        private string _version;

        public FontManager _manager;
        public Marquee _topMarquee;
        public DateTime _currentTime;
        public List<Emu> _emulators;

        public void Start()
        {
            //Initialize the log for Arcadia
            Log.Initialize();

            //Start up the window for Arcadia - if it's release version we want it to be full screen
#if RELEASE
            SwinGame.OpenGraphicsWindow("Arcadia", 
                                       (int)System.Windows.SystemParameters.PrimaryScreenWidth,
                                       (int)System.Windows.SystemParameters.PrimaryScreenHeight);
            SwinGame.ToggleFullScreen();
#else
            SwinGame.OpenGraphicsWindow("Arcadia", 1366, 768);
#endif

            //Get the version of the build to show on the marquee
            _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Initialize the font manager
            _manager = new FontManager();

            //Initialize the current time
            _currentTime = DateTime.Now;
            
            //Generate the emulators for the player to select
            _emulators = Emu.GenerateEmulators();

            //Initalize and update the marquee at the bottom of the window
            _topMarquee = new Marquee("", _manager.GetFont("PressStart2P", 8), SwinGame.ScreenHeight() - 10);
            UpdateMarquee();

            //Render Arcadia
            while (!SwinGame.WindowCloseRequested())
            {
                SwinGame.ProcessEvents();

                if (SwinGame.KeyDown(KeyCode.vk_ESCAPE))
                    Environment.Exit(0);

                SwinGame.ClearScreen(Globals.Background);

                if (_currentTime.Second != DateTime.Now.Second)
                {
                    _currentTime = DateTime.Now;
                    UpdateMarquee();
                }

                SwinGame.FillRectangle(Globals.Marquee, 0, SwinGame.ScreenHeight() - 12, SwinGame.ScreenWidth(), 12);
                _topMarquee.Draw();

                SwinGame.RefreshScreen(60);
            }
        }

        public void UpdateMarquee()
        {
            _topMarquee.UpdateText($"Arcadia v{_version} [{_currentTime.ToUniversalTime()}]");
        }
    }
}