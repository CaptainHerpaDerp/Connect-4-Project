using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Core;
using Utilities;

namespace UIManagement
{
    /// <summary>
    /// Handles the UI elements and button interactions in the game menu.
    /// </summary>
    public class GameMenuUIManager : Singleton<GameMenuUIManager>   
    {
        [BoxGroup("Buttons"), SerializeField] private Button menuButton, restartButton;

        [BoxGroup("Components"), SerializeField] private CanvasGroup gameMenuCanvasGroup;

        // Singleton Methods
        private EventBus eventBus;
        private MainMenuUIManager mainMenuUIManager;

        private void Start()
        {
            // Singletons
            eventBus = EventBus.Instance;
            mainMenuUIManager = MainMenuUIManager.Instance;

            eventBus.Subscribe<int>("GameOver", ShowMenu);
            eventBus.Subscribe("OnGamePause", ShowMenu);
            eventBus.Subscribe("OnGameResume", HideMenu);
        }

        #region Button Subscriptions

        private void OnEnable()
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        private void OnDisable()
        {
            menuButton.onClick.RemoveListener(OnMenuButtonClicked);
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        #endregion

        #region Button Methods

        private void OnMenuButtonClicked()
        {
            mainMenuUIManager.ReturnToMainMenu();
            eventBus.Publish("OnReturnMenu");

            HideMenu();
        }

        private void OnRestartButtonClicked()
        {
            eventBus.Publish("OnGameRestart");

            HideMenu();
        }

        #endregion

        #region Visibility Methods

        private void ShowMenu()
        {
            StartCoroutine(Utils.FadeInCanvasGroup(gameMenuCanvasGroup, 0.5f));
        }

        private void ShowMenu(int _)
        {
            StartCoroutine(Utils.FadeInCanvasGroup(gameMenuCanvasGroup, 0.5f));
        }

        private void HideMenu()
        {
            StartCoroutine(Utils.FadeOutCanvasGroup(gameMenuCanvasGroup, 0.25f));
        }

        #endregion
    }
}