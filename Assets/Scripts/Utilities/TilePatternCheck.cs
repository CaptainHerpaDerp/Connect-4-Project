using Core;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Static class that contains methods to check for patterns of tiles on the game board.
    /// </summary>
    public static class TilePatternCheck
    {     
        private static int rows => GamePrefs.Rows;
        private static int columns => GamePrefs.Columns;
        private static int tilesToWin => GamePrefs.TilesToWin;

        const int EMPTY = GamePrefs.EMPTY;
        const int OPPONENT = GamePrefs.OPPONENT;
        const int AI_PLAYER = GamePrefs.AI_PLAYER;

        #region Line Check and Track Methods

        /// <summary>
        /// Returns true if a line of 4 or more tiles is found horizontally from the placed tile, and returns the left and right indices of the line
        /// </summary>
        public static (bool isMatch, int leftColIndex, int rightColIndex) CheckHorizontal(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            int count = 1;

            // Tracked the farthest left and right line points
            int leftColIndex = placedCol, rightColIndex = placedCol;

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check to the right
                int col = placedCol + i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col >= columns || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest right index
                count++;
                leftColIndex = col;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check to the left
                int col = placedCol - i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col < 0 || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest left index
                count++;
                rightColIndex = col;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return (true, leftColIndex, rightColIndex);
            }

            return (false, 0, 0);
        }

        /// <summary>
        /// Returns true if a line of 4 or more tiles is found vertically from the placed tile, and returns the top and bottom indices of the line
        /// </summary>
        public static (bool isMatch, int topRowIndex, int bottomRowIndex) CheckVertical(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            // Start the count at 1 because we know the last placed tile is the player's
            int count = 1;

            /*Track the top and bottom indices of the line
             (top and bottom are set to the placed row initially, in case the placed tile becomes the top or bottom of the line)*/
            int topRowIndex = placedRow, bottomRowIndex = placedRow;

            // Start the for loop at 1 because we know the last placed tile is the player's
            for (int i = 1; i < tilesToWin; i++)
            {
                // Check upwards
                int row = placedRow + i;

                // Check bounds and ownership
                if (row >= rows || tileBoard[placedCol, row] != player) break;

                count++;

                topRowIndex = row;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check downwards
                int row = placedRow - i;

                // Check bounds and ownership
                if (row < 0 || tileBoard[placedCol, row] != player) break;

                count++;

                bottomRowIndex = row;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return (true, bottomRowIndex, topRowIndex);
            }

            else
            {
                return (false, 0, 0);
            }
        }

        /// <summary>
        /// Check both diagonal directions from a given spot and return true if a connection of 4 is made in any one direction, as well as the two pairs of indices of the line
        /// </summary>
        public static (bool isMatch, Vector2 indiceVector1, Vector2 indiceVector2) CheckDiagonal(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            /* --- Check bottom left to top right --- */

            // Start from 1, as we know the last place tile is the player's
            int count = 1;

            /* 
             * To draw the check line, we need to track the corners of the valid spots
             * Initially, we set both points to the position of the placed tile.
             */
            Vector2 trIndex = new(placedCol, placedRow);
            Vector2 blIndex = new(placedCol, placedRow);

            // Check the tiles to the right and above the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow + i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col >= columns || row >= rows || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count
                count++;

                // Set this position as the new farthest spot to the top right
                trIndex = new(col, row);
            }

            // Check the tiles to the left and below the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow - i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col < 0 || row < 0 || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count   
                count++;

                // Set this positio as the new farthest spot to the bottom left
                blIndex = new(col, row);
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return (true, blIndex, trIndex);
            }

            /* --- Check top left to bottom right --- */

            // Reset the count
            count = 1;

            Vector2 tlIndex = new(placedCol, placedRow);
            Vector2 brIndex = new(placedCol, placedRow);

            // Check the tiles to the left above the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow + i;

                if (col < 0 || row >= rows || tileBoard[col, row] != player) break;

                count++;

                tlIndex = new(col, row);
            }

            // Check the tiles to the right and below the placed tile 
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow - i;

                if (col >= columns || row < 0 || tileBoard[col, row] != player) break;

                count++;

                brIndex = new(col, row);
            }

            if (count >= tilesToWin)
            {
                return (true, tlIndex, brIndex);
            }

            return (false, Vector2.zero, Vector2.zero);
        }

        #endregion

        #region Line Check with Count Condition

        public static int CheckHorizontalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
            // Don't score if a horizontal win is not possible
            if (!HorizontalWinPossible(tileBoard, placedCol, placedRow, player))
            {
                return 0;
            }

            int count = 1;

            for (int i = 1; i < countCondition; i++)
            {
                // Check to the right
                int col = placedCol + i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col >= columns || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest right index
                count++;
            }

            for (int i = 1; i < countCondition; i++)
            {
                // Check to the left
                int col = placedCol - i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col < 0 || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest left index
                count++;
            }

            // Check if the count matches the required tiles
            if (count >= countCondition)
            {
                return 1;
            }

            return 0;
        }

        public static int CheckVerticalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
            // Don't score if a vertical win is not possible
            if (!VerticalWinPossible(tileBoard, placedCol, placedRow, player))
            {
                return 0;
            }

            // Start the count at 1 because we know the last placed tile is the player's
            int count = 1;

            // Start the for loop at 1 because we know the last placed tile is the player's
            for (int i = 1; i < countCondition; i++)
            {
                // Check upwards
                int row = placedRow + i;

                // Check bounds and ownership
                if (row >= rows || tileBoard[placedCol, row] != player) break;

                count++;
            }

            for (int i = 1; i < countCondition; i++)
            {
                // Check downwards
                int row = placedRow - i;

                // Check bounds and ownership
                if (row < 0 || tileBoard[placedCol, row] != player) break;

                count++;
            }

            // Check if the count matches the required tiles
            if (count >= countCondition)
            {
                return 1;
            }

            return 0;
        }

        public static int CheckDiagonalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
            // Don't score if a diagonal win is not possible
            if (!DiagonalWinPossible(tileBoard, placedCol, placedRow, player))
            {
                return 0;
            }

            /* --- Check bottom left to top right --- */

            // Start from 1, as we know the last place tile is the player's
            int count = 1;

            // Check the tiles to the right and above the placed tile
            for (int i = 1; i < countCondition; i++)
            {
                int col = placedCol + i, row = placedRow + i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col >= columns || row >= rows || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count
                count++;
            }

            // Check the tiles to the left and below the placed tile
            for (int i = 1; i < countCondition; i++)
            {
                int col = placedCol - i, row = placedRow - i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col < 0 || row < 0 || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count   
                count++;
            }

            // Check if the count matches the required tiles
            if (count >= countCondition)
            {
                return 1;
            }

            /* --- Check top left to bottom right --- */

            // Reset the count
            count = 1;

            // Check the tiles to the left above the placed tile
            for (int i = 1; i < countCondition; i++)
            {
                int col = placedCol - i, row = placedRow + i;

                if (col < 0 || row >= rows || tileBoard[col, row] != player) break;

                count++;
            }

            // Check the tiles to the right and below the placed tile 
            for (int i = 1; i < countCondition; i++)
            {
                int col = placedCol + i, row = placedRow - i;

                if (col >= columns || row < 0 || tileBoard[col, row] != player) break;

                count++;
            }

            if (count >= countCondition)
            {
                return 1;
            }

            return 0;
        }

        #endregion

        #region Space Availability Methods

        private static bool HorizontalWinPossible(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            // If the player is the AI, the opponent is the player.
            int opponent = player == AI_PLAYER ? OPPONENT : AI_PLAYER;

            int count = 1;

            // Check if a connection of 4 is even possible at the given position
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i;

                if (col >= columns || tileBoard[col, placedRow] == opponent) break;

                count++;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i;

                if (col < 0 || tileBoard[col, placedRow] == opponent) break;

                count++;
            }

            if (count >= tilesToWin)
            {
                return true;
            }

            return false;
        }

        private static bool VerticalWinPossible(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            // If the player is the AI, the opponent is the player.
            int opponent = player == AI_PLAYER ? OPPONENT : AI_PLAYER;

            int count = 1;

            for(int i = 1; i < tilesToWin; i++)
            {
                int row = placedRow + i;

                if (row >= rows || tileBoard[placedCol, row] == opponent) break;

                count++;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                int row = placedRow - i;

                if (row < 0 || tileBoard[placedCol, row] == opponent) break;

                count++;
            }

            if (count >= tilesToWin)
            {
                return true;
            }

            return false;
        }

        private static bool DiagonalWinPossible(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            // If the player is the AI, the opponent is the player.
            int opponent = player == AI_PLAYER ? OPPONENT : AI_PLAYER;

            int count = 1;

            /* --- Check bottom left to top right --- */

            // Check up and to the right
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow + i;

                if (col >= columns || row >= rows || tileBoard[col, row] == opponent) break;

                count++;
            }

            // Check down and to the left
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow - i;

                if (col < 0 || row < 0 || tileBoard[col, row] == opponent) break;

                count++;
            }

            if (count >= tilesToWin)
            {
                return true;
            }

            /* --- Check top left to bottom right --- */

            // Reset the count
            count = 1;

            // Check up and to the left
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow + i;

                if (col < 0 || row >= rows || tileBoard[col, row] == opponent) break;

                count++;
            }

            // Check down and to the right
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow - i;

                if (col >= columns || row < 0 || tileBoard[col, row] == opponent) break;

                count++;
            }

            if (count >= tilesToWin)
            {
                return true;
            }

            return false;
        }

        #endregion

    }
}

