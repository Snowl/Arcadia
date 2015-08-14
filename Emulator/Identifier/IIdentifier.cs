using System.Collections.Generic;

namespace Arcadia.Emulator.Identifier
{
    public interface IIdentifier
    {
        /// <summary>
        /// Called to get the games of an emulator
        /// </summary>
        /// <param name="emulator">The emulator to get the games for</param>
        /// <returns>A list of games that belong to the emulator</returns>
        List<Game> GetGames(Emu emulator);
    }
}
