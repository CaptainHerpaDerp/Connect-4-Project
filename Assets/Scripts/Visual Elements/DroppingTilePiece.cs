using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualElements
{
    /// <summary>
    /// A visualization of the tile piece that drops from the top of the board to a given position.
    /// </summary>
    public class DroppingTilePiece : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private SpriteRenderer spriteRenderer;

        [BoxGroup("Drop Settings"), SerializeField] float gravity = 9.8f;  // Acceleration
        [BoxGroup("Drop Settings"), SerializeField] float snapStrength = 5f; // Controls snap-back effect
        [BoxGroup("Drop Settings"), SerializeField] float snapDamping = 0.6f; // Controls how fast it settles

        private Vector3 targetPosition;
        private bool isDropping = false;
        private float velocity = 0f;

        public Action OnPositionReached;

        public void DropToPosition(Vector2 fromPos, Vector2 toPos, Color dropPlayerColor)
        {
            // Set the color of the sprite renderer to the player color
            spriteRenderer.color = dropPlayerColor;

            // Set the position of the tile piece to the from position
            transform.position = fromPos;

            targetPosition = toPos;

            isDropping = true;

            velocity = 0;

            StartCoroutine(DropPiece());
        }

        private IEnumerator DropPiece2()
        {
            while (isDropping)
            {
                float deltaTime = Time.deltaTime;
                velocity += gravity * deltaTime;
                transform.position += deltaTime * velocity * Vector3.down;

                // Check if the piece has reached or passed the target position
                if (transform.position.y <= targetPosition.y)
                {
                    isDropping = false;
                    StartCoroutine(SnapBackEffect());
                }

                yield return null;
            }
        }

        [SerializeField] private float snapSpeed = 1f;
        [SerializeField] private float overShoot = 0.2f;

        private IEnumerator SnapBackEffect()
        {
            float elapsedTime = 0f;
            Vector3 overshootPos = targetPosition + Vector3.down * overShoot;

            while (elapsedTime < snapSpeed)
            {
                elapsedTime += Time.deltaTime * snapStrength;
                transform.position = Vector3.Lerp(overshootPos, targetPosition, Mathf.SmoothStep(0f, snapSpeed, elapsedTime));

                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    transform.position = targetPosition;

                    OnPositionReached?.Invoke();

                    Destroy(gameObject);

                    break;
                }

                yield return null;
            }
        }

        [SerializeField] public float dropDuration = 1.5f; // Time taken to drop and bounce
        private IEnumerator DropPiece()
        {
            float elapsedTime = 0f;
            float startY = transform.position.y; // Save initial Y position

            while (elapsedTime < dropDuration)
            {
                float t = elapsedTime / dropDuration;  // Normalize time 0  1
                float bounceT = EaseOutBounce(t); // Get eased bounce value

                // Interpolate only the Y position using the bounce effect
                float newY = Mathf.Lerp(startY, targetPosition.y, bounceT);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final position is exactly the target position
            transform.position = targetPosition;
            isDropping = false;
            OnPositionReached?.Invoke();
            Destroy(gameObject);
        }


        // From https://easings.net/#easeOutBounce
        private float EaseOutBounce(float x)
        {
            float d1 = 2.75f;
            float n1 = 7.5625f;

            if (x < 1 / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2 / d1)
            {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }
            else if (x < 2.5f / d1)
            {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }
            else
            {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }
    }

}