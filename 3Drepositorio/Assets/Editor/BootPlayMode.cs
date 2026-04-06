using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class BootPlayMode
    {
    private const string BootSceneName = "_Boot";
    private static readonly string TempFilePath = Path.Combine(Application.dataPath, "..", "Temp", "boot_target_scene.txt");

    static BootPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // about to enter Play Mode from Edit Mode
            var active = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            // If the active scene is already the boot scene, do nothing
            if (active.name == BootSceneName) return;

            // find boot scene path in project
            var bootPath = FindScenePathByName(BootSceneName);
            if (string.IsNullOrEmpty(bootPath))
            {
                Debug.LogWarning($"BootPlayMode: Could not find scene '{BootSceneName}' in project.");
                return;
            }

            // Ask user to save modified scenes before switching
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // user cancelled the save / play
                EditorApplication.isPlaying = false;
                return;
            }

            var originalPath = active.path;

            // write a small temporary file so runtime has a fallback if needed
            try
            {
                File.WriteAllText(TempFilePath, originalPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"BootPlayMode: Unable to write temp file: {e.Message}");
            }

            // Also set Unity's Play Mode start scene so the Play session begins in the boot scene
            var bootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath);
            if (bootSceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = bootSceneAsset;
            }

            // Open boot scene first (single) then re-open the original scene additively in the editor
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Additive);

            // make sure boot scene is the active scene so it runs first and is visible in the Hierarchy
            // schedule on next editor loop to avoid timing issues when scenes finish opening
            EditorApplication.delayCall += () =>
            {
                // first try by path
                var bootScene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(bootPath);
                // fallback: find by name
                if (!bootScene.IsValid())
                {
                    for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
                    {
                        var s = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
                        if (s.IsValid() && s.name == BootSceneName)
                        {
                            bootScene = s;
                            break;
                        }
                    }
                }

                if (bootScene.IsValid())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(bootScene);
                    // also select the root objects so Hierarchy view focuses on it
                    var roots = bootScene.GetRootGameObjects();
                    if (roots != null && roots.Length > 0)
                    {
                        UnityEditor.Selection.objects = roots;
                        // Repaint the Hierarchy window so the selection/active scene is visible
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }
            };
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // cleanup temp file when returning to edit mode
            try
            {
                if (File.Exists(TempFilePath)) File.Delete(TempFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"BootPlayMode: Failed to remove temp file: {e}");
            }

            // clear the playModeStartScene so subsequent Play sessions use the normal start scene
            try
            {
                EditorSceneManager.playModeStartScene = null;
            }
            catch { }
        }
    }

    private static string FindScenePathByName(string sceneName)
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (Path.GetFileNameWithoutExtension(path) == sceneName) return path;
        }
        return null;
    }
}



