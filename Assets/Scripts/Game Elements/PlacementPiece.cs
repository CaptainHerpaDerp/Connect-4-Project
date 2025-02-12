using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A piece that can be placed on the board during the placement phase, representing a player's colour.
/// </summary>
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
