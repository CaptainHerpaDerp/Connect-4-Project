using UnityEngine;

namespace VisualElements
{
    /// <summary>
    /// Works with the line renderer to draw a line between two points and display a head and tail object at the ends of the line.
    /// </summary>
    public class LineRendererDrawer : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        [SerializeField] private GameObject headObject, tailObject;

        [SerializeField] private float lineRendererWidth;
        [SerializeField] private float endDistance;

        private void Start()
        {
            lineRenderer.positionCount = 2;

            // Set  the width of the line
            lineRenderer.startWidth = lineRendererWidth;
            lineRenderer.endWidth = lineRendererWidth;
        }

        /// <summary>
        /// Display the line renderer between two points
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public void Show(Vector3 startPos, Vector3 endPos)
        {
            // Enable all visual elements
            lineRenderer.enabled = true;
 
            // Set the positions of the line renderer
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        /// <summary>
        /// We need to set the position of the head and tail objects in an update method. For some reason, setting the position in the show method doesn't work.
        /// </summary>
        public void Update()
        {
            if (!lineRenderer.enabled) return;

            Vector2 direction = tailObject.transform.position - headObject.transform.position;
            Vector3 normalizeDirection = direction.normalized;

            // Set positions
            headObject.transform.position = lineRenderer.GetPosition(0) + (normalizeDirection * endDistance);
            tailObject.transform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1) - (normalizeDirection * endDistance);

            // Compute angle using Atan2 (for proper 2D rotation)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply rotation to both objects
            headObject.transform.rotation = Quaternion.Euler(0, 0, angle);
            tailObject.transform.rotation = Quaternion.Euler(0, 0, angle + 180); // Flip tail to face opposite direction

            headObject.SetActive(true);
            tailObject.SetActive(true);
        }

        public void Hide()
        {
            lineRenderer.enabled = false;
            headObject.SetActive(false);
            tailObject.SetActive(false);
        }
    }
}