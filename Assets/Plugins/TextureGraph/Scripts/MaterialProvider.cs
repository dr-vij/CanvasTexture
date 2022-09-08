using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViJApps
{
    public class MaterialProvider
    {
        //Shader Names
        public const string SIMPLE_UNLIT_SHADER = "ViJApps.SimpleUnlit";
        public const string SIMPLE_LINE_UNLIT_SHADER = "ViJApps.SimpleLineUnlit";

        //ShadersIDs
        public readonly int SimpleUnlitShaderID;
        public readonly int SimpleLineUnlitShaderID;

        //PropertyIDs
        public readonly int ColorPropertyID;
        public readonly int ThicknessPropertyID;
        public readonly int FromToCoordID;

        private static MaterialProvider m_Instance;

        private Dictionary<int, Shader> m_Shaders = new Dictionary<int, Shader>();
        private Dictionary<int, Material> m_Materials = new Dictionary<int, Material>();

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
            Shader shader;

            //Simple Unlit Shader
            SimpleUnlitShaderID = Shader.PropertyToID(SIMPLE_UNLIT_SHADER);
            shader = Shader.Find(SIMPLE_UNLIT_SHADER);
            m_Shaders.Add(SimpleUnlitShaderID, shader);
            m_Materials.Add(SimpleUnlitShaderID, new Material(shader));

            //Simple Line Unlit Shader
            SimpleLineUnlitShaderID = Shader.PropertyToID(SIMPLE_LINE_UNLIT_SHADER);
            shader = Shader.Find(SIMPLE_LINE_UNLIT_SHADER);
            m_Shaders.Add(SimpleLineUnlitShaderID, shader);
            m_Materials.Add(SimpleLineUnlitShaderID, new Material(shader));

            //Properties
            ColorPropertyID = Shader.PropertyToID("_Color");
            ThicknessPropertyID = Shader.PropertyToID("_Thickness");
            FromToCoordID = Shader.PropertyToID("_FromToCoord");
        }

        public Shader GetShader(int shader) => m_Shaders[shader];

        public Material GetMaterial(int shader) => m_Materials[shader];
    }
}