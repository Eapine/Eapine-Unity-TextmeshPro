using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TMPro
{
    [Serializable][ExcludeFromPresetAttribute]
    public class TMP_RuntimeFontSettings : ScriptableObject
    {
        private static TMP_RuntimeFontSettings s_Instance;

        [Serializable]
        public class FontConfig
        {
            public string nickName;
            public string path;
        }

        /// <summary>
        /// The relative path to a Resources folder in the project.
        /// </summary>
        public static List<FontConfig> FontNickNameAndPath
        {
            get { return Instance? Instance.m_fontNickNameAndPath : null; }
        }
        [SerializeField]
        private List<FontConfig> m_fontNickNameAndPath;

        /// <summary>
        /// 根据索引取得字体昵称。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>字体昵称。</returns>
        public static string GetNickNameByIndex(int index)
        {
            if (FontNickNameAndPath == null || index < 0 || index >= FontNickNameAndPath.Count)
            {
                return null;
            }
            return FontNickNameAndPath[index].nickName;
        }

        /// <summary>
        /// 根据索引取得字体路径。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>字体路径。</returns>
        public static string GetPathByIndex(int index)
        {
            if (FontNickNameAndPath == null || index < 0 || index >= FontNickNameAndPath.Count)
            {
                return null;
            }
            return FontNickNameAndPath[index].path;
        }

        /// <summary>
        /// 根据昵称取得字体路径。
        /// </summary>
        /// <param name="nickName">昵称。</param>
        /// <returns>字体路径。</returns>
        public static string GetPathByNickName(string nickName)
        {
            if (FontNickNameAndPath == null || nickName == null)
            {
                return null;
            }

            foreach (FontConfig fontConfig in FontNickNameAndPath)
            {
                if (fontConfig.nickName == nickName)
                {
                    return fontConfig.path;
                }
            }

            return null;
        }

        /// <summary>
        /// 取得字体数量。
        /// </summary>
        /// <returns></returns>
        public static int GetLength()
        {
            if (FontNickNameAndPath == null)
            {
                return 0;
            }
            return FontNickNameAndPath.Count;
        }

        /// <summary>
        /// Get a singleton instance of the settings class.
        /// </summary>
        public static TMP_RuntimeFontSettings Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<TMP_RuntimeFontSettings>("TMP RuntimeFont Settings");

                    #if UNITY_EDITOR
                    // Make sure TextMesh Pro UPM packages resources have been added to the user project
                    if (s_Instance == null)
                    {
                        // Open TMP Resources Importer
                        TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
                    }
                    #endif
                }

                return s_Instance;
            }
        }


        /// <summary>
        /// Static Function to load the TMP RuntimeFontSettings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_RuntimeFontSettings LoadDefaultSettings()
        {
            if (s_Instance == null)
            {
                // Load settings from TMP_RuntimeFontSettings file
                TMP_RuntimeFontSettings settings = Resources.Load<TMP_RuntimeFontSettings>("TMP RuntimeFont Settings");
                if (settings != null)
                    s_Instance = settings;
            }

            return s_Instance;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP RuntimeFontSettings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_RuntimeFontSettings GetSettings()
        {
            if (Instance == null) return null;

            return Instance;
        }
        
#if UNITY_EDITOR
        public static void CreateTMPRuntimeFontSettings()
        {
            string path = "Assets/TextMesh Pro/Resources/TMP RuntimeFont Settings";

            TMP_RuntimeFontSettings settings = AssetDatabase.LoadAssetAtPath<TMP_RuntimeFontSettings>(path);
            if (settings != null)
            {
                Debug.LogError($"Exist : {path}");
                return;
            }

            settings = ScriptableObject.CreateInstance<TMP_RuntimeFontSettings>();
            AssetDatabase.CreateAsset(settings, path + ".asset");
            Debug.Log($"Create : {path}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
