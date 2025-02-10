using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// A text element that when enabled, indicates that the AI is thinking using a "..." text animation
/// </summary>
public class AITurnPromptText : MonoBehaviour
{
    [BoxGroup("Component References"), SerializeField] private TextMeshProUGUI textComponent;

    [BoxGroup("Settings"), SerializeField] private float animationSpeed = 0.5f;

    private Coroutine animationCoroutine;

    private void OnEnable()
    {
        textComponent.text = "";
        animationCoroutine = StartCoroutine(AnimateText());
    }

    private void OnDisable()
    {
        StopCoroutine(animationCoroutine);
    }

    private IEnumerator AnimateText()
    {
        textComponent.text = "";

        int dots = 0;

        while (true)
        {
            if (dots >= 3)
            {
                dots = 0;
                textComponent.text = "";
            }
            else
            {
                dots++;
            }

            SetTextDots(dots);

            yield return new WaitForSeconds(animationSpeed);

        }
    }

    private void SetTextDots(int count)
    {
        textComponent.text = "";

        for (int i = 0; i < count; i++)
        {
            textComponent.text += ".";
        }
    }
}
