using Sirenix.OdinInspector;
using UnityEngine;
using GameElements;
using Utilities;
using Core;
using UIManagement;
using System;

namespace Management
{
    public class GameManager : Singleton<GameManager>
    {
        #region Serialized Fields

        [BoxGroup("Components"), SerializeField] GameBoard gameBoard;
        [BoxGroup("Components"), SerializeField] LineRenderer lineRenderer;

        [BoxGroup("Prefabs"), SerializeField] GameObject tileSpacePrefab;
        [BoxGroup("Prefabs"), SerializeField] GameObject tilePiecePrefab;

        [BoxGroup("Tile Board Settings"), SerializeField] private int columns = 7;
        [BoxGroup("Tile Board Settings"), SerializeField] private int rows = 6;

        [BoxGroup("Tile Placement"), SerializeField] private Transform tileParentTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePlacementAreaTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePieceParentTransform;

        [BoxGroup("Player Colours"), SerializeField] private Color[] playerColours;

        [Header("The duration of time a match will be shown before the round is reset")]
        [BoxGroup("Visual Settings"), SerializeField] private float connectionShowDuration = 0.5f;

        #endregion

        private Vector2 boardCornerPosition;
        private Vector2 tileScale;

        // Represents the ownership of each tile on the board
        private int[,] tileBoard;

        // Represents the game objects for each tile slot on the board
        private GameObject[,] tileObjects;

        // Represents the pieces that have been placed on the board
        private GameObject[,] tilePieces;

        private bool isRoundOver = false;

        int tilesToWin = 4;
        int p1TilesInARow = 0;
        int p2TilesInARow = 0;

        [ShowInInspector, ReadOnly, BoxGroup("Player Scores")] private int player1Score, player2Score;

        // Singletons
        private EventBus eventBus;

