#if UNITY_EDITOR && UNITY_WEBGL
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

            AddScriptToIndexHTML(pathToBuiltProject, "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/3.1.19/signalr.min.js");// "https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.19/dist/browser/signalr.min.js");
        }

        private static void AddScriptToIndexHTML(string pathToBuiltProject, string scriptSrc)
        {
            string indexFilePath = Path.Combine(pathToBuiltProject, "index.html");
            if (File.Exists(indexFilePath))
            {
                string indexHTML = File.ReadAllText(indexFilePath);
                indexHTML = indexHTML.Replace("</body>", $"\t<script src=\"{scriptSrc}\"></script>\n</body>");
                File.WriteAllText(indexFilePath, indexHTML);
                Debug.Log("[PostProcessBuild] Add Script To IndexHTML: " + scriptSrc);
            }
        }
    }
}
#endif