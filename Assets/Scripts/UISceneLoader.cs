using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script provides functionality to load Unity scenes.
/// It is designed to be used with UGUI buttons or other UI events in a Meta Quest environment.
/// To use it, attach this script to a GameObject in your scene. Then, on your UGUI Button component,
/// add an OnClick event, drag the GameObject with this script to the object field, and select
/// the `UISceneLoader.LoadScene(string)` or `UISceneLoader.LoadScene(int)` method.
/// You can then specify the scene name or build index you want to load.
/// </summary>
public class UISceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by its name.
    /// This method can be assigned to a UGUI Button's OnClick event in the inspector.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load. The scene must be added to the Build Settings.</param>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[UISceneLoader] Scene name provided is empty or null. Cannot load scene.", this);
            return;
        }

        Debug.Log($"[UISceneLoader] Requesting to load scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads a scene by its build index.
    /// This method can also be used with UI events.
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to load. The scene must be added to the Build Settings.</param>
    public void LoadSceneByIndex(int sceneBuildIndex)
    {
        if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[UISceneLoader] Scene build index '{sceneBuildIndex}' is out of range. " +
                           $"Please ensure the scene is added to the Build Settings and the index is correct.", this);
            return;
        }

        Debug.Log($"[UISceneLoader] Requesting to load scene with build index: {sceneBuildIndex}");
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
