using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace TMPro
{
    public static class TextMeshProExtension
    {
        public static TMP_FontAsset CreateDynamicFontAsset(Font font, int samplingPointSize, int atlasPadding, int atlasSize, bool enableMultiAtlasSupport = true)
        {
            return CreateDynamicFontAsset(font, ShaderUtilities.ShaderRef_MobileSDF, samplingPointSize, atlasPadding, GlyphRenderMode.SDFAA, atlasSize, atlasSize, enableMultiAtlasSupport);
        }

        public static TMP_FontAsset CreateDynamicFontAsset(Font font, Shader shader, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, bool enableMultiAtlasSupport = true)
        {
            // Initialize FontEngine
            FontEngine.InitializeFontEngine();

            // Load Font Face
            if (FontEngine.LoadFontFace(font, samplingPointSize) != FontEngineError.Success)
            {
                Debug.LogWarning("Unable to load font face for [" + font.name + "]. Make sure \"Include Font Data\" is enabled in the Font Import Settings.", font);
                return null;
            }

            // Create new font asset
            TMP_FontAsset fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();

            fontAsset.version = "1.1.0";
            fontAsset.faceInfo = FontEngine.GetFaceInfo();

            fontAsset.sourceFontFile = font;
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

#if UNITY_EDITOR
            fontAsset.m_SourceFontFile_EditorRef = font;
#endif

            fontAsset.atlasWidth = atlasWidth;
            fontAsset.atlasHeight = atlasHeight;
            fontAsset.atlasPadding = atlasPadding;
            fontAsset.atlasRenderMode = renderMode;

            // Initialize array for the font atlas textures.
            fontAsset.atlasTextures = new Texture2D[1];

            // Create and add font atlas texture.
            Texture2D texture = new Texture2D(0, 0, TextureFormat.Alpha8, false);
            fontAsset.atlasTextures[0] = texture;

            fontAsset.isMultiAtlasTexturesEnabled = enableMultiAtlasSupport;

            int packingModifier;

            packingModifier = 1;

            // Optimize by adding static ref to shader.
            Material tmp_material = new Material(shader);

            //tmp_material.name = texture.name + " Material";
            tmp_material.SetTexture(ShaderUtilities.ID_MainTex, texture);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
            tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);

            tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, atlasPadding + packingModifier);

            tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

            fontAsset.material = tmp_material;

            fontAsset.freeGlyphRects = new List<GlyphRect>(8) { new GlyphRect(0, 0, atlasWidth - packingModifier, atlasHeight - packingModifier) };
            fontAsset.usedGlyphRects = new List<GlyphRect>(8);

            // TODO: Consider adding support for extracting glyph positioning data

            fontAsset.ReadFontAssetDefinition();

            return fontAsset;
        }

        public static void TMP_FontAsset_Copy(TMP_FontAsset source, TMP_FontAsset dest)
        {
            if (source == null || dest == null)
            {
                Debug.LogError("TMP_FontAsset_Copy Error");
                return;
            }

            dest.name = source.name;
            dest.version = source.version;
            dest.faceInfo = source.faceInfo;
            dest.sourceFontFile = source.sourceFontFile;
            dest.atlasPopulationMode = source.atlasPopulationMode;
#if UNITY_EDITOR
            dest.m_SourceFontFile_EditorRef = source.m_SourceFontFile_EditorRef;
#endif
            dest.atlasWidth = source.atlasWidth;
            dest.atlasHeight = source.atlasHeight;
            dest.atlasPadding = source.atlasPadding;
            dest.atlasRenderMode = source.atlasRenderMode;

            dest.atlasTextures = source.atlasTextures;
            dest.isMultiAtlasTexturesEnabled = source.isMultiAtlasTexturesEnabled;
            dest.material = source.material;
            dest.freeGlyphRects = source.freeGlyphRects;
            dest.usedGlyphRects = source.usedGlyphRects;

            dest.atlas = source.atlas;
        }
    }
}