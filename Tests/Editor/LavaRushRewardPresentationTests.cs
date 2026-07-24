using System.Collections.Generic;
using ActionFit.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI.Tests
{
    public sealed class LavaRushRewardPresentationTests
    {
        private const string PackageRoot = "Packages/com.actionfit.lava-rush.ui";
        private const string MainPrefabPath =
            PackageRoot + "/Runtime/Prefabs/Main/UI_LavaRush.prefab";

        [TestCase("UI_LavaRush_Difficulty.prefab")]
        [TestCase("UI_LavaRush_Match.prefab")]
        public void CanonicalPrefab_BindsConcreteRewardGridBesideLayout(string prefabName)
        {
            string path = $"{PackageRoot}/Runtime/Prefabs/UI/{prefabName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            LavaRushRewardGridView grid = prefab.GetComponentInChildren<LavaRushRewardGridView>(true);
            var serialized = new SerializedObject(grid);

            Assert.That(grid, Is.Not.Null, path);
            Assert.That(grid.GetComponent<GridLayoutGroup>(), Is.Not.Null, path);
            Assert.That(
                serialized.FindProperty("assets.cellTemplate").objectReferenceValue,
                Is.TypeOf<LavaRushRewardCellView>(),
                path);
            Assert.That(AssetDatabase.GetDependencies(path, true), Has.None.StartsWith("Assets/"));
        }

        [Test]
        public void RewardGrid_PoolsTypedCellsAndAppliesEveryContentReward()
        {
            const string path =
                PackageRoot + "/Runtime/Prefabs/UI/UI_LavaRush_Difficulty.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var provider = new RewardPresentationProbe();
            try
            {
                LavaRushRewardGridView grid =
                    instance.GetComponentInChildren<LavaRushRewardGridView>(true);
                grid.SetRewards(
                    new[]
                    {
                        new ContentReward("Gold", 100),
                        new ContentReward("BoardItem/70003_5", 1),
                    },
                    provider);

                Assert.That(grid.ActiveCellCount, Is.EqualTo(2));
                Assert.That(provider.Resolved, Is.EqualTo(new[] { "Gold:100", "BoardItem/70003_5:1" }));
                LavaRushRewardCellView[] cells =
                    grid.GetComponentsInChildren<LavaRushRewardCellView>(true);
                foreach (LavaRushRewardCellView cell in cells)
                {
                    Assert.That(cell.IsComplete, Is.True);
                }

                provider.Resolved.Clear();
                grid.SetRewards(new[] { new ContentReward("Energy", 3) }, provider);
                Assert.That(grid.ActiveCellCount, Is.EqualTo(1));
                Assert.That(provider.Resolved, Is.EqualTo(new[] { "Energy:3" }));
            }
            finally
            {
                if (instance != null)
                    Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void DifficultyAndMatch_RenderPinnedCatalogRewardsThroughInjectedProvider()
        {
            GameObject main = null;
            GameObject bootstrapRoot = null;
            var provider = new RewardPresentationProbe();
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainPrefabPath);
                main = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                global::UI_LavaRush controller = main.GetComponent<global::UI_LavaRush>();
                bootstrapRoot = new GameObject("Lava Rush Reward Presentation Bootstrap");
                LavaRushBootstrap bootstrap = bootstrapRoot.AddComponent<LavaRushBootstrap>();
                bootstrap.InitializeDefault(
                    controller,
                    restoreEngine: false,
                    showOnInitialize: true,
                    rewardPresentation: provider);

                controller.HandleAction(LavaRushUIAction.StartEvent);
                controller.refs.uiDifficulty.Select(1);
                Assert.That(provider.Resolved, Is.EqualTo(new[] { "dia:6" }));
                Assert.That(controller.refs.uiDifficulty.refs.gridRewardPanel.ActiveCellCount, Is.EqualTo(1));

                provider.Resolved.Clear();
                controller.refs.uiDifficulty.StartSelected();
                Assert.That(
                    provider.Resolved,
                    Is.EqualTo(new[] { "energy:4", "coin:200", "dia:6" }));
                Assert.That(controller.refs.uiMatch.refs.gridRewardPanel.ActiveCellCount, Is.EqualTo(1));
            }
            finally
            {
                if (main != null)
                    Object.DestroyImmediate(main);
                if (bootstrapRoot != null)
                    Object.DestroyImmediate(bootstrapRoot);
                new PlayerPrefsContentStateStore().Delete(LavaRushBootstrap.DefaultDemoContentId);
            }
        }

        private sealed class RewardPresentationProbe : ILavaRushRewardPresentationProvider
        {
            public List<string> Resolved { get; } = new();

            public LavaRushRewardPresentation Resolve(ContentReward reward)
            {
                Resolved.Add($"{reward.RewardId}:{reward.Amount}");
                return new LavaRushRewardPresentation(
                    null,
                    reward.Amount.ToString(),
                    reward.Amount > 1,
                    false,
                    null);
            }
        }
    }
}
