using System;
using System.Threading;
using TMPro;

namespace ActionFit.LavaRush.UI
{
    public interface ILavaRushCountdownScheduler
    {
        DateTime Now { get; }
        bool TryGetNow(out DateTime now);

        void Register(
            TMP_Text target,
            DateTime endTime,
            CancellationToken cancellationToken,
            Action onExpired = null,
            Func<TimeSpan, string> formatter = null);
    }
}
