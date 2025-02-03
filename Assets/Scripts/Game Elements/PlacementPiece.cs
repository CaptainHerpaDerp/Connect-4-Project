using Sirenix.OdinInspector;
using UnityEngine;

public class PlacementPiece : MonoBehaviour
{
    [BoxGroup("Component References"), SerializeField] private SpriteRenderer spriteRenderer;

    public void SetColour(Color color)
    {
        spriteRenderer.color = color;
    }

    #region Visibility

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    public void ToggleVisibility(bool isVisible)
    {
        spriteRenderer.enabled = isVisible;
    }

    #endregion
}
