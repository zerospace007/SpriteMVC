#define TOLUA
//#define XLUA
//#define SLUA
//#define TOLUA
//#define ULUA

#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace LuaPerfect
{
    public class ApiGenerator
    {
#if XLUA
        private static bool notAddCSNamespace = false;
#else
        private static bool notAddCSNamespace = true;
#endif
        private static Dictionary<string, bool> dictionaryNamespace = new Dictionary<string, bool>();
        private static string targetDirectory = "";
        private static bool anyFileChanged = false;
        private static string[] Keywords = {
            "and", "break", "do", "else", "elseif", "end", "false", "for",
            "function", "goto", "if", "in", "local", "nil", "not", "or", "repeat",
            "return", "then", "true", "until", "while"
        };
        private static Dictionary<string, bool> dictionaryKeyword = new Dictionary<string, bool>();
        private static Dictionary<string, string> dictionaryCheckedType = new Dictionary<string, string>();

        static ApiGenerator()
        {
            foreach (string keyword in Keywords)
            {
                dictionaryKeyword[keyword] = true;
            }
        }

        [MenuItem("LuaPerfect/Generate Apis", false, 2)]
        public static void GenerateApis()
        {
            // You can implement your own api generator by calling these public functions
            BeginGenerate();
            GenerateUnityModules();
            GenerateUserModules();
            EndGenerate();
        }

        public static void GenerateUnityModules()
        {
            GenerateModule("UnityEngine");
            GenerateModule("UnityEngine.CoreModule");
            GenerateModule("UnityEngine.UI");
            GenerateModule("UnityEngine.AudioModule");
            GenerateModule("UnityEngine.PhysicsModule");
            GenerateModule("UnityEngine.Advertisements");
        }

        public static void GenerateUserModules()
        {
            GenerateModule("Assembly-CSharp");
            GenerateModule("Assembly-CSharp-firstpass");
        }

        public static void GenerateModule(string assemblyName)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                return;
            }
            Type[] types = assembly.GetTypes();
            foreach (Type t in types)
            {
                ApiGenerator.ReflectType(t);
            }
        }

        private static string GetNamespace(Type type)
        {
            string Namespace = type.Namespace;
            if (notAddCSNamespace)
            {
                if (Namespace == null)
                {
                    return "";
                }
                return Namespace;
            }
            else
            {
                if (Namespace == null)
                {
                    return "CS";
                }
                return "CS." + Namespace;
            }
        }

        private static bool IsTypeChar(char ch)
        {
            return Char.IsLetterOrDigit(ch) || ch == '_' || ch == '[' || ch == ']';
        }

        private static string CheckTypeString(string s)
        {
            if (s == "nil")
            {
                return "nil_";
            }
            bool nonIdentifierCharDetected = false;
            foreach (char ch in s)
            {
                if (!IsTypeChar(ch))
                {
                    nonIdentifierCharDetected = true;
                    break;
                }
            }
            if (!nonIdentifierCharDetected)
            {
                return s;
            }
            if (dictionaryCheckedType.ContainsKey(s))
            {
                return dictionaryCheckedType[s];
            }
            StringBuilder sb = new StringBuilder();
            foreach (char ch in s)
            {
                if (IsTypeChar(ch))
                {
                    sb.Append(ch);
                }
                else if (ch == '&' || ch == '<' || ch == ',')
                {
                }
                else if (ch == '*')
                {
                    sb.Append("Pointer");
                }
                else if (ch == '`')
                {
                    break;
                }
                else
                {
                    sb.Append('_');
                }
            }
            string s1 = sb.ToString();
            dictionaryCheckedType[s] = s1;
            return s1;
        }

        private static string TypeToString(Type type)
        {
            string Namespace = GetNamespace(type);
            string TypeName = CheckTypeString(type.Name);
            if (Namespace == "" || Namespace == null)
            {
                return TypeName;
            }
            else
            {
                return String.Format("{0}.{1}", Namespace, TypeName);
            }
        }

        private static string CheckIdentifier(string identifier)
        {
            if (dictionaryKeyword.ContainsKey(identifier))
            {
                return identifier + "_";
            }
            else
            {
                return identifier;
            }
        }

        private static string ReflectField(FieldInfo field, string typeFullName)
        {
            Type fieldType = field.FieldType;
            string fieldTypeString = TypeToString(fieldType);
            string fieldString = "";
            string checkedFieldName = CheckIdentifier(field.Name);
            fieldString += String.Format("---@field public {0}.{1} : {2}\n", typeFullName, checkedFieldName, fieldTypeString);
            fieldString += String.Format("{0}.{1} = nil\n\n", typeFullName, checkedFieldName);
            return fieldString;
        }

        private static string GetPropertyAccessString(PropertyInfo property)
        {
            bool bCanRead = property.CanRead;
            bool bCanWrite = property.CanWrite;
            if (bCanRead && bCanWrite)
            {
                return "readwrite";
            }
            else if (bCanRead && !bCanWrite)
            {
                return "readonly";
            }
            else if (!bCanRead && bCanWrite)
            {
                return "writeonly";
            }
            else
            {
                return "noaccess";
            }
        }

        private static string ReflectProperty(PropertyInfo property, string typeFullName)
        {
            Type propertyType = property.PropertyType;
            string accessString = GetPropertyAccessString(property);
            string propertyTypeString = TypeToString(propertyType);
            string propertyString = "";
            string checkedPropertyName = CheckIdentifier(property.Name);
            propertyString += String.Format("---@property {0} {1}.{2} : {3}\n", accessString, typeFullName, checkedPropertyName, propertyTypeString);
            propertyString += String.Format("{0}.{1} = nil\n\n", typeFullName, checkedPropertyName);
            return propertyString;
        }

        private static string ReflectParameter(ParameterInfo parameter)
        {
            Type parameterType = parameter.ParameterType;
            string parameterTypeString = TypeToString(parameterType);
            return String.Format("---@param {0} : {1}\n", CheckIdentifier(parameter.Name), parameterTypeString);
        }

        private static string ReflectMethod_Common(string methodName, bool isStatic, ParameterInfo[] parameters, Type returnType, string typeFullName)
        {
            string methodString = "";
            int Count = parameters.Length;
            for (int i = 0; i < Count; i++)
            {
                ParameterInfo parameter = parameters[i];
                methodString += ReflectParameter(parameter);
            }
            if (returnType != typeof(void))
            {
                methodString += String.Format("---@return {0}\n", TypeToString(returnType));
            }
            if (methodName == "")
            {
                methodString += String.Format("function {0}(", typeFullName);
            }
            else
            {
                if (isStatic)
                {
                    methodString += String.Format("function {0}.{1}(", typeFullName, methodName);
                }
                else
                {
                    methodString += String.Format("function {0}:{1}(", typeFullName, methodName);
                }
            }
            for (int i = 0; i < Count; i++)
            {
                ParameterInfo parameter = parameters[i];
                string parameterName = CheckIdentifier(parameter.Name);
                methodString += parameterName;
                if (i != Count - 1)
                {
                    methodString += ", ";
                }
            }
            methodString += String.Format(")\n");
            methodString += String.Format("end\n\n");
            return methodString;
        }

        private static string ReflectMethod(MethodInfo method, string typeFullName)
        {
            string methodName = method.Name;
            bool isStatic = method.IsStatic;
            ParameterInfo[] parameters = method.GetParameters();
            Type returnType = method.ReturnType;
            if (method.IsGenericMethod)
            {
                return "";
            }
            return ReflectMethod_Common(methodName, isStatic, parameters, returnType, typeFullName);
        }

        private static string ReflectConstructor(Type type, ConstructorInfo constructor, string typeFullName)
        {
            string methodName = "";
            bool isStatic = false;
            ParameterInfo[] parameters = constructor.GetParameters();
            Type returnType = type;
            return ReflectMethod_Common(methodName, isStatic, parameters, returnType, typeFullName);
        }

        private static string ReflectNamespace(string Namespace)
        {
            if (dictionaryNamespace.ContainsKey(Namespace))
            {
                return "";
            }
            dictionaryNamespace[Namespace] = true;
            string namespaceString = "";
            namespaceString += String.Format("---@module {0}\n", Namespace);
            namespaceString += String.Format("{0} = {{}}\n\n", Namespace);
            return namespaceString;
        }

        private static string InnerReflectType(Type type)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string Namespace = GetNamespace(type);
            Type baseType = type.BaseType;
            string baseTypeString = "";
            if (baseType != null)
            {
                baseTypeString = TypeToString(baseType);
            }
            stringBuilder.Append(ReflectNamespace(Namespace));
            string typeName = type.Name;
            if (typeName.Contains("$"))
            {
                return "";
            }
            string typeFullName = TypeToString(type);
            if (baseType != null)
            {
                stringBuilder.Append(String.Format("---@class {0} : {1}\n", typeFullName, baseTypeString));
            }
            else
            {
                stringBuilder.Append(String.Format("---@class {0}\n", typeFullName, baseTypeString));
            }
            stringBuilder.Append(String.Format("{0} = {{}}\n\n", typeFullName));
            BindingFlags generalBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase;
            FieldInfo[] fields = type.GetFields(generalBindingFlags);
            foreach (var field in fields)
            {
                stringBuilder.Append(ReflectField(field, typeFullName));
            }
            PropertyInfo[] properties = type.GetProperties(generalBindingFlags);
            foreach (var property in properties)
            {
                stringBuilder.Append(ReflectProperty(property, typeFullName));
            }
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            foreach (var constructor in constructors)
            {
                stringBuilder.Append(ReflectConstructor(type, constructor, typeFullName));
            }
            MethodInfo[] methods = type.GetMethods(generalBindingFlags);
            foreach (var method in methods)
            {
                string methodName = method.Name;
                if (!methodName.StartsWith("get_") &&
                    !methodName.StartsWith("set_"))
                {
                    stringBuilder.Append(ReflectMethod(method, typeFullName));
                }
            }
            return stringBuilder.ToString().TrimEnd('\n');
        }

        public static void ReflectType(Type type)
        {
            if (type.Name.IndexOf("`") != -1)
            {
                return;
            }
            string typeNamespace = type.Namespace;
            string targetDirectory2 = targetDirectory + (notAddCSNamespace ? "" : "\\CS");
            if (typeNamespace != null)
            {
                string typeNamespace2 = typeNamespace.Replace('.', '\\');
                targetDirectory2 += "\\" + typeNamespace2;
            }
            Directory.CreateDirectory(targetDirectory2);
            string typeString1 = InnerReflectType(type);
            if (typeString1 == "" || typeString1 == null)
            {
                return;
            }
            string targetPath = String.Format("{0}\\{1}.lua", targetDirectory2, type.Name);
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(targetPath);
                streamWriter.Write(typeString1);
                anyFileChanged = true;
            }
            catch
            {
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }

        public static void BeginGenerate()
        {
            string targetDirectory1 = Application.dataPath + "\\.lpproj\\Apis\\Unity";
            if (Directory.Exists(targetDirectory1))
            {
                DirectoryInfo di = new DirectoryInfo(targetDirectory1);
                di.Delete(true);
            }
            targetDirectory = Application.dataPath + "\\.lpproj\\Apis\\Unity";
            anyFileChanged = false;
            Directory.CreateDirectory(targetDirectory);
            dictionaryNamespace.Clear();
        }

        public static void EndGenerate()
        {
            if (anyFileChanged)
            {
                UnityEditor.EditorUtility.DisplayDialog("Tips", "Lua apis generated.", "OK");
                AssetDatabase.Refresh();
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("Tips", "No interface changed.", "OK");
            }
        }
    }
}

#endif