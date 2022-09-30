using System;
using UnityEngine;
using TMPro;

namespace ViJApps.TextureGraph
{
    [Serializable]
    public struct SpacingOptions
    {
        public float Word;
        public float Character;
        public float Line;
        public float Paragraph;
    }

    [Serializable]
    public struct TextSettings
    {
        public float FontSize;
        public Color FontColor;
        public FontStyles FontStyle;
        public FontWeight FontWeight;
        public TextAlignmentOptions TextAlignment;

        public SpacingOptions SpacingOptions;

        public bool Wrapping;
        public TextOverflowModes OverflowMode;

        public static TextSettings Default =>
            new TextSettings
            {
                FontSize = 1,
                FontColor = Color.black,
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeight.Regular,
                TextAlignment = TextAlignmentOptions.TopLeft,
                
                SpacingOptions = new SpacingOptions(),
                
                Wrapping = true,
                OverflowMode = TextOverflowModes.Overflow,
            };
    }

    public class TextComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_tmpComponent;

        public TextMeshPro TMP => m_tmpComponent;

        public void Clear()
        {
            m_tmpComponent.text = string.Empty;
            var settings = TextSettings.Default;
            
            m_tmpComponent.fontSize = settings.FontSize;
            m_tmpComponent.fontWeight = settings.FontWeight;
            m_tmpComponent.fontStyle = settings.FontStyle;
            m_tmpComponent.color = settings.FontColor;
            m_tmpComponent.alignment = settings.TextAlignment;
            
            //Spacing
            m_tmpComponent.lineSpacing = settings.SpacingOptions.Line;
            m_tmpComponent.wordSpacing = settings.SpacingOptions.Word;
            m_tmpComponent.characterSpacing = settings.SpacingOptions.Character;
            m_tmpComponent.paragraphSpacing = settings.SpacingOptions.Paragraph;
            
            m_tmpComponent.enableWordWrapping = settings.Wrapping;
            m_tmpComponent.overflowMode = settings.OverflowMode;
        }
    }
}