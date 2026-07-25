using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

namespace TMPro.EditorUtilities
{
    public static partial class TMPro_CreateObjectMenu
    {
        [MenuItem("Window/TextMeshPro/Create TMP RuntimeFont Settings", false, 2052)]
        public static void CreateTMPRuntimeFontSettings()
        {
            TMP_RuntimeFontSettings.CreateTMPRuntimeFontSettings();
        }

        [MenuItem("Window/TextMeshPro/Convert all TextMeshProUGUI to TMP_RuntimeFontUGUI", false, 2200)]
        public static void ConvertAll_TextMeshProUGUI_To_TMP_RuntimeFontUGUI()
        {
            ConvertComponentTtoWInAllPrefab<TextMeshProUGUI, TMP_RuntimeFontUGUI>((TextMeshProUGUI ori, TMP_RuntimeFontUGUI des) =>
            {
                des.font = null;
                
                string fontPath = AssetDatabase.GetAssetPath(ori.font);
                if (IsUnderResources(fontPath))
                {
                    fontPath = ToResourcesLoadPath(fontPath);
                }
                
                int index = TMP_RuntimeFontSettings.GetIndexByPath(fontPath);
                
                var so = new SerializedObject(des);
                so.Update();
                so.FindProperty("m_FontIndex").intValue = index;
                so.ApplyModifiedProperties();
                
                Debug.Log($"{fontPath} convert to {des.FontNickName}");
            });
        }

        [MenuItem("Window/TextMeshPro/Convert all TMP_RuntimeFontUGUI to TextMeshProUGUI", false, 2201)]
        public static void ConvertAll_TMP_RuntimeFontUGUI_To_TextMeshProUGUI()
        {
            ConvertComponentTtoWInAllPrefab<TMP_RuntimeFontUGUI, TextMeshProUGUI>((TMP_RuntimeFontUGUI ori, TextMeshProUGUI des) =>
            {
                string fontPath = ori.FontPath;
                if (string.IsNullOrEmpty(fontPath))
                {
                    Debug.LogError($"{ori.name} fontPath is null or empty");
                    return;
                }

                TMP_FontAsset fontAsset = null;
                bool isBuiltin = !fontPath.StartsWith("Assets/", System.StringComparison.Ordinal);
                if (isBuiltin)
                {
                    fontAsset = Resources.Load<TMP_FontAsset>(fontPath);
                }
                else
                {
                    fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
                }

                if (fontAsset == null)
                {
                    Debug.LogError($"{ori.name} {fontPath} fontAsset is null");
                    return;
                }

                des.font = fontAsset;

                Debug.Log($"{ori.FontNickName} convert to {des.font.name}");
            });
        }

