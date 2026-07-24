using NUnit.Framework;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushProductServiceContractsTests
    {
        [Test]
        public void AudioContract_CoversEveryRestoredCue()
        {
            Assert.That(System.Enum.GetValues(typeof(LavaRushAudioCue)).Length, Is.EqualTo(10));
            Assert.DoesNotThrow(() => NullLavaRushAudio.Instance.Play(LavaRushAudioCue.MatchWin));
            Assert.DoesNotThrow(() => NullLavaRushAudio.Instance.PlayPitched(
                LavaRushAudioCue.ProfileAppear,
                0.5f,
                0.85f,
                1.2f));
        }

        [Test]
        public void ProfileSnapshot_NormalizesMissingProductValues()
        {
            var snapshot = new LavaRushProfileSnapshot("", null, null, -1);

            Assert.That(snapshot.DisplayName, Is.EqualTo("Player"));
            Assert.That(snapshot.ProfileId, Is.EqualTo("0"));
            Assert.That(snapshot.FrameId, Is.EqualTo("frame_blue"));
            Assert.That(snapshot.HorizontalDirection, Is.EqualTo(-1));
        }

        [Test]
        public void LocalizationContract_CoversEighteenSemanticMessagesAndFallbackLookup()
        {
            Assert.That(LavaRushLocalizationKeys.All.Count, Is.EqualTo(18));
            Assert.That(
                LavaRushLocalizationKeys.GetFallback(LavaRushLocalizationKeys.Title),
                Is.EqualTo("Lava Rush"));
            Assert.That(LavaRushLocalizationKeys.GetFallback("unknown"), Is.Empty);
        }
    }
}
