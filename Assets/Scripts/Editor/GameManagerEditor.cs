using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);

        if (GUILayout.Button("Apply Runtime Visual Layout"))
        {
            ApplyToManager((GameManager)target);
        }
    }

    [MenuItem("Tools/RaiderGame/Apply Runtime Visual Layout")]
    private static void ApplyToOpenScene()
    {
        GameManager[] managers = Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include);

        if (managers.Length == 0)
        {
            Debug.LogWarning("GameManager not found in the open scene.");
            return;
        }

        for (int i = 0; i < managers.Length; i++)
            ApplyToManager(managers[i]);
    }

    private static void ApplyToManager(GameManager manager)
    {
        if (manager == null)
            return;

        Scene scene = manager.gameObject.scene;
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
            Undo.RegisterFullObjectHierarchyUndo(roots[i], "Apply Runtime Visual Layout");

        manager.ApplyRuntimeVisualLayout();

        for (int i = 0; i < roots.Length; i++)
            EditorUtility.SetDirty(roots[i]);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Applied runtime visual layout to the open scene.");
    }
}
