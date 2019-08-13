using System;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using Framework.Utility;
using LitJson;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WorldMap
{
    /// <summary>
    /// 世界地图管理（先更新摄像机位置，再变换地图块）
    /// </summary>
    public class WorldMapView : LuaBehaviour
    {
        public float ItemHeight { get; set; }               //地图块Y轴间距
        public float ItemWidth { get; set; }                //地图块X轴间距
        public Vector2 MinPos { get; set; }                 //范围最小坐标
        public Vector2 MaxPos { get; set; }                 //范围最大坐标
        public int InitCountX { get; set; }                 //初始化生成X轴数量
        public int InitCountY { get; set; }                 //初始化生成Y轴数量
        private float m_HalfCountX;                         //X与Y轴半数
        private float m_HalfCountY;
        private Vector2 m_HalfPivot = new Vector2(0.5f, 0.5f);//中心锚点

        private SpriteAtlas m_MapAtlas;                 //地图Atlas
        private GameObjectPool m_MapsPool;              //地块对象池
        private GameObjectPool m_BuildsPool;            //建筑（城池或者资源）对象池
        private GameObjectPool m_LinesPool;             //线条对象池
        private Dictionary<Vector2, VoMapItem> m_MapActiveDic = new Dictionary<Vector2, VoMapItem>();   //激活地图块
        private List<VoWorldUnit> m_BuildDataList = new List<VoWorldUnit>();                            //缓存服务器返回世界数据
        private List<GameObject> m_LineObjs = new List<GameObject>();   //行军路线显示列表
        private Transform m_MapsTrans;                                  //地块父对象
        private Transform m_BuildsTrans;                                //建筑父对象
        private Transform m_LineTrans;                                  //行军路线父对象

        /// <summary>
        /// 初始化地图块
        /// </summary>
        /// <param name="obj"></param>
        public void CreateMapPool(params Object[] objects)
        {
            m_MapsTrans = new GameObject("MapsPanel").transform;
            m_BuildsTrans = new GameObject("BuildsPanel").transform;
            m_LineTrans = new GameObject("LinesPanel").transform;
            Util.SetParent(transform, m_MapsTrans);
            Util.SetParent(transform, m_BuildsTrans);
            Util.SetParent(transform, m_LineTrans);
            m_MapsTrans.localScale = Vector3.one;
            m_BuildsTrans.localScale = Vector3.one;
            m_LineTrans.localScale = Vector3.one;

            m_HalfCountX = (InitCountX - 1) / 2;
            m_HalfCountY = (InitCountY - 1) / 2;
            var initCount = InitCountX * InitCountY;
            PoolManager poolManager = Singleton.GetInstance<PoolManager>();
            m_MapsPool = poolManager.CreatePool("WorldMaps", (GameObject)objects[0], initCount);
            m_BuildsPool = poolManager.CreatePool("WorldBuilds", (GameObject)objects[1], initCount);
            m_LinesPool = poolManager.CreatePool("WorldLines", (GameObject)objects[2], 10);
            m_MapAtlas = objects[3] as SpriteAtlas;
        }

        /// <summary>
        /// 刷新地块之前先回收
        /// </summary>
        private void RecyleMapItem(Vector2 position)
        {
            if (m_MapActiveDic.Count == 0) return;
            var tempListPos = new List<Vector2>();
            foreach (Vector2 pointPos in m_MapActiveDic.Keys)
            {
                var isExceedX = Mathf.Abs(pointPos.x - position.x) > m_HalfCountX;
                var isExceedY = Mathf.Abs(pointPos.y - position.y) > m_HalfCountY;
                if (isExceedX || isExceedY)
                {
                    var voMapItem = m_MapActiveDic[pointPos];
                    if (voMapItem.ObjMapItem) m_MapsPool.RecyleToPool(voMapItem.ObjMapItem);
                    if (voMapItem.ObjBuildItem) m_BuildsPool.RecyleToPool(voMapItem.ObjBuildItem);
                    tempListPos.Add(pointPos);
                }
            }
            foreach (Vector2 pointPos in tempListPos)
            {
                m_MapActiveDic.Remove(pointPos);
            }
            tempListPos.Clear();
        }

        /// <summary>
        /// 回收地块之后，需要激活未显示的新地块
        /// </summary>
        private void ActiveMapItem(Vector2 position)
        {
            var posX = (int)(position.x - m_HalfCountX);
            var posY = (int)(position.y - m_HalfCountY);
            for (int node = posX; node < posX + InitCountX; node++)
            {
                for (int repo = posY; repo < posY + InitCountY; repo++)
                {
                    var pointPos = new Vector2(node, repo);
                    if (!IsPosActive(pointPos))
                    {
                        var voMapItem = new VoMapItem();
                        voMapItem.position = pointPos;
                        voMapItem.IsOutofBounds = pointPos.IsOutofBounds(MinPos, MaxPos);
                        voMapItem.IsMapDeep = (pointPos.x + pointPos.y) % 2 == 0;
                        voMapItem.ObjMapItem = m_MapsPool.GetGameObject();
                        ShowMapItem(voMapItem);
                        if (!voMapItem.IsOutofBounds)
                        {
                            voMapItem.ObjBuildItem = m_BuildsPool.GetGameObject();
                            ShowResources(voMapItem);
                        }
                        m_MapActiveDic.Add(pointPos, voMapItem);
                    }
                }
            }
        }

        /// <summary>
        /// 判断当前位置有无激活地块
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsPosActive(Vector2 position)
        {
            var isActive = m_MapActiveDic.ContainsKey(position);
            return isActive;
        }

        /// <summary>
        /// 请求数据
        /// </summary>
        private void RequestWorldInfo(Vector2 position)
        {
            Util.CallMethod(name, "RequestWorldInfo", position.x, position.y);
        }

        /// <summary>
        /// 请求服务器数据返回
        /// </summary>
        /// <param name="jsonData"></param>
        public void UpdateWorldMapInfo(string jsonData)
        {
            var updateDataList = JsonMapper.ToObject<List<VoWorldUnit>>(jsonData);
            //本地缓存没有该数据则添加
            foreach (VoWorldUnit voWorldUnit in updateDataList)
            {
                if (!IsPosActive(voWorldUnit.pos)) continue;
                m_BuildDataList.Add(voWorldUnit);
            }
            foreach (VoWorldUnit voWorldUnit in m_BuildDataList)
            {
                var pointPos = voWorldUnit.pos;
                if (IsPosActive(pointPos))
                {
                    var voMapItem = m_MapActiveDic[pointPos];
                    voMapItem.WorldUnit = voWorldUnit;
                    UpdateResources(voMapItem);
                }
            }
        }

        /// <summary>
        /// 刷新显示行军路线
        /// </summary>
        /// <param name="jsonData"></param>
        public void UpdateMarchingsLines(string jsonData)
        {
            var lineDataList = JsonMapper.ToObject<List<VoLineItem>>(jsonData);
            //先回收线条对象列表，再重新刷新显示线条
            foreach(var lineObj in m_LineObjs) { m_LinesPool.RecyleToPool(lineObj); }
            m_LineObjs.Clear();

            foreach(var voLineItem in lineDataList)
            {
                var lineObj = m_LinesPool.GetGameObject();
                m_LineObjs.Add(lineObj);
                var lineTrans = lineObj.transform;
                Util.SetParent(m_LineTrans, lineTrans);
                lineTrans.localPosition = Vector3.zero;
                lineTrans.localScale = Vector3.one;
                lineTrans.localRotation = Quaternion.identity;
                var mapLine = lineObj.GetComponent<MapLine>();
                var tiledImage = lineObj.GetComponent<TiledImage>();
                var startPos = GetPosition(voLineItem.userPos.x, voLineItem.userPos.y);
                var endPos = GetPosition(voLineItem.targetPos.x, voLineItem.targetPos.y);
                if (voLineItem.status == MarchingStatus.Marching_Back)
                    mapLine.SetLine(endPos, startPos);
                else
                    mapLine.SetLine(startPos, endPos);
                tiledImage.color = voLineItem.isMine ? Color.green : Color.red;
            }
        }

        /// <summary>
        /// 更新显示地图
        /// </summary>
        public void UpdateShowMap(float x, float y)
        {
            var position = new Vector2(x, y);
            RecyleMapItem(position);        //先回收超过范围的地图块
            ActiveMapItem(position);        //再刷新激活新加入范围的地图块
            RequestWorldInfo(position);     //请求服务器数据
        }

        //玩家自身信息
        private VoPlyer m_VoMyPlayer;
        public void SetTempPlayer(string jsonData)
        {
            m_VoMyPlayer = JsonMapper.ToObject<VoPlyer>(jsonData);
        }

        /// <summary>
        /// 迁移基地成功
        /// </summary>
        public void WorldMoveSuccess(float movePosX, float movePosY, float x, float y)
        {
            var lastPos = new Vector2(movePosX, movePosY);
            var position = new Vector2(x, y);
            m_MapActiveDic.TryGetValue(lastPos, out VoMapItem voMapItemLast);
            if (null != voMapItemLast)
            {
                voMapItemLast.WorldUnit.player = null;
                voMapItemLast.WorldUnit.unitType = MapType.None;
                UpdateResources(voMapItemLast);
                m_MapActiveDic[lastPos] = voMapItemLast;
            }
            m_MapActiveDic.TryGetValue(position, out VoMapItem voMapItem);
            if (null != voMapItem)
            {
                voMapItem.WorldUnit = new VoWorldUnit();
                voMapItem.WorldUnit.pos = position;
                voMapItem.WorldUnit.player = m_VoMyPlayer;
                voMapItem.WorldUnit.unitType = MapType.Player;
                UpdateResources(voMapItem);
                m_MapActiveDic[position] = voMapItem;
            }
        }

        /// <summary>
        /// 转换坐标
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        private Vector3 GetPosition(float posX, float posY)
        {
            return new Vector3(ItemWidth * (posX - posY), ItemHeight * (posX + posY), 0);
        }

        /// <summary>
        /// 在地图上显示某地图块
        /// </summary>
        /// <param name="gameObj">地块对象</param>
        /// <param name="position">坐标位置</param>
        private void ShowMapItem(VoMapItem voMapItem)
        {
            var gameObj = voMapItem.ObjMapItem;
            var position = voMapItem.position;
            var localPosition = GetPosition(position.x, position.y);
            gameObj.name = "MapItem" + position.x + "x" + position.y;
            gameObj.transform.SetParent(m_MapsTrans);
            gameObj.transform.localScale = Vector3.one;
            gameObj.transform.localPosition = localPosition;
            gameObj.transform.localRotation = Quaternion.identity;
            gameObj.GetComponent<RectTransform>().pivot = m_HalfPivot;
            gameObj.GetComponent<Image>().sprite = m_MapAtlas.GetSprite(voMapItem.MapSprite);
        }

        /// <summary>
        /// 随机在地图上创建建筑
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="node"></param>
        /// <param name="repo"></param>
        private void ShowResources(VoMapItem voMapItem)
        {
            var gameObj = voMapItem.ObjBuildItem;
            var position = voMapItem.position;
            var localPosition = GetPosition(position.x, position.y);
            gameObj.name = "BuildItem" + position.x + "x" + position.y;
            gameObj.transform.SetParent(m_BuildsTrans);
            gameObj.transform.localScale = Vector3.one;
            gameObj.transform.localPosition = localPosition;
            gameObj.transform.localRotation = Quaternion.identity;
            gameObj.GetComponent<RectTransform>().pivot = m_HalfPivot;
            UpdateResources(voMapItem);
        }

        /// <summary>
        /// 刷新显示城池、资源等
        /// </summary>
        /// <param name="voMapItem"></param>
        private void UpdateResources(VoMapItem voMapItem)
        {
            var gameObj = voMapItem.ObjBuildItem;
            var transform = gameObj.transform;
            var panelCity = transform.Find("PanelCity").gameObject;
            var panelRes = transform.Find("PanelRes").gameObject;
            var panelNone = transform.Find("PanelNone").gameObject;
            void ShowByType(MapType mapType)
            {
                panelCity.SetActive(mapType == MapType.Player);
                panelRes.SetActive(mapType == MapType.Resource);
                panelNone.SetActive(mapType == MapType.None);
            }
            if (null != voMapItem.WorldUnit)
            {
                var unitType = voMapItem.WorldUnit.unitType;
                ShowByType(unitType);
                switch (unitType)
                {
                    case MapType.Player:
                        UpdatePlayer(panelCity, voMapItem);
                        break;
                    case MapType.Resource:
                        UpdateRes(panelRes, voMapItem);
                        break;
                    case MapType.None:
                        UpdateNone(panelNone, voMapItem);
                        break;
                }
            }
            else
            {
                ShowByType(MapType.None);
                UpdateNone(panelNone, voMapItem);
            }
        }

        /// <summary>
        /// 玩家地块
        /// </summary>
        /// <param name="gameObj"></param>
        /// <param name="voMapItem"></param>
        private void UpdatePlayer(GameObject gameObj, VoMapItem voMapItem)
        {
            var transform = gameObj.transform;
            var imageBg = transform.Find("ImageBg").GetComponent<Image>();
            var onImage = transform.Find("ImageBg").GetComponent<OnImage>();
            var txtLv = transform.Find("TxtLv").GetComponent<Text>();
            var txtName = transform.Find("TxtName").GetComponent<Text>();
            var txtPos = transform.Find("TxtPos").GetComponent<Text>();
            var txtFR = transform.Find("TxtFR").GetComponent<Text>();
            var imgBattle = transform.Find("ImgBattle").gameObject;
            var imgFlag = transform.Find("ImgFlag").gameObject;
            var imgSelfLegion = transform.Find("ImgSelfLegion").gameObject;
            var imgTeamLegion = transform.Find("ImgTeamLegion").gameObject;
            var sliderFR = transform.Find("SliderFR").GetComponent<Slider>();
            var player = voMapItem.WorldUnit.player;
            var flag = voMapItem.WorldUnit.flag;
            imageBg.sprite = m_MapAtlas.GetSprite(voMapItem.CitySprite);
            imageBg.SetNativeSize();
            AddListener(onImage, voMapItem);
            txtLv.text = (player.level + 1).ToString();
            var isMyself = m_VoMyPlayer.id == player.id;
            var isHasLegion = !string.IsNullOrEmpty(player.legion);
            var isMyLegion = isHasLegion && string.Equals(m_VoMyPlayer.legion, player.legion);
            imgSelfLegion.SetActive(isMyself && isHasLegion);
            imgTeamLegion.SetActive(!isMyself && isMyLegion);
            var color = isMyself ? "#f5d70f" : (isMyLegion ? "#71c8fa" : "#e23f3f"); //自身黄色、同军团绿色、敌人红色
            txtName.text = "<color="+ color + ">" + player.nickname + "</color>";
            txtPos.text = "(" + voMapItem.position.x + "," + voMapItem.position.y + ")";
            sliderFR.value = (float)(player.fr_val / (player.fr_max > 0 ? player.fr_max : 1));
            imgFlag.SetActive(sliderFR.value < 0.5f);
            txtFR.text = player.fr_val + "/" + player.fr_max;
        }

        /// <summary>
        /// 资源地块
        /// </summary>
        /// <param name="gameObj"></param>
        /// <param name="voMapItem"></param>
        private void UpdateRes(GameObject gameObj, VoMapItem voMapItem)
        {
            var transform = gameObj.transform;
            var flag = voMapItem.WorldUnit.flag;
            var imageBg = transform.Find("ImageBg").GetComponent<Image>();
            var onImage = transform.Find("ImageBg").GetComponent<OnImage>();
            var txtLv = transform.Find("TxtLv").GetComponent<Text>();
            var imgFlagSelf = transform.Find("ImgFlagSelf").gameObject;
            var imgFlagTeam = transform.Find("ImgFlagTeam").gameObject;
            imgFlagSelf.SetActive(flag == WorldFlag.Self);
            imgFlagTeam.SetActive(flag == WorldFlag.Friend);
            txtLv.text = voMapItem.WorldUnit.res.level.ToString();
            imageBg.sprite = m_MapAtlas.GetSprite(voMapItem.ResSprite);
            imageBg.SetNativeSize();
            AddListener(onImage, voMapItem);
        }

        /// <summary>
        /// 空地块
        /// </summary>
        /// <param name="gameObj"></param>
        /// <param name="voMapItem"></param>
        private void UpdateNone(GameObject gameObj, VoMapItem voMapItem)
        {
            var transform = gameObj.transform;
            var imageBg = transform.Find("ImageBg").GetComponent<Image>();
            var onImage = transform.Find("ImageBg").GetComponent<OnImage>();
            imageBg.sprite = m_MapAtlas.GetSprite(voMapItem.NoneSprite);
            imageBg.SetNativeSize();
            AddListener(onImage, voMapItem);
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="gameObj"></param>
        /// <param name="voMapItem"></param>
        private void AddListener(OnImage onImage, VoMapItem voMapItem)
        {
            //添加监听
            onImage.onDistance.RemoveAllListeners();
            onImage.onDistance.AddListener(delegate { OnItemClick(onImage, voMapItem); });
        }

        /// <summary>
        /// 单击建筑
        /// </summary>
        /// <param name="gameObj"></param>
        private void OnItemClick(OnImage onImage, VoMapItem voMapItem)
        {
            Util.Log("触发坐标：{0:G}，{1:G}", voMapItem.position.x, voMapItem.position.y);
            void WorldMove()
            {
                Util.CallMethod(name, "WorldMove", voMapItem.position.x, voMapItem.position.y);
            }
            if (null == voMapItem.WorldUnit)
            {
                //移动到该位置
                TweenImage(onImage, WorldMove);
                return;
            }
            var unitType = voMapItem.WorldUnit.unitType;
            switch (unitType)
            {
                case MapType.None://移动到该位置
                    TweenImage(onImage, WorldMove);
                    break;
                case MapType.Player:
                    var player = voMapItem.WorldUnit.player;
                    if (null == player) return;
                    Util.CallMethod(name, "CheckPlayer", voMapItem.position.x, voMapItem.position.y, player.id);
                    break;
                case MapType.Resource: //攻击某点
                    Util.CallMethod(name, "CheckRes", voMapItem.position.x, voMapItem.position.y, voMapItem.WorldUnit.res.id);
                    break;
            }
        }

        //点击效果
        private void TweenImage(OnImage onImage, Action action)
        {
            GameObject objSelect = Util.FindPeer(onImage.gameObject, "ImgSelect");
            if (null == objSelect) return;
            var imgSelect = objSelect.GetComponent<Image>();
            objSelect.SetActive(true);
            imgSelect.color = Color.clear;
            Sequence mySequence = DOTween.Sequence();
            Tweener color1 = imgSelect.DOColor(Color.white, 0.25f);
            Tweener color2 = imgSelect.DOColor(Color.clear, 0.3f);
            mySequence.Append(color1);
            mySequence.AppendInterval(0.05f);
            mySequence.Append(color2);
            mySequence.OnComplete(delegate ()
            {
                action.Invoke();
                mySequence.Kill();
                objSelect.SetActive(false);
            });
        }

        new void OnDestroy()
        {
            m_BuildDataList.Clear();
            m_MapActiveDic.Clear();
            base.OnDestroy();
        }
    }
}