using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script
{
    public class BootLoader : MonoBehaviour
    {
        private const string BootSceneName = "_Boot";

        private void Start()
        {
            // If this is not running in the boot scene, do nothing
            if (SceneManager.GetActiveScene().name != BootSceneName) return;

            // Try to find the additive scene that isn't _Boot
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (!s.IsValid()) continue;
                if (s.name == BootSceneName) continue;

                // make the other scene active and unload boot
                SceneManager.SetActiveScene(s);
                SceneManager.UnloadSceneAsync(BootSceneName);
                return;
            }

            // fallback: try to read temp file created by the editor
            try
            {
                var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var fullPath = Path.Combine(projectPath, "Temp", "boot_target_scene.txt");
                if (File.Exists(fullPath))
                {
                    var scenePath = File.ReadAllText(fullPath).Trim();
                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        SceneManager.LoadScene(sceneName);
                        SceneManager.UnloadSceneAsync(BootSceneName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"BootLoader: Failed to load target scene from temp file: {e}");
            }
        }
    }
}


