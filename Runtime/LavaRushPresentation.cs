using System;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>
    /// Compatibility wrapper for consumers that previously referenced LavaRushPresentation.
    /// It delegates to one authored UI_LavaRush family and never creates screens.
    /// </summary>
    public class LavaRushPresentation : MonoBehaviour
    {
        [SerializeField] private global::UI_LavaRush controller;

        public global::UI_LavaRush Controller =>
            controller != null ? controller : GetComponentInChildren<global::UI_LavaRush>(true);
        public bool IsInitialized => Controller?.IsInitialized == true;
        public bool IsVisible => Controller != null && Controller.gameObject.activeSelf;

        protected virtual LavaRushUITheme ResolveDefaultTheme() => new();

        public void Initialize(LavaRushControllerContext context, bool restoreEngine = true)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (Controller == null)
                throw new InvalidOperationException("LavaRushPresentation requires an authored UI_LavaRush child.");

            controller = Controller;
            controller.Initialize(context, restoreEngine);
        }

        public void Show()
        {
            if (Controller == null)
                throw new InvalidOperationException("LavaRushPresentation requires an authored UI_LavaRush child.");
            Controller.gameObject.SetActive(true);
            Controller.OpenMatchFlow();
        }

        public void Hide()
        {
            if (Controller != null)
                Controller.gameObject.SetActive(false);
        }
    }
}
