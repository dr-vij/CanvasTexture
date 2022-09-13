using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.Utils;

namespace ViJApps
{
    public static class MeshTools
    {
        public static Mesh CreateLine(float2 fromCoord, float2 toCoord, float3x3 aspectMatrix, float width, bool extendStartEnd = false, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            var direction = toCoord - fromCoord;

            var aspectDir = math.normalize(direction.InverseTransformDirection(aspectMatrix)) * width * 0.5f;
            var dir = aspectDir.RotateVectorCWHalfPi().TransformDirection(aspectMatrix);

            var extend = extendStartEnd ? aspectDir.TransformDirection(aspectMatrix) : float2.zero;
            var texSpaceFrom = (fromCoord - extend);
            var texSpaceTo = (toCoord + extend);

            var p0 = texSpaceFrom - dir;
            var p1 = texSpaceTo - dir;
            var p2 = texSpaceTo + dir;
            var p3 = texSpaceFrom + dir;

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