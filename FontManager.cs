using SwinGameSDK;
using System.Collections.Generic;

namespace Arcadia
{
    /// <summary>
    /// A manager that caches fonts when they are used
    /// </summary>
    public class FontManager
    {
        /// <summary>
        /// A dictionary that stores the fonts, ordered by name then size
        /// </summary>
        public Dictionary<string, Dictionary<int, Font>> _fonts;

        /// <summary>
        /// Called when the font manager is created
        /// </summary>
        public FontManager()
        {
            //Initialize the dictionary to cache the fonts in
            _fonts = new Dictionary<string, Dictionary<int, Font>>();
        }

        /// <summary>
        /// Gets a font from the cache. If it's not cached, initialize it.
        /// </summary>
        /// <param name="Name">The name of the font</param>
        /// <param name="Size">The size (pt) of the font</param>
        /// <returns>The requested font</returns>
        public Font GetFont(string Name, int Size)
        {
            //If the font doesn't exist in the dictionary, initialize the cache
            if (!_fonts.ContainsKey(Name))
                _fonts[Name] = new Dictionary<int, Font>();

            //If the requested sized font doesn't exist in the cache, add it in
            if (!_fonts[Name].ContainsKey(Size))
                _fonts[Name][Size] = SwinGame.LoadFont(Name, Size);

            //Return the font
            return _fonts[Name][Size];
        }
    }
}
