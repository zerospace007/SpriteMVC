using DG.Tweening;
using Framework.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Framework
{
    /// <summary>
    /// Tips管理类
    /// </summary>
    public class TipsManager : MonoBehaviour
    {
        private const int TipsCount = 15;                   //Tips数量
        private const string TipsPoolName = "NormalTips";   //Tips对象池名称
        private const string ReducePoolName = "ReduceTips"; //减血Tips对象池名称
        private const string IconPoolName = "IconTips";     //带IconTips对象池名称
        private GameObjectPool m_TipsPool;                  //Tips对象池
        private GameObjectPool m_ReducePool;                //减血Tips对象池名称
        private GameObjectPool m_IconPool;                  //带有Icon的tips对象池
        private SpriteAtlas m_SpriteAtlas;                  //Icon sprite atlas
        private Transform m_TipsCanvas;                     //Tips画布

        /// <summary>
        /// gui根摄像机
        /// </summary>
        public Transform TipsCanvas
        {
            get
            {
                if (m_TipsCanvas == null)
                {
                    GameObject gameObj = GameObject.FindWithTag("TipsCanvas");
                    if (gameObj != null) m_TipsCanvas = gameObj.transform;
                }
                return m_TipsCanvas;
            }
        }

        private void Awake()
        {
            var normalTips = Resources.Load<GameObject>("Tips/" + TipsPoolName);
            var ReduceTips = Resources.Load<GameObject>("Tips/" + ReducePoolName);
            var poolManager = Singleton.GetInstance<PoolManager>();
            m_TipsPool = poolManager.CreatePool(TipsPoolName, normalTips, TipsCount);
            m_ReducePool = poolManager.CreatePool(ReducePoolName, ReduceTips, TipsCount);
        }

        public void OnInit()
        {
            Singleton.GetInstance<ResourceManager>().LoadPrefab("atlas/tipsatlas", "tipsatlas", OnLoadIconAtlas);
            Singleton.GetInstance<ResourceManager>().LoadPrefab("ui/icontips", "icontips", OnLoadIconPrefab);
            void OnLoadIconAtlas(Object[] objects)
            {
                if (objects.Length == 0) return;
                m_SpriteAtlas = objects[0] as SpriteAtlas;
            }
            void OnLoadIconPrefab(Object[] objects)
            {
                if (objects.Length == 0) return;
                GameObject prefab = objects[0] as GameObject;
                if (prefab == null) return;

                m_IconPool = Singleton.GetInstance<PoolManager>().CreatePool(IconPoolName, prefab, TipsCount);
            }
        }
        /// <summary>
        /// 显示一个Tips
        /// </summary>
        /// <param name="content">文本内容</param>
        /// <param name="centerX">中心点坐标X</param>
        /// <param name="centerY">中心点坐标Y</param>
        public void ShowTips(string content, float centerX = 0f, float centerY = 0f)
        {
            var pos = new Vector3(centerX, centerY, 0f);
            ShowTips(content, pos);
        }

        /// <summary>
        /// 显示一个Tips
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="pos">位置</param>
        /// <param name="y">y偏移</param>
        public void ShowTips(string content, Vector3 pos, float y = 150f)
        {
            var txtObject = m_TipsPool.GetGameObject();
            var tipsText = txtObject.GetComponentInChildren<Text>();
            Util.SetParent(TipsCanvas, txtObject.transform);

            tipsText.text = content;
            txtObject.SetActive(true);

            txtObject.transform.localRotation = Quaternion.identity;
            txtObject.transform.localScale = Vector3.one;
            txtObject.transform.localPosition = new Vector3(pos.x, pos.y - y, 0);

            var color = tipsText.color;
            color.a = 0;
            tipsText.color = color;
            Sequence mySequence = DOTween.Sequence();
            Tweener move1 = txtObject.transform.DOLocalMoveY(pos.y, 0.4f);
            Tweener move2 = txtObject.transform.DOLocalMoveY(pos.y + y, 0.4f);
            Tweener alpha1 = tipsText.DOColor(new Color(color.r, color.g, color.b, 1), 0.4f);
            Tweener alpha2 = tipsText.DOColor(new Color(color.r, color.g, color.b, 0), 0.4f);
            mySequence.Append(move1);
            mySequence.Join(alpha1);
            mySequence.AppendInterval(1);
            mySequence.Append(move2);
            mySequence.Join(alpha2);

            mySequence.OnComplete(delegate ()
            {
                mySequence.Kill();
                m_TipsPool.RecyleToPool(txtObject);
            });
        }

        /// <summary>
        /// 显示一个Icon Tips
        /// </summary>
        /// <param name="type">icon 类型（也是Sprite名称）</param>
        /// <param name="content">加分</param>
        /// <param name="y"></param>
        public void ShowTips(string type, string content, float y = 120f)
        {
            var txtObject = m_IconPool.GetGameObject();
            var tipsIcon = txtObject.GetComponentInChildren<Image>();
            var tipsText = txtObject.GetComponentInChildren<TextMeshProUGUI>();
            Util.SetParent(TipsCanvas, txtObject.transform);

            tipsIcon.sprite = m_SpriteAtlas.GetSprite(type);
            tipsIcon.SetNativeSize();
            tipsText.text = content;
            txtObject.SetActive(true);

            var vector = string.IsNullOrEmpty(content) ? Vector2.zero : Random.insideUnitCircle * 240;
            txtObject.transform.localRotation = Quaternion.identity;
            txtObject.transform.localScale = Vector3.one;
            txtObject.transform.localPosition = new Vector3(vector.x, vector.y - y, 0);

            var color = tipsText.color;
            color.a = 0;
            tipsText.color = color;
            Sequence mySequence = DOTween.Sequence();
            Tweener move1 = txtObject.transform.DOLocalMoveY(vector.y, 0.2f);
            Tweener move2 = txtObject.transform.DOLocalMoveY(vector.y + 1.5f * y, 1.7f);
            Tweener alpha1 = tipsText.DOColor(new Color(color.r, color.g, color.b, 1), 0.2f);
            Tweener alpha2 = tipsText.DOColor(new Color(color.r, color.g, color.b, 0), 1.7f);
            mySequence.Append(move1);
            mySequence.Join(alpha1);
            mySequence.Append(move2);
            mySequence.Join(alpha2);

            mySequence.OnComplete(delegate ()
            {
                mySequence.Kill();
                m_IconPool.RecyleToPool(txtObject);
            });
        }

        /// <summary>
        /// 显示减血Tips
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="pos">位置</param>
        /// <param name="y">y偏移</param>
        public void ShowReduceTips(string content, Vector3 pos, float y = 120f)
        {
            var txtObject = m_ReducePool.GetGameObject();
            var tipsText = txtObject.GetComponent<Text>();
            Util.SetParent(TipsCanvas, txtObject.transform);

            tipsText.text = content;
            txtObject.SetActive(true);

            txtObject.transform.localRotation = Quaternion.identity;
            txtObject.transform.localScale = Vector3.one;
            txtObject.transform.localPosition = new Vector3(pos.x, pos.y - y, 0);

            var color = tipsText.color;
            color.a = 0;
            tipsText.color = color;
            Sequence mySequence = DOTween.Sequence();
            Tweener move1 = txtObject.transform.DOLocalMoveY(pos.y, 0.4f);
            move1.SetEase(Ease.InCubic);
            Tweener move2 = txtObject.transform.DOLocalMoveY(pos.y + y, 0.4f);
            move2.SetEase(Ease.OutCubic);
            Tweener alpha1 = tipsText.DOColor(new Color(color.r, color.g, color.b, 1), 0.4f);
            Tweener alpha2 = tipsText.DOColor(new Color(color.r, color.g, color.b, 0), 0.4f);
            Tweener scale1 = txtObject.transform.DOScale(new Vector3(2f,2f,1f), 0.4f);
            scale1.SetEase(Ease.InOutCubic);
            Tweener scale2 = txtObject.transform.DOScale(new Vector3(1f, 1f, 1f), 0.4f);
            scale2.SetEase(Ease.InOutCubic);

            mySequence.Append(move1);
            mySequence.Join(alpha1);
            mySequence.Join(scale1);
            mySequence.AppendInterval(0.4f);
            mySequence.Append(move2);
            mySequence.Join(alpha2);
            mySequence.Join(scale2);

            mySequence.OnComplete(delegate ()
            {
                mySequence.Kill();
                m_ReducePool.RecyleToPool(txtObject);
            });
        }

    }
}