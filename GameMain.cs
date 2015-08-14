using System;
using System.Linq;
using SwinGameSDK;
using System.Collections.Generic;
using Arcadia.Emulator;
using System.IO;
using System.Diagnostics;

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

    //Any magic numbers in this code are just guesses. your guess is as good as mine :')
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

        //Amount of items to skip in the game listing
        private int _skip = 0;

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

            if (SwinGame.ScreenHeight() < 768 || SwinGame.ScreenWidth() < 1366)
            {
                Log.Write("Screen resolution too small...");
                Environment.Exit(0);
            }

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
                    Y = SwinGame.ScreenHeight() - _gameBack.Height - 50
                };
                SwinGame.DrawBitmap(_gameBack, gameBackLoc);

                //Poor man's iterator while still having foreach
                int i = 0;

                //Go through each game in the selected emulator
                foreach (Game game in _emulators[_selectedEmulator].Games.Skip(_skip).Take(12))
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
                    if (i == (_emulators[_selectedEmulator].SelectedGame - _skip))
                    {
                        //Only load bitmap into memory if the game changes
                        if (shouldUpdateData)
                        {
                            shouldUpdateData = false;
                            
                            //Load the logo to memory if it exists
                            if (File.Exists(Path.Combine(game.DataDirectory, "logo.png")))
                            {
                                _gameLogoBitmap = SwinGame.LoadBitmap(Path.Combine(game.DataDirectory, "logo.png"));
                            }
                            else
                            {
                                _gameLogoBitmap = null;
                            }
                        }

                        //If we haven't loaded a bitmap, show a textual logo of the games name
                        if (_gameLogoBitmap == null)
                        {
                            //Ensure that the game's name fits on screen
                            string TruncatedName = game.Name.Substring(0, Math.Min(game.Name.Length, 16));
                            if (TruncatedName != game.Name)
                                TruncatedName += "...";

                            //Render the game logo on screen
                            int LogoTextWidth = SwinGame.TextWidth(_manager.GetFont("PressStart2P", 60), TruncatedName);
                            int LogoTextHeight = SwinGame.TextHeight(_manager.GetFont("PressStart2P", 60), TruncatedName);
                            SwinGame.DrawText(TruncatedName,
                                              Globals.TextColor,
                                              _manager.GetFont("PressStart2P", 60),
                                              new Point2D() { X = (SwinGame.ScreenWidth() / 2) - (LogoTextWidth / 2),
                                                              Y = LogoPadding + (Globals.LogoMaxSize / 2) - (LogoTextHeight / 2) });
                        }
                        else
                        {
                            //Draw the offical game logo to screen
                            SwinGame.DrawBitmap(_gameLogoBitmap, 
                                                new Point2D() { X = (SwinGame.ScreenWidth() / 2) - (_gameLogoBitmap.Width / 2),
                                                                Y = LogoPadding + (Globals.LogoMaxSize / 2) - (_gameLogoBitmap.Height / 2) });
                        }

                        //Draw a rectangle around the selected item
                        SwinGame.FillRectangle(Globals.Marquee, RenderLocation.X - 4, RenderLocation.Y - 4, TextWidth + 8, TextHeight + 8);

                        //If the user presses enter, launch the game with the specified parameters.
                        if (SwinGame.KeyTyped(KeyCode.vk_RETURN))
                        {
                            var proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(_emulators[_selectedEmulator].Location, _emulators[_selectedEmulator].LaunchExecutable),
                                    Arguments = string.Format(_emulators[_selectedEmulator].LaunchArguments, _emulators[_selectedEmulator].Games[i + _skip].Location),
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();
                        }
                    }

                    //Draw the games into the list
                    SwinGame.DrawText(game.Name, Globals.TextColor,  _manager.GetFont("Geometria", 24), RenderLocation);

                    i += 1;
                }


                //Draws the scrollbar track on the side
                SwinGame.FillRectangle(Globals.ScrollBarTrack, _gameBack.Width + gameBackLoc.X - 30, gameBackLoc.Y + 7,
                                       10, _gameBack.Height - 28);

                //If we have more than 12 games, we need to show the track to show how far the user has scrolled
                if (_emulators[_selectedEmulator].Games.Count > 12)
                {
                    //Get the amount of games that are exceeding the specified game
                    int amtOfExtraGames = _emulators[_selectedEmulator].Games.Count - 12;
                    //Get the ratio size that the scroll thumb should be
                    int ratio = 100 - (amtOfExtraGames * 10);

                    //Get the thumb height based off the ratio
                    int ThumbHeight = ((_gameBack.Height - 28) * ratio) / 100;

                    //Get the interval height. The 26 was just random guessing
                    int IntervalHeight = (_gameBack.Height - ThumbHeight - 28) / amtOfExtraGames;

                    //Draw the thumb at the location the user is at
                    SwinGame.FillRectangle(Globals.ScrollBarThumb, _gameBack.Width + gameBackLoc.X - 30, gameBackLoc.Y + 7 + (_skip * IntervalHeight),
                                      10, ThumbHeight);
                }

                //Draw the emulators down the bottom
                DrawEmulatorText();

                //If the user scrolls through the game index
                if (SwinGame.KeyTyped(KeyCode.vk_UP))
                {
                    //Ensure that the logo gets updated
                    shouldUpdateData = true;
                    //Decrement the index (ensure it doesn't go below 0)
                    _emulators[_selectedEmulator].SelectedGame = Math.Max(0, _emulators[_selectedEmulator].SelectedGame - 1);

                    //If the user needs to scroll, decrease the skip amount by 1
                    if (_emulators[_selectedEmulator].SelectedGame - _skip < 6 && _skip > 0)
                    {
                        _skip -= 1;
                    }
                }
                else if (SwinGame.KeyTyped(KeyCode.vk_DOWN))
                {
                    //Ensure that the logo gets updated
                    shouldUpdateData = true;
                    //Decrement the index (ensure it doesn't go above the amount of games)
                    _emulators[_selectedEmulator].SelectedGame = Math.Min(_emulators[_selectedEmulator].SelectedGame + 1, 
                                                                          _emulators[_selectedEmulator].Games.Count - 1);

                    //If the user needs to scroll, decrease the skip amount by 1
                    if (_emulators[_selectedEmulator].SelectedGame > 6 && _skip + 12 < _emulators[_selectedEmulator].Games.Count)
                    {
                        _skip += 1;
                    }
                }

                //If the user scrolls through the emulator index
                if (SwinGame.KeyTyped(KeyCode.vk_LEFT))
                {
                    //Ensure that the logo gets updated
                    shouldUpdateData = true;
                    _selectedEmulator = Math.Max(0, _selectedEmulator - 1);

                    FixSkip();
                }
                else if(SwinGame.KeyTyped(KeyCode.vk_RIGHT))
                {
                    shouldUpdateData = true;
                    _selectedEmulator = Math.Min(_selectedEmulator + 1, _emulators.Count - 1);

                    FixSkip();
                }

                //Draw the marquee and the background for the marquee
                SwinGame.FillRectangle(Globals.Marquee, 0, SwinGame.ScreenHeight() - 12, SwinGame.ScreenWidth(), 12);
                _topMarquee.Draw();

                //Draw Arcadia to Screen
                SwinGame.RefreshScreen(60);
            }
        }

        /// <summary>
        /// Fix the amount of games to skip if the user changes emulators
        /// </summary>
        public void FixSkip()
        {
            _skip = 0;
            if (_emulators[_selectedEmulator].SelectedGame > 6)
            {
                _skip = _emulators[_selectedEmulator].SelectedGame - 6;
            }
            if (_skip + 12 > Math.Max(_emulators[_selectedEmulator].Games.Count, 12))
            {
                _skip = _emulators[_selectedEmulator].Games.Count - 12;
            }
        }

        /// <summary>
        /// Draws the selected emulator at the bottom, along with the rest on the side
        /// </summary>
        public void DrawEmulatorText()
        {
            //Gets the width & height of the main emulator text
            int EmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 24), _emulators[_selectedEmulator].EmulatorName);
            int EmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 24), _emulators[_selectedEmulator].EmulatorName);

            //Gets the x position of the main emulator text
            int EmulatorX = ((SwinGame.ScreenWidth() + 30) / 2) - (EmulatorWidth / 2);

            //Draws the main emulator text to screen
            SwinGame.DrawText(_emulators[_selectedEmulator].EmulatorName, Globals.TextColor, _manager.GetFont("Geometria", 24),
                              new Point2D()
                              {
                                  X = EmulatorX,
                                  Y = SwinGame.ScreenHeight() - 12 - EmulatorHeight - 8
                              });

            //The accumulatedWidth is the width of all the current items drawn
            int AccumulatedWidth = EmulatorX;

            //Goes through each emulator from the main emulator backwards
            for (int n = _selectedEmulator - 1; n >= 0; n--)
            {
                //Draws each emulator text to screen
                int SmallEmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName) + 10;
                int SmallEmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName);
                SwinGame.DrawText(_emulators[n].EmulatorName, Globals.TextColor, _manager.GetFont("Geometria", 14),
                                  new Point2D()
                                  {
                                      X = AccumulatedWidth - SmallEmulatorWidth,
                                      Y = SwinGame.ScreenHeight() - 12 - SmallEmulatorHeight - 8
                                  });
                AccumulatedWidth -= SmallEmulatorWidth;
            }

            //Goes through each emulator from the main emulator forwards
            AccumulatedWidth = EmulatorX + EmulatorWidth;
            for (int n = _selectedEmulator + 1; n < _emulators.Count; n++)
            {
                //Draws each emulator text to screen
                int SmallEmulatorWidth = SwinGame.TextWidth(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName) + 10;
                int SmallEmulatorHeight = SwinGame.TextHeight(_manager.GetFont("Geometria", 14), _emulators[n].EmulatorName);
                SwinGame.DrawText(_emulators[n].EmulatorName, Globals.TextColor, _manager.GetFont("Geometria", 14),
                                  new Point2D()
                                  {
                                      X = AccumulatedWidth + 10,
                                      Y = SwinGame.ScreenHeight() - 12 - SmallEmulatorHeight - 8
                                  });
                AccumulatedWidth += SmallEmulatorWidth;
            }
        }

        public void UpdateMarquee()
        {
            _topMarquee.UpdateText($"Arcadia v{_version} [{_currentTime.ToString()}]");
        }
    }
}