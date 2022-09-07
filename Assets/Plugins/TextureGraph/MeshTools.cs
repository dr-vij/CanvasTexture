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
        public static readonly float2 Floa2One = new float2(1, 1);

        public static readonly float2 Float2MinusOne = new float2(-1, -1);

        public static float2 RotateVectorCWHalfPi(this float2 vector) => new float2(vector.y, -vector.x);

        public static float2 RotateVectorCCWHalfPi(this float2 vector) => new float2(-vector.y, vector.x);

        public static float3 ToFloat3(this float2 val, float z = 0) => new float3(val.x, val.y, z);

        public static float2 RemapFromPercentToTexture(this float2 percentPosition) => math.remap(float2.zero, Floa2One, Float2MinusOne, Floa2One, percentPosition);

        public static float2 RemapFromPixelsToTexture(this float2 pixelPosition, float2 textureSize) => math.remap(float2.zero, textureSize, Float2MinusOne, Floa2One, pixelPosition);

        #region transformations2d

        public static float2 TransformPoint(this float2 point, float3x3 matrix) => math.mul(matrix, point.ToFloat3(1)).xy;

        public static float2 TransformDirection(this float2 direction, float3x3 matrix) => math.mul(matrix, direction.ToFloat3(0)).xy;

        public static float2 InverseTransformPoint(this float2 point, float3x3 matrix) => math.mul(math.inverse(matrix), point.ToFloat3(1)).xy;

        public static float2 InverseTransformDirection(this float2 direction, float3x3 matrix) => math.mul(math.inverse(matrix), direction.ToFloat3(0)).xy;

        #endregion

        public static float3x3 CreateScaleMatrix2d(float2 scale) => new float3x3(new float3(scale.x, 0, 0), new float3(0, scale.y, 0), new float3(0, 0, 1));

        public static float3x3 CreateTranslationMatrix2d(float2 translation) => new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(translation.x, translation.y, 1));

        public static float3x3 CreateTranslationScaleMatrix(float2 translation, float2 scale) => new float3x3(new float3(scale.x, 0, 0), new float3(0, scale.y, 0), new float3(translation.x, translation.y, 1));
    }
}

namespace ViJApps
{
    public static class MeshTools
    {
        public static Mesh CreateLine(float2 from, float2 to, float3x3 transform2d, float width, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            var direction = math.normalizesafe(to - from, math.right().xy);
            var halfWidth = width * 0.5f;
            var sideDir = (direction.RotateVectorCWHalfPi() * halfWidth);

            //4 vertices from BL CW
            var p0 = (from - sideDir).InverseTransformPoint(transform2d);
            var p1 = (to - sideDir).InverseTransformPoint(transform2d);
            var p2 = (to + sideDir).InverseTransformPoint(transform2d);
            var p3 = (from + sideDir).InverseTransformPoint(transform2d);

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