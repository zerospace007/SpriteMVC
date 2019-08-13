#region 引用空间
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#endregion

/// <summary>
/// 部分类静态函数扩展
/// </summary>
public static class ObjetExtensions
{
    /// <summary>
    /// 是否出界（例如是否在（0，0）（600，600）的范围内）
    /// </summary>
    /// <param name="pointPos"></param>
    /// <param name="minVec"></param>
    /// <param name="MaxVec"></param>
    /// <returns></returns>
    public static bool IsOutofBounds(this Vector2 pointPos, Vector2 minVec, Vector2 MaxVec)
    {
        return (pointPos.x < minVec.x || pointPos.y < minVec.y ||
                        pointPos.x > MaxVec.x || pointPos.y > MaxVec.y);
    }

    /// <summary>
    /// 根据键Key以及对应的键值Value查找列表当中的某对象（线性查找，时间复杂度O(n)）
    /// </summary>
    /// <typeparam name="T">泛型列表项</typeparam>
    /// <param name="list">列表</param>
    /// <param name="value">键值value</param>
    /// <param name="feild">字段key</param>
    /// <returns></returns>
    public static T FindItem<T>(this List<T> list, object value, string feild = "ID")
    {
        return list.Find(
            delegate (T item)
            {
                var feildInfo = item.GetType().GetField(feild, BindingFlags.Public | BindingFlags.Instance);
                return null != feildInfo && value.Equals(feildInfo.GetValue(item));
            });
    }

    /// <summary>
    /// 根据键Key以及对应的键值Value查找列表当中的对象列表（线性查找，时间复杂度O(n)）
    /// </summary>
    /// <typeparam name="T">泛型列表项</typeparam>
    /// <param name="list">列表</param>
    /// <param name="value">键值value</param>
    /// <param name="feild">字段key</param>
    /// <returns></returns>
    public static List<T> FindItems<T>(this List<T> list, object value, string feild = "ID")
    {
        var tmpList = new List<T>();
        list.ForEach(delegate (T item)
        {
            var feildInfo = item.GetType().GetField(feild, BindingFlags.Public | BindingFlags.Instance);
            if (null != feildInfo && value.Equals(feildInfo.GetValue(item)))
            {
                tmpList.Add(item);
            }
        });
        return tmpList;
    }

    /// <summary>
    /// 根据键Key以及对应的键值Value查找列表当中的某对象（线性查找，时间复杂度O(n)）；并从列表当中删除
    /// </summary>
    /// <typeparam name="T">泛型列表项</typeparam>
    /// <param name="list">列表</param>
    /// <param name="value">键值value</param>
    /// <param name="feild">字段key</param>
    public static void RemoveItem<T>(this List<T> list, object value, string feild = "ID")
    {
        var item = list.FindItem(value, feild);
        if (item != null) list.Remove(item);
    }

    /// <summary>
    /// 根据键Key以及对应的键值Value拷贝到新string数组
    /// </summary>
    /// <typeparam name="T">泛型列表项</typeparam>
    /// <param name="list">列表</param>
    /// <param name="feild">字段key</param>
    /// <returns></returns>
    public static string[] CloneFeildArray<T>(this List<T> list, string feild = "ID")
    {
        var cloneArr = new string[list.Count];
        for (var node = 0; node < list.Count; node++)
        {
            var item = list[node];
            if (null == item) continue;

            var feildInfo = item.GetType().GetField(feild, BindingFlags.Public | BindingFlags.Instance);
            cloneArr[node] = feildInfo != null ? feildInfo.GetValue(item).ToString() : string.Empty;
        }
        return cloneArr;
    }

    /// <summary>
    /// 去除冗余项
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="list">列表</param>
    /// <returns></returns>
    public static List<T> RemoveDuplicates<T>(this List<T> list)
    {
        var tempList = new List<T>();
        foreach (var item in list)
        {
            if (!tempList.Contains(item))
            {
                tempList.Add(item);
            }
        }

        return tempList;
    }

    /// <summary>
    /// 转换Bool转string
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public static string GetCountStr(this bool boolean)
    {
        return boolean ? "1" : "0";
    }

    /// <summary>
    /// 转换Boolean转string
    /// </summary>
    /// <param name="isCondition"></param>
    /// <returns></returns>
    public static string GetConitionStr(this bool isCondition)
    {
        return isCondition ? "on" : "off";
    }

