using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJApps
{
    public class MaterialProvider
    {
        public readonly int ColorPropertyID;
        public readonly int ThicknessPropertyID;

        private static MaterialProvider m_Instance;

        private Dictionary<ShaderType, Shader> m_Shaders = new Dictionary<ShaderType, Shader>();

        private Dictionary<ShaderType, Material> m_Materials = new Dictionary<ShaderType, Material>();

        public static MaterialProvider Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new MaterialProvider();
                return m_Instance;
            }
        }

        private MaterialProvider()
        {
            var shader = Shader.Find("ViJApps.SimpleUnlit");
            m_Shaders.Add(ShaderType.SimpleUnlit, Shader.Find("ViJApps.SimpleUnlit"));
            m_Materials.Add(ShaderType.SimpleUnlit, new Material(shader));

            ColorPropertyID = Shader.PropertyToID("_Color");
            ThicknessPropertyID = Shader.PropertyToID("_Thickness");
        }

        public Shader GetShader(ShaderType shader) => m_Shaders[shader];

        public Material GetMaterial(ShaderType shader) => m_Materials[shader];
    }
}