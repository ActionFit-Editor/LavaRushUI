namespace ActionFit.LavaRush.UI
{
    /// <summary>Identifies the ten presentation sounds used by the restored Lava Rush flow.</summary>
    public enum LavaRushAudioCue
    {
        DifficultySelect,
        RewardSpawn,
        RewardArrive,
        WinJump,
        BlockClear,
        MatchWin,
        MatchLose,
        ProfileAppear,
        TutorialStep,
        FinalRewardOpen,
    }

    /// <summary>Routes typed presentation cues without exposing clips, mixers, or product audio types.</summary>
    public interface ILavaRushAudio
    {
        void Play(LavaRushAudioCue cue);

        void PlayPitched(
            LavaRushAudioCue cue,
            float volume,
            float pitchMin,
            float pitchMax);
    }

    public sealed class NullLavaRushAudio : ILavaRushAudio
    {
        public static NullLavaRushAudio Instance { get; } = new();

        private NullLavaRushAudio()
        {
        }

        public void Play(LavaRushAudioCue cue)
        {
        }

        public void PlayPitched(
            LavaRushAudioCue cue,
            float volume,
            float pitchMin,
            float pitchMax)
        {
        }
    }
}
