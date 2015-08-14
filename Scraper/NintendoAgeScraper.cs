using System.Net;

namespace Arcadia.Scraper
{
    /// <summary>
    /// A class to scrape information from NintendoAge
    /// </summary>
    public class NintendoAgeScraper
    {
        /// <summary>
        /// This will get the game title of a game by it's nintendo game code.
        /// </summary>
        /// <param name="console">The console to search for</param>
        /// <param name="GameCode">The gamecode of the game</param>
        /// <returns>The game's name</returns>
        public static string IdentifyGameByGameCode(NintendoAgeConsole console, string GameCode)
        {
            string GameTitle = "";

            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                int ConsoleID = (int)console;
                //Searches nintendoage for the game. Do not remove the cast - it will break if you do!!! VS is lying to you that it's redundant
                string URL = $"http://nintendoage.com/index.cfm?FuseAction=Search.Results&pId={ConsoleID}&Code_Element={GameCode}";
                string Html = client.DownloadString(URL);

                //Poor man's html parsing - looks for the first link that matches what we want. Do not remove the cast - it will break if you do!!! VS is lying to you that it's redundant
                int LinkIndex = Html.IndexOf($"index.cfm?FuseAction=Element.View&amp;pId={ConsoleID}&amp;egID=");
                if (LinkIndex == -1)
                    return GameTitle;

                //Gets the title attribute of the a tag we found
                int GameIndex = Html.IndexOf("title=\"", LinkIndex) + 7;
                if (GameIndex == -1)
                    return GameTitle;

                //Parse the a tag for the game title
                GameTitle = Html.Substring(GameIndex, Html.IndexOf('"', GameIndex) - GameIndex);
                GameTitle = GameTitle.Substring(5);
            }
            
            /* For some reason GameDB doesn't like finding these games unless they have 'Version' after them (because
             * that's "technically" their true name, so we just fix that here. */
            if ((GameTitle == "Pokemon Sapphire" || GameTitle == "Pokemon Emerald") && console == NintendoAgeConsole.GameboyAdvance)
            {
                GameTitle += " Version";
            }

            return GameTitle;
        }
    }

    /// <summary>
    /// This enum contains the console->number mapping that is used to search within NintendoAge's database
    /// </summary>
    public enum NintendoAgeConsole
    {
        NES = 1,
        Gameboy = 2,
        GameboyColor = 3,
        SNES = 4,
        N64 = 5,
        GameboyAdvance = 6,
        Gamecube = 7,
        NintendoDS = 8,
        GameAndWatch = 12,
        Wii = 14,
        VirtualBoy = 15,
        Nintendo3DS = 183,
        WiiU = 195,
        ColorTV = 196
    }
}
