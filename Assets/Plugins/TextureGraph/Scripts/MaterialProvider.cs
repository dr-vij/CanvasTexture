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

        public const string COLOR_PROPERTY = "_Color";
        public const string THICKNESS_PROPERTY = "_Thickness";
        public const string FROM_TO_COORD_PROPERTY = "_FromToCoord";

        //ShadersIDs
        public readonly int SimpleUnlit_ShaderID;
        public readonly int SimpleLineUnlit_ShaderID;

        //PropertyIDs
        public readonly int Color_PropertyID = Shader.PropertyToID(COLOR_PROPERTY);
        public readonly int Thickness_PropertyID = Shader.PropertyToID(THICKNESS_PROPERTY);
        public readonly int FromToCoord_PropertyID = Shader.PropertyToID(FROM_TO_COORD_PROPERTY);

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
            SimpleUnlit_ShaderID = Shader.PropertyToID(SIMPLE_UNLIT_SHADER);
            shader = Shader.Find(SIMPLE_UNLIT_SHADER);
            m_Shaders.Add(SimpleUnlit_ShaderID, shader);
            m_Materials.Add(SimpleUnlit_ShaderID, new Material(shader));

            //Simple Line Unlit Shader
            SimpleLineUnlit_ShaderID = Shader.PropertyToID(SIMPLE_LINE_UNLIT_SHADER);
            shader = Shader.Find(SIMPLE_LINE_UNLIT_SHADER);
            m_Shaders.Add(SimpleLineUnlit_ShaderID, shader);
            m_Materials.Add(SimpleLineUnlit_ShaderID, new Material(shader));
        }

        public Shader GetShader(int shader) => m_Shaders[shader];

        public Material GetMaterial(int shader) => m_Materials[shader];
    }
}