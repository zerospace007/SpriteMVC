using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(UICircle), true)]
[CanEditMultipleObjects]
public class UICircleInspector : RawImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UICircle circle = target as UICircle;
        circle.segments = Mathf.Clamp(EditorGUILayout.IntField("UICircle多边形", circle.segments), 4, 360);
    }
}