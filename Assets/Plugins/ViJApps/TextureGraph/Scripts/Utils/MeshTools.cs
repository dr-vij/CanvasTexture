using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace ViJApps.TextureGraph.Utils
{
    public static class MeshTools
    {
        public static Mesh CreateRect(float2 centerCoord, float2 size, float3x3 aspectMatrix, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            var aspectSize = size.TransformDirection(aspectMatrix);
            
            var halfSize = aspectSize / 2;
            var p0 = centerCoord + new float2(-halfSize.x, -halfSize.y);
            var p1 = centerCoord + new float2(-halfSize.x, +halfSize.y);
            var p2 = centerCoord + new float2(+halfSize.x, +halfSize.y);
            var p3 = centerCoord + new float2(+halfSize.x, -halfSize.y);
            
            mesh = CreateMeshFromFourPoints(p0, p1, p2, p3, mesh);
            return mesh;
        }

        public static Mesh CreateLine(float2 fromCoord, float2 toCoord, float3x3 aspectMatrix, float width,
            bool extendStartEnd = false, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            var direction = toCoord - fromCoord;

            var aspectDir = math.normalize(direction.InverseTransformDirection(aspectMatrix)) * width * 0.5f;
            var dir = aspectDir.RotateVectorCwHalfPi().TransformDirection(aspectMatrix);

            var extend = extendStartEnd ? aspectDir.TransformDirection(aspectMatrix) : float2.zero;
            var texSpaceFrom = (fromCoord - extend);
            var texSpaceTo = (toCoord + extend);

            var p0 = texSpaceFrom - dir;
            var p1 = texSpaceTo - dir;
            var p2 = texSpaceTo + dir;
            var p3 = texSpaceFrom + dir;

            mesh = CreateMeshFromFourPoints(p0, p1, p2, p3, mesh);
            return mesh;
        }

        public static Mesh CreateMeshFromFourPoints(float2 p0, float2 p1, float2 p2, float2 p3, Mesh mesh = null)
        {
            if (mesh == null)
                mesh = new Mesh();
            else
                mesh.Clear();

            //Create mesh for rect
            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];
            meshData.SetVertexBufferParams(4,
                new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3, stream: 0));
            meshData.SetIndexBufferParams(6, IndexFormat.UInt16);

            var vertices = meshData.GetVertexData<float3>();
            var indices = meshData.GetIndexData<ushort>();
            vertices[0] = p0.ToFloat3();
            vertices[1] = p1.ToFloat3();
            vertices[2] = p2.ToFloat3();
            vertices[3] = p3.ToFloat3();

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            var subMesh = new SubMeshDescriptor()
            {
                vertexCount = 4,
                indexCount = 6,
                topology = MeshTopology.Triangles,
            };
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, subMesh);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArr, mesh);

            return mesh;
        }
    }
}