using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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
        public readonly int Color_PropertyID = Shader.PropertyToID("_Color");
        public readonly int Thickness_PropertyID = Shader.PropertyToID("_Thickness");
        public readonly int FromToCoord_PropertyID = Shader.PropertyToID("_FromToCoord");

        public readonly int Trs2dCol0_PropertyID = Shader.PropertyToID("_Trs2dCol0");
        public readonly int Trs2dCol1_PropertyID = Shader.PropertyToID("_Trs2dCol1");
        public readonly int Trs2dCol2_PropertyID = Shader.PropertyToID("_Trs2dCol2");

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
        }

        public Shader GetShader(int shader) => m_Shaders[shader];

        public Material GetMaterial(int shader) => m_Materials[shader];
    }
}