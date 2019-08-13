#define TOLUA
//#define XLUA
//#define SLUA
//#define TOLUA
//#define ULUA
//#define PANDORA

//ToLua用户需要手动在Editor/Custom/CustomSettings.cs的customTypeList数组中添加:
//ULua用户需要手动在Editor/WrapFile.cs的customTypeList数组中添加:
//_GT(typeof(LuaPerfect.ObjectRef)),
//_GT(typeof(LuaPerfect.ObjectItem)),
//_GT(typeof(LuaPerfect.ObjectFormater)),

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;

#if XLUA
using XLua;
#endif
#if SLUA
using SLua;
#endif
#if PANDORA
using com.tencent.pandora;
#endif

namespace LuaPerfect
{
    public class ObjectCache
    {
        public static List<object> s_objectList = new List<object>();

        public static Dictionary<object, int> s_objectDirectionary = new Dictionary<object, int>();

        static ObjectCache()
        {
            ClearAll();
        }

        public static int SaveObject(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            int objectID = -1;
            if (s_objectDirectionary.TryGetValue(obj, out objectID))
            {
                return objectID;
            }
            objectID = s_objectList.Count;
            s_objectList.Add(obj);
            s_objectDirectionary[obj] = objectID;
            return objectID;
        }

        public static object GetObject(int objectID)
        {
            if (objectID >= 0 && objectID < s_objectList.Count)
            {
                return s_objectList[objectID];
            }
            return null;
        }

        public static void ClearAll()
        {
            s_objectList.Clear();
            s_objectDirectionary.Clear();
            s_objectList.Add(null);
        }
    }

#if XLUA
    [LuaCallCSharp]
#endif
#if SLUA || PANDORA
    [CustomLuaClass]
#endif
    //考虑改成struct
    public class ObjectRef
    {
        public int objectID;

        public ObjectRef(int objectID)
        {
            this.objectID = objectID;
        }
    }

#if XLUA
    [LuaCallCSharp]
#endif
#if SLUA || PANDORA
    [CustomLuaClass]
#endif
    public class ObjectItem
    {
        private string value;

        private List<string> childNameList;

        public List<ObjectRef> childObjectRefList;

        public ObjectItem()
        {
            childNameList = new List<string>();
            childObjectRefList = new List<ObjectRef>();
        }

        public int GetChildCount()
        {
            return childNameList.Count;
        }

        public string GetChildName(int i)
        {
            return (string)childNameList[i];
        }

        public void SetValue(string value)
        {
            this.value = value;
        }

        public string GetValue()
        {
            return value;
        }

        public void AddChild(string name, System.Object obj)
        {
            childNameList.Add(name);
            int objectID = ObjectCache.SaveObject(obj);
            ObjectRef objectRef = new ObjectRef(objectID);
            childObjectRefList.Add(objectRef);
        }

        public ObjectRef GetChildObject(int i)
        {
            return childObjectRefList[i];
        }

        public ObjectRef GetChildObjectByName(string name)
        {
            int count = childNameList.Count;
            for (int i = 0; i < count; i++)
            {
                if (childNameList[i] == name)
                {
                    return childObjectRefList[i];
                }
            }
            return new ObjectRef(-1);
        }

        public static int StaticGetChildCount(ObjectItem objectItem)
        {
            return objectItem.GetChildCount();
        }

        public static string StaticGetValue(ObjectItem objectItem)
        {
            return objectItem.GetValue();
        }

        public static string StaticGetChildName(ObjectItem objectItem, int i)
        {
            return objectItem.GetChildName(i);
        }

        public static ObjectRef StaticGetChildObject(ObjectItem objectItem, int i)
        {
            return objectItem.GetChildObject(i);
        }

        public static ObjectRef StaticGetChildObjectByName(ObjectItem objectItem, string name)
        {
            return objectItem.GetChildObjectByName(name);
        }
    }

