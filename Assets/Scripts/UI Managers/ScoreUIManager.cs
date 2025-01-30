using Sirenix.OdinInspector;
using UnityEngine;
using UIElements;
using Core;

namespace UIManagement
{
    public class ScoreUIManager : Singleton<ScoreUIManager>
    {
        [BoxGroup("Component References"), SerializeField] private ScorePanel player1ScorePanel, player2ScorePanel;

        // Singleton References
        private EventBus eventBus;

        private void Start()
        {
            eventBus = EventBus.Instance;

            eventBus.Subscribe<int, int>("RoundOver", SetPlayerScore);
        }

        public void SetPlayerScore(int playerNumber, int score)
        {
            switch (playerNumber)
            {
                case 1:
                    player1ScorePanel.SetScore(playerNumber, score);
                    break;
                case 2:
                    player2ScorePanel.SetScore(playerNumber, score);
                    break;
            }
        }
    }
}