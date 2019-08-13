using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBrowser.AssetBundleDataSource
{
    internal class AssetDatabaseABDataSource : IABDataSource
    {
        public static List<IABDataSource> CreateDataSources()
        {
            var op = new AssetDatabaseABDataSource();
            var retList = new List<IABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name{ get { return "Default"; } }
        public string ProviderName { get { return "Built-in"; } }

        public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
        }

        public string GetAssetBundleName(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return string.Empty;
            }
            var bundleName = importer.assetBundleName;
            if (importer.assetBundleVariant.Length > 0)
            {
                bundleName = bundleName + "." + importer.assetBundleVariant;
            }
            return bundleName;
        }

        public string GetImplicitAssetBundleName(string assetPath)
        {
            return AssetDatabase.GetImplicitAssetBundleName(assetPath);
        }

        public string[] GetAllAssetBundleNames()
        {
            return AssetDatabase.GetAllAssetBundleNames();
        }

        public bool IsReadOnly => false;

        public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
        {
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, variantName);
        }

        public void RemoveUnusedAssetBundleNames()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public bool CanSpecifyBuildTarget
        {
            get { return true; }
        }
        public bool CanSpecifyBuildOutputDirectory
        {
            get { return true; }
        }

        public bool CanSpecifyBuildOptions
        {
            get { return true; }
        }

        public bool BuildAssetBundles(ABBuildInfo info)
        {
            if (info == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            var outPath = info.OutputDirectory + "/assets_dir";
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

            var buildManifest = BuildPipeline.BuildAssetBundles(outPath, info.Options, info.BuildTarget);
            if (buildManifest == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                info.OnBuild?.Invoke(assetBundleName);
            }
            return true;
        }
    }
}