    /// <summary>
    /// 销毁挂载脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameObject"></param>
    public static void Destroy<T>(this GameObject gameObject) where T : Component
    {
        Object.Destroy(gameObject.GetComponent<T>());
    }

    /// <summary>
    /// 取BoxCollider中心点大小
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static Vector3 CenterScale(this GameObject gameObject)
    {
        return new Vector3(gameObject.GetComponent<BoxCollider>().center.x *
                           gameObject.transform.localScale.x,
            gameObject.GetComponent<BoxCollider>().center.y *
            gameObject.transform.localScale.y,
            gameObject.GetComponent<BoxCollider>().center.z *
            gameObject.transform.localScale.z);
    }

    /// <summary>
    /// 取BoxCollider中心点大小
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Vector3 CenterScale(this Transform transform)
    {
        return new Vector3(transform.GetComponent<BoxCollider>().center.x *
                           transform.localScale.x,
            transform.GetComponent<BoxCollider>().center.y *
            transform.localScale.y,
            transform.GetComponent<BoxCollider>().center.z *
            transform.localScale.z);
    }

    /// <summary>
    /// 是否激活所有Collider
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isAble"></param>
    public static void SetableAllExternalColliders(this GameObject obj, bool isAble)
    {
        var boxColls = obj.GetComponentsInChildren<BoxCollider>();

        foreach (var coll in boxColls)
        {
            if (coll) coll.enabled = isAble;
        }

        var mrColls = obj.GetComponentsInChildren<MeshCollider>();

        foreach (var coll in mrColls)
        {
            if (coll) coll.enabled = isAble;
        }

        var spColls = obj.GetComponentsInChildren<SphereCollider>();

        foreach (var coll in spColls)
        {
            if (coll) coll.enabled = isAble;
        }

        var cpColls = obj.GetComponentsInChildren<CapsuleCollider>();

        foreach (var coll in cpColls)
        {
            if (coll) coll.enabled = isAble;
        }
    }

