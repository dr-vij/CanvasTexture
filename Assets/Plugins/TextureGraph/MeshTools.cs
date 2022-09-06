using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.MathematicsExtensions;

namespace ViJApps.MathematicsExtensions
{
    public static class Utils
    {
        public static float2 RotateVectorCWHalfPi(this float2 vector) => new float2(vector.y, -vector.x);

        public static float2 RotateVectorCCWHalfPi(this float2 vector) => new float2(-vector.y, vector.x);

        public static float3 ToFloat3(this float2 val, float z = 0) => new float3(val.x, val.y, z);

        public static float2 ToPercentCoord(float2 position) => math.lerp(new float2(-1, -1), new float2(1, 1), position);
    }
}

namespace ViJApps
{
    public static class MeshTools
    {
        public static Mesh CreateLine(float2 from, float2 to, float2x2 mtrx, float width, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            var direction = math.normalizesafe(to - from, math.right().xy);
            var halfWidth = width * 0.5f;
            var sideDir = direction.RotateVectorCWHalfPi() * halfWidth;

            //4 vertices from BL CW
            var p0 = from - sideDir;
            var p1 = to - sideDir;
            var p2 = to + sideDir;
            var p3 = from + sideDir;

            //Create mesh for rect
            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];
            meshData.SetVertexBufferParams(4, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, dimension: 3, stream: 0));
            meshData.SetIndexBufferParams(6, IndexFormat.UInt16);

            var vertices = meshData.GetVertexData<float3>();
            var indicies = meshData.GetIndexData<ushort>();
            vertices[0] = p0.ToFloat3();
            vertices[1] = p1.ToFloat3();
            vertices[2] = p2.ToFloat3();
            vertices[3] = p3.ToFloat3();

            indicies[0] = 0;
            indicies[1] = 1;
            indicies[2] = 2;

            indicies[3] = 0;
            indicies[4] = 2;
            indicies[5] = 3;

            var submesh = new SubMeshDescriptor()
            {
                vertexCount = 4,
                indexCount = 6,
                topology = MeshTopology.Triangles,
            };
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, submesh, MeshUpdateFlags.Default);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArr, mesh, MeshUpdateFlags.Default);

            return mesh;
        }
    }
}