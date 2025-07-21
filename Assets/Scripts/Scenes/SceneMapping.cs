using UnityEngine;
using System.Collections.Generic;

public class SceneMapping
{
    //Remember to check SceneName enum. A script I created as a global enum so everyone can access it. 
    private static readonly Dictionary<SceneName, string> sceneNameToString = new Dictionary<SceneName, string>
    {
        { SceneName.MainMenu, "MainMenu" },
        { SceneName.LevelMenu, "LevelMenu" },
        { SceneName.Level1, "Level 1" },
        { SceneName.TestLevel, "TestLevel" },
        { SceneName.Credits, "Credits" },
        { SceneName.Tutorial, "Tutorial" },
        // Add more scenes here
    };

    public static string GetSceneName(SceneName scene)
    {
        return sceneNameToString.TryGetValue(scene, out var name) ? name : null;
    }
}
