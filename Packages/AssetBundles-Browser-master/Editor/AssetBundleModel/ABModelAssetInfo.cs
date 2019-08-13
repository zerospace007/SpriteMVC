using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using System.IO;

namespace AssetBundleBrowser.AssetBundleModel
{
    internal sealed class AssetTreeItem : TreeViewItem
    {
        internal AssetInfo AssetInfo { get; }

        internal AssetTreeItem() : base(-1, -1) { }
        internal AssetTreeItem(AssetInfo asset) : base(asset != null ? asset.FullAssetName.GetHashCode() : Random.Range(int.MinValue, int.MaxValue), 0, asset != null ? asset.DisplayName : "failed")
        {
            AssetInfo = asset;
            if (asset != null)
                icon = AssetDatabase.GetCachedIcon(asset.FullAssetName) as Texture2D;
        }

        private Color m_color = new Color(0, 0, 0, 0);
        internal Color itemColor
        {
            get
            {
                if (m_color.a == 0.0f && AssetInfo != null)
                {
                    m_color = AssetInfo.Color;
                }
                return m_color;
            }
            set { m_color = value; }
        }

        internal Texture2D MessageIcon => MessageSystem.GetIcon(HighestMessageLevel);

        internal MessageType HighestMessageLevel => AssetInfo != null ? AssetInfo.HighestMessageLevel() : MessageType.Error;

        internal bool ContainsChild(AssetInfo asset)
        {
            bool contains = false;
            if (children == null)
                return contains;

            if (asset == null)
                return false;
            foreach (var child in children)
            {
                var childNode = child as AssetTreeItem;
                if (childNode != null && childNode.AssetInfo != null && childNode.AssetInfo.FullAssetName == asset.FullAssetName)
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }
    }

    internal class AssetInfo
    {
        internal bool IsScene { get; set; }
        internal bool IsFolder { get; set; }
        internal long fileSize;
        internal string DisplayName { get; private set; }

        private HashSet<string> m_Parents;
        private string m_AssetName;
        private string m_BundleName;
        private MessageSystem.MessageState m_AssetMessages = new MessageSystem.MessageState();

        internal AssetInfo(string inName, string bundleName = "")
        {
            FullAssetName = inName;
            m_BundleName = bundleName;
            m_Parents = new HashSet<string>();
            IsScene = false;
            IsFolder = false;
        }

        internal string FullAssetName
        {
            get { return m_AssetName; }
            set
            {
                m_AssetName = value;
                DisplayName = Path.GetFileNameWithoutExtension(m_AssetName);

                //TODO - maybe there's a way to ask the AssetDatabase for this size info.
                FileInfo fileInfo = new FileInfo(m_AssetName);
                fileSize = fileInfo.Exists ? fileInfo.Length : 0;
            }
        }

        internal string bundleName { get { return string.IsNullOrEmpty(m_BundleName) ? "auto" : m_BundleName; } }

        internal Color Color => string.IsNullOrEmpty(m_BundleName) ? Model.k_LightGrey : Color.white;

        internal bool IsMessageSet(MessageSystem.MessageFlag flag)
        {
            return m_AssetMessages.IsSet(flag);
        }
        internal void SetMessageFlag(MessageSystem.MessageFlag flag, bool on)
        {
            m_AssetMessages.SetFlag(flag, on);
        }
        internal MessageType HighestMessageLevel()
        {
            return m_AssetMessages.HighestMessageLevel();
        }
        internal IEnumerable<MessageSystem.Message> GetMessages()
        {
            List<MessageSystem.Message> messages = new List<MessageSystem.Message>();
            if (IsMessageSet(MessageSystem.MessageFlag.SceneBundleConflict))
            {
                var message = DisplayName + "\n";
                if (IsScene)
                    message += "Is a scene that is in a bundle with non-scene assets. Scene bundles must have only one or more scene assets.";
                else
                    message += "Is included in a bundle with a scene. Scene bundles must have only one or more scene assets.";
                messages.Add(new MessageSystem.Message(message, MessageType.Error));
            }
            if (IsMessageSet(MessageSystem.MessageFlag.DependencySceneConflict))
            {
                var message = DisplayName + "\n";
                message += MessageSystem.GetMessage(MessageSystem.MessageFlag.DependencySceneConflict).message;
                messages.Add(new MessageSystem.Message(message, MessageType.Error));
            }
            if (IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
            {
                var bundleNames = AssetBundleModel.Model.CheckDependencyTracker(this);
                string message = DisplayName + "\n" + "Is auto-included in multiple bundles:\n";
                foreach (var bundleName in bundleNames)
                {
                    message += bundleName + ", ";
                }
                message = message.Substring(0, message.Length - 2);//remove trailing comma.
                messages.Add(new MessageSystem.Message(message, MessageType.Warning));
            }

            if (string.IsNullOrEmpty(m_BundleName) && m_Parents.Count > 0)
            {
                //TODO - refine the parent list to only include those in the current asset list
                var message = DisplayName + "\n" + "Is auto included in bundle(s) due to parent(s): \n";
                foreach (var parent in m_Parents)
                {
                    message += parent + ", ";
                }
                message = message.Substring(0, message.Length - 2);//remove trailing comma.
                messages.Add(new MessageSystem.Message(message, MessageType.Info));
            }

            if (m_dependencies != null && m_dependencies.Count > 0)
            {
                var message = string.Empty;
                var sortedDependencies = m_dependencies.OrderBy(d => d.bundleName);
                foreach (var dependent in sortedDependencies)
                {
                    if (dependent.bundleName != bundleName)
                    {
                        message += dependent.bundleName + " : " + dependent.DisplayName + "\n";
                    }
                }
                if (string.IsNullOrEmpty(message) == false)
                {
                    message = message.Insert(0, DisplayName + "\n" + "Is dependent on other bundle's asset(s) or auto included asset(s): \n");
                    message = message.Substring(0, message.Length - 1);//remove trailing line break.
                    messages.Add(new MessageSystem.Message(message, MessageType.Info));
                }
            }

            messages.Add(new MessageSystem.Message(DisplayName + "\n" + "Path: " + FullAssetName, MessageType.Info));

            return messages;
        }

        internal void AddParent(string name)
        {
            m_Parents.Add(name);
        }

        internal void RemoveParent(string name)
        {
            m_Parents.Remove(name);
        }

        internal string GetSizeString()
        {
            if (fileSize == 0)
                return "--";
            return EditorUtility.FormatBytes(fileSize);
        }

        List<AssetInfo> m_dependencies = null;

        internal List<AssetInfo> GetDependencies()
        {
            //TODO - not sure this refreshes enough. need to build tests around that.
            if (m_dependencies == null)
            {
                m_dependencies = new List<AssetInfo>();
                if (AssetDatabase.IsValidFolder(m_AssetName))
                {
                    //if we have a folder, its dependencies were already pulled in through alternate means.  no need to GatherFoldersAndFiles
                    //GatherFoldersAndFiles();
                }
                else
                {
                    foreach (var dep in AssetDatabase.GetDependencies(m_AssetName, true))
                    {
                        if (dep != m_AssetName)
                        {
                            var asset = Model.CreateAsset(dep, this);
                            if (asset != null) m_dependencies.Add(asset);
                        }
                    }
                }
            }
            return m_dependencies;
        }

    }

}
