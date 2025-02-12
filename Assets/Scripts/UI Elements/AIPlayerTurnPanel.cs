using Sirenix.OdinInspector;
using UnityEngine;

namespace UIElements
{
    /// <summary>
    /// A turn panel that has control over the AI move prompt text (dots)
    /// </summary>
    public class AIPlayerTurnPanel : PlayerTurnPanel
    {
        [BoxGroup("Components"), SerializeField] private AITurnPromptText aiPromptText;

        public override void SetWinner()
        {
            aiPromptText.enabled = false;
        }

        public override void ResetText()
        {
            aiPromptText.enabled = true;
        }
    }
}
