using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TMP_RuntimeFontUGUI), true), CanEditMultipleObjects]
    public class TMP_RuntimeFont_EditorPanelUI : TMP_EditorPanelUI
    {
        protected SerializedProperty m_FontIndexProp;

        protected override void OnEnable()
        {
            m_FontIndexProp = serializedObject.FindProperty("m_FontIndex");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            // Make sure Multi selection only includes TMP Text objects.
            if (IsMixSelectionTypes()) return;

            serializedObject.Update();

            DrawTextInput();

            DrawRuntimeFont();

            DrawMainSettings();

            DrawExtraSettings();

            EditorGUILayout.Space();

            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_TextComponent.havePropertiesChanged = true;
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawRuntimeFont()
        {
            GUILayout.Label(new GUIContent("<b>Runtime Font</b>"), TMP_UIStyleManager.sectionHeader);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_FontIndexProp);
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }

            EditorGUI.BeginDisabled(true);
            {
                EditorGUILayout.ObjectField("Settings", TMP_RuntimeFontSettings.instance, typeof(TMP_RuntimeFontSettings), false);
                EditorGUILayout.TextField("NickName", TMP_RuntimeFontSettings.GetNickNameByIndex(m_FontIndexProp.intValue));
                EditorGUILayout.ObjectField("FontAsset", TMP_RuntimeFontUGUI.GetFontAsset(m_FontIndexProp.intValue), typeof(TMP_FontAsset), false);
            }
            EditorGUI.EndDisabled();


        }
    }
}
