#if UNITY_EDITOR
using System;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public static class CatDetectiveLavaRushAddressables
{
    private const string MenuRoot = "Tools/Package/ActionFit Lava Rush UI/CatDetective Addressables/";
    private const string PopupPath = "Assets/Contents/LavaRush/Prefabs/UI_LavaRush.prefab";
    private const string PopupAddress = "UI_LavaRush";
    private const string CoreLabel = "base";

    public static void PreviewForAutomation()
    {
        ValidateImportedAssets();
        RegistrationPlan plan = BuildPlan();
        string report = plan.Report();
        UnityEngine.Debug.Log(report);
        if (plan.IsBlocked)
        {
            throw new InvalidOperationException(plan.Blocker);
        }
    }

    [MenuItem(MenuRoot + "Preview Registration", false, 120)]
    private static void Preview()
    {
        RegistrationPlan plan = BuildPlan();
        string report = plan.Report();
        if (plan.IsBlocked)
        {
            UnityEngine.Debug.LogError(report);
        }
        else
        {
            UnityEngine.Debug.Log(report);
        }
        EditorUtility.DisplayDialog("CatDetective Lava Rush Addressables", report, "OK");
    }

    [MenuItem(MenuRoot + "Register Popup", false, 121)]
    private static void Register()
    {
        RegistrationPlan plan = BuildPlan();
        string report = plan.Report();
        if (plan.IsBlocked)
        {
            UnityEngine.Debug.LogError(report);
            EditorUtility.DisplayDialog(
                "CatDetective Lava Rush Addressables Blocked",
                report + "\n\nNo Addressables settings were changed.",
                "OK");
            return;
        }

        if (!plan.HasChanges)
        {
            UnityEngine.Debug.Log(report);
            EditorUtility.DisplayDialog(
                "CatDetective Lava Rush Addressables",
                report + "\n\nThe popup registration is already current.",
                "OK");
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Register CatDetective Lava Rush Popup",
            report + "\n\nThis will update the project's serialized Addressables settings.",
            "Register",
            "Cancel");
        if (!confirmed)
        {
            return;
        }

        AddressableAssetEntry entry = plan.Entry
            ?? plan.Settings.CreateOrMoveEntry(plan.PopupGuid, plan.Settings.DefaultGroup);
        entry.address = PopupAddress;
        entry.SetLabel(CoreLabel, true, true);
        EditorUtility.SetDirty(plan.Settings);
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log(
            $"[CatDetectiveLavaRushAddressables] Registered {PopupPath} as {PopupAddress} with label {CoreLabel}.");
    }

    private static RegistrationPlan BuildPlan()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        string popupGuid = AssetDatabase.AssetPathToGUID(PopupPath);
        if (settings == null)
        {
            return RegistrationPlan.Blocked("AddressableAssetSettings is unavailable.");
        }
        if (string.IsNullOrEmpty(popupGuid))
        {
            return RegistrationPlan.Blocked($"Popup prefab is missing: {PopupPath}");
        }

        AddressableAssetEntry ownEntry = settings.FindAssetEntry(popupGuid);
        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
            {
                continue;
            }
            foreach (AddressableAssetEntry entry in group.entries)
            {
                if (entry != null
                    && string.Equals(entry.address, PopupAddress, StringComparison.Ordinal)
                    && !string.Equals(entry.guid, popupGuid, StringComparison.Ordinal))
                {
                    return RegistrationPlan.Blocked(
                        $"Address collision: {PopupAddress} is already owned by {entry.AssetPath}.");
                }
            }
        }

        bool addressMatches = ownEntry != null
            && string.Equals(ownEntry.address, PopupAddress, StringComparison.Ordinal);
        bool labelMatches = ownEntry != null && ownEntry.labels.Contains(CoreLabel);
        if (ownEntry == null && settings.DefaultGroup == null)
        {
            return RegistrationPlan.Blocked("Addressables has no default group for the new popup entry.");
        }
        return new RegistrationPlan(settings, ownEntry, popupGuid, addressMatches && labelMatches, null);
    }

    private static void ValidateImportedAssets()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PopupPath);
        if (prefab == null
            || prefab.GetComponent<UI_LavaRush>() == null
            || prefab.GetComponent<CatDetectiveLavaRushCompositionRoot>() == null
            || prefab.transform.Find("Popup") == null)
        {
            throw new InvalidOperationException("The imported UI_LavaRush prefab bindings are incomplete.");
        }

        CatDetectiveLavaRushSettings settings = AssetDatabase.LoadAssetAtPath<CatDetectiveLavaRushSettings>(
            "Assets/Contents/LavaRush/Resources/CatDetectiveLavaRushSettings.asset");
        if (settings == null)
        {
            throw new InvalidOperationException("The imported CatDetectiveLavaRushSettings asset is unavailable.");
        }
    }

    private sealed class RegistrationPlan
    {
        internal RegistrationPlan(
            AddressableAssetSettings settings,
            AddressableAssetEntry entry,
            string popupGuid,
            bool isCurrent,
            string blocker)
        {
            Settings = settings;
            Entry = entry;
            PopupGuid = popupGuid;
            IsCurrent = isCurrent;
            Blocker = blocker;
        }

        internal AddressableAssetSettings Settings { get; }
        internal AddressableAssetEntry Entry { get; }
        internal string PopupGuid { get; }
        internal bool IsCurrent { get; }
        internal string Blocker { get; }
        internal bool IsBlocked => !string.IsNullOrEmpty(Blocker);
        internal bool HasChanges => !IsBlocked && !IsCurrent;

        internal static RegistrationPlan Blocked(string blocker)
        {
            return new RegistrationPlan(null, null, null, false, blocker);
        }

        internal string Report()
        {
            var builder = new StringBuilder();
            builder.AppendLine("[CatDetectiveLavaRushAddressables] Preview");
            builder.AppendLine($"Prefab: {PopupPath}");
            builder.AppendLine($"Address: {PopupAddress}");
            builder.AppendLine($"Label: {CoreLabel}");
            builder.AppendLine($"Group: {Entry?.parentGroup?.Name ?? Settings?.DefaultGroup?.Name ?? "<unavailable>"}");
            if (IsBlocked)
            {
                builder.Append("Blocked: ").Append(Blocker);
            }
            else
            {
                builder.Append(IsCurrent
                    ? "Result: unchanged"
                    : Entry == null ? "Result: create one entry" : "Result: update the existing prefab entry");
            }
            return builder.ToString();
        }
    }
}
#endif
