using System;
using SwinGameSDK;
using System.Collections.Generic;
using Arcadia.Emulator;
using System.IO;

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

        public Bitmap _gameLogoBitmap;
        public int _selectedEmulator;
        public bool shouldUpdateData = true;

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

            //Get the square to display the games on
            var LogoPadding = 50;
            var _gameBack = SwinGame.LoadBitmap("back.png");

            //Add 40 to the max size just to ensure some empty space before the list
            Globals.LogoMaxSize = (SwinGame.ScreenHeight() - _gameBack.Height - 30) - LogoPadding - 40;

            //Generate the emulators for the player to select
            _emulators = Emu.GenerateEmulators();

            //Initalize and update the marquee at the bottom of the window
            _topMarquee = new Marquee("", _manager.GetFont("PressStart2P", 8), SwinGame.ScreenHeight() - 10);
            UpdateMarquee();

            //Render loop while the user hasn't clicked the X button
            while (!SwinGame.WindowCloseRequested())
            {
                //Process any key/joystick/mouse events
                SwinGame.ProcessEvents();

                //If the user presses escape exit the program
                if (SwinGame.KeyDown(KeyCode.vk_ESCAPE))
                    Environment.Exit(0);

                //Clear the screen with the defined background
                SwinGame.ClearScreen(Globals.Background);

                //Update the marquee if the second changes (we do this so it doesn't need to re-update every second)
                if (_currentTime.Second != DateTime.Now.Second)
                {
                    _currentTime = DateTime.Now;
                    UpdateMarquee();
                }

                var gameBackLoc = new Point2D()
                {
                    X = (SwinGame.ScreenWidth() / 2) - (_gameBack.Width / 2),
                    Y = SwinGame.ScreenHeight() - _gameBack.Height - 30
                };
                SwinGame.DrawBitmap(_gameBack, gameBackLoc);

                //Poor man's iterator while still having foreach
                int i = 0;

                //Go through each game in the selected emulator
                foreach (Game game in _emulators[_selectedEmulator].Games)
                {
                    //Get the size of the text of the game
                    int TextWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 24), game.Name);
                    int TextHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 24), game.Name);

                    //Draw the text in the middle of the game rectangle. The rectangle has +30 pixels on the sides for the drop shadow so we compensate for that.
                    Point2D RenderLocation = new Point2D()
                    {
                        X = gameBackLoc.X + ((_gameBack.Width + 30) / 2) - (TextWidth / 2),
                        Y = gameBackLoc.Y + ((i * 35) + 20)
                    };

                    //Show the image and highlight the text if the game is selected
                    if (i == _emulators[_selectedEmulator].SelectedGame)
                    {
                        //Only load bitmap into memory if the game changes
                        if (shouldUpdateData)
                        {
                            shouldUpdateData = false;
                            if (File.Exists(Path.Combine(game.DataDirectory, "logo.png")))
                            {
                                _gameLogoBitmap = SwinGame.LoadBitmap(Path.Combine(game.DataDirectory, "logo.png"));
                            }
                        }
                        SwinGame.DrawBitmap(_gameLogoBitmap, new Point2D() { X = (SwinGame.ScreenWidth() / 2) - (_gameLogoBitmap.Width / 2), Y = LogoPadding });
                        SwinGame.FillRectangle(Globals.Marquee, RenderLocation.X - 4, RenderLocation.Y - 4, TextWidth + 8, TextHeight + 8);
                    }

                    //Draw the games into the list
                    SwinGame.DrawText(game.Name, Color.White,  _manager.GetFont("Geometria", 24), RenderLocation);

                    i += 1;
                }

                //Draw the emulators down the bottom
                DrawEmulatorText();

                if (SwinGame.KeyTyped(KeyCode.vk_UP))
                {
                    shouldUpdateData = true;
                    _emulators[_selectedEmulator].SelectedGame = Math.Max(0, _emulators[_selectedEmulator].SelectedGame - 1);
                }
                if (SwinGame.KeyTyped(KeyCode.vk_DOWN))
                {
                    shouldUpdateData = true;
                    _emulators[_selectedEmulator].SelectedGame = Math.Min(_emulators[_selectedEmulator].SelectedGame + 1, _emulators[_selectedEmulator].Games.Count - 1);
                }

                if (SwinGame.KeyTyped(KeyCode.vk_LEFT))
                {
                    shouldUpdateData = true;
                    _selectedEmulator = Math.Max(0, _selectedEmulator - 1);
                }
                if (SwinGame.KeyTyped(KeyCode.vk_RIGHT))
                {
                    shouldUpdateData = true;
                    _selectedEmulator = Math.Min(_selectedEmulator + 1, _emulators.Count - 1);
                }

                //Draw the marquee and the background for the marquee
                SwinGame.FillRectangle(Globals.Marquee, 0, SwinGame.ScreenHeight() - 12, SwinGame.ScreenWidth(), 12);
                _topMarquee.Draw();

                //Draw Arcadia to Screen
                SwinGame.RefreshScreen(60);
            }
        }

        /// <summary>
        /// todo: comment this
        /// </summary>
        public void DrawEmulatorText()
        {
            int EmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 24), _emulators[_selectedEmulator].EmulatorName);
            int EmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 24), _emulators[_selectedEmulator].EmulatorName);
            int EmulatorX = ((SwinGame.ScreenWidth() + 30) / 2) - (EmulatorWidth / 2);
            SwinGame.DrawText(_emulators[_selectedEmulator].EmulatorName, Color.White, _manager.GetFont("Geometria", 24),
                              new Point2D()
                              {
                                  X = EmulatorX,
                                  Y = SwinGame.ScreenHeight() - 12 - EmulatorHeight - 6
                              });

            int AccumulatedWidth = EmulatorX;
            for (int n = _selectedEmulator - 1; n >= 0; n--)
            {
                int SmallEmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName) + 10;
                int SmallEmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName);
                SwinGame.DrawText(_emulators[n].EmulatorName, Color.White, _manager.GetFont("Geometria", 14),
                                  new Point2D()
                                  {
                                      X = AccumulatedWidth - SmallEmulatorWidth,
                                      Y = SwinGame.ScreenHeight() - 12 - SmallEmulatorHeight - 6
                                  });
                AccumulatedWidth -= SmallEmulatorWidth;
            }

            AccumulatedWidth = EmulatorX + EmulatorWidth;
            for (int n = _selectedEmulator + 1; n < _emulators.Count; n++)
            {
                int SmallEmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName) + 10;
                int SmallEmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName);
                SwinGame.DrawText(_emulators[n].EmulatorName, Color.White, _manager.GetFont("Geometria", 14),
                                  new Point2D()
                                  {
                                      X = AccumulatedWidth + 10,
                                      Y = SwinGame.ScreenHeight() - 12 - SmallEmulatorHeight - 6
                                  });
                AccumulatedWidth += SmallEmulatorWidth;
            }
        }

        public void UpdateMarquee()
        {
            _topMarquee.UpdateText($"Arcadia v{_version} [{_currentTime.ToUniversalTime()}]");
        }
    }
}