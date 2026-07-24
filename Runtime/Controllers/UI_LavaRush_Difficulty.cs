using System;
using ActionFit.LavaRush;
using ActionFit.LavaRush.UI;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Original three-option difficulty controller identity.</summary>
public class UI_LavaRush_Difficulty : LavaRushControllerView
{
    [Serializable]
    public class DifficultyOption
    {
        public UI_Button btnDifficulty;
        public UI_Image imgEffect;
    }

    [Serializable]
    public class Refs
    {
        public DifficultyOption easy;
        [FormerlySerializedAs("hard")]
        public DifficultyOption normal;
        [FormerlySerializedAs("veryHard")]
        public DifficultyOption hard;
        public UI_Button btnStart;
        public LavaRushRewardGridView gridRewardPanel;
        public UI_Image imgRewardPanel;
        public UI_Text txtTimer;
        public UI_Mask mask;
    }

    public Refs refs = new();
    [SerializeField] private LavaRushControllerRefs controller = new();

    public int SelectedDifficulty { get; private set; } = -1;

    protected override LavaRushControllerRefs ControllerRefs => controller;
    protected override bool BindGenericActions => false;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.Difficulty;

    protected override void OnBound()
    {
        refs?.easy?.btnDifficulty?.AddListener(OnClickEasy);
        refs?.normal?.btnDifficulty?.AddListener(OnClickNormal);
        refs?.hard?.btnDifficulty?.AddListener(OnClickHard);
        refs?.btnStart?.AddListener(StartSelected);
        ResetSelection();
    }

    protected override void OnWillOpen()
    {
        base.OnWillOpen();
        ResetSelection();
    }

    protected override void OnShown()
    {
        ResetSelection();
    }

    protected override void OnDestroy()
    {
        refs?.easy?.btnDifficulty?.RemoveListener(OnClickEasy);
        refs?.normal?.btnDifficulty?.RemoveListener(OnClickNormal);
        refs?.hard?.btnDifficulty?.RemoveListener(OnClickHard);
        refs?.btnStart?.RemoveListener(StartSelected);
        base.OnDestroy();
    }

    public void Select(int index)
    {
        if (index < 0 || index > 2)
            throw new ArgumentOutOfRangeException(nameof(index));

        bool deselect = SelectedDifficulty == index;
        SelectedDifficulty = deselect ? -1 : index;
        refs?.btnStart?.SetInteractable(!deselect);

        DifficultyOption[] options = Options();
        for (int optionIndex = 0; optionIndex < options.Length; optionIndex++)
            SetSelected(options[optionIndex], !deselect && optionIndex == index);

        if (deselect)
        {
            refs?.mask?.Collapse();
            return;
        }

        Owner?.PlayAudio(LavaRushAudioCue.DifficultySelect);
        MoveRewardPanelTo(index);
        ChangeRewardPanel(index);
        refs?.mask?.Expand();
    }

    public void StartSelected()
    {
        if (SelectedDifficulty >= 0)
            Owner?.StartMatch(SelectedDifficulty + 1);
    }

    private void ResetSelection()
    {
        SelectedDifficulty = -1;
        refs?.btnStart?.SetInteractable(false);
        refs?.mask?.AnimHeight(0f, 0f);

        DifficultyOption[] options = Options();
        for (int index = 0; index < options.Length; index++)
            SetSelected(options[index], false);
    }

    private void MoveRewardPanelTo(int index)
    {
        DifficultyOption[] options = Options();
        if (refs?.imgRewardPanel == null || options[index]?.btnDifficulty == null)
            return;

        RectTransform panel = refs.imgRewardPanel.RectTransform;
        RectTransform button = options[index].btnDifficulty.RectTransform;
        Vector2 position = panel.anchoredPosition;
        position.x = button.anchoredPosition.x;
        panel.anchoredPosition = position;
    }

    private void ChangeRewardPanel(int index)
    {
        int difficulty = index + 1;
        if (refs?.gridRewardPanel == null
            || Owner?.Engine?.Catalog?.ContainsDifficulty(difficulty) != true)
        {
            return;
        }

        LavaRushDifficultyDefinition definition = Owner.Engine.Catalog.GetDifficulty(difficulty);
        refs.gridRewardPanel.SetRewards(
            definition.GetStage(definition.StageCount).Rewards,
            Owner.RewardPresentation);
    }

    private static void SetSelected(DifficultyOption option, bool selected)
    {
        if (option?.imgEffect != null)
            option.imgEffect.Alpha = selected ? 1f : 0f;
    }

    private DifficultyOption[] Options() =>
        new[] { refs?.easy, refs?.normal, refs?.hard };

    private void OnClickEasy() => Select(0);
    private void OnClickNormal() => Select(1);
    private void OnClickHard() => Select(2);
}
