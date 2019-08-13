#region
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Framework.Utility;
using UnityEditor;
using UnityEngine;
#endregion

public class Packager
{
    #region Feilds And Properties
    private const string LuaTempDir = "LuaTemp/";                   //临时目录
    private static List<string> mPaths = new List<string>();        //目录列表
    private static List<string> mFiles = new List<string>();        //文件列表
	private static List<AssetBundleBuild> mMaps = new List<AssetBundleBuild>(); //打包列表
    #endregion

    #region Methods

    [MenuItem("Game/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource()
    {
        BuildAssetResource(BuildTarget.iOS);
    }

    [MenuItem("Game/Build Android Resource", false, 101)]
    public static void BuildAndroidResource()
    {
        BuildAssetResource(BuildTarget.Android);
    }

    [MenuItem("Game/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource()
    {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target)
    { 
        mMaps.Clear();
        string resPath = Util.ContentPath;

        string luaPath = resPath + "lua/";
        if (Directory.Exists(luaPath)) Directory.Delete(luaPath, true);
        Directory.CreateDirectory(luaPath);
        string versionFile = resPath + GameConst.VersionBytes;
        if (File.Exists(versionFile)) File.Delete(versionFile);

        if (GameConst.LuaBundleMode) HandleLuaBundle();
        else  HandleLuaFile();

        BuildPipeline.BuildAssetBundles(resPath, mMaps.ToArray(), BuildAssetBundleOptions.None, target);
        string streamFile = resPath + "StreamingAssets";
        if (File.Exists(streamFile))
        {
            File.Delete(streamFile);
            File.Delete(streamFile + ".manifest");
        }
        BuildVersionFile();

        string tempDir = Application.dataPath + "/" + LuaTempDir;
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 添加到打包Map
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="pattern"></param>
    /// <param name="path"></param>
    private static void AddBuildMap(string bundleName, string pattern, string path)
    {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        for (int node = 0; node < files.Length; node++)
        {
            files[node] = files[node].Replace('\\', '/');
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files;
        mMaps.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    private static void HandleLuaBundle()
    {
        string tempDir = Application.dataPath + "/" + LuaTempDir;
        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        string[] srcDirs = { CustomSettings.luaDir, CustomSettings.baseLuaDir };
        for (int node = 0; node < srcDirs.Length; node++)
        {
            if (GameConst.LuaByteMode)
            {
                string sourceDir = srcDirs[node];
                string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                {
                    --len;
                }
                for (int repo = 0; repo < files.Length; repo++)
                {
                    string str = files[repo].Remove(0, len);
                    string dest = tempDir + str + ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    EncodeLuaFile(files[repo], dest);
                }    
            }
            else
            {
                ToLuaMenu.CopyLuaBytesFiles(srcDirs[node], tempDir);
            }
        }
        string[] dirs = Directory.GetDirectories(tempDir, "*", SearchOption.AllDirectories);
        for (int node = 0; node < dirs.Length; node++)
        {
            string name = dirs[node].Replace(tempDir, string.Empty);
            name = name.Replace('\\', '_').Replace('/', '_');
            name = "lua/lua_" + name.ToLower() + GameConst.BundleSuffix;

            string path = "Assets" + dirs[node].Replace(Application.dataPath, "");
            AddBuildMap(name, "*.bytes", path);
        }
        AddBuildMap("lua/lua" + GameConst.BundleSuffix, "*.bytes", "Assets/" + LuaTempDir);

        //-------------------------------处理非Lua文件----------------------------------
        string luaPath = Application.dataPath + "/StreamingAssets/lua/";
        for (int node = 0; node < srcDirs.Length; node++)
        {
            mPaths.Clear(); mFiles.Clear();
            string luaDataPath = srcDirs[node].ToLower();
            Recursive(luaDataPath);
            foreach (string file in mFiles)
            {
                if (file.EndsWith(".meta") || file.EndsWith(".lua")) continue;
                string newfile = file.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(file);
                File.Copy(file, destfile, true);
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    private static void HandleLuaFile()
    {
        var luaPath = Util.ContentPath + "Lua/";
        //----------复制Lua文件----------------
        string[] luaPaths =
        {
            LuaConst.luaDir,
            LuaConst.toluaDir
        };

        foreach (var pathValue in luaPaths)
        {
            mPaths.Clear();
            mFiles.Clear();
            Recursive(pathValue);
            var index = 0;
            foreach (var fileName in mFiles)
            {
                if (fileName.EndsWith(".meta") || fileName.EndsWith(".bat")) continue;
                var newfile = fileName.Replace(pathValue, "");
                var newpath = luaPath + newfile;
                var path = Path.GetDirectoryName(newpath);
                if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath)) File.Delete(newpath);
                if (GameConst.LuaByteMode)
                {
                    EncodeLuaFile(fileName, newpath);
                }
				else
                {
                	File.Copy(fileName, newpath, true);
				}
                UpdateProgress(index++, mFiles.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 生成更新资源文件列表
    /// </summary>
    private static void BuildVersionFile()
    {
        var resPath = Application.dataPath + "/StreamingAssets/";
        //----------------------创建文件列表-----------------------
        var newFilePath = resPath + GameConst.VersionBytes;
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        mPaths.Clear();
        mFiles.Clear();
        Recursive(resPath);

        var fileStream = new FileStream(newFilePath, FileMode.CreateNew);
        var streamWriter = new StreamWriter(fileStream);
        foreach (var file in mFiles)
        {
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            var md5 = Util.GetMd5Code(file);
            var size = Util.FileSize(file);
            var value = file.Replace(resPath, string.Empty);
            streamWriter.WriteLine(value + "|" + md5 + "|" + size);
        }
        streamWriter.Close();
        fileStream.Close();
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    private static void Recursive(string path)
    {
        var files = Directory.GetFiles(path);
        var directories = Directory.GetDirectories(path);
        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;
            mFiles.Add(file.Replace('\\', '/'));
        }
        foreach (var dir in directories)
        {
            if (dir.Contains(".svn")) continue;
            mPaths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    /// <summary>
    /// 更新编译进度
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="progressMax"></param>
    /// <param name="desc"></param>
    private static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    /// <summary>
    /// 使用LuaJit或者luac编译加密lua文件
    /// </summary>
    /// <param name="srcFile">原始lua文件</param>
    /// <param name="outFile">输出lua文件</param>
    private static void EncodeLuaFile(string srcFile, string outFile)
    {
        if (!srcFile.ToLower().EndsWith(".lua"))
        {
            File.Copy(srcFile, outFile, true);
            return;
        }
        var isWin = true;
        var luaexe = string.Empty;
        var args = string.Empty;
        var exedir = string.Empty;
        var currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            luaexe = "luajit.exe";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = Application.dataPath.Replace("Assets", "") + "LuaEncoder/luajit/";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            luaexe = "./luajit";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = Application.dataPath.Replace("Assets", "") + "LuaEncoder/luajit_mac/";
        }
        Directory.SetCurrentDirectory(exedir);
        var processStartinfo = new ProcessStartInfo
        {
            FileName = luaexe,
            Arguments = args,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = isWin,
            ErrorDialog = true
        };
        Util.Log(processStartinfo.FileName + " " + processStartinfo.Arguments);

        var process = Process.Start(processStartinfo);
        if (process != null) process.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    //[MenuItem("Game/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile()
    {
        //"若使用编码Protobuf-lua-gen功能，需要自己配置外部环境！！"
        var dir = Application.dataPath + "/Lua/3rd/pblua";
        mPaths.Clear();
        mFiles.Clear();
        Recursive(dir);

        var protoc = "d:/protobuf-2.4.1/src/protoc.exe";
        var protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\"";

        foreach (var file in mFiles)
        {
            var name = Path.GetFileName(file);
            var extension = Path.GetExtension(file);
            if (extension != null && !extension.Equals(".proto")) continue;

            var processStartinfo = new ProcessStartInfo
            {
                FileName = protoc,
                Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                WorkingDirectory = dir,
                ErrorDialog = true
            };
            Util.Log(processStartinfo.FileName + " " + processStartinfo.Arguments);

            var process = Process.Start(processStartinfo);
            if (process != null) process.WaitForExit();
        }
        AssetDatabase.Refresh();
    }

    #endregion
}