    /// <summary>
    /// 创建Collider到对象上
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="objRes"></param>
    /// <returns></returns>
    public static List<GameObject> CreateColliderToObject(GameObject obj, GameObject objRes)
    {
        var lowestPointY = 10000.0f;
        var highestPointY = -10000.0f;
        var lowestPointZ = 10000.0f;
        var highestPointZ = -10000.0f;
        var lowestPointX = 10000.0f;
        var highestPointX = -10000.0f;
        float finalYSize;
        float finalZSize;
        float finalXSize;
        var divX = 2.0f;
        var divY = 2.0f;
        var divZ = 2.0f;

        var objScale = obj.transform.localScale;
        obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        var meshFilter = obj.GetComponent<MeshFilter>();
        var meshFilterArr = obj.GetComponentsInChildren<MeshFilter>();
        var skinnedMeshRenderer = (SkinnedMeshRenderer)obj.GetComponent(typeof(SkinnedMeshRenderer));
        var skinnedMeshRendererArr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
        var transforms = obj.GetComponentsInChildren<Transform>();

        if (meshFilter && meshFilter.GetComponent<Renderer>())
        {
            lowestPointY = meshFilter.GetComponent<Renderer>().bounds.min.y;
            highestPointY = meshFilter.GetComponent<Renderer>().bounds.max.y;
        }

        if (meshFilterArr.Length > 0)
        {
            foreach (var filter in meshFilterArr)
            {
                if (!filter || !filter.GetComponent<Renderer>()) continue;
                if (filter.GetComponent<Renderer>().bounds.min.y < lowestPointY)
                {
                    lowestPointY = filter.GetComponent<Renderer>().bounds.min.y;
                }

                if (filter.GetComponent<Renderer>().bounds.max.y > highestPointY)
                {
                    highestPointY = filter.GetComponent<Renderer>().bounds.max.y;
                }

                if (filter.GetComponent<Renderer>().bounds.min.x < lowestPointX)
                {
                    lowestPointX = filter.GetComponent<Renderer>().bounds.min.x;
                }

                if (filter.GetComponent<Renderer>().bounds.max.x > highestPointX)
                {
                    highestPointX = filter.GetComponent<Renderer>().bounds.max.x;
                }

                if (filter.GetComponent<Renderer>().bounds.min.z < lowestPointZ)
                {
                    lowestPointZ = filter.GetComponent<Renderer>().bounds.min.z;
                }

                if (filter.GetComponent<Renderer>().bounds.max.z > highestPointZ)
                {
                    highestPointZ = filter.GetComponent<Renderer>().bounds.max.z;
                }
            }
        }

        if (skinnedMeshRenderer)
        {
            lowestPointY = skinnedMeshRenderer.GetComponent<Renderer>().bounds.min.y;
            highestPointY = skinnedMeshRenderer.GetComponent<Renderer>().bounds.max.y;
        }

        if (skinnedMeshRendererArr.Length > 0)
        {
            foreach (var meshRenderer in skinnedMeshRendererArr)
            {
                if (!meshRenderer) continue;
                if (meshRenderer.GetComponent<Renderer>().bounds.min.y < lowestPointY)
                {
                    lowestPointY = meshRenderer.GetComponent<Renderer>().bounds.min.y;
                }

                if (meshRenderer.GetComponent<Renderer>().bounds.max.y > highestPointY)
                {
                    highestPointY = meshRenderer.GetComponent<Renderer>().bounds.max.y;
                }

                if (meshRenderer.GetComponent<Renderer>().bounds.min.x < lowestPointX)
                {
                    lowestPointX = meshRenderer.GetComponent<Renderer>().bounds.min.x;
                }

                if (meshRenderer.GetComponent<Renderer>().bounds.max.x > highestPointX)
                {
                    highestPointX = meshRenderer.GetComponent<Renderer>().bounds.max.x;
                }

                if (meshRenderer.GetComponent<Renderer>().bounds.min.z < lowestPointZ)
                {
                    lowestPointZ = meshRenderer.GetComponent<Renderer>().bounds.min.z;
                }

                if (meshRenderer.GetComponent<Renderer>().bounds.max.z > highestPointZ)
                {
                    highestPointZ = meshRenderer.GetComponent<Renderer>().bounds.max.z;
                }
            }
        }

        if (highestPointX - lowestPointX != -20000)
        {
            finalXSize = highestPointX - lowestPointX;
        }
        else
        {
            finalXSize = 1.0f;
            divX = 1.0f;
            lowestPointX = 0;
            Debug.Log("X Something wrong with " + objRes.name);
        }

        if (highestPointY - lowestPointY != -20000)
        {
            finalYSize = highestPointY - lowestPointY;
        }
        else
        {
            finalYSize = 1.0f;
            divY = 1.0f;
            lowestPointY = 0;
            Debug.Log("Y Something wrong with " + objRes.name);
        }

        if (highestPointZ - lowestPointZ != -20000)
        {
            finalZSize = highestPointZ - lowestPointZ;
        }
        else
        {
            finalZSize = 1.0f;
            divZ = 1.0f;
            lowestPointZ = 0;
            Debug.Log("Z Something wrong with " + objRes.name);
        }

        foreach (var transform1 in transforms)
        {
            var trmGo = transform1.gameObject;
            trmGo.layer = 2;
        }

        var behindGO = new GameObject(obj.name);
        behindGO.AddComponent<BoxCollider>();
        obj.transform.parent = behindGO.transform;

        if (Mathf.Approximately(finalXSize, 1.0f) || finalXSize < 1.0f)
        {
            if (finalXSize < 1.0f)
            {
                divX = 1.0f;
                lowestPointX = -1.0f;
            }

            finalXSize = 1.0f;
        }

        if (Mathf.Approximately(finalYSize, 0.0f))
        {
            finalYSize = 0.01f;
            divY = 0.1f;
            lowestPointY = 0.0f;
        }

        if (Mathf.Approximately(finalZSize, 1.0f) || finalZSize < 1.0f)
        {
            if (finalZSize < 1.0f)
            {
                divZ = 1.0f;
                lowestPointZ = -1.0f;
            }

            finalZSize = 1.0f;
        }
        behindGO.transform.localScale = objScale;
        behindGO.GetComponent<BoxCollider>().size = new Vector3(finalXSize, finalYSize, finalZSize);
        behindGO.GetComponent<BoxCollider>().center = new Vector3(finalXSize / divX + lowestPointX,
            finalYSize / divY + lowestPointY, finalZSize / divZ + lowestPointZ);

        obj.SetableAllExternalColliders(false);

        var goList = new List<GameObject>();
        goList.Add(behindGO);
        goList.Add(obj);

        return goList;
    }
}