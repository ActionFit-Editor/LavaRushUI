using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Exposes the package-owned order-reward target and arrival notification.</summary>
    public interface ILavaRushProgressView
    {
        RectTransform TargetProgress { get; }

        void NotifyProgressArrived();
    }
}
