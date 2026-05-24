using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// Mac构建命令 - 菜单 JeremyGame > Build Mac
/// </summary>
public class BuildCommand
{
    private const string BuildPath = "Builds/Mac/TeamDeathmatch2.app";
    private static readonly string[] Scenes = new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/TeamSelect.unity",
        "Assets/Scenes/GamePlay.unity",
        "Assets/Scenes/EndScreen.unity"
    };

    [MenuItem("JeremyGame/Build Mac")]
    public static void BuildMac()
    {
        Debug.Log("开始构建 Mac 版本...");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("构建成功! 输出: " + BuildPath);
            EditorUtility.RevealInFinder(BuildPath);
        }
        else
        {
            Debug.LogError("构建失败: " + report.summary.result);
            foreach (var step in report.steps)
            {
                foreach (var message in step.messages)
                {
                    if (message.type == LogType.Error)
                    {
                        Debug.LogError("  " + message.content);
                    }
                }
            }
        }
    }

    [MenuItem("JeremyGame/Copy Scripts to Unity Project")]
    public static void CopyScriptsToProject()
    {
        string srcDir = "Assets/Scripts/";
        string dstDir = "../My project/Assets/Scripts/";

        if (!System.IO.Directory.Exists(dstDir))
        {
            System.IO.Directory.CreateDirectory(dstDir);
        }

        // 复制所有 .cs 文件
        string[] files = System.IO.Directory.GetFiles(srcDir, "*.cs", System.IO.SearchOption.AllDirectories);
        int copied = 0;

        foreach (string srcFile in files)
        {
            // 跳过损坏的测试文件
            if (srcFile.Contains("_BrokenTests")) continue;

            string relPath = srcFile.Substring(srcDir.Length);
            string dstFile = dstDir + relPath;

            string dstDirPath = System.IO.Path.GetDirectoryName(dstFile);
            if (!System.IO.Directory.Exists(dstDirPath))
            {
                System.IO.Directory.CreateDirectory(dstDirPath);
            }

            System.IO.File.Copy(srcFile, dstFile, true);
            copied++;
        }

        Debug.Log($"复制了 {copied} 个脚本文件到 {dstDir}");

        // 刷新资源
        AssetDatabase.Refresh();
    }
}
