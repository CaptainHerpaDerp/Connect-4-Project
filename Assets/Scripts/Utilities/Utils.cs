using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Waits a given period of time and executes an action
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator WaitDurationAndExecuteCR(float duration, Action action)
        {
            yield return new WaitForSeconds(duration);
            action.Invoke();
        }

        public static IEnumerator WaitConditionAndExecuteCR(Func<bool> condition, Action action)
        {
            yield return new WaitUntil(condition);
            action.Invoke();
        }

        public static float EaseInOut(float t)
        {
            return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        }

        #region Particle Effects

        /// <summary>
        /// Plays a particle effect along will all of its child particle effects
        /// </summary>
        /// <param name="particleSystem"></param>
        public static void PlayParticleEffect(ParticleSystem particleSystem)
        {
            particleSystem.Play();

            foreach (Transform child in particleSystem.transform)
            {
                // Ignore the child if it's not active
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                if (child.TryGetComponent(out ParticleSystem childParticleSystem))
                {
                    childParticleSystem.Play();
                }

                if (child.TryGetComponent(out Light childLight))
                {
                    childLight.enabled = true;
                }
            }
        }

        public static void PlayParticleEffect(Transform particleSystemTransform)
        {
            if (particleSystemTransform.TryGetComponent(out ParticleSystem particleSystem))
            {
                PlayParticleEffect(particleSystem);
            }
            else
            {
                Debug.LogError("No particle system found on transform!");
            }
        }

        /// <summary>
        /// Stops a particle effect along will all of its child particle effects
        /// </summary>
        /// <param name="particleSystem"></param>
        public static void StopParticleEffect(ParticleSystem particleSystem)
        {
            particleSystem.Stop();

            foreach (Transform child in particleSystem.transform)
            {
                // Ignore the child if it's not active
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                if (child.TryGetComponent(out ParticleSystem childParticleSystem))
                {
                    childParticleSystem.Stop();
                }

                if (child.TryGetComponent(out Light childLight))
                {
                    childLight.enabled = false;
                }
            }
        }


        public static void StopParticleEffect(Transform particleSystemTransform)
        {
            if (particleSystemTransform.TryGetComponent(out ParticleSystem particleSystem))
            {
                StopParticleEffect(particleSystem);
            }
            else
            {
                Debug.LogError("No particle system found on transform!");
            }
        }

        #endregion

        #region UI Elements

        public static IEnumerator FadeInText(TextMeshProUGUI textComponent, float fadeInTime)
        {
            float timeElapsed = 0;

            while (timeElapsed < fadeInTime)
            {
                textComponent.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeInTime);
                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            textComponent.alpha = 1;
        }

        public static IEnumerator FadeOutText(TextMeshProUGUI textComponent, float fadeOutTime)
        {
            float timeElapsed = 0;

            while (timeElapsed < fadeOutTime)
            {
                textComponent.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            textComponent.alpha = 0;
        }

        public static IEnumerator FadeInButton(Button button, float fadeInTime)
        {
            // See if the button has a text component
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            float timeElapsed = 0;

            while (timeElapsed < fadeInTime)
            {
                button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b,
                                       Mathf.Lerp(0, 1, timeElapsed / fadeInTime));

                if (buttonText != null)
                {
                    buttonText.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeInTime);
                }

                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b, 1);
            if (buttonText != null)
            {
                buttonText.alpha = 1;
            }
        }

        public static IEnumerator FadeOutButton(Button button, float fadeOutTime)
        {
            // See if the button has a text component
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            float timeElapsed = 0;

            while (timeElapsed < fadeOutTime)
            {
                button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b,
                                       Mathf.Lerp(1, 0, timeElapsed / fadeOutTime));

                if (buttonText != null)
                {
                    buttonText.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
                }

                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            button.image.color = new Color(button.image.color.r, button.image.color.g, button.image.color.b, 0);
            if (buttonText != null)
            {
                buttonText.alpha = 0;
            }
        }

        public static IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float fadeOutTime, float fromAlpha = 1, float targetAlpha = 0)
        {
            float timeElapsed = 0;

            while (timeElapsed < fadeOutTime)
            {
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, targetAlpha, timeElapsed / fadeOutTime);
                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float fadeOutTime, float fromAlpha = 0, float targetAlpha = 1)
        {
            float timeElapsed = 0;

            while (timeElapsed < fadeOutTime)
            {
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, targetAlpha, timeElapsed / fadeOutTime);
                timeElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        #endregion

        #region NavMesh

        public static Vector3 SampleNavMesh(Vector3 position, float maxDistance = 1)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                return Vector3.zero;
            }
        }

        #endregion
    }
}