#if UNITY_EDITOR && UNITY_WEBGL
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Fireball.Editor
{
    public class PostProcessFireballWebGL
    {
        [PostProcessBuild]
        public static void ChangeWebGLTemplate(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.WebGL)
                return;

            // add scripts here
        }

        private static void AddFireballGCLIScript(string pathToBuiltProject)
        {
            if (CopyAsset(pathToBuiltProject, "Packages/com.kau.fireball/Runtime/Modules/GameClientInterface/Plugins/WebGL/firebal-gci.js", "firebal-gci.js"))
            {
                AddScriptToIndexHTML(pathToBuiltProject, "firebal-gci.js");
            }
        }

        private static void AddScriptToIndexHTML(string pathToBuiltProject, string scriptSrc)
        {
            string indexFilePath = Path.Combine(pathToBuiltProject, "index.html");
            if (File.Exists(indexFilePath))
            {
                string indexHTML = File.ReadAllText(indexFilePath);
                string scriptTag = $"<script src=\"{scriptSrc}\"></script>";
                if (!indexHTML.Contains(scriptTag))
                {
                    indexHTML = indexHTML.Replace("</body>", $"\t{scriptTag}\n</body>");
                    File.WriteAllText(indexFilePath, indexHTML);
                    Debug.Log("[PostProcessBuild] Add Script To IndexHTML: " + scriptSrc);
                }
            }
        }

        private static bool CopyAsset(string pathToBuiltProject, string assetPath, string assetFileName)
        {
            string fullAssetPath = Path.Combine(Application.dataPath.Replace("Assets/", "").Replace("Assets", ""), assetPath);
            string destinationPath = Path.Combine(pathToBuiltProject, assetFileName);

            bool copyResult = false;
            try
            {
                File.Copy(fullAssetPath, destinationPath, true);
                copyResult = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PostProcessBuild] Copy Script Exception: {e}");
                copyResult = false;
            }

            Debug.Log($"[PostProcessBuild] Copy Asset: {assetFileName}, result = {copyResult}" +
                $"\n - Path from: {fullAssetPath}" +
                $"\n - Path into: {destinationPath}");

            return copyResult;
        }
    }
}
#endif