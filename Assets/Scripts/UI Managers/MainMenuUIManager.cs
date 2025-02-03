using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UIManagement
{
    /// <summary>
    /// Manages button interaction and gameplay initiation in the main menu.
    /// </summary>
    public class MainMenuUIManager : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private CanvasGroup mainMenuCanvasGroup;

        [BoxGroup("Component References"), SerializeField] private CameraController cameraController;

        [BoxGroup("Buttons"), SerializeField] private Button play1v1Button, playVsCPUButton, quitButton;

        private void Start()
        {
            if (mainMenuCanvasGroup == null)
            {
                Debug.LogError("MainMenuUIManager: mainMenuCanvasGroup is not assigned!");
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
        }

        private void DoButtonSubscriptions()
        {
            play1v1Button.onClick.AddListener(StartGame1v1);
            playVsCPUButton.onClick.AddListener(StartGameVsCPU);
            quitButton.onClick.AddListener(QuitGame);
        }

        #region Button Action Methods

        private void StartGame1v1()
        {
            HideMainMenu();

            cameraController.MoveCameraDown();
        }

        private void StartGameVsCPU()
        {
            HideMainMenu();

        }

        private void QuitGame()
        {
            HideMainMenu();

        }

        #endregion

        #region Visibility

        private void HideMainMenu()
        {
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;

            StartCoroutine(Utils.FadeOutCanvasGroup(mainMenuCanvasGroup, 0.5f));          
        }

        #endregion
    }
}