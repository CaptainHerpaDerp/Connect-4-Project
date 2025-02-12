using Core;
using Sirenix.OdinInspector;
using UIElements;
using UnityEngine;

namespace UIManagement {

    /// <summary>
    /// Manages the player turn panels and their visibility, as well as their displayed text
    /// </summary>
    public class PlayerTurnUIManager : MonoBehaviour
    {
        [BoxGroup("Turn Panels"), SerializeField] private PlayerTurnPanel player1TurnPanel, player2TurnPanel, cpuTurnPanel;

        private ref PlayerTurnPanel activePlayer2TurnPanel => ref player2TurnPanel;

        // Singleton Methods
        private EventBus eventBus;

        private void Start()
        {
            // Singleton Assignment
            eventBus = EventBus.Instance;

            eventBus.Subscribe<int>("OnPlayerTurnChanged", SetPlayerTurnPanel);

            eventBus.Subscribe<bool>("OnGameStart", SetPlayerFrames);

            eventBus.Subscribe<int>("GameOver", SetPlayerWinner);

            // Reset the player frames when the game is restarted or the player returns to the main menu
            eventBus.Subscribe("OnGameRestart", ResetPlayerFrames);
            eventBus.Subscribe("OnReturnMenu", ResetPlayerFrames);
        }

        private void SetPlayerFrames(bool isAIEnabled)
        {
            player1TurnPanel.SetVisibility(true);

            // Depending on whether the AI is enabled or not, show the appropriate turn panel
            if (isAIEnabled)
            {
                player2TurnPanel.gameObject.SetActive(false);
                cpuTurnPanel.gameObject.SetActive(true);

                activePlayer2TurnPanel = cpuTurnPanel;
            }
            else
            {
                player2TurnPanel.gameObject.SetActive(true);
                cpuTurnPanel.gameObject.SetActive(false);

                activePlayer2TurnPanel = player2TurnPanel;
            }

            activePlayer2TurnPanel.SetVisibility(false);
        }

        private void ResetPlayerFrames()
        {
            player1TurnPanel.ResetText();
            activePlayer2TurnPanel.ResetText();

            player1TurnPanel.SetVisibility(true);
            activePlayer2TurnPanel.SetVisibility(false);
        }

        private void SetPlayerTurnPanel(int playerIndex)
        {
            if (playerIndex == 1)
            {
                player1TurnPanel.SetVisibility(true);
                activePlayer2TurnPanel.SetVisibility(false);
            }
            else if (playerIndex == 2)
            {
                player1TurnPanel.SetVisibility(false);
                activePlayer2TurnPanel.SetVisibility(true);
            }
            else
            {
                Debug.LogError("PlayerTurnUIManager: Invalid player index!");
            }
        }

        /// <summary>
        /// Set the panels so that they display the winner and the loser as their captions
        /// </summary>
        /// <param name="winner"></param>
        private void SetPlayerWinner(int winner)
        {
            PlayerTurnPanel winnerPanel = winner == 1 ? player1TurnPanel : activePlayer2TurnPanel;
            PlayerTurnPanel loserPanel = winner == 1 ? activePlayer2TurnPanel : player1TurnPanel;

            // If the AI wins, we're not going to tell them that they have won, we will display the player's panel as having lost, and hide the AI's panel
            if (winner == 2 && activePlayer2TurnPanel == cpuTurnPanel)
            {
                winnerPanel.Hide();
                loserPanel.SetLoser();
            }

            // Otherwise, display the winner's panel and hide the loser's panel
            else
            {
                winnerPanel.SetWinner();
                loserPanel.Hide();
            }
        }
    }
}