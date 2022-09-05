using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ViJApps
{
    public partial class TextureData : IDisposable
    {
        protected CommandBuffer m_Cmd;
        protected RenderTexture m_RTex;
        protected RenderTextureDescriptor m_Desc;

        public RenderTexture RTex => m_RTex;

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            if (m_Cmd == null)
                m_Cmd = new CommandBuffer();
            else
                m_Cmd.Clear();
            if (m_RTex == null || m_RTex.width != width || m_RTex.height != height)
                ReinitTexture(width, height);
        }

        public void Flush()
        {
            Graphics.ExecuteCommandBuffer(m_Cmd);
            m_Cmd.Clear();
        }

        public void SaveToAssets(string assetName)
        {
            var texture2d = new Texture2D(m_RTex.width, m_RTex.height);
            var buffer = RenderTexture.active;
            RenderTexture.active = RTex;
            texture2d.ReadPixels(new Rect(0, 0, m_RTex.width, m_RTex.height), 0, 0);
            texture2d.Apply();
            var bytes = texture2d.EncodeToPNG();
            System.IO.File.WriteAllBytes($"Assets/" + assetName + ".png", bytes);
            RenderTexture.active = buffer;
        }

        private void ReinitTexture(int width, int height)
        {
            m_Desc = new RenderTextureDescriptor(width, height);
            if (m_RTex != null)
                UnityEngine.Object.Destroy(m_RTex);
            m_RTex = new RenderTexture(m_Desc);
        }

        public void Dispose()
        {
            if (m_Cmd != null)
            {
                m_Cmd.Dispose();
                m_Cmd = null;
            }
            if (m_RTex != null)
            {
                UnityEngine.Object.Destroy(m_RTex);
                m_RTex = null;
            }
        }
    }
}