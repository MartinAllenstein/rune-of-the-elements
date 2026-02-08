using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        MapLevel1,
        MapLevel2,
        MapLevel3,
        MapLevel4,
        LobbyScene,
        CharacterSelectScene,
    }
    private static Scene targetScene;
    

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
        
    }

    public static void LoadNextLevel()
    {
        // Build Index from cur Scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check next scene in Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Load Next Scene
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                string nextSceneName = GetSceneNameFromBuildIndex(nextSceneIndex);
                NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
        else
        {
            // No more Levels
            Load(Scene.MainMenuScene);
        }
    }
    
    public static void ReloadCurrentLevel()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            NetworkManager.Singleton.SceneManager.LoadScene(currentSceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }


    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
    
    private static string GetSceneNameFromBuildIndex(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }
}
