using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UIElements
{
    public class ScorePanel : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private TextMeshProUGUI playerScoreTextComponent;

        public string PlayerText
        {
            get
            {
                return playerScoreTextComponent.text;
            }
            set
            {
                playerScoreTextComponent.text = value;
            }
        }

        public void SetScore(int playerNumber, int score)
        {
            PlayerText = $"P{playerNumber}: {score}";
        }
    }
}