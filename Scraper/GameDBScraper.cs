using Arcadia.Emulator;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml;

namespace Arcadia.Scraper
{
    /// <summary>
    /// A class to scrape information from GameDB
    /// </summary>
    public class GameDBScraper
    {
        /// <summary>
        /// Gets information about the game and saves the logo of the game if possible in the DataDirectory
        /// </summary>
        /// <param name="game">The game to get information about</param>
        /// <returns>The game with updated information</returns>
        public static Game UpdateGameData(Game game)
        {
            //If the game's name is empty we can't get information about it... so just return the game.
            if (string.IsNullOrEmpty(game.Name))
                return game;

            using (WebClient client = new WebClient())
            {
                //Request the game from the website. This is a loose search so it might not return the game requested, but a game related to it
                string dbXML = client.DownloadString($"http://thegamesdb.net/api/GetGamesList.php?name={game.Name}");

                //The data returned is XML so we can parse it using XMLDocument
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(dbXML);

                //Get all nodes that belong within Data/Game
                var nodes = xmlDoc.SelectNodes("Data/Game");

                //If we couldn't find any nodes, this game probably doesn't exist in the DB, so we just return the current information.
                if (nodes.Count == 0)
                    return game;

                //Get the first game (probably the best match)
                var gameInfo = nodes[0];
                //Get the numeric ID of the game within GameDB's database
                var id = gameInfo["id"].InnerText;

                //Download the information about the requested game
                string gameDataXML = client.DownloadString($"http://thegamesdb.net/api/GetGame.php?id={id}");

                //Load the game data
                xmlDoc.LoadXml(gameDataXML);

                //Get the data from the XML document
                var gameData = xmlDoc.SelectNodes("Data/Game")[0];
                game.Name = gameData["GameTitle"].InnerText;
                game.ReleaseDate = gameData["ReleaseDate"]?.InnerText;
                game.Players = gameData["Players"]?.InnerText;

                string BaseImgUrl = xmlDoc.SelectNodes("Data")[0]["baseImgUrl"]?.InnerText;
                string ClearLogo = gameData["Images"]?["clearlogo"]?.InnerText;

                //If the "ClearLogo" isn't specified, we can't download it, so just return the updated game
                if (string.IsNullOrEmpty(ClearLogo))
                    return game;

                //If we found the clearlogo, download it and save it into the game's data directory.
                client.DownloadFile(BaseImgUrl + ClearLogo, Path.Combine(game.DataDirectory, "logo.png"));

                var clearLogoImage = Bitmap.FromFile(Path.Combine(game.DataDirectory, "logo.png"));

                if (clearLogoImage.Height > Globals.LogoMaxSize)
                {
                    var scaledImage = ScaleImage(clearLogoImage, Int32.MaxValue, Globals.LogoMaxSize);
                    clearLogoImage.Dispose();
                    scaledImage.Save(Path.Combine(game.DataDirectory, "logo.png"));
                }
            }

            //Return the game with updated information
            return game;
        }

        static public Bitmap ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            Bitmap bmp = new Bitmap(newImage);

            return bmp;
        }
    }
}
