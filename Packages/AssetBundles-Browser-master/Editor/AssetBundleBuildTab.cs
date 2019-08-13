using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AssetBundleBrowser.AssetBundleDataSource;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBrowser
{
    [Serializable]
    internal class AssetBundleBuildTab
    {
        private readonly string m_streamingPath = "Assets/StreamingAssets";
        [SerializeField]
        private bool m_AdvancedSettings;
        [SerializeField]
        private Vector2 m_ScrollPosition;

        private class ToggleData
        {
            internal ToggleData(bool isState,
                string title,
                string tooltip,
                List<string> onToggles,
                BuildAssetBundleOptions opt = BuildAssetBundleOptions.None)
            {
                state = onToggles.Contains(title) ? true : isState;
                content = new GUIContent(title, tooltip);
                option = opt;
            }
            internal bool state;
            internal GUIContent content;
            internal BuildAssetBundleOptions option;
        }

        private AssetBundleInspectTab m_InspectTab;

        [SerializeField]
        private BuildTabData m_UserData;
        private List<ToggleData> m_ToggleData;
        private ToggleData m_ForceRebuild;
        private ToggleData m_CopyToStreaming;
        private GUIContent m_TargetContent;
        private GUIContent m_CompressionContent;

        internal enum CompressOptions
        {
            Uncompressed = 0,
            StandardCompression,
            ChunkBasedCompression,
        }

        private readonly GUIContent[] m_CompressionOptions =
        {
            new GUIContent("No Compression"),
            new GUIContent("Standard Compression (LZMA)"),
            new GUIContent("Chunk Based Compression (LZ4)")
        };
        private readonly int[] m_CompressionValues = { 0, 1, 2 };


        internal AssetBundleBuildTab()
        {
            m_AdvancedSettings = false;
            m_UserData = new BuildTabData();
            m_UserData.m_OnToggles = new List<string>();
            m_UserData.m_UseDefaultPath = true;
        }

        internal void OnDisable()
        {
            var dataPath = Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/AssetBundleBrowserBuild.dat";

            BinaryFormatter binary = new BinaryFormatter();
            FileStream file = File.Create(dataPath);

            binary.Serialize(file, m_UserData);
            file.Close();

        }
        internal void OnEnable(EditorWindow parent)
        {
            m_InspectTab = (parent as AssetBundleBrowserMain).m_InspectTab;

            //LoadData...
            var dataPath = Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/AssetBundleBrowserBuild.dat";

            if (File.Exists(dataPath))
            {
                BinaryFormatter binary = new BinaryFormatter();
                FileStream file = File.Open(dataPath, FileMode.Open);
                var data = binary.Deserialize(file) as BuildTabData;
                if (data != null) m_UserData = data;
                file.Close();
            }

            m_ToggleData = new List<ToggleData>();
            m_ToggleData.Add(new ToggleData(
                false,
                "Exclude Type Information",
                "Do not include type information within the asset bundle (don't write type tree).",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.DisableWriteTypeTree));
            m_ToggleData.Add(new ToggleData(
                false,
                "Force Rebuild",
                "Force rebuild the asset bundles",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.ForceRebuildAssetBundle));
            m_ToggleData.Add(new ToggleData(
                false,
                "Ignore Type Tree Changes",
                "Ignore the type tree changes when doing the incremental build check.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.IgnoreTypeTreeChanges));
            m_ToggleData.Add(new ToggleData(
                false,
                "Append Hash",
                "Append the hash to the assetBundle name.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.AppendHashToAssetBundleName));
            m_ToggleData.Add(new ToggleData(
                false,
                "Strict Mode",
                "Do not allow the build to succeed if any errors are reporting during it.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.StrictMode));
            m_ToggleData.Add(new ToggleData(
                false,
                "Dry Run Build",
                "Do a dry run build.",
                m_UserData.m_OnToggles,
                BuildAssetBundleOptions.DryRunBuild));


            m_ForceRebuild = new ToggleData(
                false,
                "Clear Folders",
                "Will wipe out all contents of build directory as well as StreamingAssets/AssetBundles if you are choosing to copy build there.",
                m_UserData.m_OnToggles);
            m_CopyToStreaming = new ToggleData(
                false,
                "Copy to StreamingAssets",
                "After build completes, will copy all build content to " + m_streamingPath + " for use in stand-alone player.",
                m_UserData.m_OnToggles);

            m_TargetContent = new GUIContent("Build Target", "Choose target platform to build for.");
            m_CompressionContent = new GUIContent("Compression", "Choose no compress, standard (LZMA), or chunk based (LZ4)");

            if (m_UserData.m_UseDefaultPath)
            {
                ResetPathToDefault();
            }
        }

        internal void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            bool newState = false;
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Example build setup"), centeredStyle);
            //basic options
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            // build target
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildTarget))
            {
                ValidBuildTarget tgt = (ValidBuildTarget)EditorGUILayout.EnumPopup(m_TargetContent, m_UserData.m_BuildTarget);
                if (tgt != m_UserData.m_BuildTarget)
                {
                    m_UserData.m_BuildTarget = tgt;
                    if (m_UserData.m_UseDefaultPath)
                    {
                        m_UserData.m_OutputPath = "AssetBundles/";
                        m_UserData.m_OutputPath += m_UserData.m_BuildTarget.ToString();
                    }
                }
            }

            ////output path
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory))
            {
                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                var newPath = EditorGUILayout.TextField("Output Path", m_UserData.m_OutputPath);
                if (!string.IsNullOrEmpty(newPath) && newPath != m_UserData.m_OutputPath)
                {
                    m_UserData.m_UseDefaultPath = false;
                    m_UserData.m_OutputPath = newPath;
                    //EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", m_OutputPath);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
                    BrowseForFolder();
                if (GUILayout.Button("Reset", GUILayout.MaxWidth(75f)))
                    ResetPathToDefault();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                newState = GUILayout.Toggle(
                    m_ForceRebuild.state,
                    m_ForceRebuild.content);
                if (newState != m_ForceRebuild.state)
                {
                    if (newState)
                        m_UserData.m_OnToggles.Add(m_ForceRebuild.content.text);
                    else
                        m_UserData.m_OnToggles.Remove(m_ForceRebuild.content.text);
                    m_ForceRebuild.state = newState;
                }
                newState = GUILayout.Toggle(
                    m_CopyToStreaming.state,
                    m_CopyToStreaming.content);
                if (newState != m_CopyToStreaming.state)
                {
                    if (newState)
                        m_UserData.m_OnToggles.Add(m_CopyToStreaming.content.text);
                    else
                        m_UserData.m_OnToggles.Remove(m_CopyToStreaming.content.text);
                    m_CopyToStreaming.state = newState;
                }
            }

            // advanced options
            using (new EditorGUI.DisabledScope(!AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions))
            {
                EditorGUILayout.Space();
                m_AdvancedSettings = EditorGUILayout.Foldout(m_AdvancedSettings, "Advanced Settings");
                if (m_AdvancedSettings)
                {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    CompressOptions cmp = (CompressOptions)EditorGUILayout.IntPopup(
                        m_CompressionContent,
                        (int)m_UserData.m_Compression,
                        m_CompressionOptions,
                        m_CompressionValues);

                    if (cmp != m_UserData.m_Compression)
                    {
                        m_UserData.m_Compression = cmp;
                    }
                    foreach (var tog in m_ToggleData)
                    {
                        newState = EditorGUILayout.ToggleLeft(
                            tog.content,
                            tog.state);
                        if (newState != tog.state)
                        {

                            if (newState)
                                m_UserData.m_OnToggles.Add(tog.content.text);
                            else
                                m_UserData.m_OnToggles.Remove(tog.content.text);
                            tog.state = newState;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = indent;
                }
            }

            // build.
            EditorGUILayout.Space();
            if (GUILayout.Button("Build"))
            {
                EditorApplication.delayCall += ExecuteBuild;
            }
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void ExecuteBuild()
        {
            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOutputDirectory)
            {
                if (string.IsNullOrEmpty(m_UserData.m_OutputPath))
                    BrowseForFolder();

                if (string.IsNullOrEmpty(m_UserData.m_OutputPath)) //in case they hit "cancel" on the open browser
                {
                    Debug.LogError("AssetBundle Build: No valid output path for build.");
                    return;
                }

                if (m_ForceRebuild.state)
                {
                    string message = "Do you want to delete all files in the directory " + m_UserData.m_OutputPath;
                    if (m_CopyToStreaming.state)
                        message += " and " + m_streamingPath;
                        message += "?";
                    if (EditorUtility.DisplayDialog("File delete confirmation", message, "Yes", "No"))
                    {
                        try
                        {
                            if (Directory.Exists(m_UserData.m_OutputPath))
                                Directory.Delete(m_UserData.m_OutputPath, true);

                            if (m_CopyToStreaming.state)
                                if (Directory.Exists(m_streamingPath))
                                    Directory.Delete(m_streamingPath, true);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
                if (!Directory.Exists(m_UserData.m_OutputPath))
                    Directory.CreateDirectory(m_UserData.m_OutputPath);
            }

            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;

            if (AssetBundleModel.Model.DataSource.CanSpecifyBuildOptions)
            {
                if (m_UserData.m_Compression == CompressOptions.Uncompressed)
                    opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
                else if (m_UserData.m_Compression == CompressOptions.ChunkBasedCompression)
                    opt |= BuildAssetBundleOptions.ChunkBasedCompression;
                foreach (var tog in m_ToggleData)
                {
                    if (tog.state) opt |= tog.option;
                }
            }

            ABBuildInfo buildInfo = new ABBuildInfo();

            buildInfo.OutputDirectory = m_UserData.m_OutputPath;
            buildInfo.Options = opt;
            buildInfo.BuildTarget = (BuildTarget)m_UserData.m_BuildTarget;
            buildInfo.OnBuild = (assetBundleName) =>
            {
                if (m_InspectTab == null) return;
                m_InspectTab.AddBundleFolder(buildInfo.OutputDirectory);
                m_InspectTab.RefreshBundles();
            };

            AssetBundleModel.Model.DataSource.BuildAssetBundles(buildInfo);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (m_CopyToStreaming.state)
                DirectoryCopy(m_UserData.m_OutputPath, m_streamingPath);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }

        private void BrowseForFolder()
        {
            m_UserData.m_UseDefaultPath = false;
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", m_UserData.m_OutputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                m_UserData.m_OutputPath = newPath;
            }
        }
        private void ResetPathToDefault()
        {
            m_UserData.m_UseDefaultPath = true;
            m_UserData.m_OutputPath = "AssetBundles/";
            m_UserData.m_OutputPath += m_UserData.m_BuildTarget.ToString();
        }

        //Note: this is the provided BuildTarget enum with some entries removed as they are invalid in the dropdown
        internal enum ValidBuildTarget
        {
            //NoTarget = -2,        --doesn't make sense
            //iPhone = -1,          --deprecated
            //StandaloneOSXUniversal = 2,
            //StandaloneOSXIntel = 4,
            StandaloneWindows = 5,
            iOS = 9,
            Android = 13,
            //StandaloneLinux = 17,
            //StandaloneWindows64 = 19,
            //StandaloneLinux64 = 24,
            //StandaloneLinuxUniversal = 25,
            ////StandaloneOSXIntel64 = 27,
        }

        [Serializable]
        internal class BuildTabData
        {
            internal List<string> m_OnToggles;
            internal ValidBuildTarget m_BuildTarget = ValidBuildTarget.StandaloneWindows;
            internal CompressOptions m_Compression = CompressOptions.StandardCompression;
            internal string m_OutputPath = string.Empty;
            internal bool m_UseDefaultPath = true;
        }
    }

}