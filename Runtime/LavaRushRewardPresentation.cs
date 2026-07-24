using System;
using System.Globalization;
using ActionFit.Content;
using UnityEngine;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Immutable visual data resolved for one engine-owned reward snapshot.</summary>
    public sealed class LavaRushRewardPresentation
    {
        public LavaRushRewardPresentation(
            Sprite icon,
            string amountText,
            bool showAmount,
            bool showInfo,
            Action infoRequested)
        {
            Icon = icon;
            AmountText = amountText ?? string.Empty;
            ShowAmount = showAmount;
            ShowInfo = showInfo;
            InfoRequested = infoRequested;
        }

        public Sprite Icon { get; }
        public string AmountText { get; }
        public bool ShowAmount { get; }
        public bool ShowInfo { get; }
        public Action InfoRequested { get; }
    }

    /// <summary>
    /// Resolves project-owned reward assets and navigation without exposing them to package views.
    /// </summary>
    public interface ILavaRushRewardPresentationProvider
    {
        LavaRushRewardPresentation Resolve(ContentReward reward);
    }

    /// <summary>Standalone fallback that preserves authored cell art and formats only the amount.</summary>
    public sealed class DefaultLavaRushRewardPresentationProvider :
        ILavaRushRewardPresentationProvider
    {
        public static DefaultLavaRushRewardPresentationProvider Instance { get; } = new();

        private DefaultLavaRushRewardPresentationProvider()
        {
        }

        public LavaRushRewardPresentation Resolve(ContentReward reward)
        {
            if (reward == null)
                throw new ArgumentNullException(nameof(reward));

            return new LavaRushRewardPresentation(
                null,
                reward.Amount.ToString(CultureInfo.InvariantCulture),
                reward.Amount > 1,
                false,
                null);
        }
    }

}