        [MenuItem("Window/TextMeshPro/Print all TMP_RuntimeFontUGUI", false, 2202)]
        public static void PrintAll_TMP_RuntimeFontUGUI()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                TMP_RuntimeFontUGUI[] components = prefab.GetComponentsInChildren<TMP_RuntimeFontUGUI>(true);
                foreach (var item in components)
                {
                    Debug.Log($"{path} | {item.name}");
                }
            }
        }

        //把prefab中的所以T换成W
        public static void ConvertComponentTtoWInAllPrefab<T, W>(Action<T, W> action = null) where T : MonoBehaviour where W : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

            Debug.Log($"Convert Component {typeof(T)} to {typeof(W)} In All Prefab, Length:" + guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

                //实例化物体
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                bool flag = ConvertMonoInGameObject<T, W>(instance, action);

                // 将数据替换到asset
                PrefabUtility.SaveAsPrefabAsset(instance, path);

                instance.hideFlags = HideFlags.HideAndDontSave;

                //删除掉实例化的对象
                GameObject.DestroyImmediate(instance);

                if (flag)
                {
                    Debug.Log($"Path:{path} Convert {typeof(T)} to {typeof(W)}");
                }
            }
        }

        public static MonoScript FindMonoScript<T>() where T : MonoBehaviour
        {
            foreach (MonoScript script in Resources.FindObjectsOfTypeAll<MonoScript>())
            {
                if (script.GetClass() == typeof(T))
                {
                    return script;
                }
            }

            return null;
        }

        public static bool ConvertMonoInGameObject<T, W>(GameObject gameObject, Action<T, W> action = null) where T : MonoBehaviour where W : MonoBehaviour
        {
            MonoScript script = FindMonoScript<W>();

            bool flag = false;
            T[] components = gameObject.GetComponentsInChildren<T>(true);
            foreach (var item in components)
            {
                if (item.GetType() != typeof(T)) //判断类型绝对相等
                {
                    continue;
                }

                T backup = null;
                if (action != null)
                {
                    backup = UnityEngine.Object.Instantiate(item);
                }

                var so = new SerializedObject(item);
                so.Update();

                bool oldEnable = item.enabled;
                item.enabled = false;

                so.FindProperty("m_Script").objectReferenceValue = script;
                so.ApplyModifiedProperties();

                (so.targetObject as MonoBehaviour).enabled = oldEnable;

                if (action != null)
                {
                    action(backup, so.targetObject as W);
                    so.ApplyModifiedProperties();
                    UnityEngine.Object.DestroyImmediate(backup.gameObject);
                }

                flag = true;
            }

            return flag;
        }


        [MenuItem("GameObject/UI/RuntimeFont/Text - TextMeshPro", false, 2001)]
        public static void CreateTextMeshProGuiObjectPerform_RuntimeFont(MenuCommand menuCommand)
        {
            GameObject go = ObjectFactory.CreateGameObject("Text (TMP)");
            TMP_RuntimeFontUGUI textComponent = ObjectFactory.AddComponent<TMP_RuntimeFontUGUI>(go);

            if (textComponent.m_isWaitingOnResourceLoad == false)
            {
                // Get reference to potential Presets for <TMP_RuntimeFontUGUI> component
                Preset[] presets = Preset.GetDefaultPresetsForObject(textComponent);

                if (presets == null || presets.Length == 0)
                {
                    textComponent.fontSize = TMP_Settings.defaultFontSize;
                    textComponent.color = Color.white;
                    textComponent.text = "New Text";
                }

                if (TMP_Settings.autoSizeTextContainer)
                {
                    Vector2 size = textComponent.GetPreferredValues(TMP_Math.FLOAT_MAX, TMP_Math.FLOAT_MAX);
                    textComponent.rectTransform.sizeDelta = size;
                }
                else
                {
                    textComponent.rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProUITextContainerSize;
                }
            }
            else
            {
                textComponent.fontSize = -99;
                textComponent.color = Color.white;
                textComponent.text = "New Text";
            }

            PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/RuntimeFont/Button - TextMeshPro", false, 2031)]
        public static void AddRuntimeFontButton(MenuCommand menuCommand)
        {
            GameObject go = TMP_DefaultControls.CreateButton(GetStandardResources());

            // Override font size
            TMP_Text textComponent = go.GetComponentInChildren<TMP_Text>();
            textComponent.fontSize = 24;

            ConvertMonoInGameObject<TextMeshProUGUI, TMP_RuntimeFontUGUI>(go);

            PlaceUIElementRoot(go, menuCommand);
        }
        
        public static bool IsUnderResources(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;

            string[] segments = assetPath.Replace('\\', '/').Split('/');
            foreach (var seg in segments)
            {
                if (string.Equals(seg, "Resources", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        
        public static string ToResourcesLoadPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return null;

            string[] segments = assetPath.Replace('\\', '/').Split('/');

            int resourcesIndex = -1;
            for (int i = 0; i < segments.Length; i++)
            {
                if (string.Equals(segments[i], "Resources", System.StringComparison.OrdinalIgnoreCase))
                {
                    resourcesIndex = i;
                    break;
                }
            }

            // 不在 Resources 下，或路径本身就是 Resources 根目录
            if (resourcesIndex < 0 || resourcesIndex >= segments.Length - 1)
                return null;

            var sb = new StringBuilder();
            for (int i = resourcesIndex + 1; i < segments.Length; i++)
            {
                string seg = segments[i];
                if (i == segments.Length - 1)
                    seg = Path.GetFileNameWithoutExtension(seg); // 去掉扩展名
                if (sb.Length > 0)
                    sb.Append('/');
                sb.Append(seg);
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }
    }
}