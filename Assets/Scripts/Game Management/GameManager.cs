using Sirenix.OdinInspector;
using UnityEngine;
using GameElements;
using Utilities;
using Core;
using VisualElements;
using AI;

namespace Management
{
    public enum GameState { MainMenu, PlayerTurn, RoundOver, GameOver, GamePause }

    /// <summary>
    /// Manages the game flow, player turns, and game state
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region Serialized Fields

        [BoxGroup("Components"), SerializeField] GameBoard gameBoard;
        [BoxGroup("Components"), SerializeField] LineRendererDrawer lineRenderer;

        [BoxGroup("Prefabs"), SerializeField] GameObject tileSpacePrefab;
        [BoxGroup("Prefabs"), SerializeField] PlacementPiece placementPiecePrefab;
        [BoxGroup("Prefabs"), SerializeField] DroppingTilePiece droppingTilePiece;

        [BoxGroup("Tile Placement"), SerializeField] private Transform tileParentTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePlacementAreaTransform;
        [BoxGroup("Tile Placement"), SerializeField] private Transform tilePieceParentTransform;

        [Header("The height of the preview tile piece above the game board")]
        [BoxGroup("Visual Settings"), SerializeField] private float tilePieceYPos;

        [Header("The delay before showing a connection with a line renderer")]
        [BoxGroup("Visual Settings"), SerializeField] private float connectionShowDelay = 0.5f;

        [Header("The duration of time a match will be shown before the round is reset")]
        [BoxGroup("Visual Settings"), SerializeField] private float connectionShowDuration = 0.5f;

        [Header("The modification of tile piece scale by the tile scale")]
        [BoxGroup("Visual Settings"), SerializeField] private float pieceScaleMod = 1;

        [Header("The minimum wait duration before another tile can be placed")]
        [BoxGroup("Placement Settings"), SerializeField] private float minPlaceWaitTime = 0.5f;

        [BoxGroup("Key Codes"), SerializeField] private KeyCode menuKey = KeyCode.Escape;

        [BoxGroup("AI Settings"), SerializeField] private bool isAIEnabled = false;

        #endregion

        private Vector2 boardCornerPosition;
        private Vector2 tileScale;
        private Vector2 tilePieceScale;

        // Represents the ownership of each tile on the board
        private int[,] tileBoard;

        // Represents the game objects for each tile slot on the board
        private GameObject[,] tileObjects;

        // Represents the pieces that have been placed on the board
        private PlacementPiece[,] placedPieces;

        private static readonly Vector2 OffscreenPosition = new Vector2(1000, 1000);

        // Either 1 or 2
        private int playerTurn;

        // Score of each player
        private int player1Score = 0, player2Score = 0;

        // The tile piece that will be placed on the board
        private PlacementPiece previewPlacementPiece;

        private bool isDropping;

        // Tile Waiting
        private bool isOnPlaceCooldown;

        // AI Checks
        private bool aiPlaced;
        private bool isAIProcessing;

        // Singletons
        private EventBus eventBus;
        private AIPlayer aiPlayer;

        // Game State
        private GameState gameState = GameState.MainMenu;

        // Game State Getters
        private bool isRoundOver => gameState == GameState.RoundOver;
        private bool isGameActive => (gameState != GameState.MainMenu && gameState != GameState.GameOver && gameState != GameState.GamePause);

        // Game Prefs Getters
        private int rows => GamePrefs.Rows;
        private int columns => GamePrefs.Columns;
        private int tilesToWin => GamePrefs.TilesToWin;

        #region Initialization

        private void Start()
        {
            // Singleton initialization
            eventBus = EventBus.Instance;
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

            // Create the preview tile piece
            previewPlacementPiece = Instantiate(placementPiecePrefab, new Vector2(1000, 1000), Quaternion.identity);

            // Blue player goes first as the start of each game
            SetPlayerTurn(1);

            gameBoard.Initialize(columns);

            InitializeTileBoardEven();

            // Default the player turn to 1
            eventBus.Publish<int>("OnPlayerTurnChanged", playerTurn);

            eventBus.Subscribe<bool>("OnGameStart", (aiEnabled) =>
            {
                SetGameState(GameState.PlayerTurn);

                if (aiEnabled)
                {
                    isAIEnabled = true;
                }
            });

            eventBus.Subscribe("OnGameRestart", RestartGame);

            eventBus.Subscribe("OnReturnMenu", () =>
            {
                // Clear the game board
                RestartGame();

                // Reset the game state to the main menu
                SetGameState(GameState.MainMenu);

                // Hide the preview tile piece
                previewPlacementPiece.transform.position = OffscreenPosition;
            });
        }

