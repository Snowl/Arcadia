using Arcadia.Emulator.Identifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Arcadia.Emulator
{
    public class Emu
    {
        public string Location;
        public string EmulatorName;
        public string LaunchExecutable;
        public string LaunchArguments;
        public string RomLocation;
        public string SearchPattern;
        public string IdentifierClass;
        public List<Game> Games;

        /// <summary>
        /// Generates a list of emulators with their games/roms
        /// </summary>
        /// <returns>The list of found emulators</returns>
        public static List<Emu> GenerateEmulators()
        {
            //If we don't have a directory called games, we should just exit.
            if (!Directory.Exists("Games"))
            {
                Log.Write("Couldn't find any emulators/games...");
                Environment.Exit(0);
            }

            //Create the data directory and make it hidden for game data to be stored in
            if (!Directory.Exists(Path.Combine("Games", "Data")))
            {
                Directory.CreateDirectory(Path.Combine("Games", "Data"));
                File.SetAttributes(Path.Combine("Games", "Data"), FileAttributes.Hidden);
            }

            //Initialize the emulator list to return
            List<Emu> emulatorList = new List<Emu>();

            //Create a new json serializer to serialize the data to
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //Get all the emulator directories within the Games directories
            var EmuDirectories = Directory.EnumerateDirectories("Games");
            foreach (string directory in EmuDirectories)
            {
                //The data directory doesn't contain an emulator so just skip it
                if (directory == Path.Combine("Games", "Data"))
                    continue;

                //Read the emulator data
                string informationJson = Path.Combine(directory, "emu.json");
                Emu emu = serializer.Deserialize<Emu>(File.ReadAllText(informationJson));
                emu.Location = directory;

                //Create the class specified within the emu.json to parse games for the emulator
                IIdentifier romIdentifier = (IIdentifier)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(emu?.IdentifierClass);
                //Identify the roms based off the class requested and assign them to the emulator
                emu.Games = romIdentifier.GetGames(emu);

                //Add the emulator to the list
                emulatorList.Add(emu);
            }

            //No emulators found, so we should exit.
            if (emulatorList.Count == 0)
            {
                Log.Write("Couldn't find any emulators/games...");
                Environment.Exit(0);
            }

            return emulatorList;
        }
    }
}
