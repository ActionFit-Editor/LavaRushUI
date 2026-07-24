using System;
using System.Collections.Generic;
using ActionFit.Content;
using ReferenceBinding;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>
    /// Package-owned grid binder that pools clones of the authored Lava Rush reward cell.
    /// Reward lookup and navigation remain behind <see cref="ILavaRushRewardPresentationProvider"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    [AddComponentMenu("ActionFit/Lava Rush Reward Grid View")]
    public sealed class LavaRushRewardGridView : MonoBehaviour
    {
        [Serializable]
        public sealed class Assets
        {
            [SerializeField, RequiredReference("LAVA_RUSH_UI_REWARD_CELL_TEMPLATE_MISSING")]
            private LavaRushRewardCellView cellTemplate;

            public LavaRushRewardCellView CellTemplate => cellTemplate;
        }

        [SerializeField] private Assets assets = new();

        private readonly List<LavaRushRewardCellView> _cells = new();

        internal int ActiveCellCount
        {
            get
            {
                int count = 0;
                for (int index = 0; index < _cells.Count; index++)
                    if (_cells[index] != null && _cells[index].gameObject.activeSelf)
                        count++;
                return count;
            }
        }

        internal void SetRewards(
            IReadOnlyList<ContentReward> rewards,
            ILavaRushRewardPresentationProvider provider)
        {
            int requiredCount = rewards?.Count ?? 0;
            EnsureCellCount(requiredCount);
            provider ??= DefaultLavaRushRewardPresentationProvider.Instance;

            for (int index = 0; index < _cells.Count; index++)
            {
                if (index < requiredCount)
                    _cells[index].SetPresentation(provider.Resolve(rewards[index]));
                else
                    _cells[index].Hide();
            }
        }

        private void EnsureCellCount(int count)
        {
            while (_cells.Count < count)
            {
                if (assets?.CellTemplate == null)
                {
                    Debug.LogError(
                        "[LavaRushRewardGridView] EnsureCellCount: reward cell template is missing.",
                        this);
                    return;
                }

                LavaRushRewardCellView cell = Instantiate(
                    assets.CellTemplate,
                    transform,
                    false);
                cell.name = $"LavaRush_Reward_{_cells.Count + 1}";
                _cells.Add(cell);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReferenceBindingRequests.Enqueue(this);
        }
#endif
    }
}
