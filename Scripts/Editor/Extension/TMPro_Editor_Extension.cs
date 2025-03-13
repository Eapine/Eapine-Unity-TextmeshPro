using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

namespace TMPro.EditorUtilities
{
    public static class TMPro_Editor_Extension
    {
        [MenuItem("Window/TextMeshPro/Create TMP RuntimeFont Settings", false, 2052)]
        public static void CreateTMPRuntimeFontSettings()
        {
            TMP_RuntimeFontSettings.CreateTMPRuntimeFontSettings();
        }

        [MenuItem("Window/TextMeshPro/Convert all TextMeshProUGUI to TMP_RuntimeFontUGUI", false, 2200)]
        public static void ConvertAll_TextMeshProUGUI_To_TMP_RuntimeFontUGUI()
        {
            ConvertComponentTtoWInAllPrefab<TextMeshProUGUI, TMP_RuntimeFontUGUI>();
        }

        //把prefab中的所以T换成W
        public static void ConvertComponentTtoWInAllPrefab<T, W>() where T : MonoBehaviour where W : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });

            Debug.Log($"Convert Component {typeof(T)} to {typeof(W)} In All Prefab, Length:" + guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

                //实例化物体
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                bool flag = ConvertMonoInGameObject<T, W>(instance);

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

        public static bool ConvertMonoInGameObject<T, W>(GameObject gameObject) where T : MonoBehaviour where W : MonoBehaviour
        {
            MonoScript script = FindMonoScript<W>();

            bool flag = false;
            T[] components = gameObject.GetComponentsInChildren<T>(true);
            foreach (var item in components)
            {
                if (item.GetType() != typeof(T))//判断类型绝对相等
                {
                    continue;
                }

                var so = new SerializedObject(item);
                so.Update();

                bool oldEnable = item.enabled;
                item.enabled = false;

                so.FindProperty("m_Script").objectReferenceValue = script;
                so.ApplyModifiedProperties();

                (so.targetObject as MonoBehaviour).enabled = oldEnable;
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

            TMPro_CreateObjectMenu.PlaceUIElementRoot(go, menuCommand);
        }

        [MenuItem("GameObject/UI/RuntimeFont/Button - TextMeshPro", false, 2031)]
        public static void AddButton(MenuCommand menuCommand)
        {
            GameObject go = TMP_DefaultControls.CreateButton(TMPro_CreateObjectMenu.GetStandardResources());

            // Override font size
            TMP_Text textComponent = go.GetComponentInChildren<TMP_Text>();
            textComponent.fontSize = 24;

            ConvertMonoInGameObject<TextMeshProUGUI, TMP_RuntimeFontUGUI>(go);

            TMPro_CreateObjectMenu.PlaceUIElementRoot(go, menuCommand);
        }
    }
}