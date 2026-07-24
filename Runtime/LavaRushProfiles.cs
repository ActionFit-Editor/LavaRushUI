using System;

namespace ActionFit.LavaRush.UI
{
    /// <summary>Immutable presentation data for one player or generated opponent.</summary>
    public readonly struct LavaRushProfileSnapshot
    {
        public const string DefaultProfileId = "0";
        public const string DefaultFrameId = "frame_blue";

        public LavaRushProfileSnapshot(
            string displayName,
            string profileId,
            string frameId,
            int horizontalDirection)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName;
            ProfileId = string.IsNullOrWhiteSpace(profileId) ? DefaultProfileId : profileId;
            FrameId = string.IsNullOrWhiteSpace(frameId) ? DefaultFrameId : frameId;
            HorizontalDirection = horizontalDirection;
        }

        public string DisplayName { get; }
        public string ProfileId { get; }
        public string FrameId { get; }
        public int HorizontalDirection { get; }
    }

    /// <summary>Reads stable player/opponent records and explicitly regenerates stage opponents.</summary>
    public interface ILavaRushProfileRoster
    {
        LavaRushProfileSnapshot GetPlayer();
        LavaRushProfileSnapshot LoadOrGenerateOpponent(int stage, int slot);
        void DeleteOpponents(int stage, int slotCount);
    }

    /// <summary>Presents package-owned snapshots without exposing a consuming project's prefab type.</summary>
    public interface ILavaRushProfileGroupView
    {
        void BindPlayer(LavaRushProfileSnapshot profile);
        void BindOpponent(int slot, LavaRushProfileSnapshot profile);
        void HidePlayer();
        void ShowPlayerOnly();
        void HideAll();
        void SetOpponentCount(int count, bool animate, Action onAppear = null);
        void PlayPlayerAppear(
            float delay,
            Action onComplete = null,
            Action onAppear = null);
        void CancelAnimations();
    }

    public sealed class DefaultLavaRushProfileRoster : ILavaRushProfileRoster
    {
        public static DefaultLavaRushProfileRoster Instance { get; } = new();

        private DefaultLavaRushProfileRoster()
        {
        }

        public LavaRushProfileSnapshot GetPlayer()
        {
            return new LavaRushProfileSnapshot("Player", "0", "frame_blue", 0);
        }

        public LavaRushProfileSnapshot LoadOrGenerateOpponent(int stage, int slot)
        {
            if (stage < 0)
                throw new ArgumentOutOfRangeException(nameof(stage));
            if (slot < 0)
                throw new ArgumentOutOfRangeException(nameof(slot));

            return new LavaRushProfileSnapshot(
                $"Bot {slot + 1}",
                "0",
                "frame_blue",
                0);
        }

        public void DeleteOpponents(int stage, int slotCount)
        {
            if (stage < 0)
                throw new ArgumentOutOfRangeException(nameof(stage));
            if (slotCount < 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount));
        }
    }
}
