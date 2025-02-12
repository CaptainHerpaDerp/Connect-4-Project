using Sirenix.OdinInspector;
using UnityEngine;
using UIElements;
using Core;

namespace UIManagement
{
    /// <summary>
    /// Manages the score banners for each player.  
    /// </summary>
    public class ScoreUIManager : Singleton<ScoreUIManager>
    {
        [BoxGroup("Component References"), SerializeField] private ScoreBannerGroup player1ScoreBannerGroup, player2ScoreBannerGroup;

        // Singleton References
        private EventBus eventBus;

        private void Start()
        {
            eventBus = EventBus.Instance;

            eventBus.Subscribe<int>("RoundOver", SetPlayerScore);

            // Reset the player scores when the game is restarted or the player returns to the main menu    
            eventBus.Subscribe("OnGameRestart", ResetScores);
            eventBus.Subscribe("OnReturnMenu", ResetScores);

            // Assign the player colours

            player1ScoreBannerGroup.Instantiate(GamePrefs.GetPlayerColour(1), GamePrefs.ScoreToWin);
            player2ScoreBannerGroup.Instantiate(GamePrefs.GetPlayerColour(2), GamePrefs.ScoreToWin);
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

        public void ResetScores()
        {
            Debug.Log("Resetting Scores");

            player1ScoreBannerGroup.ResetPlayerScore();
            player2ScoreBannerGroup.ResetPlayerScore();
        }
    }
}