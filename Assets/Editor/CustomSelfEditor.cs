#region

using UnityEditor;
using UnityEngine;

#endregion

public class CustomSelfEditor : ScriptableObject
{
    /// <summary>
    /// 点击，展开对象子孙树
    /// </summary>
    [MenuItem("GameObject/Expand the tree", false, 0)]
    public static void ExpandTree()
    {
        SetExpandedRecursive(Selection.activeGameObject, true);
    }

    public static void SetExpandedRecursive(GameObject gameObj, bool isExpand)
    {
        var type = typeof (EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var methodInfo = type.GetMethod("SetExpandedRecursive");

        EditorApplication.ExecuteMenuItem("Window/Hierarchy");
        var window = EditorWindow.focusedWindow;
        methodInfo.Invoke(window, new object[] {gameObj.GetInstanceID(), isExpand});
    }

    [MenuItem("Assets/Auto Open")]
    private static void Run()
    {
        var obj = Selection.activeObject;
        if (obj == null) return;
        if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj.GetInstanceID())))
        {
            AssetDatabase.OpenAsset(obj);
        }
    }

    [InitializeOnLoadMethod]
    private static void Start()
    {
        PrefabUtility.prefabInstanceUpdated = delegate
        {
            GameObject gameObj = null;
            if (Selection.activeTransform)
            {
                gameObj = Selection.activeGameObject;
            }
            AssetDatabase.SaveAssets();
            if (gameObj)
            {
                EditorApplication.delayCall = delegate { Selection.activeGameObject = gameObj; };
            }
        };
    }
}