using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class TransformEditor : Editor
{
    [InitializeOnLoadMethod]
    private static void IInitializeOnLoadMethod()
    {
        onPostion = delegate (Transform transform)
        {

            Debug.Log(string.Format("transform = {0}  positon = {1}", transform.name, transform.localPosition));
        };

        onRotation = delegate (Transform transform)
        {

            Debug.Log(string.Format("transform = {0}   rotation = {1}", transform.name, transform.localRotation.eulerAngles));
        };

        onScale = delegate (Transform transform)
        {

            Debug.Log(string.Format("transform = {0}   scale = {1}", transform.name, transform.localScale));
        };
    }

    public delegate void Change(Transform transform);
    public static Change onPostion;
    public static Change onRotation;
    public static Change onScale;

    private Editor editor;
    private Transform transform;
    private Vector3 startPostion = Vector3.zero;
    private Vector3 startRotation = Vector3.zero;
    private Vector3 startScale = Vector3.zero;

    private void OnEnable()
    {
        transform = target as Transform;
        editor = CreateEditor(target, Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.TransformInspector", true));
        startPostion = transform.localPosition;
        startRotation = transform.localRotation.eulerAngles;
        startScale = transform.localScale;
    }

    public override void OnInspectorGUI()
    {
        editor.OnInspectorGUI();
        if (GUI.changed)
        {
            if (startPostion != transform.localPosition)
            {
                onPostion?.Invoke(transform);
            }

            if (startRotation != transform.localRotation.eulerAngles)
            {
                onRotation?.Invoke(transform);
            }

            if (startScale != transform.localScale)
            {
                onScale?.Invoke(transform);
            }
            startPostion = transform.localPosition;
            startRotation = transform.localRotation.eulerAngles;
            startScale = transform.localScale;
        }
    }
}