using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using Unity.Mathematics;
using UnityEngine;

namespace ViJApps.TextureGraph.ThirdParty
{
    public static class ClipperConverters 
    {
        private const double DefaultMult = 1e3;
    
        public static Point64[] UnityToClipper(IEnumerable<Vector3> positions, double mult = DefaultMult)
        {
            return positions.Select(c => new Point64(c.x * mult, c.y * mult)).ToArray();
        }
    
        public static Point64[] UnityToClipper(IEnumerable<float3> positions, double mult = DefaultMult)
        {
            return positions.Select(c => new Point64(c.x * mult, c.y * mult)).ToArray();
        }

        public static Vector2[] ClipperToUnityVector(Path64 positions, double mult = DefaultMult)
        {
            var result = new Vector2[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                result[i] = new Vector2((float)(positions[i].X / mult), (float)(positions[i].Y / mult));
            return result;
        }
    
        public static float2[] ClipperToUnityFloat2(Path64 positions, double mult = DefaultMult)
        {
            var result = new float2[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                result[i] = new float2((float)(positions[i].X / mult), (float)(positions[i].Y / mult));
            return result;
        }
    }
}
