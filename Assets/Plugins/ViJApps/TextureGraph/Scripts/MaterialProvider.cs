using System;
using UnityEngine;

namespace ViJApps.TextureGraph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShaderPropertiesProviderAttribute: Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyAttribute : Attribute
    {
    }

    [ShaderPropertiesProvider]
    public static partial class MaterialProvider
    {
        //Shader Names
        [Shader]
        public const string SIMPLE_UNLIT = "ViJApps.SimpleUnlit";
        [Shader]
        public const string SIMPLE_UNLIT_TRANSPARENT = "ViJApps.SimpleUnlitTransparent";
        [Shader]
        public const string SIMPLE_LINE_UNLIT = "ViJApps.SimpleLineUnlit";
        [Shader]
        public const string SIMPLE_CIRCLE_UNLIT = "ViJApps.SimpleCircleUnlit";
        [Shader]
        public const string SIMPLE_ELLIPSE_UNLIT = "ViJApps.SimpleEllipseUnlit";

        //Property 
        [ShaderProperty]
        public const string COLOR = "_Color";
        [ShaderProperty]
        public const string THICKNESS = "_Thickness";
        [ShaderProperty]
        public const string FROM_TO_COORD = "_FromToCoord";
        [ShaderProperty]
        public const string ASPECT = "_Aspect";
        [ShaderProperty]
        public const string RADIUS = "_Radius";
        [ShaderProperty]
        public const string CENTER = "_Center";
        [ShaderProperty]
        public const string AB = "_AB";
    }
}