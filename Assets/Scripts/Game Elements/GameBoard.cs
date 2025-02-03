using UnityEngine;

namespace GameElements
{
    /// <summary>
    /// The board on which the game is played, used to determine which row a player has selected
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class GameBoard : MonoBehaviour
    {
        private int columns;

        public void Initialize(int columns)
        {
            this.columns = columns;
        }

        public int GetSelectedColumnNumber()
        {
            if (columns == 0)
            {
                Debug.LogError("Game board has not been initialized!");
                return -1;
            }

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (!Physics2D.Raycast(mousePosition, Vector2.zero))
            {
                return -1;
            }

            // Get the local mouse position by simply subtracting the position of the game board from the mouse position
            Vector2 localMousePos = mousePosition - (Vector2)transform.position;

            // Add half the scale of the game board to the local mouse position, so that our check starts at the left edge of the game board
            float localX = localMousePos.x + transform.lossyScale.x / 2;

            // Calculate the column number by dividing the local x position by the scale of the game board and multiplying by the number of columns
            int columnNumber = Mathf.FloorToInt(localX / transform.lossyScale.x * columns);

            return columnNumber;
        }
    }
}
