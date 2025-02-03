using Sirenix.OdinInspector;
using UnityEngine;
using UIElements;
using Core;

namespace UIManagement
{
    public class ScoreUIManager : Singleton<ScoreUIManager>
    {
        [BoxGroup("Component References"), SerializeField] private ScoreBannerGroup player1ScoreBannerGroup, player2ScoreBannerGroup;

        // Singleton References
        private EventBus eventBus;
        private GamePrefs gamePrefs;

        private void Start()
        {
            eventBus = EventBus.Instance;
            gamePrefs = GamePrefs.Instance;

            eventBus.Subscribe<int>("RoundOver", SetPlayerScore);

            // Assign the player colours

            player1ScoreBannerGroup.Instantiate(gamePrefs.GetPlayerColour(1), gamePrefs.ScoreToWin);
            player2ScoreBannerGroup.Instantiate(gamePrefs.GetPlayerColour(2), gamePrefs.ScoreToWin);
        }

        public void SetPlayerScore(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1:
                    player1ScoreBannerGroup.AddPlayerScore();
                    break;
                case 2:
                    player2ScoreBannerGroup.AddPlayerScore();
                    break;
            }
        }
    }
}