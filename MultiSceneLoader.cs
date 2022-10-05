using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLoader : Editor
{
    private const string path = "Assets/Scenes/Beograd";

    [MenuItem("Scene/Open Beograd Stable")]
    private static void OpenBeogradGameplayStable()
    {
        CloseAllScenes();
        OpenScenes(new[] {
            ("Environment"   , false),
            ("GameplayStable", true),
        });
    }

    [MenuItem("Scene/Open Beograd Experimental (NO GUS ALLOWED)")]
    private static void OpenBeogradGameplay()
    {
        CloseAllScenes();
        OpenScenes(new[] {
            ("Environment", false),
            ("Gameplay"   , true),
        });
    }

    private static void OpenScenes((string name, bool canPick)[] sceneInfos)
    {
        for (int i = 0; i < sceneInfos.Length; i++)
        {
            var sceneInfo = sceneInfos[i];
            var mode = i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive; // first should replace leftover scene

            var scene = EditorSceneManager.OpenScene($"{path}/{sceneInfo.name}.unity", mode);
            if (sceneInfo.canPick)
            {
                SceneVisibilityManager.instance.EnablePicking(scene);
                SetExpanded(scene, true);
            }
            else
            {
                SceneVisibilityManager.instance.DisablePicking(scene);
                SetExpanded(scene, false);
            }
        }
    }

    private static void CloseAllScenes()
    {
        int count = EditorSceneManager.sceneCount-1; // can't unload all
        for (int i = 0; i < count; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void SetExpanded(Scene scene, bool expand)
    {
        foreach (var window in Resources.FindObjectsOfTypeAll<SearchableEditorWindow>())
        {
            if (window.GetType().Name != "SceneHierarchyWindow")
                continue;

            var method = window.GetType().GetMethod("SetExpanded",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance, null,
                new[] { typeof(int), typeof(bool) }, null);

            if (method == null)
            {
                Debug.LogError(
                    "Could not find method 'UnityEditor.SceneHierarchyWindow.SetExpanded(int, bool)'.");
                return;
            }

            var field = scene.GetType().GetField("m_Handle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogError("Could not find field 'int UnityEngine.SceneManagement.Scene.m_Handle'.");
                return;
            }

            var sceneHandle = field.GetValue(scene);
            method.Invoke(window, new[] { sceneHandle, expand });
        }
    }
}
