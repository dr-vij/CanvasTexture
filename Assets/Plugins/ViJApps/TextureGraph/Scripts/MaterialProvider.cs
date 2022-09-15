using System.Collections.Generic;
using UnityEngine;

namespace ViJApps.TextureGraph
{
    public class MaterialProvider
    {
        //Shader Names
        public const string SIMPLE_UNLIT_SHADER = "ViJApps.SimpleUnlit";
        public const string SIMPLE_LINE_UNLIT_SHADER = "ViJApps.SimpleLineUnlit";

        public const string COLOR_PROPERTY = "_Color";
        public const string THICKNESS_PROPERTY = "_Thickness";
        public const string FROM_TO_COORD_PROPERTY = "_FromToCoord";
        public const string ASPECT_PROPERTY = "_Aspect";

        //ShadersIDs
        public readonly int SimpleUnlit_ShaderID;
        public readonly int SimpleLineUnlit_ShaderID;

        //PropertyIDs
        public readonly int Color_PropertyID = Shader.PropertyToID(COLOR_PROPERTY);
        public readonly int Thickness_PropertyID = Shader.PropertyToID(THICKNESS_PROPERTY);
        public readonly int FromToCoord_PropertyID = Shader.PropertyToID(FROM_TO_COORD_PROPERTY);
        public readonly int Aspect_PropertyID = Shader.PropertyToID(ASPECT_PROPERTY);

        private static MaterialProvider s_instance;

        private readonly Dictionary<int, Shader> m_shaders = new();
        private readonly Dictionary<int, Material> m_materials = new();

        public static MaterialProvider Instance => s_instance ??= new MaterialProvider();

        private MaterialProvider()
        {
            //Simple Unlit Shader
            SimpleUnlit_ShaderID = Shader.PropertyToID(SIMPLE_UNLIT_SHADER);
            var shader = Shader.Find(SIMPLE_UNLIT_SHADER);
            m_shaders.Add(SimpleUnlit_ShaderID, shader);
            m_materials.Add(SimpleUnlit_ShaderID, new Material(shader));

            //Simple Line Unlit Shader
            SimpleLineUnlit_ShaderID = Shader.PropertyToID(SIMPLE_LINE_UNLIT_SHADER);
            shader = Shader.Find(SIMPLE_LINE_UNLIT_SHADER);
            m_shaders.Add(SimpleLineUnlit_ShaderID, shader);
            m_materials.Add(SimpleLineUnlit_ShaderID, new Material(shader));
        }

        public Shader GetShader(int shader) => m_shaders[shader];

        public Material GetMaterial(int shader) => m_materials[shader];
    }
}