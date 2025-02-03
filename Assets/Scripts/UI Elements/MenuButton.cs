using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UIElements
{
    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [BoxGroup("Component References"), SerializeField] private TextMeshProUGUI buttonText;

        [BoxGroup("Button Text Colours"), SerializeField] private Color normalColour, selectedColour;

        private void Start()
        {
            if (buttonText == null)
            {
                Debug.LogError("MenuButton: buttonText is not assigned!");
            }
        }

        public void OnPointerEnter(PointerEventData _)
        {
            buttonText.color = selectedColour;
        }

        public void OnPointerExit(PointerEventData _)
        {
            buttonText.color = normalColour;
        }
    }
}