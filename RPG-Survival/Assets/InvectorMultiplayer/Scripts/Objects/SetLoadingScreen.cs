using CBGames.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CBGames.Objects {
    [AddComponentMenu("CB GAMES/Objects/Set Loading Screen")]
    public class SetLoadingScreen : MonoBehaviour
    {
        [Tooltip("The images to cycle through while on the loading screen. Used in conjunction with the `UICoreLogic` component.")]
        [SerializeField] protected List<Sprite> LoadingImages = new List<Sprite>();
        [Tooltip("The load title to display while on the loading screen.")]
        [SerializeField] protected string LoadingTitle = "";
        [Tooltip("The loading description to display while on the loading screen.")]
        [SerializeField] protected List<string> LoadingDescriptions = new List<string>();

        protected ExampleUI ui;
        protected UICoreLogic logic;

        /// <summary>
        /// Sets the loading image, loading desc and load title on the UICoreLogic component.
        /// </summary>
        public virtual void SetLoadingScreenItems()
        {
            logic = FindObjectOfType<UICoreLogic>();
            if (logic != null)
            {
                logic.loadingImages = (LoadingImages.Count > 0) ? LoadingImages : logic.loadingImages;
                logic.loadingTitle = LoadingTitle;
                logic.loadingDesc = (LoadingDescriptions.Count > 0) ? LoadingDescriptions : logic.loadingDesc;
            }
            else
            {
                ui = FindObjectOfType<ExampleUI>();
                if (ui != null)
                {
                    if (LoadingImages.Count > 0) ui.SetLoadingImages(LoadingImages);
                    ui.SetLoadingTitleText(LoadingTitle);
                    ui.ResetLoadingBar();
                    if (LoadingDescriptions.Count > 0) ui.SetLoadingDescriptionText(LoadingDescriptions);
                }
            }
        }

        /// <summary>
        /// Calls `EnableLoadingPage` on `UICoreLogic` to enable the loading screen.
        /// Also calls `SetLoadingScreenItems` function to set the `UICoreLogic`
        /// values.
        /// </summary>
        public virtual void EnableLoadingScreen()
        {
            SetLoadingScreenItems();
            if (logic != null)
            {
                logic.EnableLoadingPage(true);
            }
            else if (ui != null)
            {
                ui.EnableLoadingPage(true);
            }
        }
    }
}