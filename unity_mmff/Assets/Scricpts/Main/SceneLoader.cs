using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    [System.Serializable]
    public class SceneData
    {
        public string sceneName;
        [TextArea(1, 2)]
        public string description;
    }

    public string game_name;

    [Header("Scene List")]
    public List<SceneData> scenes = new List<SceneData>();

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("No Scene Name");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    [ContextMenu("Show All Scenes")]
    void ShowAllScenes()
    {
        Debug.Log("=== Scene List ===");
        for (int i = 0; i < scenes.Count; i++)
        {
            Debug.Log($"[{i}] {scenes[i].sceneName} - {scenes[i].description}");
        }
    }

    public void Continue()
    {   
        game_name = PlayerPrefs.GetString("game_name");
        SceneManager.LoadScene(game_name);
    }
}