    public class ClassInfoItem
    {
        public string fullName;
        public string fullName2;
        public List<FieldInfo> fields;
        public List<PropertyInfo> properties;
    }

#if XLUA
    [LuaCallCSharp]
#endif
#if SLUA || PANDORA
    [CustomLuaClass]
#endif
    public class ObjectFormater
    {
        public static Dictionary<Type, ClassInfoItem> s_classInfoDirectionary = new Dictionary<Type, ClassInfoItem>();

        public static bool IsBasicType(Type type)
        {
            if (type == typeof(System.Boolean) ||
                type == typeof(System.Int32) ||
                type == typeof(System.UInt32) ||
                type == typeof(System.Single) ||
                type == typeof(System.String) ||
                type == typeof(System.Char) ||
                type == typeof(System.Byte) ||
                type == typeof(System.SByte) ||
                type == typeof(System.Decimal) ||
                type == typeof(System.Double) ||
                type == typeof(System.Int64) ||
                type == typeof(System.UInt64) ||
                type == typeof(System.IntPtr) ||
                type == typeof(System.UIntPtr) ||
                type == typeof(System.Int16) ||
                type == typeof(System.UInt16))
            {
                return true;
            }
            return false;
        }

        public static List<FieldInfo> GetAllFields(Type type)
        {
            List<FieldInfo> allFields = new List<FieldInfo>();
            Type type1 = type;
            while (type1 != null)
            {
                FieldInfo[] fields = type1.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (fields.Length > 0)
                {
                    foreach (FieldInfo field in fields)
                    {
                        allFields.Add(field);
                    }
                }
                type1 = type1.BaseType;
            }
            return allFields;
        }

