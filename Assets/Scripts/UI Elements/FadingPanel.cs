using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace UIElements
{

    /// <summary>
    /// A panel that can fade in and out to a certain alpha value.
    /// </summary>
    public class FadingPanel : MonoBehaviour
    {
        [BoxGroup("Components"), SerializeField] private CanvasGroup canvasGroup;

        [BoxGroup("Settings"), SerializeField] private float fadeTime = 0.25f;
        [BoxGroup("Settings"), SerializeField] private float fadeAlpha = 0.72f;

        // Singletons
        private EventBus eventBus;

        private void Start()
        {
            eventBus = EventBus.Instance;
            DoEventSubscriptions();
        }

        private void DoEventSubscriptions()
        {

            // When the game is over, fade in the panel (discard the winning player variable)
            eventBus.Subscribe("GameOver", (int _) =>
            {
                FadeIn();
            });

            // When we restart the game or go back to the menu, fade out the panel
            eventBus.Subscribe("OnGameRestart", () =>
            {
                FadeOut();
            });

            eventBus.Subscribe("OnReturnMenu", () =>
            {
                FadeOut();
            });

            eventBus.Subscribe("OnGamePause", () =>
            {
                FadeIn();
            });

            eventBus.Subscribe("OnGameResume", () =>
            {
                FadeOut();
            });
        }

        private void FadeIn()
        {
            StartCoroutine(Utils.FadeInCanvasGroup(canvasGroup, fadeTime, 0, fadeAlpha));
        }

        private void FadeOut()
        {
            StartCoroutine(Utils.FadeOutCanvasGroup(canvasGroup, fadeTime, fromAlpha: fadeAlpha));
        }
    }
}