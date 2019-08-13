using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 根据sprites图集自动生成序列帧动画
/// </summary>
public class BuildAnimationEditor : Editor
{
    [MenuItem("Assets/BuildSpriteRenderAnimation")]
    public static void BuildSpriteRenderAnimation()
    {
        BuildAnimation(true, typeof(SpriteRenderer));
    }

    [MenuItem("Assets/BuildImageAnimation")]
    public static void BuildImageAnimation()
    {
        BuildAnimation(true, typeof(Image));
    }

    public static void BuildAnimation(bool isLoop, System.Type curveBindingType)
    {
        Object[] array = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);

        foreach (Object obj in array)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                .Select(x => x as Sprite).Where(x => x != null).ToArray();

            if (sprites == null || sprites.Length == 0)
                continue;

            string directoryName = Path.GetDirectoryName(path);
            string animationName = Path.GetFileNameWithoutExtension(path);
            if (curveBindingType == typeof(Image))
                animationName = "UI-" + animationName;

            //这里是否循环的参数我写死了为true，可以根据自己的需要进行修改.
            AnimationClip clip = BuildAnimationClip(directoryName, animationName, sprites, true, curveBindingType);
            List<AnimationClip> clips = new List<AnimationClip>();
            clips.Add(clip);

            AnimatorController controller = BuildAnimationController(clips, directoryName, animationName);
            BuildPrefab(directoryName, animationName, controller, sprites[0], curveBindingType);
        }
    }

    static AnimationClip BuildAnimationClip(string directoryPath, string animationName, Sprite[] sprites, bool isLoop, System.Type curveBindingType)
    {
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = curveBindingType;
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Length];

        //动画帧率，这里统一用24帧
        clip.frameRate = 24f;

        //这里的keyFrames[node].time是float类型，而float的精度会影响到sprite的切换，解决方法就是使用double类型计算time。。。。
        //动画长度是按秒为单位，1/10就表示1秒切10张图片，根据项目的情况可以自己调节
        double frameTime = 1d / 24d;
        for (int node = 0; node < sprites.Length; node++)
        {
            Sprite sprite = sprites[node];
            keyFrames[node] = new ObjectReferenceKeyframe();
            keyFrames[node].time = (float)(frameTime * node);
            keyFrames[node].value = sprite;
        }

        if (curveBindingType == typeof(Image))
        {
            //如果是Image动画，要额外设置宽高、pivot等属性
            BuildImageAnimationClip(clip, sprites, frameTime);
        }

        //有些动画我希望天生它就动画循环
        if (isLoop)
        {
            SerializedObject serializedClip = new SerializedObject(clip);
            AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
            clipSettings.loopTime = true;
            serializedClip.ApplyModifiedProperties();
        }

        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, directoryPath + "/" + animationName + ".anim");
        AssetDatabase.SaveAssets();

        return clip;
    }

    private static void BuildImageAnimationClip(AnimationClip clip, Sprite[] sprites, double frameTime)
    {
        EditorCurveBinding sizeDelta_x_curveBinding = new EditorCurveBinding();
        sizeDelta_x_curveBinding.type = typeof(RectTransform);
        sizeDelta_x_curveBinding.path = "";
        sizeDelta_x_curveBinding.propertyName = "m_SizeDelta.x";

        EditorCurveBinding sizeDelta_y_curveBinding = new EditorCurveBinding();
        sizeDelta_y_curveBinding.type = typeof(RectTransform);
        sizeDelta_y_curveBinding.path = "";
        sizeDelta_y_curveBinding.propertyName = "m_SizeDelta.y";

        EditorCurveBinding pivot_x_curveBinding = new EditorCurveBinding();
        pivot_x_curveBinding.type = typeof(RectTransform);
        pivot_x_curveBinding.path = "";
        pivot_x_curveBinding.propertyName = "m_Pivot.x";

        EditorCurveBinding pivot_y_curveBinding = new EditorCurveBinding();
        pivot_y_curveBinding.type = typeof(RectTransform);
        pivot_y_curveBinding.path = "";
        pivot_y_curveBinding.propertyName = "m_Pivot.y";

        Keyframe[] sizeDelta_x_keyframes = new Keyframe[sprites.Length];
        Keyframe[] sizeDelta_y_keyframes = new Keyframe[sprites.Length];
        Keyframe[] pivot_x_keyframes = new Keyframe[sprites.Length];
        Keyframe[] pivot_y_keyframes = new Keyframe[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            sizeDelta_x_keyframes[i] = new Keyframe((float)(frameTime * i), sprites[i].rect.width);
            sizeDelta_y_keyframes[i] = new Keyframe((float)(frameTime * i), sprites[i].rect.height);

            float x_pivot = sprites[i].pivot.x / sprites[i].rect.width;
            float y_pivot = sprites[i].pivot.y / sprites[i].rect.height;
            pivot_x_keyframes[i] = new Keyframe((float)(frameTime * i), x_pivot);
            pivot_y_keyframes[i] = new Keyframe((float)(frameTime * i), y_pivot);
        }

        AnimationCurve sizeDelta_x_curve = new AnimationCurve(sizeDelta_x_keyframes);
        AnimationCurve sizeDelta_y_curve = new AnimationCurve(sizeDelta_y_keyframes);
        AnimationCurve pivot_x_curve = new AnimationCurve(pivot_x_keyframes);
        AnimationCurve pivot_y_curve = new AnimationCurve(pivot_y_keyframes);

        for (int i = 0; i < sprites.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(sizeDelta_x_curve, i, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(sizeDelta_x_curve, i, AnimationUtility.TangentMode.Constant);

            AnimationUtility.SetKeyLeftTangentMode(sizeDelta_y_curve, i, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(sizeDelta_y_curve, i, AnimationUtility.TangentMode.Constant);

            AnimationUtility.SetKeyLeftTangentMode(pivot_x_curve, i, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(pivot_x_curve, i, AnimationUtility.TangentMode.Constant);

            AnimationUtility.SetKeyLeftTangentMode(pivot_y_curve, i, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(pivot_y_curve, i, AnimationUtility.TangentMode.Constant);
        }

        AnimationUtility.SetEditorCurve(clip, sizeDelta_x_curveBinding, sizeDelta_x_curve);
        AnimationUtility.SetEditorCurve(clip, sizeDelta_y_curveBinding, sizeDelta_y_curve);
        AnimationUtility.SetEditorCurve(clip, pivot_x_curveBinding, pivot_x_curve);
        AnimationUtility.SetEditorCurve(clip, pivot_y_curveBinding, pivot_y_curve);
    }

    static AnimatorController BuildAnimationController(List<AnimationClip> clips, string directoryName, string name)
    {
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(directoryName + "/" + name + ".controller");
        AnimatorControllerLayer layer = animatorController.layers[0];
        AnimatorStateMachine asm = layer.stateMachine;
        foreach (AnimationClip newClip in clips)
        {
            AnimatorState state = asm.AddState(newClip.name);
            state.motion = newClip;
            AnimatorStateTransition trans = asm.AddAnyStateTransition(state);
            trans.hasExitTime = false;
        }
        AssetDatabase.SaveAssets();
        return animatorController;
    }

    static void BuildPrefab(string directoryName, string goName, AnimatorController animatorCountorller, Sprite defaultSprite, System.Type curveBindingType)
    {
        //生成Prefab 添加一张预览用的Sprite
        GameObject go = new GameObject(goName);

        if (curveBindingType == typeof(SpriteRenderer))
        {
            SpriteRenderer spriteRender = go.AddComponent<SpriteRenderer>();
            spriteRender.sprite = defaultSprite;
            spriteRender.sortingLayerName = "Effect";
        }
        else if (curveBindingType == typeof(Image))
        {
            Image image = go.AddComponent<Image>();
            image.sprite = defaultSprite;
            image.rectTransform.sizeDelta = new Vector2(defaultSprite.rect.width, defaultSprite.rect.height);

            float x_pivot = defaultSprite.pivot.x / defaultSprite.rect.width;
            float y_pivot = defaultSprite.pivot.y / defaultSprite.rect.height;
            image.rectTransform.pivot = new Vector2(x_pivot, y_pivot);
        }

        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorCountorller;
        animator.cullingMode = AnimatorCullingMode.CullCompletely;

        PrefabUtility.SaveAsPrefabAsset(go, directoryName + "/" + goName + ".prefab");
        DestroyImmediate(go);
    }

    private class AnimationClipSettings
    {
        private SerializedProperty m_Property;

        private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

        public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }

        public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
        public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
        public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
        public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
        public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }

        public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
        public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
        public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
        public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
        public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
        public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
        public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
        public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
        public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
        public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
    }

}