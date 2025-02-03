using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [BoxGroup("Movement Settings"), SerializeField] private float startY = 63f;
    [BoxGroup("Movement Settings"), SerializeField] private float endY = 0f;
    [BoxGroup("Movement Settings"), SerializeField] private float dropDuration = 1f;

    // Have a toggle button to set the easing function
    [BoxGroup("Movement Settings"), SerializeField] private bool useEaseInOutSine = true;



    [Button]
    private void MoveCamera()
    {
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        StartCoroutine(DropCameraToPosition());
    }

    private IEnumerator DropCameraToPosition()
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

        transform.position = new Vector3(transform.position.x, endY, transform.position.z);
    }

    private float EaseInOutSine(float t)
    {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    private float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}
