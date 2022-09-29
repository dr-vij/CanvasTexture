using System;
using UnityEngine;
using TMPro;

namespace ViJApps.TextureGraph
{
    public struct TextSettings
    {
        public float FontSize;
        public Color FontColor;
        public FontStyles FontStyle;
        public FontWeight FontWeight;

        public static TextSettings Default =>
            new TextSettings
            {
                FontSize = 12,
                FontColor = Color.black,
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeight.Regular
            };
    }
    
    public class TextComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_textComponent;

        public TextMeshPro Text => m_textComponent;

        public void Clear()
        {
            m_textComponent.text = string.Empty;
            
            var defaultSettings = TextSettings.Default;
            m_textComponent.fontSize = defaultSettings.FontSize;
            m_textComponent.fontWeight = defaultSettings.FontWeight;
            m_textComponent.fontStyle = defaultSettings.FontStyle;
            m_textComponent.color = defaultSettings.FontColor;
        }
    }
}
