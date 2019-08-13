--region ViewName.lua
--Date 2019/3/15
--Author NormanYang
--endregion

------------------------------------Framework 相关-----------------------------------------------
Util = Framework.Utility.Util;								--工具方法
LuaHelper = Framework.Utility.LuaHelper;					--Lua调用C#帮助
ByteBuffer = Framework.ByteBuffer;							--网络协议内容封装

ResourceManager = LuaHelper.GetResManager();				--资源管理
TipsManager = LuaHelper.GetTipsManager();					--Tips管理
UIManager = LuaHelper.GetUIManager();						--UI管理
NetManager = LuaHelper.GetNetManager();						--网络管理
PoolManager = LuaHelper.GetPoolManager();					--对象池管理

NormalCanvas = UIManager.NormalCanvas;						--界面显示画布
MapCanvas = UIManager.MapCanvas;							--世界地图画布
TipsCanvas = TipsManager.TipsCanvas;						--提示界面画布
GuideCanvas = UIManager.GuideCanvas;						--引导界面画布

------------------------------------第三方 比如DoTween---------------------------------------------------
DOTween = DG.Tweening.DOTween;
Tween = DG.Tweening.Tween;
Sequence = DG.Tweening.Sequence;
Tweener = DG.Tweening.Tweener;
RotateMode = DG.Tweening.RotateMode;
LoopType = DG.Tweening.LoopType;
Ease = DG.Tweening.Ease

TMP_SubMeshUI = TMPro.TMP_SubMeshUI;
TextMeshProUGUI = TMPro.TextMeshProUGUI;

------------------------------------Unity 相关---------------------------------------------------
RectTransform = UnityEngine.RectTransform					--位置信息等
Input = UnityEngine.Input;									--输入
Screen = UnityEngine.Screen;								--屏幕相关
Renderer = UnityEngine.Renderer;							--渲染
Shader = UnityEngine.Shader;								--Shader
ParticleSystem = UnityEngine.ParticleSystem;				--特效
EventSystem = UnityEngine.EventSystems.EventSystem;			--事件监测
Application = UnityEngine.Application;						--App信息
GameObject = UnityEngine.GameObject;						--游戏对象
AudioSource = UnityEngine.AudioSource;						--音效源
UnityWebRequest = UnityEngine.Networking.UnityWebRequest;	--Web请求
SpriteAtlasManager = UnityEngine.U2D.SpriteAtlasManager;	--SpriteAtlas管理

------------------------------------UGUI---------------------------------------------------
Text = UnityEngine.UI.Text;
Image = UnityEngine.UI.Image;
RawImage = UnityEngine.UI.RawImage;
Button = UnityEngine.UI.Button;
Toggle = UnityEngine.UI.Toggle;
Slider = UnityEngine.UI.Slider;
Scrollbar = UnityEngine.UI.Scrollbar;
Dropdown = UnityEngine.UI.Dropdown;
InputField = UnityEngine.UI.InputField;
ScrollRect = UnityEngine.UI.ScrollRect;
GridLayoutGroup = UnityEngine.UI.GridLayoutGroup;
Canvas = UnityEngine.Canvas;
Animator = UnityEngine.Animator;