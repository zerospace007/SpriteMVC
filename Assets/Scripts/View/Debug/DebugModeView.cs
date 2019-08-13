using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug模式显示
/// </summary>
public class DebugModeView : MonoBehaviour
{
    private bool isShowLog;                             //是否显示日志,
    public KeyCode keyOpenLog = KeyCode.Space;          //按键打开/关闭日志
    public GUISkin guiSkin;                             //日志的OnGUI样式设定

    private Vector2 m_scroll = new Vector2(0, 0);
    private string m_logs;                              //日志内容
    private string m_listStr;                           //日志列表
    private string m_logListStr;                        //log列表
    private string m_WarnListStr;                       //Warn列表
    private string m_ErrorListStr;                      //Error列表
    private string m_ExceptionListStr;                  //Exception列表

    private void Awake()
    {
        guiSkin = Resources.Load<GUISkin>("DebugGUI");
    }

    internal void OnEnable()
    {
        isShowLog = true;                               //当脚本打开，是否可以显示日志， Log = True;//这个变量也必须为True
        Application.logMessageReceived += HandleLog;    //注册Unity的日志回调
    }

    internal void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;    //去掉Unity的日志回调
    }

    /// <summary>
    /// Unity日志回调
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string[] splitStr = stackTrace.Split('\n');
        var tempStr = string.Empty;
        var strLogItem = "【" + type.ToString() + "】:" + logString;
        switch (type)
        {//给日志类型加颜色
            case LogType.Assert:
                break;
            case LogType.Log:
            strLogItem = "<color=white>" + strLogItem + "</color>";
            m_logListStr += strLogItem + "\t\n";
            break;
            case LogType.Warning:
                strLogItem = "<color=yellow>" + strLogItem + "</color>";
                m_WarnListStr += strLogItem + "\t\n";
                break;
            case LogType.Error:
                strLogItem = "<color=red>" + strLogItem + "</color>";
                if (splitStr.Length >= 2) tempStr = splitStr[0] + "\t\n" + splitStr[1] + "\t\n";
                m_ErrorListStr += strLogItem + "\n"  + tempStr;
                break;
            case LogType.Exception:
                strLogItem = "<color=red>" + strLogItem + "</color>";
                if (splitStr.Length >= 2) tempStr = splitStr[0] + "\t\n" + splitStr[1] + "\t\n";
                m_ExceptionListStr += strLogItem + "\n" + tempStr;
                break;
            default:
                break;
        }
        m_listStr += strLogItem + "\t\n";
        m_logs = m_listStr;
    }

    private string m_TextInput;
    void OnGUI()
    {
        if (!GameConst.LogMode) return;
        if (GUI.Button(new Rect(0, 0, 50, 30), "Debug"))
        {
            isShowLog = !isShowLog;
        }
        if (!isShowLog) return;

        if (GUI.Button(new Rect(50, 0, 50, 30), "Clear"))
        {
            m_listStr = "";
            m_logListStr = "";
            m_WarnListStr = "";
            m_ErrorListStr = "";
            m_ExceptionListStr = "";
            m_logs = "";
        }
      
        if (GUI.Button(new Rect(100, 0, 50, 30), "All")) m_logs = m_listStr;
        if (GUI.Button(new Rect(150, 0, 50, 30), "Logs")) m_logs = m_logListStr;
        if (GUI.Button(new Rect(200, 0, 50, 30), "Warns")) m_logs = m_WarnListStr;
        if (GUI.Button(new Rect(250, 0, 50, 30), "Errors")) m_logs = m_ErrorListStr;
        if (GUI.Button(new Rect(300, 0, 80, 30), "Exceptions")) m_logs = m_ExceptionListStr;

        GUILayout.BeginArea(new Rect(0, 30, Screen.width, Screen.height - 60));
        m_scroll = GUILayout.BeginScrollView(m_scroll, guiSkin.GetStyle("scrollview"));
        GUILayout.Label(m_logs, guiSkin.GetStyle("textarea"));
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        m_TextInput = GUI.TextField(new Rect(0, Screen.height - 30, Screen.width - 80, 30), m_TextInput);
        if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 30, 80, 30), "Enter"))
        {
            if (m_TextInput == string.Empty) return;
            Debug.Log("Send GM Command in the fututre:" + m_TextInput);
            m_TextInput = string.Empty;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(keyOpenLog))
        {
            isShowLog = !isShowLog;
        }

        if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
        {
            if (m_TextInput == string.Empty) return;
            Debug.Log("Send GM Command in the fututre:" + m_TextInput);
            m_TextInput = string.Empty;
        }
    }
}
