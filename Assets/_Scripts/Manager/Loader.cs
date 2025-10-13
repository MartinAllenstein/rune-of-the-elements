using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        MapScene,
        Level1,
        Level2
    }
    private static Scene targetScene;
    

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
        
    }

    public static void LoadNextLevel()
    {
        // หา Build Index ของ Scene ปัจจุบัน
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // ตรวจสอบว่ามีด่านต่อไปใน Build Settings หรือไม่
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // ถ้ามี ให้โหลดด่านถัดไป
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // ถ้าไม่มี (จบด่านสุดท้ายแล้ว) ให้กลับไปที่เมนูหลัก
            Load(Scene.MainMenuScene);
        }
    }
    
    public static void ReloadCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
