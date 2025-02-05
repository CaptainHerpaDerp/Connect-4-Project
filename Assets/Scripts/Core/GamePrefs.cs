using UnityEngine;

namespace Core
{
    /// <summary>
    /// Stores constant variables for the game
    /// </summary>
    public static class GamePrefs
    {
        public const int ScoreToWin = 3;

        public const int Columns = 7;
        public const int Rows = 6;
        public const int TilesToWin = 4;

        public const int EMPTY = 0;
        public const int OPPONENT = 1;
        public const int AI_PLAYER = 2;

        private static Color player1Colour = new(14f / 255f, 107f / 255f, 220f / 255f);
        private static Color player2Colour = new(221f / 255f, 14f / 255f, 27f / 255f);

        private static Color[] playerColours = { player1Colour, player2Colour };

        public static Color GetPlayerColour(int playerNumber)
        {
            return playerColours[playerNumber - 1];
        }
    }
}