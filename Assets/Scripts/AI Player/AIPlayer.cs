using UnityEngine;
using Core;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Utilities;
using System.Threading.Tasks;
using System;

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

        [BoxGroup("AI Settings"), SerializeField] private bool useAIV2 = false;

        // Define some constant values to make the code more readable
        const int EMPTY = GamePrefs.EMPTY;
        const int OPPONENT = GamePrefs.OPPONENT;
        const int AI_PLAYER = GamePrefs.AI_PLAYER;

        // Game Prefs Getters
        private int rows => GamePrefs.Rows;
        private int columns => GamePrefs.Columns;
        private int tilesToWin => GamePrefs.TilesToWin;

        public async void GetMoveAsync(int[,] gameBoard, Action<int> callback)
        {
            int bestColumn = await Task.Run(() => GetMove(gameBoard));

            callback(bestColumn);
        }

        public int GetMove(int[,] gameBoard)
        {
            highestBudget = 0;

            int bestScore = int.MinValue;
            int bestColumn = -1;

            List<int> validMoves = GetValidDropPoints(gameBoard);

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
                int score = 0;

                if (useAIV2)
                {
                     score = EvaluateBoard2(tileBoard, depth);
                }
                else
                {
                    score = EvaluateBoard(tileBoard, placedCol, placedRow, depth);
                }

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

        #region Evaluation

        private int EvaluateBoard(int[,] tileBoard, int placedCol, int placedRow, int depth)
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

            // TODO: The AI should not be concerned about a row of 2 if there isn't 2 more tiles to complete the row

            score += TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;

            score += TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;

            score += TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 4) * aiWinEval + depth;
            score += TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 3) * aiThreeEval;
            score += TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, AI_PLAYER, 2) * aiTwoEval;


            score -= TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= TilePatternCheck.CheckHorizontalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            score -= TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= TilePatternCheck.CheckVerticalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            score -= TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 4) * playerWinEval - depth;
            score -= TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 3) * playerThreeEval;
            score -= TilePatternCheck.CheckDiagonalScore(tileBoard, placedCol, placedRow, OPPONENT, 2) * playerTwoEval;

            return score;
        }

        private int EvaluateBoard2(int[,] board, int depth)
        {
            // Check for immediate opponent win
            if (OpponentCanWinNextMove(board))
            {
                return -10000; // Arbitrary huge penalty
            }

            int score = 0;

            for (int col = 0; col <= columns - tilesToWin; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    int[] window = GetWindow(board, col, row, 1, 0);
                    score += EvaluateWindow(window, depth);
                }
            }

            // Evaluate vertical windows
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row <= rows - tilesToWin; row++)
                {
                    int[] window = GetWindow(board, col, row, 0, 1);
                    score += EvaluateWindow(window, depth);
                }
            }

            // Evaluate diagonal windows (bottom left to top right)
            for (int col = 0; col <= columns - tilesToWin; col++)
            {
                for (int row = 0; row <= rows - tilesToWin; row++)
                {
                    int[] window = GetWindow(board, col, row, 1, 1);
                    score += EvaluateWindow(window, depth);
                }
            }

            // Evaluate diagonal windows (top left to bottom right)
            for (int col = 0; col <= columns - tilesToWin; col++)
            {
                for (int row = tilesToWin - 1; row < rows; row++)
                {
                    int[] window = GetWindow(board, col, row, 1, -1);
                    score += EvaluateWindow(window, depth);
                }
            }

            return score;
        }

        private int EvaluateWindow(int[] window, int depth)
        {
            int score = 0;
            int emptyCount = 0;
            int playerCount = 0;
            int opponentCount = 0;

            foreach (int tile in window)
            {
                if (tile == AI_PLAYER)
                {
                    playerCount += 1;
                }
                else if (tile == OPPONENT)
                {
                    opponentCount += 1;
                }
                else
                {
                    emptyCount += 1;
                }
            }
            
            if (playerCount > 0 && opponentCount > 0)
            {
                return 0;
            }

            if (playerCount == 4)
            {
                score += aiWinEval + depth;
            }
            else if (playerCount == 3 && emptyCount == 1)
            {
                score += aiThreeEval + depth;
            }
            else if (playerCount == 2 && emptyCount == 2)
            {
                score += aiTwoEval + depth;
            }

            if (opponentCount == 4)
            {
                score -= playerWinEval - depth;
            }
            else if (opponentCount == 3 && emptyCount == 1)
            {
                score -= playerThreeEval - depth;
            }
            else if (opponentCount == 2 && emptyCount == 2)
            {
                score -= playerTwoEval - depth;
            }

            return score;
        }

        /// <summary>
        /// Extract a window of tiles from the game board with the given start column and row, and the direction to move in
        /// </summary>
        private int[] GetWindow(int[,] board, int startCol, int startRow, int colDir, int rowDir)
        {
            // The size of the window only needs to be the size of the tiles to win
            int[] window = new int[tilesToWin];

            for (int i = 0; i < tilesToWin; i++)
            {
                window[i] = board[startCol + i * colDir, startRow + i * rowDir];
            }

            return window;
        }

        #endregion

        #region Victory Check

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
        /// Checks for a victory in the game board by checking for 4 tiles in a row horizontally, vertically, or diagonally
        /// </summary>
        private bool CheckVictory(int[,] tileBoard, int placedCol, int placedRow, int player)
        {
            var resultHor = TilePatternCheck.CheckHorizontal(tileBoard, placedCol, placedRow, player);
            if (resultHor.isMatch)
            {
                return true;
            }

            var resultVert = TilePatternCheck.CheckVertical(tileBoard, placedCol, placedRow, player);
            if (resultVert.isMatch)
            {
                return true;
            }

            var resultDiag = TilePatternCheck.CheckDiagonal(tileBoard, placedCol, placedRow, player);
            if (resultDiag.isMatch)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}

