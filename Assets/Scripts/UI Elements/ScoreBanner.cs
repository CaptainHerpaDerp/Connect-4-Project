using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{
    /// <summary>
    /// A UI element that represents a score point for a player.
    /// </summary>
    public class ScoreBanner : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private Image image;

        public void SetColour(Color color)
        {
            image.color = color;
        }
    }
}