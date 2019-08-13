using UnityEngine;

/// <summary>
/// </summary>
public class GameEntry : MonoBehaviour
{
    #region Methods

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Screen.SetResolution(576, 1024, false);
        }

        Application.targetFrameRate = GameConst.GameFrameRate;
        Application.runInBackground = true;
        Input.multiTouchEnabled = true;
        Screen.autorotateToPortrait = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //初始化框架，实例化游戏管理器
        GameFacade.Instance.StartUp();

        Destroy(gameObject);
    }

    #endregion
}