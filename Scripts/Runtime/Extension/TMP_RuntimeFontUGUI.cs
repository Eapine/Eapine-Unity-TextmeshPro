using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TMPro
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/TextMeshPro - Runtime Font Text (UI)", 11)]
    [ExecuteAlways]
    public class TMP_RuntimeFontUGUI : TextMeshProUGUI
    {
        [SerializeField]
        private int m_FontIndex = 0;

        public string FontNickName
        {
            get { return TMP_RuntimeFontSettings.GetNickNameByIndex(m_FontIndex); }
        }

        public string FontPath
        {
            get { return TMP_RuntimeFontSettings.GetPathByIndex(m_FontIndex); }
        }

        public override TMP_FontAsset font
        {
            get { return m_runtimeFontAsset; }
            set { if (m_runtimeFontAsset == value) return; m_runtimeFontAsset = value; LoadFontAsset(); m_havePropertiesChanged = true; SetVerticesDirty(); SetLayoutDirty(); }
        }
        private TMP_FontAsset m_runtimeFontAsset;

        protected override void OnEnable()
        {
            if (m_fontAsset != null)
            {
                Debug.LogWarning($"{name} has default font {m_fontAsset.name}");
                m_fontAsset = null;//避免序列化
            }

            if (string.IsNullOrEmpty(FontNickName))
            {
                Debug.LogWarning($"{name} {m_FontIndex} {FontNickName}");
            }
            else
            {
                if (m_FontAssetDict.ContainsKey(FontNickName))
                {
                    font = m_FontAssetDict[FontNickName];
                }
            }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            font = null;
            base.OnDisable();
        }

        private static Dictionary<string, TMP_FontAsset> m_FontAssetDict = new Dictionary<string, TMP_FontAsset>(8);

        public static TMP_FontAsset GetFontAsset(int fontIndex)
        {
            string fontNickName = TMP_RuntimeFontSettings.GetNickNameByIndex(fontIndex);
            return GetFontAsset(fontNickName);
        }

        public static TMP_FontAsset GetFontAsset(string fontNickName)
        {
            if (string.IsNullOrEmpty(fontNickName))
            {
                return null;
            }

            if (!m_FontAssetDict.ContainsKey(fontNickName))
            {
                return null;
            }

            return m_FontAssetDict[fontNickName];
        }

        public static void AddFontAsset(string fontNickName, Font font)
        {
            if (string.IsNullOrEmpty(fontNickName))
            {
                Debug.LogError($"AddFontAsset Error, fontNickName is null or empty");
                return;
            }

            if (font == null)
            {
                Debug.LogError($"AddFontAsset Error, font is null");
                return;
            }

            if (m_FontAssetDict.ContainsKey(fontNickName))
            {
                Debug.LogError($"AddFontAsset Error, ContainsKey: {fontNickName}");
                return;
            }

            TMP_FontAsset fontAsset = TextMeshProExtension.CreateDynamicFontAsset(font, 90, 9, 2048);
            fontAsset.name = font.name;
            AddFontAsset(fontNickName, fontAsset);
        }

        public static void AddFontAsset(string fontNickName, TMP_FontAsset fontAsset)
        {
            if (string.IsNullOrEmpty(fontNickName))
            {
                Debug.LogError($"AddFontAsset Error, fontNickName is null or empty");
                return;
            }

            if (fontAsset == null)
            {
                Debug.LogError($"AddFontAsset Error, fontAsset is null");
                return;
            }

            if (m_FontAssetDict.ContainsKey(fontNickName))
            {
                Debug.LogError($"AddFontAsset Error, ContainsKey: {fontNickName}");
                return;
            }

            m_FontAssetDict.Add(fontNickName, fontAsset);
        }

        public static void RemoveFontAsset(string fontNickName)
        {
            if (m_FontAssetDict.ContainsKey(fontNickName))
            {
                if (m_FontAssetDict[fontNickName] != null)
                {
                    m_FontAssetDict[fontNickName].ClearFontAssetData();
                    ScriptableObject.Destroy(m_FontAssetDict[fontNickName]);
                }
                m_FontAssetDict.Remove(fontNickName);
            }
        }

        public static void ClearFontAsset()
        {
            foreach (var item in m_FontAssetDict)
            {
                if (item.Value != null)
                {
                    item.Value.ClearFontAssetData();
                    ScriptableObject.Destroy(item.Value);
                }
            }
            m_FontAssetDict.Clear();
        }

        protected override void LoadFontAsset()
        {
            ShaderUtilities.GetShaderPropertyIDs(); // Initialize & Get shader property IDs.

#if UNITY_EDITOR
            TryAddFontInEditor();
#endif

            if (font == null)
            {
                return;
            }
            base.LoadFontAsset();
        }

#if UNITY_EDITOR
        private void TryAddFontInEditor()
        {
            if (FontNickName == null)
            {
                return;
            }

            if (m_runtimeFontAsset == null)
            {
                if (m_FontAssetDict.ContainsKey(FontNickName) && m_FontAssetDict[FontNickName] == null)
                {
                    RemoveFontAsset(FontNickName);
                }

                if (!m_FontAssetDict.ContainsKey(FontNickName))
                {
                    Object fontData = AssetDatabase.LoadAssetAtPath<Object>(FontPath);
                    if (fontData == null)
                    {
                        Debug.LogError($"{FontPath}");
                    }
                    else
                    {
                        if (fontData is Font)
                        {
                            AddFontAsset(FontNickName, fontData as Font);
                            m_runtimeFontAsset = m_FontAssetDict[FontNickName];
                        }
                        else if (fontData is TMP_FontAsset)
                        {
                            AddFontAsset(FontNickName, fontData as TMP_FontAsset);
                            m_runtimeFontAsset = m_FontAssetDict[FontNickName];
                        }
                        else
                        {
                            Debug.LogError($"{FontPath} {fontData.GetType()}");
                        }
                    }
                }
            }
        }
#endif
    }
}