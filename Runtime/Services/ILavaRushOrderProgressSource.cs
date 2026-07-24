using System;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Supplies positive order progress for the enabled lifetime of a Lava Rush controller.</summary>
    public interface ILavaRushOrderProgressSource
    {
        IDisposable Subscribe(Action<int> onOrderProgress);
    }
}
