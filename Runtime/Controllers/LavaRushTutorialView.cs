using ActionFit.LavaRush.UI;

/// <summary>Package replacement for the project-owned UITutorial component.</summary>
public sealed class LavaRushTutorialView : LavaRushControllerView
{
    [System.Serializable]
    public sealed class Refs : LavaRushControllerRefs
    {
    }

    public Refs refs = new();

    protected override LavaRushControllerRefs ControllerRefs => refs;
    public override LavaRushControllerScreen Screen => LavaRushControllerScreen.Tutorial;
}
