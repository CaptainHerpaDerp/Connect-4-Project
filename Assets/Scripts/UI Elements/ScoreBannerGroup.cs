using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{
    /// <summary>
    /// A group of score banners that represent the score of a player.
    /// </summary>
    public class ScoreBannerGroup : MonoBehaviour
    {
        [SerializeField] private ScoreBanner scoreBannerPrefab;

        [SerializeField] private HorizontalLayoutGroup scoreLayoutGroupParent;

        private Color playerColour;

        #region Instantiation

        public void Instantiate(Color colour, int maxScore)
        {
            playerColour = colour;
            SetMaxScore(maxScore);   
        }

        /// <summary>
        /// Instantiates the maximum number of score banners for the group.
        /// </summary>
        /// <param name="maxScore"></param>
        public void SetMaxScore(int maxScore)
        {
            for (int i = 0; i < maxScore; i++)
            {
                ScoreBanner newScoreBanner = Instantiate(scoreBannerPrefab, scoreLayoutGroupParent.transform);
                newScoreBanner.SetColour(playerColour);

                // Deactivate the score banner so it can be activated later
                newScoreBanner.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the colour of all future score banners added to the group.
        /// </summary>
        /// <param name="colour"></param>
        public void SetPlayerColour(Color colour)
        {
            playerColour = colour;
        }

        #endregion

        /// <summary>
        /// Adds a new score banner to the group.
        /// </summary>
        public void AddPlayerScore()
        {
            // Get the first child of the layout group that isn't active, and activate it
            foreach (Transform child in scoreLayoutGroupParent.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true); 
                    return;
                }
            }
        }

        /// <summary>
        /// Removes all score banners from the group.
        /// </summary>
        public void ResetPlayerScore()
        {
            foreach (Transform child in scoreLayoutGroupParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}