        private void Start()
        {
            // Singleton initialization
            eventBus = EventBus.Instance;

            if (rows == 0 || columns == 0)
            {
                Debug.LogError("Error initializing tile board: rows or columns are set to 0!");
                return;
            }

            if (tileSpacePrefab == null)
            {
                Debug.LogError("Error initializing tile board: tile space prefab is not set!");
                return;
            }

            gameBoard.Initialize(columns);

            InitializeTileBoardEven();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitializeTileBoardEven();
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!isRoundOver)
                    PlaceTileOnColumn(gameBoard.GetSelectedColumnNumber(), 1);
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (!isRoundOver)
                    PlaceTileOnColumn(gameBoard.GetSelectedColumnNumber(), 2);
            }
        }

        /// <summary>
        /// Generate the game board
        /// </summary>
        private void InitializeTileBoardEven()
        {
            // Destroy any existing tile objects
            if (tileObjects != null)
            {
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (tileObjects[x, y] != null)
                        {
                            Destroy(tileObjects[x, y]);
                        }
                    }
                }
            }

            // Destroy any existing tile pieces
            if (tilePieces != null)
            {
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (tilePieces[x, y] != null)
                        {
                            Destroy(tilePieces[x, y]);
                        }
                    }
                }
            }

            tileBoard = new int[columns, rows];
            tileObjects = new GameObject[columns, rows];
            tilePieces = new GameObject[columns, rows];

            Vector2 boardSize = new Vector2(tilePlacementAreaTransform.lossyScale.x, tilePlacementAreaTransform.lossyScale.y);
            boardCornerPosition = (Vector2)tilePlacementAreaTransform.position - boardSize / 2;

            // Determine the spacing between each tile object based on the scale of the tile placement area and the number of columns/rows
            float spacingX = (tilePlacementAreaTransform.transform.localScale.x / columns);
            float spacingY = (tilePlacementAreaTransform.transform.localScale.y / rows);
            tileScale = new Vector3(spacingX, spacingY, 1f);

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    // Get the placement position for the tile object
                    Vector2 placementPos = new Vector2(x * spacingX, y * spacingY) + boardCornerPosition;

                    // Instantiate the new tile object
                    GameObject newTileSpace = Instantiate(tileSpacePrefab, placementPos, Quaternion.identity, parent: tileParentTransform);

                    // Assign the tile object to the tileObjects array
                    tileObjects[x, y] = newTileSpace;

                    newTileSpace.transform.localScale = tileScale;

                    newTileSpace.transform.position += new Vector3(spacingX / 2, spacingY / 2, 0);
                }
            }
        }

        /// <summary>
        /// Place a piece on the game board on the specified column on the lowest available row 
        /// </summary>
        /// <param name="colNumber"></param>
        private void PlaceTileOnColumn(int colNumber, int playerNumber)
        {
            // If -1 is returned, the mouse is not over the game board or another error occurred
            if (colNumber == -1)
                return;

            // Check downwards from the top of the column to find the first empty space
            for (int row = 0; row < rows; row++)
            {
                if (tileBoard[colNumber, row] == 0)
                {
                    // Set the ownership of the tile on the board
                    tileBoard[colNumber, row] = playerNumber;
                    GameObject newTilePiece = Instantiate(tilePiecePrefab, tileObjects[colNumber, row].transform.position, Quaternion.identity, parent: tilePieceParentTransform);

                    // Set the tile piece in the tilePieces array to the new tile piece
                    tilePieces[colNumber, row] = newTilePiece;

                    Debug.Log($"Placed tile at ({colNumber},{row})");

                    newTilePiece.GetComponent<SpriteRenderer>().color = playerColours[playerNumber - 1];

                    newTilePiece.transform.localScale = tileScale;

                    // Since a tile was placed, check for a victory
                    CheckVictory(colNumber, row, playerNumber);

                    break;
                }
            }
        }

        /// <summary>
        /// Checks for a victory in the game board by checking for 4 tiles in a row horizontally, vertically, or diagonally
        /// </summary>
        private void CheckVictory(int placedCol, int placedRow, int player)
        {
            if (CheckHorizontal(placedCol, placedRow, player)) return;

            else if (CheckVertical(placedCol, placedRow, player)) return;

            else CheckDiagonal(placedCol, placedRow, player);
        }

        #region Line Check Methods

        private bool CheckHorizontal(int placedCol, int placedRow, int player)
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
                DrawLineRenderer(tileObjects[leftColIndex, placedRow].transform.position, tileObjects[rightColIndex, placedRow].transform.position);
                EndRound(player);
                return true;
            }

            return false;
        }

        private bool CheckVertical(int placedCol, int placedRow, int player)
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
                // Check upwards
                int row = placedRow - i;

                // Check bounds and ownership
                if (row < 0 || tileBoard[placedCol, row] != player) break;

                count++;

                bottomRowIndex = row;
            }

            // Check if the count matches the required tiles
            if (count >= tilesToWin)
            {
                DrawLineRenderer(tileObjects[placedCol, bottomRowIndex].transform.position, tileObjects[placedCol, topRowIndex].transform.position);
                EndRound(player);
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
        private bool CheckDiagonal(int placedCol, int placedRow, int player)
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

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check the tile to the right and above the placed tile
                int col = placedCol + i, row = placedRow + i;

                // If we are out of bounds, or the tile is not the player's, we break the count 'streak'
                if (col >= columns || row >= rows || tileBoard[col, row] != player) break;
 
                // Otherwise, we increase the count
                count++;               

                // Set this position as the new farthest spot to the top right
                trIndex = new(col, row);
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check the tile to the left and below the placed tile
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
                DrawLineRenderer(tileObjects[(int)blIndex.x, (int)blIndex.y].transform.position, tileObjects[(int)trIndex.x, (int)trIndex.y].transform.position);
                EndRound(player);
                return true;
            }

            /* --- Check top left to bottom right --- */

            // Reset the count
            count = 1;

            Vector2 tlIndex = new(placedCol, placedRow);
            Vector2 brIndex = new(placedCol, placedRow);

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check the tile to the left above the placed tile
                int col = placedCol - i, row = placedRow + i;

                if (col < 0 || row >= rows || tileBoard[col, row] != player) break;

                count++;

                tlIndex = new(col, row);
            }

            for (int i = 1; i < tilesToWin; i++)
            {
                // Check the tile to the right and below the placed tile 
                int col = placedCol + i, row = placedRow - i;

                if (col >= columns || row < 0 || tileBoard[col, row] != player) break;

                count++;

                brIndex = new(col, row);
            }

            if (count >= tilesToWin)
            {
                DrawLineRenderer(tileObjects[(int)tlIndex.x, (int)tlIndex.y].transform.position, tileObjects[(int)brIndex.x, (int)brIndex.y].transform.position);
                EndRound(player);
                return true;
            }

            return false;
        }

        #endregion

        private void EndRound(int winner)
        {
            isRoundOver = true;
            StartCoroutine(Utils.WaitDurationAndExecuteCR(connectionShowDuration, () =>
            {
                ResetPlacedPieces();
                isRoundOver = false;
            }));

            if (winner == 1)
            {
                player1Score++;
                eventBus.Publish<int, int>("RoundOver", 1, player1Score);
            }
            else if (winner == 2)
            {
                player2Score++;
                eventBus.Publish<int, int>("RoundOver", 2, player2Score);
            }

            // Repaint the inspector to be able to see the updated scores
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void ResetPlacedPieces()
        {
            // Hide the line renderer
            lineRenderer.positionCount = 0;

            // Reset the ownership of each tile on the board
            tileBoard = new int[columns, rows];

            // Destroy any existing tile pieces
            if (tilePieces != null)
            {
                tilePieces = new GameObject[columns, rows];
            }

            // Destroy any existing tile piece objects
            foreach (Transform child in tilePieceParentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        private void DrawLineRenderer(Vector2 startPos, Vector2 endPos)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }
}