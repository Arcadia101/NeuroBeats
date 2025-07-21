using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScenesManager : MonoBehaviour
{
    public static GameScenesManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;    
            
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
    //Remember to check SceneName enum. A script I created as a global enum so everyone can access it. 
    // Every time you add a new scene to the builder, add it to the SceneName, and SceneMapping dictionary.

    public void ChangeScene(SceneName sceneEnum)
    {
        if (GameManager.Instance == null)
        {  
            string sceneName = SceneMapping.GetSceneName(sceneEnum);

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"Scene name for {sceneEnum} is not defined!");
                return;
            }

            if (SceneManager.GetActiveScene().name != sceneName)
            {
                Debug.Log($"Changing scene to: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            
        }
    }
}
