using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Clean BootPlayMode implementation
[InitializeOnLoad]
internal static class BootPlayMode
{
    private const string PrefOriginalScene = "BootPlayMode_OriginalScene";
    private const string PrefBootPath = "BootPlayMode_BootPath";
    private const string BootSceneName = "_Boot";
    private static string pendingOriginalPath;

    static BootPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                HandleExitingEditMode();
                break;
            case PlayModeStateChange.EnteredPlayMode:
                HandleEnteredPlayMode();
                break;
            case PlayModeStateChange.EnteredEditMode:
                HandleEnteredEditMode();
                break;
        }
    }

    private static void HandleExitingEditMode()
    {
        string bootPath = FindBootScenePath();
        if (string.IsNullOrEmpty(bootPath))
        {
            Debug.Log("BootPlayMode: scene '_Boot' not found — Play will behave normally.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("BootPlayMode: user canceled saving scenes — Play will behave normally.");
            return;
        }

        var active = EditorSceneManager.GetActiveScene();
        string activePath = active.path;
        if (string.IsNullOrEmpty(activePath))
        {
            Debug.LogWarning("BootPlayMode: active scene is not saved — cannot restore after Play.");
            return;
        }

        EditorPrefs.SetString(PrefOriginalScene, activePath);
        EditorPrefs.SetString(PrefBootPath, bootPath);

        Debug.Log($"BootPlayMode: saved original='{activePath}', boot='{bootPath}' to EditorPrefs.");

        var bootAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath);
        if (bootAsset != null)
        {
            EditorSceneManager.playModeStartScene = bootAsset;
            Debug.Log($"BootPlayMode: playModeStartScene set to '{bootPath}'.");
        }
    }

    private static void HandleEnteredPlayMode()
    {
        if (!EditorPrefs.HasKey(PrefOriginalScene))
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        string originalPath = EditorPrefs.GetString(PrefOriginalScene, string.Empty);
        if (string.IsNullOrEmpty(originalPath))
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        pendingOriginalPath = originalPath;
        Debug.Log($"BootPlayMode: EnteredPlayMode. pendingOriginalPath='{pendingOriginalPath}'");
        SceneManager.sceneLoaded += OnSceneLoadedInPlay;
        try
        {
            EditorSceneManager.LoadSceneInPlayMode(originalPath, new LoadSceneParameters(LoadSceneMode.Additive));
            Debug.Log($"BootPlayMode: requested additive load of '{originalPath}'.");
            // schedule a deferred unload attempt in case of timing issues
            EditorApplication.delayCall += TryUnloadBootDeferred;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"BootPlayMode: failed to load original scene in Play: {ex}");
            SceneManager.sceneLoaded -= OnSceneLoadedInPlay;
            pendingOriginalPath = null;
        }

        EditorSceneManager.playModeStartScene = null;
    }

    private static void OnSceneLoadedInPlay(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingOriginalPath))
            return;

        string expectedName = Path.GetFileNameWithoutExtension(pendingOriginalPath);
        if (!string.Equals(scene.path, pendingOriginalPath, System.StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(scene.name, expectedName, System.StringComparison.OrdinalIgnoreCase))
            return; // not the scene we requested

        try { SceneManager.SetActiveScene(scene); } catch { }
        // Diagnostic: list loaded scenes
        var loadedList = new System.Text.StringBuilder();
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var sc = SceneManager.GetSceneAt(i);
            loadedList.AppendFormat("[{0}] name='{1}' path='{2}' isLoaded={3}; ", i, sc.name, sc.path, sc.isLoaded);
        }
        Debug.Log("BootPlayMode: scenes after loading original: " + loadedList.ToString());

        // Try to unload any loaded scene that corresponds to _Boot.
        // Prefer the saved boot path (EditorPrefs) when available.
        string savedBootPath = EditorPrefs.GetString(PrefBootPath, string.Empty);
        bool unloadedAny = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var sc = SceneManager.GetSceneAt(i);
            if (!sc.IsValid() || !sc.isLoaded)
                continue;
            if (string.Equals(sc.path, scene.path, System.StringComparison.OrdinalIgnoreCase))
                continue; // don't unload the scene we just loaded

            string filename = Path.GetFileNameWithoutExtension(sc.path);
            bool matchBySavedPath = !string.IsNullOrEmpty(savedBootPath) && string.Equals(sc.path, savedBootPath, System.StringComparison.OrdinalIgnoreCase);
            bool matchByName = string.Equals(sc.name, BootSceneName, System.StringComparison.OrdinalIgnoreCase);
            bool matchByFilename = string.Equals(filename, BootSceneName, System.StringComparison.OrdinalIgnoreCase);
            bool matchByContains = (!string.IsNullOrEmpty(sc.name) && sc.name.IndexOf(BootSceneName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                   || (!string.IsNullOrEmpty(filename) && filename.IndexOf(BootSceneName, System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (matchBySavedPath || matchByName || matchByFilename || matchByContains)
            {
                SceneManager.UnloadSceneAsync(sc);
                Debug.Log($"BootPlayMode: unloading scene '{sc.name}' (path: {sc.path}) detected as Boot.");
                unloadedAny = true;
            }
        }

        if (!unloadedAny)
            Debug.LogWarning("BootPlayMode: did not find any loaded scene matching _Boot to unload.");

        pendingOriginalPath = null;
        if (EditorPrefs.HasKey(PrefBootPath))
            EditorPrefs.DeleteKey(PrefBootPath);
        SceneManager.sceneLoaded -= OnSceneLoadedInPlay;
    }

    private static void TryUnloadBootDeferred()
    {
        // run once
        EditorApplication.delayCall -= TryUnloadBootDeferred;

        if (string.IsNullOrEmpty(pendingOriginalPath))
            return;

        var orig = SceneManager.GetSceneByPath(pendingOriginalPath);
        if (!orig.IsValid() || !orig.isLoaded)
            return; // not loaded yet; OnSceneLoadedInPlay will handle it later

        string savedBootPath = EditorPrefs.GetString(PrefBootPath, string.Empty);
        bool unloadedAny = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            var sc = SceneManager.GetSceneAt(i);
            if (!sc.IsValid() || !sc.isLoaded)
                continue;
            if (string.Equals(sc.path, orig.path, System.StringComparison.OrdinalIgnoreCase))
                continue;

            string filename = Path.GetFileNameWithoutExtension(sc.path);
            bool matchBySavedPath = !string.IsNullOrEmpty(savedBootPath) && string.Equals(sc.path, savedBootPath, System.StringComparison.OrdinalIgnoreCase);
            bool matchByName = string.Equals(sc.name, BootSceneName, System.StringComparison.OrdinalIgnoreCase);
            bool matchByFilename = string.Equals(filename, BootSceneName, System.StringComparison.OrdinalIgnoreCase);

            if (matchBySavedPath || matchByName || matchByFilename)
            {
                SceneManager.UnloadSceneAsync(sc);
                Debug.Log($"BootPlayMode: (deferred) unloading scene '{sc.name}' (path: {sc.path}) detected as Boot.");
                unloadedAny = true;
            }
        }

        if (!unloadedAny)
            Debug.LogWarning("BootPlayMode: (deferred) did not find any loaded scene matching _Boot to unload.");

        pendingOriginalPath = null;
        if (EditorPrefs.HasKey(PrefBootPath))
            EditorPrefs.DeleteKey(PrefBootPath);
    }

    private static void HandleEnteredEditMode()
    {
        if (!EditorPrefs.HasKey(PrefOriginalScene))
            return;

        string originalPath = EditorPrefs.GetString(PrefOriginalScene, string.Empty);
        EditorPrefs.DeleteKey(PrefOriginalScene);
        if (EditorPrefs.HasKey(PrefBootPath))
            EditorPrefs.DeleteKey(PrefBootPath);
        if (string.IsNullOrEmpty(originalPath))
            return;

        EditorApplication.delayCall += () =>
        {
            try
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(originalPath) != null || File.Exists(Path.GetFullPath(originalPath)))
                {
                    EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
                    Debug.Log($"BootPlayMode: restored editor scene '{originalPath}' after Stop.");
                }
                else
                {
                    Debug.LogWarning($"BootPlayMode: original scene '{originalPath}' not found when restoring in Editor.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"BootPlayMode: failed to restore original scene in Editor: {ex}");
            }
        };
    }

    private static string FindBootScenePath()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string filename = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(filename, BootSceneName, System.StringComparison.OrdinalIgnoreCase))
                return path;
        }
        return null;
    }
}