        /// <summary>
        /// Generate the game board
        /// </summary>
        private void InitializeTileBoardEven()
        {
            // Destroy any existing tile objects
            foreach (Transform child in tileParentTransform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in tilePieceParentTransform)
            {
                Destroy(child.gameObject);
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

            tilePieceScale = tileScale * pieceScaleMod;

            // Set the scale of the preview tile piece and the dropping tile piece
            previewPlacementPiece.transform.localScale = tilePieceScale;
            droppingTilePiece.transform.localScale = tilePieceScale;

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
            if (isGameActive)
            {
                DoPlayerTurns();
                DoTilePieceVisualization();
            }

            CheckKeys();
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
                if (!isAIProcessing && !aiPlaced)
                {
                    // Safety check to prevent multiple asyncs 
                    isAIProcessing = true;
                    aiPlayer.GetMoveAsync(tileBoard, (bestMove) =>
                    {
                        StartCoroutine(Utils.WaitConditionAndExecuteCR(() => !isOnPlaceCooldown, () =>
                        {
                            PlaceTileOnColumn(bestMove, playerTurn);
                            isAIProcessing = false;
                        }));
                    });

                    aiPlaced = true;
                }

                // Prevent the player from doing anything while the AI is processing
                return; 
            }

            if (Input.GetMouseButtonDown(0) && !isRoundOver && !isOnPlaceCooldown)
            {
                PlaceTileOnColumn(gameBoard.GetSelectedColumnNumber(), playerTurn);
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
                    newTilePiece.SetColour(GamePrefs.GetPlayerColour(playerNumber));
                    newTilePiece.transform.localScale = tilePieceScale;

                    newTilePiece.Hide();

                    DroppingTilePiece newDroppingTilePiece = Instantiate(droppingTilePiece, new Vector2(1000, 1000), Quaternion.identity);
                    Vector2 dropPoint = GetDropPointPosition(colNumber);

                    // Drop the tile piece from the top of the board to the placed position
                    newDroppingTilePiece.DropToPosition(dropPoint, newTilePiece.transform.position, GamePrefs.GetPlayerColour(playerNumber));

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
                    SwitchPlayerTurn(playerNumber);
                    break;
                }
            }
        }

        private void SwitchPlayerTurn(int currentTurn)
        {
            SetPlayerTurn(currentTurn == 1 ? 2 : 1);

            eventBus.Publish<int>("OnPlayerTurnChanged", playerTurn);

            // Reset the AI placed flag if the AI is enabled
            if (isAIEnabled && playerTurn == 2)
            {
                aiPlaced = false;
            }
        }

        /// <summary>
        /// Checks for a victory in the game board by checking for 4 tiles in a row horizontally, vertically, or diagonally
        /// </summary>
        private void CheckVictory(int placedCol, int placedRow, int player)
        {
            (bool isMatchHor, int leftColIndex, int rightColIndex) = TilePatternCheck.CheckHorizontal(tileBoard, placedCol, placedRow, player);
            if (isMatchHor)
            {
                DrawLineRenderer(tileObjects[leftColIndex, placedRow].transform.position, tileObjects[rightColIndex, placedRow].transform.position, player);
                EndRound(player);
                return;
            }

            (bool isMatchVert, int topRowIndex, int bottomRowIndex) = TilePatternCheck.CheckVertical(tileBoard, placedCol, placedRow, player);
            if (isMatchVert)
            {
                DrawLineRenderer(tileObjects[placedCol, bottomRowIndex].transform.position, tileObjects[placedCol, topRowIndex].transform.position, player);
                EndRound(player);
                return;
            }

            (bool isMatchDiag, Vector2 indice1, Vector2 indice2) = TilePatternCheck.CheckDiagonal(tileBoard, placedCol, placedRow, player);
            if (isMatchDiag)
            {
                DrawLineRenderer(tileObjects[(int)indice1.x, (int)indice1.y].transform.position, tileObjects[(int)indice2.x, (int)indice2.y].transform.position, player);
                EndRound(player);
            }
        }

        private void EndRound(int winner)
        {
            SetGameState(GameState.RoundOver);

            StartCoroutine(Utils.WaitConditionAndExecuteCR(() => !isDropping, () =>
            {
                lineRenderer.enabled = true;

                StartCoroutine(Utils.WaitDurationAndExecuteCR(connectionShowDuration, () =>
                {
                    // Reset the round if the game is still active
                    if (isGameActive)
                        ResetRound();
                }));

                if (winner == 1)
                {
                    // Publish to the event bus that the round is over, along with the winning player number
                    eventBus.Publish<int>("RoundOver", 1);

                    player1Score++;
                }
                else if (winner == 2)
                {
                    eventBus.Publish<int>("RoundOver", 2);

                    player2Score++;
                }

                // Check if the game is won by either player
                CheckGameEnd();

            }));
        }

        private void ResetRound()
        {
            ResetPlacedPieces();

            SetGameState(GameState.PlayerTurn);
        }

        private void RestartGame()
        {
            ResetPlacedPieces();
            player1Score = 0;
            player2Score = 0;
            SetGameState(GameState.PlayerTurn);

            SetPlayerTurn(1);
        }

        private void ResetPlacedPieces()
        {
            // Hide the line renderer
            lineRenderer.Hide();

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

        /// <summary>
        /// Check if the game has ended by checking if either player has reached the score limit
        /// </summary>
        private void CheckGameEnd()
        {
            if (player1Score >= 3 || player2Score >= 3)
            {
                // Set the game as being over
                SetGameState(GameState.GameOver);

            }
            else if (IsGameboardFull())
            {
                ResetRound();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Return true if the game board no longer has any empty spaces (tie)
        /// </summary>
        /// <returns></returns>
        private bool IsGameboardFull()
        {
            foreach (var tile in tileBoard)
            {
                if (tile == 0) return false;
            }
            return true;
        }

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

        #region Keycode Methods

        /// <summary>
        /// Check if a key is pressed
        /// </summary>
        private void CheckKeys()
        {
            if (Input.GetKeyDown(menuKey))
            {
                // Don't pause/unpause if the game is over (menu is already shown)
                if (gameState == GameState.GameOver)
                {
                    return;
                }

                if (gameState == GameState.GamePause)
                {
                    eventBus.Publish("OnGameResume");
                    SetGameState(GameState.PlayerTurn);
                }
                else
                {
                    SetGameState(GameState.GamePause);
                }
            }

            // Debug
            //if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.W))
            //{
            //    if (gameState == GameState.GameOver)
            //    {
            //        return;
            //    }

            //    player2Score = 3;
            //    CheckGameEnd();
            //}
            //else if (Input.GetKeyDown(KeyCode.W))
            //{
            //    if (gameState == GameState.GameOver)
            //    {
            //        return;
            //    }

            //    player1Score = 3;
            //    CheckGameEnd();
            //}
        }

        #endregion

        #region Game State Management

        private void SetGameState(GameState newState)
        {
            // We don't want to unnecessarily call an event
            if (newState == gameState)
            {
                return;
            }

            gameState = newState;

            switch (newState)
            {
                case GameState.GameOver:
                    eventBus.Publish<int>("GameOver", player1Score > player2Score ? 1 : 2);
                    previewPlacementPiece.transform.position = OffscreenPosition;
                    break;
                case GameState.GamePause:
                    eventBus.Publish("OnGamePause");
                    break;
            }
        }


        #endregion

        private void SetPlayerTurn(int turn)
        {
            playerTurn = turn;

            eventBus.Publish<int>("OnPlayerTurnChanged", playerTurn);

            // Set the colour of the preview tile piece to the colour of the current player
            previewPlacementPiece.SetColour(GamePrefs.GetPlayerColour(playerTurn));
        }

        /// <summary>
        /// Show the tile piece that will be placed on the board
        /// </summary>
        private void DoTilePieceVisualization()
        {
            if (isAIEnabled && playerTurn == 2)
            {
                // Do not visualize the tile piece if the AI is enabled and it is the AI's turn
                previewPlacementPiece.transform.position = OffscreenPosition;
                return;
            }

            int colNumber = gameBoard.GetSelectedColumnNumber();

            // Clamp the column number to be within the bounds of the board
            if (colNumber == -1)
            {
                previewPlacementPiece.transform.position = OffscreenPosition;
                return;
            }

            previewPlacementPiece.transform.position = GetDropPointPosition(colNumber);
        }

        public void ToggleAI()
        {
            isAIEnabled = !isAIEnabled;
        }

        private void DrawLineRenderer(Vector2 startPos, Vector2 endPos, int player)
        {
            StartCoroutine(Utils.WaitDurationAndExecuteCR(connectionShowDelay, () =>
            {
                lineRenderer.Show(startPos, endPos);
            }));
        }
    }
}