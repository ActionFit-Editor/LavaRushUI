using System;

namespace ActionFit.LavaRush.UI
{
    public interface ILavaRushFrameScheduler
    {
        IDisposable SubscribeUpdate(Action<float> handler);
        IDisposable SubscribeLateUpdate(Action<float> handler);
        IDisposable SubscribeEverySecond(Action handler);
    }
}
