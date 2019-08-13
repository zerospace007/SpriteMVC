#region 命名空间
using System;
using System.Collections.Generic;
using Framework;
using Framework.Core;
using Framework.Network;
using Framework.Utility;
using LuaInterface;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;
using WorldMap;
using BindType = ToLuaMenu.BindType;
#endregion

public static class CustomSettings
{
    public static string luaDir = Application.dataPath + "/Lua/";
    public static string baseLuaDir = Application.dataPath + "/ToLua/Lua";

    public static string saveDir = Application.dataPath + "/ToLua/Source/Generate/";
    public static string toluaBaseType = Application.dataPath + "/ToLua/BaseType/";
	public static string injectionFilesPath = Application.dataPath + "/ToLua/Injection/";

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(Application),
        typeof(Time),
        typeof(Screen),
        typeof(SleepTimeout),
        typeof(Input),
        typeof(SpriteAtlasManager),
        //typeof(EventSystem),
        typeof(Resources),
        typeof(Physics),
        typeof(RenderSettings),
        typeof(QualitySettings),
        typeof(GL),
        typeof(Graphics),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action)),                
        _DT(typeof(UnityEngine.Events.UnityAction)),
        _DT(typeof(Predicate<int>)),
        _DT(typeof(Action<int>)),
        _DT(typeof(Comparison<int>)),
        _DT(typeof(Func<int, int>)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {
        _GT(typeof(LuaInjectionStation)),
        _GT(typeof(InjectType)),
        _GT(typeof(Debugger)).SetNameSpace(null),

        //for LuaPerfect
        _GT(typeof(LuaPerfect.ObjectRef)),
        _GT(typeof(LuaPerfect.ObjectItem)),
        _GT(typeof(LuaPerfect.ObjectFormater)),

        _GT(typeof(AudioClip)),
        _GT(typeof(AudioBehaviour)),

#if USING_DOTWEENING
        _GT(typeof(DG.Tweening.DOTween)),
        _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)),
        _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.Ease)),
        _GT(typeof(DG.Tweening.LoopType)),
        _GT(typeof(DG.Tweening.PathMode)),
        _GT(typeof(DG.Tweening.PathType)),
        _GT(typeof(DG.Tweening.RotateMode)),
        _GT(typeof(Component)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Transform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(Light)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Material)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Rigidbody)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Camera)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioSource)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(LineRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(TrailRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),

        //for ugui
        _GT(typeof(Text)).AddExtendType(typeof(DG.Tweening.DOTweenModuleUI)),
        _GT(typeof(Image)).AddExtendType(typeof(DG.Tweening.DOTweenModuleUI)),
#else
        _GT(typeof(Component)),
        _GT(typeof(Transform)),
        _GT(typeof(Material)),
        //_GT(typeof(Light)),
        _GT(typeof(Rigidbody)),
        _GT(typeof(Camera)),
        _GT(typeof(AudioSource)),
        //_GT(typeof(LineRenderer))
        //_GT(typeof(TrailRenderer))
#endif
      
        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),
        _GT(typeof(GameObject)),
        _GT(typeof(TrackedReference)),
        _GT(typeof(Physics)),
        _GT(typeof(Collider)),
        _GT(typeof(Texture)),
        _GT(typeof(Texture2D)),
        _GT(typeof(Shader)),
        _GT(typeof(Renderer)),

        _GT(typeof(CameraClearFlags)),
        _GT(typeof(EventSystem)),
        _GT(typeof(ParticleSystem)),
        _GT(typeof(GraphicRaycaster)),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)),
        _GT(typeof(LightType)),

        _GT(typeof(Resources)),
        _GT(typeof(SleepTimeout)),
        _GT(typeof(Time)),
        _GT(typeof(Screen)),
        _GT(typeof(Input)),
        _GT(typeof(Application)),
        _GT(typeof(SpriteAtlasManager)),

        _GT(typeof(KeyCode)),
        _GT(typeof(SkinnedMeshRenderer)),
        _GT(typeof(Space)),

        _GT(typeof(MeshRenderer)),
        _GT(typeof(BoxCollider)),
        _GT(typeof(MeshCollider)),
        _GT(typeof(SphereCollider)),
        _GT(typeof(CharacterController)),
        _GT(typeof(CapsuleCollider)),

        _GT(typeof(Animator)),
        _GT(typeof(Animation)),
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)),
        _GT(typeof(AnimationState)),
        _GT(typeof(AnimationBlendMode)),
        _GT(typeof(QueueMode)),
        _GT(typeof(PlayMode)),
        _GT(typeof(WrapMode)),

        //_GT(typeof(QualitySettings)),
        _GT(typeof(RenderSettings)),
        _GT(typeof(BlendWeights)),
        _GT(typeof(RenderTexture)),
        _GT(typeof(LuaProfiler)),

        //for UnityWebRequest
        _GT(typeof(UnityWebRequestAsyncOperation)),
        _GT(typeof(DownloadHandlerBuffer)),
        _GT(typeof(DownloadHandler)),
        _GT(typeof(UnityWebRequest)),

        //for UGUI
        _GT(typeof(Rect)),
        _GT(typeof(RectTransform)),
        _GT(typeof(UIBehaviour)),
        _GT(typeof(MaskableGraphic)),
        _GT(typeof(Selectable)),
        //_GT(typeof(Text)),
        //_GT(typeof(Image)),
        _GT(typeof(Canvas)),
        _GT(typeof(InputField)),
        _GT(typeof(RawImage)),
        _GT(typeof(Button)),
        _GT(typeof(Slider)),
        _GT(typeof(Toggle)),
        _GT(typeof(Sprite)),
        _GT(typeof(SpriteAtlas)),
        _GT(typeof(GridLayoutGroup)),

        //for TextMesh Pro
        _GT(typeof(TextMeshProUGUI)).SetBaseType(typeof(TMP_Text)),
        _GT(typeof(TextMeshPro)).SetBaseType(typeof(TMP_Text)),
        _GT(typeof(TMP_SubMeshUI)),
        _GT(typeof(TMP_InputField)),
        _GT(typeof(TMP_Dropdown)),

         //for Framework
        _GT(typeof(Util)),
        _GT(typeof(LayerSetting)),
        _GT(typeof(GameConst)),
        _GT(typeof(LuaHelper)),
        _GT(typeof(ByteBuffer)),
        _GT(typeof(View)),
        _GT(typeof(LuaListItem)),
        _GT(typeof(LuaBehaviour)),
        _GT(typeof(WorldMapView)),
        _GT(typeof(Guide)),

        _GT(typeof(GameObjectPool)),
        _GT(typeof(PoolManager)),
        _GT(typeof(GameManager)),
        _GT(typeof(LuaManager)),
        _GT(typeof(TipsManager)),
        _GT(typeof(UIManager)),
        _GT(typeof(Framework.NetworkManager)),
        _GT(typeof(ResourceManager)),		  
    };

    public static List<Type> dynamicList = new List<Type>()
    {
        typeof(MeshRenderer),
        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),

        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),

        typeof(BlendWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };
        
    //ngui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {
        /*typeof(Transform),
        typeof(UIRoot),
        typeof(UICamera),
        typeof(UIViewport),
        typeof(UIPanel),
        typeof(UILabel),
        typeof(UIAnchor),
        typeof(UIAtlas),
        typeof(UIFont),
        typeof(UITexture),
        typeof(UISprite),
        typeof(UIGrid),
        typeof(UITable),
        typeof(UIWrapGrid),
        typeof(UIInput),
        typeof(UIScrollView),
        typeof(UIEventListener),
        typeof(UIScrollBar),
        typeof(UICenterOnChild),
        typeof(UIScrollView),        
        typeof(UIButton),
        typeof(UITextList),
        typeof(UIPlayTween),
        typeof(UIDragScrollView),
        typeof(UISpriteAnimation),
        typeof(UIWrapContent),
        typeof(TweenWidth),
        typeof(TweenAlpha),
        typeof(TweenColor),
        typeof(TweenRotation),
        typeof(TweenPosition),
        typeof(TweenScale),
        typeof(TweenHeight),
        typeof(TypewriterEffect),
        typeof(UIToggle),
        typeof(Localization),*/
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    


    [MenuItem("Lua/Attach Profiler", false, 151)]
    private static void AttachProfiler()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "请在运行时执行此功能", "确定");
            return;
        }

        LuaClient.Instance.AttachProfiler();
    }

    [MenuItem("Lua/Detach Profiler", false, 152)]
    private static void DetachProfiler()
    {
        if (!Application.isPlaying) return;
        LuaClient.Instance.DetachProfiler();
    }
}
