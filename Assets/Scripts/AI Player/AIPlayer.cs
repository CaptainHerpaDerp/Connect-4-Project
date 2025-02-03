using UnityEngine;
using Core;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace AI
{
    public enum AIDifficulty
    {
        Easy = 2, Medium = 4, Hard = 6
    }

    public class AIPlayer : Singleton<AIPlayer>
    {
        [BoxGroup("AI Settings"), SerializeField] private AIDifficulty aiDifficulty;

        private float highestBudget;

        [BoxGroup("AI Evaluation")]
        [BoxGroup("AI Evaluation/Player"), SerializeField] private int playerWinEval = 1000;
        [BoxGroup("AI Evaluation/Player"), SerializeField] private int playerThreeEval = 5;
        [BoxGroup("AI Evaluation/Player"), SerializeField] private int playerTwoEval = 2;

        [BoxGroup("AI Evaluation/AI"), SerializeField] private int aiWinEval = 1000;
        [BoxGroup("AI Evaluation/AI"), SerializeField] private int aiThreeEval = 5;
        [BoxGroup("AI Evaluation/AI"), SerializeField] private int aiTwoEval = 2;

        // Define some constant values to make the code more readable
        const int EMPTY = 0;
        const int OPPONENT = 1;
        const int AI_PLAYER = 2;

        // Singletons
        private GamePrefs gamePrefs;

        // Game Prefs Getters
        private int rows => gamePrefs.Rows;
        private int columns => gamePrefs.Columns;
        private int tilesToWin => gamePrefs.TilesToWin;

        private void Start()
        {
            gamePrefs = GamePrefs.Instance;
        }

        public int GetMove(int[,] gameBoard)
        {
            highestBudget = 0;

            int bestScore = int.MinValue;
            int bestColumn = -1;

            List<int> validMoves = GetValidDropPoints(gameBoard);
            //Debug.Log($"Valid moves: {string.Join(", ", validMoves)}");

            List<int> scores = new List<int>();

            foreach (int colDropIndex in validMoves)
            {
                int[,] newBoard;
                int rowIndex;

                // Simulate the AI's move only if the column is not full
                if (GetLowestRowAtColumn(gameBoard, colDropIndex) == -1)
                {
                    continue;  // Skip this column as it's full
                }

                (newBoard, rowIndex) = SimulateMove(gameBoard, colDropIndex, AI_PLAYER);
 
                // Evaluate this move using Minimax       // Difficulty represents search depth
                int score = MinMax(newBoard, colDropIndex, rowIndex, (int)aiDifficulty, int.MinValue, int.MaxValue, false);

                scores.Add(score);

                // Update offensive score
                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = colDropIndex;
                }
            }

            Debug.Log($"Highest Budget: {highestBudget}");

            return bestColumn;
        }

        #region Utilities

        /// <summary>
        /// Returns a list of all columns that aren't full
        /// </summary>
        /// <param name="gameBoard"></param>
        /// <returns></returns>
        private List<int> GetValidDropPoints(int[,] gameBoard)
        {
            List<int> validColumns = new List<int>();

            // Iterate through each column
            for (int col = 0; col < columns; col++)
            {
                if (gameBoard[col, rows - 1] == EMPTY)
                {
                    validColumns.Add(col);
                }
            }

            return validColumns;
        }

        /// <summary>
        /// Returns the lowest available point at a given column
        /// </summary>
        /// <param name="gameBoard"></param>
        /// <param name="column"></param>
        /// <returns>Row index</returns>
        private int GetLowestRowAtColumn(int[,] gameBoard, int column)
        {
            for (int row = 0; row < rows; row++)
            {
                if (gameBoard[column, row] == EMPTY)
                {
                    return row;
                }
            }

            Debug.LogError("Error finding lowest tile at column, all columns are full!");

            return -1;
        }

        #endregion

        int MinMax(int[,] tileBoard, int placedCol, int placedRow, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            // First check if the game is already over
            if (depth == 0 || CheckVictory(tileBoard, placedCol, placedRow, AI_PLAYER) || CheckVictory(tileBoard, placedCol, placedRow, OPPONENT))
            {
                int score = EvaluateBoard(tileBoard, placedCol, placedRow, depth);
                //Debug.Log($"Score: {score}");
                return score;
            }

            List<int> validMoves = GetValidDropPoints(tileBoard);

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;

                foreach (int colIndex in validMoves)
                {
                    int rowIndex = 0;
                    int[,] newBoard;

                    (newBoard, rowIndex) = SimulateMove(tileBoard, colIndex, AI_PLAYER);

                    int eval = MinMax(newBoard, colIndex, rowIndex, depth - 1, alpha, beta, false);

                    int searchBudget = (int)Mathf.Pow(validMoves.Count, depth);
                    if (searchBudget > highestBudget)
                    {
                        highestBudget = searchBudget;
                    }

                    maxEval = Mathf.Max(maxEval, eval);

                    alpha = Mathf.Max(alpha, eval);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return maxEval;
            }

            else
            {
                int minEval = int.MaxValue;

                foreach (int colIndex in validMoves)
                {
                    int rowIndex = 0;
                    int[,] newBoard;

                    (newBoard, rowIndex) = SimulateMove(tileBoard, colIndex, OPPONENT);

                    int eval = MinMax(newBoard, colIndex, rowIndex, depth - 1, alpha, beta, true);

                    // To determine the budget used, we raise the valid moves to the power of the depth

                    int searchBudget = (int)Mathf.Pow(validMoves.Count, depth);
                    if (searchBudget > highestBudget)
                    {
                        highestBudget = searchBudget;
                    }

                    minEval = Mathf.Min(minEval, eval);

                    beta = Mathf.Min(beta, eval);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return minEval;
            }
        }

        /// <summary>
        /// Return a copy of the current game board with the token placed from the given column
        /// </summary>
        /// <param name="board"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        (int[,], int) SimulateMove(int[,] board, int column, int player)
        {
            int[,] newBoard = (int[,])board.Clone();

            // Get the lowest available 
            int row = GetLowestRowAtColumn(newBoard, column);

            // Set the ownership of the tile to the given player's
            if (row != -1)
            {
                newBoard[column, row] = player;
            }
            else
            {
                Debug.LogError("Error simulating move, all columns are full!");
            }

            // Return the new board, as well as the row we placed a tile on
            return (newBoard, row);
        }

        #region Victory Check



        int EvaluateBoard(int[,] tileBoard, int placedCol, int placedRow, int depth)
        {
            // Check for immediate opponent win
            if (OpponentCanWinNextMove(tileBoard))
            {
                return -10000; // Arbitrary huge penalty
            }

            if (CheckVictory(tileBoard, placedCol, placedRow, AI_PLAYER))
            {
                return aiWinEval + depth;  // A win sooner (with more remaining depth) gets a higher score.
            }
            if (CheckVictory(tileBoard, placedCol, placedRow, OPPONENT))
            {
                return -playerWinEval - depth; // Similarly, a loss sooner is worse.
            }

            int score = 0;

            // Center column bonus
            int centerCol = Mathf.RoundToInt(columns / 2);
            for (int row = 0; row < rows; row++)
            {
                if (tileBoard[centerCol, row] == AI_PLAYER) score += 3;
                if (tileBoard[centerCol, row] == OPPONENT) score -= 3;
            }

            score += CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;

            score += CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;

            score += CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;


            score -= CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            score -= CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            score -= CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            return score;
        }

        // Helper function that simulates all opponent moves
        private bool OpponentCanWinNextMove(int[,] board)
        {
            List<int> validMoves = GetValidDropPoints(board);
            foreach (int col in validMoves)
            {
                (int[,] newBoard, int row) = SimulateMove(board, col, OPPONENT);
                if (CheckVictory(newBoard, col, row, OPPONENT))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Based on the current board, check if the AI can win in the next move
        /// </summary>
        /// <param name="tileBoard"></param>
        /// <returns>Return the column the AI needs to place a tile on</returns>
        int CheckVictory(int[,] tileBoard)
        {
            // Loop through each tile on the board
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    // If the tile is the AI's, check for a victory
                    if (tileBoard[col, row] == AI_PLAYER)
                    {
                        if (CheckVictory(tileBoard, col, row, AI_PLAYER))
                        {
                            return col;
                        }
                    }
                }
            }

            return 0;
        }
        private int CheckHorizontalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
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

        private int CheckVerticalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
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

        private int CheckDiagonalScore(int[,] tileBoard, int placedCol, int placedRow, int player, int countCondition)
        {
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

        /// <summary>
        /// Checks for a victory in the game board by checking for 4 tiles in a row horizontally, vertically, or diagonally
        /// </summary>
        private bool CheckVictory(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            if (CheckHorizontal(tileBoard, placedCol, placedRow, player)) return true;

            else if (CheckVertical(tileBoard, placedCol, placedRow, player)) return true;

            else if (CheckDiagonal(tileBoard, placedCol, placedRow, player)) return true;

            else return false;
        }

        private bool CheckHorizontal(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            int count = 1;

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check to the right
                int col = placedCol + i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col >= columns || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest right index
                count++;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check to the left
                int col = placedCol - i;

                // Check for bounds and ownership, break the loop if the tile is not the player's or if we are out of bounds
                if (col < 0 || tileBoard[col, placedRow] != player) break;

                // Otherwise, increase the count and set the new farthest left index
                count++;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return true;
            }

            return false;
        }

        private bool CheckVertical(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            // Start the count at 1 because we know the last placed tile is the player's
            int count = 1;

            // Start the for loop at 1 because we know the last placed tile is the player's
            for (int i = 1; i < tilesToWin; i++)
            {
                // Check upwards
                int row = placedRow + i;

                // Check bounds and ownership
                if (row >= rows || tileBoard[placedCol, row] != player) break;

                count++;
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check downwards
                int row = placedRow - i;

                // Check bounds and ownership
                if (row < 0 || tileBoard[placedCol, row] != player) break;

                count++;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check both diagonal directions from a given spot and return true if a connection of 4 is made in any one direction
        /// </summary>
        /// <param name="placedCol"></param>
        /// <param name="placedRow"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckDiagonal(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            /* --- Check bottom left to top right --- */

            // Start from 1, as we know the last place tile is the player's
            int count = 1;

            // Check the tiles to the right and above the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow + i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col >= columns || row >= rows || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count
                count++;
            }

            // Check the tiles to the left and below the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow - i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col < 0 || row < 0 || tileBoard[col, row] != player) break;

                // Otherwise, we increase the count   
                count++;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                return true;
            }

            /* --- Check top left to bottom right --- */

            // Reset the count
            count = 1;

            // Check the tiles to the left above the placed tile
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol - i, row = placedRow + i;

                if (col < 0 || row >= rows || tileBoard[col, row] != player) break;

                count++;
            }

            // Check the tiles to the right and below the placed tile 
            for (int i = 1; i < tilesToWin; i++)
            {
                int col = placedCol + i, row = placedRow - i;

                if (col >= columns || row < 0 || tileBoard[col, row] != player) break;

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

