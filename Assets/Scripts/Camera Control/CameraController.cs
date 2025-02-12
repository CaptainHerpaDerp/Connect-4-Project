using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera animations and movements between the menu and game scenes.
/// </summary>
public class CameraController : MonoBehaviour
{
    [BoxGroup("Movement Settings"), SerializeField] private float startY = 63f;
    [BoxGroup("Movement Settings"), SerializeField] private float endY = 0f;
    [BoxGroup("Movement Settings"), SerializeField] private float dropDuration = 1f;

    // Have a toggle button to set the easing function
    [BoxGroup("Movement Settings"), SerializeField] private bool useEaseInOutSine = true;

    #region Camera Positioning

    /// <summary>
    /// Moves the camera down to the end position.
    /// </summary>
    [Button]
    public void MoveCameraDown(Action onPositionReached = null)
    {
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        StartCoroutine(DropCameraToEndPosition(onPositionReached));
    }

    /// <summary>
    /// Moves the camera up to the start position.
    /// </summary>
    [Button]
    public void MoveCameraUp(Action onPositionReached = null)
    {
        transform.position = new Vector3(transform.position.x, endY, transform.position.z);

        StartCoroutine(RaiseCameraToStartPosition(onPositionReached));
    }

    private IEnumerator DropCameraToEndPosition(Action onPositionReached = null)
    {
        float elapsedTime = 0f;
        float startY = transform.position.y;

        while (elapsedTime < dropDuration)
        {
            float t = elapsedTime / dropDuration; // Normalize time 0  1

            float easedT;

            if (useEaseInOutSine)
            {
                easedT = EaseInOutSine(t);
            }
            else
            {
                easedT = EaseOutCubic(t);
            }

            float newY = Mathf.Lerp(startY, endY, easedT);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set the final position
        transform.position = new Vector3(transform.position.x, endY, transform.position.z);

        // Invoke the callback (if not null)
        onPositionReached?.Invoke();
    }

    private IEnumerator RaiseCameraToStartPosition(Action onPositionReached = null)
    {
        float elapsedTime = 0f;
        float endY = transform.position.y;

        while (elapsedTime < dropDuration)
        {
            float t = elapsedTime / dropDuration; // Normalize time 0  1

            float easedT = EaseInOutCubic(t);

            float newY = Mathf.Lerp(endY, startY, easedT);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set the final position
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        // Invoke the callback (if not null)
        onPositionReached?.Invoke();
    }

    /// <summary>
    /// Sets the camera to the start position using the given start Y value
    /// </summary>
    public void SetCameraStartPosition()
    {
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
    }

    #endregion

    private float EaseInOutSine(float t)
    {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    private float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }
}
