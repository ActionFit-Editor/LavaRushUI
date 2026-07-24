namespace ActionFit.LavaRush.UI
{
    public enum LavaRushUIAction
    {
        None = 0,
        StartEvent = 1,
        SelectEasy = 2,
        SelectNormal = 3,
        SelectHard = 4,
        CompleteTutorial = 5,
        StartStage = 6,
        AddProgress = 7,
        EvaluateStage = 8,
        ConfirmResult = 9,
        EndEvent = 10,
        Close = 11,
    }

    /// <summary>One immutable controller action rendered by an authored button.</summary>
    public sealed class LavaRushUIButtonModel
    {
        public LavaRushUIButtonModel(
            LavaRushUIAction action,
            string label,
            bool visible = true,
            bool interactable = true)
        {
            Action = action;
            Label = label ?? string.Empty;
            Visible = visible;
            Interactable = interactable;
        }

        public LavaRushUIAction Action { get; }
        public string Label { get; }
        public bool Visible { get; }
        public bool Interactable { get; }

        public static LavaRushUIButtonModel Hidden { get; } =
            new(LavaRushUIAction.None, string.Empty, false, false);
    }
}
