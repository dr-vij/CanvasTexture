using System;
using UnityEngine;
using TMPro;

namespace ViJApps.TextureGraph
{
    public class TextComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshPro m_tmpComponent;
        [SerializeField] private RectTransform m_rectTransform;

        public string Text
        {
            get => m_tmpComponent.text;
            set => m_tmpComponent.text = value;
        }

        public Vector2 Position
        {
            get => m_rectTransform.position;
            set => m_rectTransform.position = value;
        }

        public Vector2 SizeDelta
        {
            get => m_rectTransform.sizeDelta;
            set => m_rectTransform.sizeDelta = value;
        }
        public Vector2 Pivot
        {
            get => m_rectTransform.pivot;
            set => m_rectTransform.pivot = value;
        }

        public Vector2 AnchorMin
        {
            get => m_rectTransform.anchorMin;
            set => m_rectTransform.anchorMin = value;
        }

        public Vector2 AnchorMax
        {
            get => m_rectTransform.anchorMax;
            set => m_rectTransform.anchorMax = value;
        }

        public void UpdateText() => m_tmpComponent.ForceMeshUpdate();

        public Material Material => m_tmpComponent.renderer.material;
        
        public Renderer Renderer => m_tmpComponent.renderer;
        
        public void Clear()
        {
            m_tmpComponent.text = string.Empty;
            var settings = TextSettings.Default;
            SetSettings(settings);
        }

        public void SetSettings(TextSettings settings)
        {
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