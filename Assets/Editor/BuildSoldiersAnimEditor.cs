using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BuildAnimation : Editor
{
    //生成出的Prefab的路径
    private static string PrefabPath = "Assets/ResExport/Soldiers";
    //生成出的SpriteAtlasd的路径
    private static string SpriteAtlasPath = "Assets/ResExport/AtlasSoldiers";
    //生成出的AnimationController的路径
    private static string AnimationControllerPath = "Assets/ResQuote/AnimationController/Soldiers";
    //生成出的Animation的路径
    private static string AnimationPath = "Assets/ResQuote/Animation/Soldiers";
    //美术给的原始图片路径
    private static string ImagePath = Application.dataPath + "/ResQuote/SequenceFrame/Soldiers";

    [MenuItem("Game/Build Soldiers Animaiton")]
    private static void BuildAniamtion()
    {
        DirectoryInfo imgePaths = new DirectoryInfo(ImagePath);
        foreach (DirectoryInfo dictory in imgePaths.GetDirectories())
        {
            //每个文件夹就是一个Animator，对应n个Animation动画
            var clips = BuildAnimationClips(dictory);
            //把所有的动画文件生成在一个AnimationController里
            var controller = BuildAnimationController(clips, dictory.Name);
            //最后生成程序用的Prefab文件
            BuildPrefab(dictory, controller);
            //生成SpriteAtlas
            BuildSpriteAtlas(dictory);
        }
    }

    /// <summary>
    /// 将某个士兵所有帧动画分拆，并生成n个Animation
    /// </summary>
    /// <param name="dictory"></param>
    /// <returns></returns>
    private static List<AnimationClip> BuildAnimationClips(DirectoryInfo dictory)
    {
        //查找所有图片，因为我找的测试动画是.jpg 
        FileInfo[] images = dictory.GetFiles("*.png");
        List<AnimationClip> animationClips = new List<AnimationClip>();
        Dictionary<string, List<FileInfo>> filesDic = new Dictionary<string, List<FileInfo>>();

        for (int node = 0, count = images.Length; node < count; node++)
        {
            var animationName = images[node].Name.Split('_')[0];
            if (filesDic.ContainsKey(animationName))
            {
                filesDic[animationName].Add(images[node]);
            }
            else
            {
                var tempList = new List<FileInfo>();
                tempList.Add(images[node]);
                filesDic[animationName] = tempList;
            }
        }

        foreach (var dicItem in filesDic)
        {
            animationClips.Add(BuildAnimationClip(dictory, dicItem.Value, dicItem.Key));
        }

        return animationClips;
    }

    /// <summary>
    /// 根据一组帧动画生成一个animation
    /// </summary>
    /// <param name="dictory"></param>
    /// <param name="images"></param>
    /// <param name="animationName"></param>
    /// <returns></returns>
    private static AnimationClip BuildAnimationClip(DirectoryInfo dictory, List<FileInfo> images, string animationName)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 24f;  //动画帧率，这里统一用24帧

        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(Image);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Count];
        //这里的keyFrames[node].time是float类型，而float的精度会影响到sprite的切换，解决方法就是使用double类型计算time。。。。
        //动画长度是按秒为单位，1/12就表示1秒切12张图片，根据项目的情况可以自己调节
        double frameTime = 1 / 12d;
        for (int node = 0; node < images.Count; node++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[node].FullName));
            keyFrames[node] = new ObjectReferenceKeyframe();
            keyFrames[node].time = (float)frameTime * node;
            keyFrames[node].value = sprite;
        }

        if (!((animationName.IndexOf("die", StringComparison.OrdinalIgnoreCase) >= 0) 
            || (animationName.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0)))
        {
            //设置为循环动画（除了死亡die和攻击attack）
            SerializedObject serializedClip = new SerializedObject(clip);
            AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
            clipSettings.loopTime = true;
            serializedClip.ApplyModifiedProperties();
        }

        var dictoryPath = AnimationPath + "/" + dictory.Name;
        if (!Directory.Exists(dictoryPath))
            Directory.CreateDirectory(dictoryPath);

        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, dictoryPath + "/" + animationName + ".anim");
        AssetDatabase.SaveAssets();
        return clip;
    }

    /// <summary>
    /// 根据一组animation生成一个Animator
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private static AnimatorController BuildAnimationController(List<AnimationClip> clips, string name)
    {
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" + name + ".controller");
        AnimatorControllerLayer layer = animatorController.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;
        foreach (AnimationClip newClip in clips)
        {
            var conditionName = newClip.name;
            animatorController.AddParameter(conditionName, AnimatorControllerParameterType.Trigger);

            AnimatorState state = stateMachine.AddState(newClip.name);
            state.motion = newClip;
            if (newClip.name.Equals("idle1"))
            {
                stateMachine.defaultState = state;
            }
            AnimatorStateTransition trans = stateMachine.AddAnyStateTransition(state);
            trans.hasExitTime = false;
            trans.AddCondition(AnimatorConditionMode.If, 0, conditionName);
        }
        AssetDatabase.SaveAssets();
        return animatorController;
    }

    /// <summary>
    /// 生成一个预制体
    /// </summary>
    /// <param name="dictory"></param>
    /// <param name="animatorCountroller"></param>
    private static void BuildPrefab(DirectoryInfo dictory, AnimatorController animatorCountroller)
    {
        //生成Prefab 添加一张预览用的Sprite
        FileInfo pngImage = dictory.GetFiles("*.png")[0];
        GameObject gameObj = new GameObject();
        gameObj.name = dictory.Name;
        Image image = gameObj.AddComponent<Image>();
        string path = DataPathToAssetPath(pngImage.FullName);
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        image.SetNativeSize();

        Animator animator = gameObj.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorCountroller;
        PrefabUtility.SaveAsPrefabAsset(gameObj, PrefabPath + "/" + gameObj.name + ".prefab");
        DestroyImmediate(gameObj);
    }

    /// <summary>
    /// 生成一个SpriteAtlas
    /// </summary>
    /// <param name="dictory"></param>
    private static void BuildSpriteAtlas(DirectoryInfo dictory)
    {
        var spriteAtlasName = SpriteAtlasPath + "/" + dictory.Name + "_Atlas.spriteAtlas";
        SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasName);
        if (null == spriteAtlas)
        {
            spriteAtlas = new SpriteAtlas();
            spriteAtlas.SetIncludeInBuild(false);
            var packingSettings = spriteAtlas.GetPackingSettings();
            packingSettings.enableRotation = false;
            packingSettings.enableTightPacking = false;
            spriteAtlas.SetPackingSettings(packingSettings);
            var path = DataPathToAssetPath(dictory.FullName);
            var folderObj = AssetDatabase.LoadMainAssetAtPath(path);
            var objectArr = new Object[] { folderObj };
            spriteAtlas.Add(objectArr);
            AssetDatabase.CreateAsset(spriteAtlas, spriteAtlasName);
        }
    }

    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return path.Substring(path.IndexOf("Assets\\"));
        else
            return path.Substring(path.IndexOf("Assets/"));
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