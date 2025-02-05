using Core;
using Sirenix.OdinInspector;
using UIElements;
using UnityEngine;

namespace UIManagement {

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
        }

        private void SetPlayerFrames(bool isAIEnabled)
        {
            player1TurnPanel.SetActive(true);

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

            activePlayer2TurnPanel.SetActive(false);
        }

        private void SetPlayerTurnPanel(int playerIndex)
        {
            if (playerIndex == 1)
            {
                player1TurnPanel.SetActive(true);
                activePlayer2TurnPanel.SetActive(false);
            }
            else if (playerIndex == 2)
            {
                player1TurnPanel.SetActive(false);
                activePlayer2TurnPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("PlayerTurnUIManager: Invalid player index!");
            }
        }
    }
}