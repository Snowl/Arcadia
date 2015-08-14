using SwinGameSDK;

namespace Arcadia
{
    /// <summary>
    /// A class to show a marquee of text
    /// </summary>
    public class Marquee
    {
        private Font _font;
        private string _text;
        private int _y = 0;

        //The width of the text being displayed
        private int _width = 0;
        //The X position of the main text
        private int _mainX = 0;
        /* The X position of the side text (this is 
         * shown when the main text is going off the 
         * screen to give the illusion of "wrapping") */
        private int _sideX = 0;

        /// <summary>
        /// Creates a new marquee to render to the screen
        /// </summary>
        /// <param name="Text">The text to display on the screen</param>
        /// <param name="Font">The font to use for the marquee</param>
        /// <param name="Y">The Y position to display the marquee at</param>
        public Marquee(string Text, Font Font, int Y)
        {
            _font = Font;
            _text = Text;
            _y = Y;
            _mainX = 0;

            //Get the width of the text with the font used
            _width = SwinGame.TextWidth(_font, Text);
            //Dont show the sideX text, so put it off the left side of the screen
            _sideX = -_width;
        }

        /// <summary>
        /// Updates the text in the marquee
        /// </summary>
        /// <param name="Text">The text to display</param>
        public void UpdateText(string Text)
        {
            _text = Text;
            _width = SwinGame.TextWidth(_font, Text);
        }

        /// <summary>
        /// Draws the text to the screen
        /// </summary>
        public void Draw()
        {
            //If the text is starting to go off the screen, start rendering the sideX. Increment the sideX variable each time it is rendered
            if (_mainX + _width >= SwinGame.ScreenWidth())
                SwinGame.DrawText(_text, Globals.TextColor, _font, new Point2D() { X = _sideX++, Y = _y });
            else
                _sideX = -_width; //Reset the sideX to be off the screen once the main text is fully on screen

            //Reset the main text once it is fully off screen
            if (_mainX >= SwinGame.ScreenWidth())
                _mainX = 0;

            //Render the text and scroll it each time the marquee is rendered
            SwinGame.DrawText(_text, Globals.TextColor, _font, new Point2D() { X = _mainX++, Y = _y });
        }
    }
}
