using Sirenix.OdinInspector;
using UnityEngine;
using GameElements;
using Utilities;
using Core;
using UIManagement;
using System;
using VisualElements;
using AI;

namespace Management
{
    public class GameManager : Singleton<GameManager>
    {
        #region Serialized Fields

        [BoxGroup("Components"), SerializeField] GameBoard gameBoard;
        [BoxGroup("Components"), SerializeField] LineRenderer lineRenderer;

        [BoxGroup("Prefabs"), SerializeField] GameObject tileSpacePrefab;
        [BoxGroup("Prefabs"), SerializeField] PlacementPiece placementPiecePrefab;
        [BoxGroup("Prefabs"), SerializeField] DroppingTilePiece droppingTilePiece;

        [BoxGroup("Tile Placement"), SerializeField] private Transform tileParentTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePlacementAreaTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePieceParentTransform;

        [Header("The duration of time a match will be shown before the round is reset")]
        [BoxGroup("Visual Settings"), SerializeField] private float connectionShowDuration = 0.5f;

        [Header("The minimum wait duration before another tile can be placed")]
        [BoxGroup("Placement Settings"), SerializeField] private float minPlaceWaitTime = 0.5f;

        [BoxGroup("AI Settings"), SerializeField] private bool isAIEnabled = false;

        #endregion

        private Vector2 boardCornerPosition;
        private Vector2 tileScale;

        // Represents the ownership of each tile on the board
        private int[,] tileBoard;

        // Represents the game objects for each tile slot on the board
        private GameObject[,] tileObjects;

        // Represents the pieces that have been placed on the board
        private PlacementPiece[,] placedPieces;

        // Either 1 or 2
        private int playerTurn = 1;
        private bool visualizeTilePiece = true;
        private bool isRoundOver = false;

        // The tile piece that will be placed on the board
        private PlacementPiece previewPlacementPiece;

        [SerializeField] private float tilePieceYPos;
        private bool isDropping = false;

        // Tile Waiting
        private bool isOnPlaceCooldown = false;

        // Singletons
        private EventBus eventBus;
        private GamePrefs gamePrefs;
        private AIPlayer aiPlayer;

        // Game Prefs Getters
        private int rows => gamePrefs.Rows;
        private int columns => gamePrefs.Columns;
        private int tilesToWin => gamePrefs.TilesToWin;

        #region Initialization

        private void Start()
        {
            // Singleton initialization
            eventBus = EventBus.Instance;
            gamePrefs = GamePrefs.Instance;
            aiPlayer = AIPlayer.Instance;

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

            previewPlacementPiece = Instantiate(placementPiecePrefab, Vector3.zero, Quaternion.identity);

            gameBoard.Initialize(columns);

            InitializeTileBoardEven();
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
            if (placedPieces != null)
            {
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (placedPieces[x, y] != null)
                        {
                            Destroy(placedPieces[x, y]);
                        }
                    }
                }
            }

            tileBoard = new int[columns, rows];
            tileObjects = new GameObject[columns, rows];
            placedPieces = new PlacementPiece[columns, rows];

            Vector2 boardSize = new Vector2(tilePlacementAreaTransform.lossyScale.x, tilePlacementAreaTransform.lossyScale.y);
            boardCornerPosition = (Vector2)tilePlacementAreaTransform.position - boardSize / 2;

            // Determine the spacing between each tile object based on the scale of the tile placement area and the number of columns/rows
            float spacingX = (tilePlacementAreaTransform.transform.localScale.x / columns);
            float spacingY = (tilePlacementAreaTransform.transform.localScale.y / rows);
            tileScale = new Vector3(spacingX, spacingY, 1f);

            // Set the scale of the preview tile piece and the dropping tile piece
            previewPlacementPiece.transform.localScale = tileScale;
            droppingTilePiece.transform.localScale = tileScale;

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

        #endregion

        #region Update Methods

        private void Update()
        {
            DoPlayerTurns();
        }

        private void FixedUpdate()
        {
            // Visualize the piece that will be placed on the board

            if (visualizeTilePiece)
            {
                if (isAIEnabled && playerTurn == 2)
                {
                    // Do not visualize the tile piece if the AI is enabled and it is the AI's turn
                    previewPlacementPiece.transform.position = new Vector2(1000, 1000);
                    return;
                }

                int colNumber = gameBoard.GetSelectedColumnNumber();
                // Clamp the column number to be within the bounds of the board
                if (colNumber == -1)
                {
                    previewPlacementPiece.transform.position = new Vector2(1000, 1000);
                    return;
                }

                // To get the last item in an array, we subtract 1 from the length or we can use the ^ operator

                // get the last row in the column
                int row = rows - 1;

                previewPlacementPiece.transform.position = GetDropPointPosition(colNumber);

                previewPlacementPiece.SetColour(gamePrefs.GetPlayerColour(playerTurn));
            }
        }

