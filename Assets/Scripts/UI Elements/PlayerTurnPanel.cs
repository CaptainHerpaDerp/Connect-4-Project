using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using TMPro;

namespace UIElements
{
    public class PlayerTurnPanel : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private CanvasGroup canvasGroup;

        [BoxGroup("Component References"), SerializeField] private TextMeshProUGUI turnPromptTextComponent;

        [BoxGroup("Visibility Settings"), SerializeField] private float fadeTime = 0.25f;

        [BoxGroup("Visibility Settings"), SerializeField] private float fadedAlpha = 0.5f;


        #region Visibility Methods

        public void Show()
        {
            StartCoroutine(Utils.FadeInCanvasGroup(canvasGroup, fadeTime, fromAlpha: fadedAlpha));
            turnPromptTextComponent.enabled = true;
        }

        public void Hide()
        {
            StartCoroutine(Utils.FadeOutCanvasGroup(canvasGroup, fadeTime, targetAlpha: fadedAlpha));
            turnPromptTextComponent.enabled = false;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        #endregion
    }
}
