#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ActionFit.LavaRush.UI.Editor
{
    public static class LavaRushUIPackageMenu
    {
        private const string MenuRoot = "Tools/Package/ActionFit Lava Rush UI/";
        private const string ReadmePath = "Packages/com.actionfit.lava-rush.ui/README.md";
        private const string DemoPrefabPath =
            "Packages/com.actionfit.lava-rush.ui/Runtime/Prefabs/LavaRushDemo.prefab";

        [MenuItem(MenuRoot + "Create Demo", false, 80)]
        private static void CreateDemo()
        {
            GameObject demoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DemoPrefabPath);
            GameObject demo = demoPrefab != null
                ? PrefabUtility.InstantiatePrefab(demoPrefab) as GameObject
                : null;
            if (demo == null)
            {
                Debug.LogError($"[LavaRushUIPackageMenu] Could not instantiate {DemoPrefabPath}.");
                return;
            }

            demo.name = "Lava Rush UI Demo";
            Undo.RegisterCreatedObjectUndo(demo, "Create Lava Rush UI Demo");
            Selection.activeGameObject = demo;
        }

        [MenuItem(MenuRoot + "README", false, 906)]
        private static void OpenReadme()
        {
            var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(ReadmePath);
            if (readme == null)
            {
                EditorUtility.DisplayDialog("Package README", $"README was not found.\n{ReadmePath}", "OK");
                return;
            }
            Selection.activeObject = readme;
            AssetDatabase.OpenAsset(readme);
        }
    }
}
#endif
