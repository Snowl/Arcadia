using Arcadia.Scraper;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;

namespace Arcadia.Emulator.Identifier
{
    public class Mame : IIdentifier
    {
        public List<Game> GetGames(Emu emulator)
        {
            List<Game> games = new List<Game>();

            //Create a JSON serializer to read/write the game data json files.
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            //Get all ROM files within the specified directory
            var RomFiles = Directory.GetFiles(Path.Combine(emulator.Location, emulator.RomLocation), emulator.SearchPattern);

            //Go through each rom within the rom directory
            foreach (var rom in RomFiles)
            {
                //Find the data for the rom within the data directory
                string infoJsonPath = Path.Combine("Games", "Data", Path.GetFileNameWithoutExtension(rom), "data.json");

                //If the data doesn't exist, we need to look it up through MAME
                if (!File.Exists(infoJsonPath))
                {
                    //Create the directory for the game data to be held in
                    if (!Directory.Exists(Path.GetDirectoryName(infoJsonPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(infoJsonPath));

                    //Create a new Game object to hold all the game data
                    Game romGame = new Game();

                    //The data directory is located where we specified it
                    romGame.DataDirectory = Path.GetDirectoryName(infoJsonPath);
                    romGame.Location = rom;

                    //Create a new mame process with the argument -listfull {rom} to get the rom name
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Path.Combine(emulator.Location, emulator.LaunchExecutable),
                            Arguments = $"-listfull {Path.GetFileNameWithoutExtension(rom)}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    proc.Start();

                    //Read the console output from MAME
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        //Read the input from the console
                        string output = proc.StandardOutput.ReadLine();
                        //Check if the string has a "
                        int gameLoc = output.IndexOf("\"");

                        //If it doesn't, it's the header and we don't care about this line
                        if (gameLoc == -1)
                            continue;

                        //Get the game name that's within the quotes
                        string gameName = output.Substring(gameLoc + 1, output.Length - gameLoc - 2);
                        Log.Write("Adding information for game \"" + gameName + "\"");

                        //Assign the game name to the game
                        romGame.Name = gameName;
                    }

                    //Get the game data from GameDB
                    romGame = GameDBScraper.UpdateGameData(romGame);

                    //Write the game data to the data directory
                    File.WriteAllText(infoJsonPath, serializer.Serialize(romGame));
                }

                Game game = serializer.Deserialize<Game>(File.ReadAllText(infoJsonPath));

                //Add the game to the game list
                if (!game.Skip)
                    games.Add(game);
            }

            return games;
        }
    }
}
