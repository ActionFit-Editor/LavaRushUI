using System;
using System.IO;
using ActionFit.LavaRush.UI.Editor;
using NUnit.Framework;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class CatDetectiveStarterInstallerTests
    {
        private string _root;
        private string _source;
        private string _target;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "actionfit-lava-rush-ui-tests", Guid.NewGuid().ToString("N"));
            _source = Path.Combine(_root, "source");
            _target = Path.Combine(_root, "target");
            Directory.CreateDirectory(Path.Combine(_source, "Runtime"));
            File.WriteAllText(Path.Combine(_source, "Runtime", "Starter.cs"), "starter-v1");
            File.WriteAllText(Path.Combine(_source, "README.md"), "starter-readme");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }
        }

        [Test]
        public void FirstInstallThenRepeat_IsCreateOnlyAndIdempotent()
        {
            CatDetectiveStarterInstallPlan first = CatDetectiveStarterInstaller.BuildPlan(
                _source,
                _target,
                false);

            Assert.That(first.NewFiles, Has.Count.EqualTo(2));
            Assert.That(first.CanInstall, Is.True);
            CatDetectiveStarterInstaller.Apply(first);

            CatDetectiveStarterInstallPlan repeat = CatDetectiveStarterInstaller.BuildPlan(
                _source,
                _target,
                false);

            Assert.That(repeat.NewFiles, Is.Empty);
            Assert.That(repeat.UnchangedFiles, Has.Count.EqualTo(2));
            Assert.That(repeat.ConflictingFiles, Is.Empty);
        }

        [Test]
        public void DifferingTarget_BlocksTheWholePlanAndPreservesUserFile()
        {
            Directory.CreateDirectory(Path.Combine(_target, "Runtime"));
            string userFile = Path.Combine(_target, "Runtime", "Starter.cs");
            File.WriteAllText(userFile, "user-owned-change");

            CatDetectiveStarterInstallPlan plan = CatDetectiveStarterInstaller.BuildPlan(
                _source,
                _target,
                false);

            Assert.That(plan.CanInstall, Is.False);
            Assert.That(plan.ConflictingFiles, Is.EqualTo(new[] { "Runtime/Starter.cs" }));
            Assert.Throws<InvalidOperationException>(() => CatDetectiveStarterInstaller.Apply(plan));
            Assert.That(File.ReadAllText(userFile), Is.EqualTo("user-owned-change"));
            Assert.That(File.Exists(Path.Combine(_target, "README.md")), Is.False);
        }
    }
}
