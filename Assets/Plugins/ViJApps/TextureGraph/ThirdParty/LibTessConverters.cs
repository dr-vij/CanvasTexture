using System.Collections.Generic;
using LibTessDotNet;
using Unity.Collections;
using Unity.Mathematics;

namespace ViJApps.TextureGraph.ThirdParty
{
    /// <summary>
    /// Class helper for conversion from tess data to unity data
    /// </summary>
    public static class LibTessConverters
    {
        /// <summary>
        /// Converts List of float2 to array of ContourVertex
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static ContourVertex[] ToContourVertices(this List<float2> vertices)
        {
            var result = new ContourVertex[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                result[i] = new ContourVertex(vertices[i].ToVec3());
            return result;
        }

        /// <summary>
        /// Converts array of float2 to array of ContourVertex
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static ContourVertex[] ToContourVertices(this float2[] vertices)
        {
            var result = new ContourVertex[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                result[i] = new ContourVertex(vertices[i].ToVec3());
            return result;
        }
        
        /// <summary>
        /// Converts tess result to native arrays of vertices and indices
        /// </summary>
        /// <param name="tess"></param>
        /// <returns></returns>
        public static (NativeArray<float3> positions, NativeArray<ushort> indices) ToPositionsAndUshortIndicesNativeArrays(this Tess tess)
        {
            var positions = new NativeArray<float3>(tess.Vertices.Length, Allocator.Persistent);
            var indices = new NativeArray<ushort>(tess.Elements.Length, Allocator.Persistent);
            for (int i = 0; i < tess.Vertices.Length; i++)
                positions[i] = tess.Vertices[i].Position.ToFloat3();
            for (int i = 0; i < tess.Elements.Length; i++)
                indices[i] = (ushort)tess.Elements[i];
            return (positions, indices);
        }
        
        /// <summary>
        /// Converts tess result to native arrays of vertices and indices
        /// </summary>
        /// <param name="tess"></param>
        /// <returns></returns>
        public static (NativeArray<float3> positions, NativeArray<int> indices) ToPositionsAndIntIndicesNativeArrays(this Tess tess)
        {
            var positions = new NativeArray<float3>(tess.Vertices.Length, Allocator.Persistent);
            var indices = new NativeArray<int>(tess.Elements.Length, Allocator.Persistent);
            for (int i = 0; i < tess.Vertices.Length; i++)
                positions[i] = tess.Vertices[i].Position.ToFloat3();
            for (int i = 0; i < tess.Elements.Length; i++)
                indices[i] = tess.Elements[i];
            return (positions, indices);
        }
        
        /// <summary>
        /// From Vec3 to float2
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static float2 ToFloat2(this Vec3 vec3) => new float2(vec3.X, vec3.Y);
        
        /// <summary>
        /// From Vec3 to float3
        /// </summary>
        /// <param name="vec3"></param>
        /// <returns></returns>
        public static float3 ToFloat3(this Vec3 vec3) => new float3(vec3.X, vec3.Y, vec3.Z);
        
        /// <summary>
        /// From float2 to Vec3
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vec3 ToVec3(this float2 vertex, float z = 0) => new Vec3(vertex.x, vertex.y, z);
        
        /// <summary>
        /// From float3 to Vec3
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public  static Vec3 ToVec3(this float3 vertex) => new Vec3(vertex.x, vertex.y, vertex.z);
    }
}
