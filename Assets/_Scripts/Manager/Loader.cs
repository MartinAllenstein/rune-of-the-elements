using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        MapScene1,
        MapScene2,
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
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // No more Levels
            Load(Scene.MainMenuScene);
        }
    }
    
    public static void ReloadCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
