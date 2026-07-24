using System;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Exposes the active Lava Rush access state without project manager dependencies.</summary>
    public interface ILavaRushAccessService
    {
        bool IsEventActive { get; }
        bool IsEventStarted { get; }
        DateTime EventEndTime { get; }
        TimeSpan EventRemainTime { get; }

        void OpenContent();
    }
}
