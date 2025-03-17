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

        private void DrawRuntimeFont()
        {
            EditorGUILayout.BeginVertical(EditorStyles.frameBox);
            {
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
                    EditorGUILayout.ObjectField("RuntimeFont", TMP_RuntimeFontUGUI.GetFontAsset(m_FontIndexProp.intValue), typeof(TMP_FontAsset), false);
                }
                EditorGUI.EndDisabled();
            }
            EditorGUILayout.EndVertical();
        }

        protected override void DrawFont(bool isRuntimeFontAsset = false)
        {
            DrawRuntimeFont();
            base.DrawFont(true);
        }
    }
}
