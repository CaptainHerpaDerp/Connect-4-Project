using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using TMPro;

namespace UIElements
{
    public class PlayerTurnPanel : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private CanvasGroup canvasGroup;
        [BoxGroup("Component References"), SerializeField] private TextMeshProUGUI captionTextComponent;

        [BoxGroup("Visibility Settings"), SerializeField] private float fadeTime = 0.25f;
        [BoxGroup("Visibility Settings"), SerializeField] private float fadedAlpha = 0.5f;

        [BoxGroup("Text Settings"), SerializeField] private string winnerText = "You Win!";
        [BoxGroup("Text Settings"), SerializeField] private string turnText = "Your Turn!";


        #region Visibility Methods

        /// <summary>
        /// Increase the alpha of the panel and show the caption text.
        /// </summary>
        public void Show()
        {
            // Don't show if it's already visible
            if (canvasGroup.alpha == 1)
            {
                return;
            }

            StartCoroutine(Utils.FadeInCanvasGroup(canvasGroup, fadeTime, fromAlpha: fadedAlpha));
            captionTextComponent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Reduce the alpha of the panel and hide the caption text.
        /// </summary>
        public void Hide()
        {
            // Don't hide if it's already faded
            if (canvasGroup.alpha == fadedAlpha)
            {
                return;
            }

            StartCoroutine(Utils.FadeOutCanvasGroup(canvasGroup, fadeTime, targetAlpha: fadedAlpha));
            captionTextComponent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Toggle the visibility of the panel and the caption text.
        /// </summary>
        /// <param name="active"></param>
        public void SetVisibility(bool active)
        {
            if (active)
            {
                Show();
            }
            else
            {
                // No point in hiding if it's already disabled
                if (gameObject.activeInHierarchy)
                Hide();
            }
        }

        /// <summary>
        /// Sets the text to the winner text.
        /// </summary>
        public void SetWinner()
        {
            // Show the panel in case it's hidden
            SetVisibility(true);

            // Enable the text component and set the winner text
            captionTextComponent.gameObject.SetActive(true);
            captionTextComponent.text = winnerText;
        }

        /// <summary>
        /// Reverse the text back to the original turn text.
        /// </summary>
        public void ResetText()
        {
            captionTextComponent.text = turnText;
        }

        #endregion
    }
}
