using Sirenix.OdinInspector;
using UnityEngine;

namespace Core
{
    public class GamePrefs : Singleton<GamePrefs>
    {
        [BoxGroup("Player Colours"), SerializeField] private Color[] playerColours;

        [BoxGroup("Game Conditions"), SerializeField] private int scoreToWin = 3;

        [BoxGroup("Tile Board Settings"), SerializeField] private int columns = 7;
        [BoxGroup("Tile Board Settings"), SerializeField] private int rows = 6;
        [BoxGroup("Tile Board Settings"), SerializeField] private int tilesToWin = 4;


        // Public accessors
        public int ScoreToWin => scoreToWin;
        public int Columns => columns;
        public int Rows => rows;
        public int TilesToWin => tilesToWin;    


        public Color GetPlayerColour(int playerNumber)
        {
            return playerColours[playerNumber - 1];
        }
    }
}