        public static bool FindProperty(List<PropertyInfo> allProperties, string name)
        {
            foreach (object obj in allProperties)
            {
                PropertyInfo property = (PropertyInfo)obj;
                if (property.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<PropertyInfo> GetAllProperties(Type type)
        {
            List<PropertyInfo> allProperties = new List<PropertyInfo>();
            Type type1 = type;
            while (type1 != null)
            {
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                if (properties.Length > 0)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        if (FindProperty(allProperties, property.Name) == false)
                        {
                            allProperties.Add(property);
                        }
                    }
                }
                type1 = type1.BaseType;
            }
            return allProperties;
        }

        public static string GetClassFullName(Type type)
        {
            if (type.Namespace == "")
            {
                return type.Name;
            }
            return type.Namespace + "." + type.Name;
        }

        public static ClassInfoItem GetClassInfo(Type type)
        {
            ClassInfoItem classInfoItem;
            if (s_classInfoDirectionary.TryGetValue(type, out classInfoItem))
            {
                return classInfoItem;
            }
            else
            {
                classInfoItem = new ClassInfoItem();
                classInfoItem.fullName = GetClassFullName(type);
                classInfoItem.fullName2 = String.Format("<C# Object> ({0})", classInfoItem.fullName);
                classInfoItem.fields = GetAllFields(type);
                classInfoItem.properties = GetAllProperties(type);
                s_classInfoDirectionary[type] = classInfoItem;
                return classInfoItem;
            }
        }

        public static List<GameObject> GetChildrenGameObjects(GameObject gameObject)
        {
            List<GameObject> children = new List<GameObject>();
            if (gameObject != null)
            {
                foreach (Transform childTransform in gameObject.transform)
                {
                    GameObject childGameObject = childTransform.gameObject;
                    if (childGameObject != null)
                    {
                        children.Add(childGameObject);
                    }
                }
            }
            return children;
        }

        public static ObjectItem FormatObject(System.Object obj, bool collectChildren)
        {
            System.Object obj1 = obj;
            if (obj.GetType() == typeof(ObjectRef))
            {
                ObjectRef objectRef = (ObjectRef)obj;
                obj1 = ObjectCache.GetObject(objectRef.objectID);
            }
            ObjectItem objectItem = null;
            try
            {
                objectItem = InnerFormatObject(obj1, collectChildren);
            }
            catch
            {
                objectItem = new ObjectItem();
            }
            return objectItem;
        }

        public static ObjectItem InnerFormatObject(System.Object obj, bool collectChildren)
        {
            ObjectItem objectItem = new ObjectItem();
            if (obj == null)
            {
                objectItem.SetValue("null");
                return objectItem;
            }
            Type type = obj.GetType();
            if (IsBasicType(type))
            {
                if (type == typeof(System.Boolean))
                {
                    objectItem.SetValue((bool)obj ? "true" : "false");
                }
                else if (type == typeof(System.String))
                {
                    objectItem.SetValue("\"" + (string)obj + "\"");
                }
                else
                {
                    objectItem.SetValue(obj.ToString());
                }
            }
            else if (type.IsEnum)
            {
                objectItem.SetValue(obj.ToString());
            }
            else if (type == typeof(ArrayList))
            {
                ArrayList arrayList = (ArrayList)obj;
                objectItem.SetValue(String.Format("<C# ArrayList> Count = {0}", arrayList.Count));
                if (collectChildren)
                {
                    int i = 0;
                    foreach (object obj1 in arrayList)
                    {
                        string name = i.ToString();
                        objectItem.AddChild(name, obj1);
                        i++;
                    }
                }
            }
            else if (type == typeof(Hashtable))
            {
                Hashtable hashtable = (Hashtable)obj;
                objectItem.SetValue(String.Format("<C# Hashtable> Count = {0}", hashtable.Count));
                if (collectChildren)
                {
                    foreach (object key in hashtable.Keys)
                    {
                        string name = key.ToString();
                        objectItem.AddChild(name, hashtable[key]);
                    }
                }
            }
            else if (type.IsArray)
            {
                Array array = (Array)obj;
                objectItem.SetValue(String.Format("<C# Array> Length = {0}", array.Length));
                if (collectChildren)
                {
                    int i = 0;
                    foreach (object obj1 in array)
                    {
                        string name = i.ToString();
                        objectItem.AddChild(name, obj1);
                        i++;
                    }
                }
            }
            else
            {
                ClassInfoItem classInfoItem = GetClassInfo(type);
                if (classInfoItem != null)
                {
                    objectItem.SetValue(classInfoItem.fullName2);
                    if (collectChildren)
                    {
                        if (type == typeof(GameObject))
                        {
                            GameObject gameObject = (GameObject)obj;
                            Component[] components = gameObject.GetComponents(typeof(Component));
                            int i = 0;
                            foreach (Component component in components)
                            {
                                Type componentType = component.GetType();
                                bool isScript = componentType.IsSubclassOf(typeof(MonoBehaviour));
                                string name = String.Format("Component_{0}_{1}{2}", i, componentType.Name, isScript ? "_Script" : "");
                                objectItem.AddChild(name, component);
                                i++;
                            }
                            List<GameObject> children = GetChildrenGameObjects(gameObject);
                            i = 0;
                            foreach (GameObject childGameObject in children)
                            {
                                string name = String.Format("Child_{0}_{1}", i, childGameObject.name);
                                objectItem.AddChild(name, childGameObject);
                                i++;
                            }
                        }
                        foreach (FieldInfo field in classInfoItem.fields)
                        {
                            object value = field.GetValue(obj);
                            string name = field.Name;
                            objectItem.AddChild(name, value);
                        }
                        foreach (PropertyInfo property in classInfoItem.properties)
                        {
                            object value = null;
                            try
                            {
                                value = property.GetValue(obj, null);
                            }
                            catch (Exception)
                            {
                            }
                            string name = property.Name;
                            objectItem.AddChild(name, value);
                        }
                    }
                }
            }
            return objectItem;
        }

        public static void ClearObjectCache()
        {
            ObjectCache.ClearAll();
        }
    }
}