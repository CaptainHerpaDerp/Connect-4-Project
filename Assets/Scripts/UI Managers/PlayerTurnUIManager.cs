using Core;
using Sirenix.OdinInspector;
using UIElements;
using UnityEngine;

namespace UIManagement {

    public class PlayerTurnUIManager : MonoBehaviour
    {
        [BoxGroup("Turn Panels"), SerializeField] private PlayerTurnPanel player1TurnPanel, player2TurnPanel;

        // Singleton Methods
        private EventBus eventBus;

        private void Start()
        {
            // Singleton Assignment
            eventBus = EventBus.Instance;

            eventBus.Subscribe<int>("OnPlayerTurnChanged", SetPlayerTurnPanel);
        }

        private void SetPlayerTurnPanel(int playerIndex)
        {
            if (playerIndex == 1)
            {
                player1TurnPanel.SetActive(true);
                player2TurnPanel.SetActive(false);
            }
            else if (playerIndex == 2)
            {
                player1TurnPanel.SetActive(false);
                player2TurnPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("PlayerTurnUIManager: Invalid player index!");
            }
        }
    }
}