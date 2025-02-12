using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UIManagement
{
    /// <summary>
    /// Manages button interaction and gameplay initiation in the main menu.
    /// </summary>
    public class MainMenuUIManager : Singleton<MainMenuUIManager>   
    {
        [BoxGroup("Component References"), SerializeField] private CameraController cameraController;

        [BoxGroup("Menu Canvas Groups"), SerializeField] private CanvasGroup mainMenuCanvasGroup;
        [BoxGroup("Menu Canvas Groups"), SerializeField] private CanvasGroup gameMenuCanvasGroup;

        [BoxGroup("Buttons"), SerializeField] private Button play1v1Button, playVsCPUButton, quitButton;

        [BoxGroup("Menu Settings"), SerializeField] private bool showMainMenuOnStart = true;

        // Singleton Methods
        private EventBus eventBus;

        private void Start()
        {
            eventBus = EventBus.Instance;

            if (mainMenuCanvasGroup == null)
            {
                Debug.LogError("MainMenuUIManager: mainMenuCanvasGroup is not assigned!");
            }

            if (gameMenuCanvasGroup == null)
            {
                Debug.LogError("MainMenuUIManager: gameMenuCanvasGroup is not assigned!");
            }

            if (cameraController == null)
            {
                Debug.LogError("MainMenuUIManager: cameraController is not assigned!");
            }

            if (play1v1Button == null || playVsCPUButton == null || quitButton == null)
            {
                Debug.LogError("MainMenuUIManager: A button is not assigned!");
            }
            
            DoButtonSubscriptions();

            if (showMainMenuOnStart)
            {
                mainMenuCanvasGroup.alpha = 1;
                mainMenuCanvasGroup.interactable = true;
                mainMenuCanvasGroup.blocksRaycasts = true;

                gameMenuCanvasGroup.alpha = 0;
                gameMenuCanvasGroup.interactable = false;
                gameMenuCanvasGroup.blocksRaycasts = false;

                cameraController.SetCameraStartPosition();
            }
            else
            {
                mainMenuCanvasGroup.alpha = 0;
                mainMenuCanvasGroup.interactable = false;
                mainMenuCanvasGroup.blocksRaycasts = false;

                ShowGameMenu();

                // Publish the game start event, with the CPU flag set to false
                eventBus.Publish<bool>("OnGameStart", false);
            }
        }

        private void DoButtonSubscriptions()
        {
            play1v1Button.onClick.AddListener(StartGame1v1);
            playVsCPUButton.onClick.AddListener(StartGameVsCPU);
            quitButton.onClick.AddListener(QuitGame);
        }


        public void ReturnToMainMenu()
        {
            // Fade the game group out
            StartCoroutine(Utils.FadeOutCanvasGroup(gameMenuCanvasGroup, 0.25f));

            cameraController.MoveCameraUp(onPositionReached: () =>
            {
                ShowMainMenu();
            });
        }

        #region Button Action Methods

        private void StartGame1v1()
        {
            HideMainMenu();

            // Move the camera down, when the position is reached, start the game
            cameraController.MoveCameraDown(onPositionReached: () =>
            {
                ShowGameMenu();

                // Publish the game start event, with the CPU flag set to false
                eventBus.Publish<bool>("OnGameStart", false);
            });
        }

        private void StartGameVsCPU()
        {
            HideMainMenu();

            // Move the camera down, when the position is reached, start the game
            cameraController.MoveCameraDown(onPositionReached: () =>
            {
                ShowGameMenu();

                // Publish the game start event, with the CPU flag set to true
                eventBus.Publish<bool>("OnGameStart", true);
            });
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        #endregion

        #region Visibility

        private void HideMainMenu()
        {
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;

            StartCoroutine(Utils.FadeOutCanvasGroup(mainMenuCanvasGroup, 0.5f));          
        }

        private void ShowMainMenu()
        {
            StartCoroutine(Utils.FadeInCanvasGroup(mainMenuCanvasGroup, 0.25f));
        }

        private void ShowGameMenu()
        {
            StartCoroutine(Utils.FadeInCanvasGroup(gameMenuCanvasGroup, 0.25f));
        }

        #endregion
    }
}