        #endregion

        #region Round Management

        private void DoTilePlaceCooldown()
        {
            isOnPlaceCooldown = true;

            StartCoroutine(Utils.WaitDurationAndExecuteCR(minPlaceWaitTime, () =>
            {
                isOnPlaceCooldown = false;
            }));
        }

        private void DoPlayerTurns()
        {

            if (isAIEnabled && playerTurn == 2)
            {
                PlaceTileOnColumn(aiPlayer.GetMove(tileBoard), playerTurn);
            }

            if (isOnPlaceCooldown)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!isRoundOver)
                {
                    // Do not place a tile if the AI is enabled and it is the AI's turn
                    if (isAIEnabled && playerTurn == 2)
                    {
                        return;
                    }

                    PlaceTileOnColumn(gameBoard.GetSelectedColumnNumber(), playerTurn);
                }
            }
        }

        /// <summary>
        /// Place a piece on the game board on the specified column on the lowest available row 
        /// </summary>
        /// <param name="colNumber"></param>
        private void PlaceTileOnColumn(int colNumber, int playerNumber)
        {
            // Put the tile placement on cooldown
            DoTilePlaceCooldown();

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
                    PlacementPiece newTilePiece = Instantiate(placementPiecePrefab, tileObjects[colNumber, row].transform.position, Quaternion.identity, parent: tilePieceParentTransform);

                    // Set the tile piece in the placedPieces array to the new tile piece
                    placedPieces[colNumber, row] = newTilePiece;

                    // Instantiate the new tile piece
                    newTilePiece.SetColour(gamePrefs.GetPlayerColour(playerNumber));
                    newTilePiece.transform.localScale = tileScale;

                    newTilePiece.Hide();

                    DroppingTilePiece newDroppingTilePiece = Instantiate(droppingTilePiece, new Vector2(1000, 1000), Quaternion.identity);
                    Vector2 dropPoint = GetDropPointPosition(colNumber);

                    // Drop the tile piece from the top of the board to the placed position
                    newDroppingTilePiece.DropToPosition(dropPoint, newTilePiece.transform.position, gamePrefs.GetPlayerColour(playerNumber));

                    isDropping = true;
                    CheckVictory(colNumber, row, playerNumber);

                    newDroppingTilePiece.OnPositionReached += () =>
                    {
                        if (newTilePiece != null)
                        {
                            newTilePiece.Show();
                        }

                        isDropping = false;
                    };

                    // Switch the player turn
                    playerTurn = playerNumber == 1 ? 2 : 1;

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

        private void EndRound(int winner)
        {
            isRoundOver = true;

            StartCoroutine(Utils.WaitConditionAndExecuteCR(() => !isDropping, () =>
            {
                Debug.Log("Drop stopped");

                lineRenderer.enabled = true;

                StartCoroutine(Utils.WaitDurationAndExecuteCR(connectionShowDuration, () =>
                {
                    ResetPlacedPieces();
                    isRoundOver = false;
                }));

                if (winner == 1)
                {
                    // Publish to the event bus that the round is over, along with the winning player number
                    eventBus.Publish<int>("RoundOver", 1);
                }
                else if (winner == 2)
                {
                    eventBus.Publish<int>("RoundOver", 2);
                }
            }));
        }

        private void ResetPlacedPieces()
        {
            // Hide the line renderer
            lineRenderer.positionCount = 0;

            // Reset the ownership of each tile on the board
            tileBoard = new int[columns, rows];

            // Destroy any existing tile pieces
            if (placedPieces != null)
            {
                placedPieces = new PlacementPiece[columns, rows];
            }

            // Destroy any existing tile piece objects
            foreach (Transform child in tilePieceParentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

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
                DrawLineRenderer(tileObjects[(int)blIndex.x, (int)blIndex.y].transform.position, tileObjects[(int)trIndex.x, (int)trIndex.y].transform.position);
                EndRound(player);
                return true;
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
                DrawLineRenderer(tileObjects[(int)tlIndex.x, (int)tlIndex.y].transform.position, tileObjects[(int)brIndex.x, (int)brIndex.y].transform.position);
                EndRound(player);
                return true;
            }

            return false;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Returns the position above the board where the tile piece will be dropped from
        /// </summary>
        /// <returns></returns>
        private Vector2 GetDropPointPosition(int col)
        {
            // Get the position based on the tile position on the board corresponding to the column and row
            Vector2 position = tileObjects[col, rows - 1].transform.position;

            // Apply a vertical offset to the position to place the tile piece above the board
            position += new Vector2(0, tilePieceYPos);

            return position;
        }


        #endregion

        public void ToggleAI()
        {
            isAIEnabled = !isAIEnabled;
        }

        private void DrawLineRenderer(Vector2 startPos, Vector2 endPos)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            lineRenderer.enabled = false;
        }
    }
}