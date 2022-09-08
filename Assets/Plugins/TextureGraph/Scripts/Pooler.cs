using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ViJApps
{
    public class Pooler
    {
        public class MeshPool : ObjectPool<Mesh>
        {
            public MeshPool() : base(() => new Mesh(), null, (c) => c.Clear(), null, true, 10, 10000) { }
        }

        public class PropertyBlockPool : ObjectPool<MaterialPropertyBlock>
        {
            public PropertyBlockPool() : base(() => new MaterialPropertyBlock(), null, (c) => c.Clear(), null, true, 10, 1000) { }
        }

        private MeshPool m_MeshPool = new MeshPool();
        private PropertyBlockPool m_PropertyBlockPool = new PropertyBlockPool();

        private List<Mesh> m_AllocatedMeshes = new List<Mesh>();
        private List<MaterialPropertyBlock> m_AllocatedPropertyBlocks = new List<MaterialPropertyBlock>();

        private static Pooler m_Instance;

        public static Pooler Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new Pooler();
                return m_Instance;
            }
        }

        private Pooler()
        {
        }

        public MaterialPropertyBlock GetPropertyBlock()
        {
            var item = m_PropertyBlockPool.Get();
            m_AllocatedPropertyBlocks.Add(item);
            return item;
        }

        public Mesh GetMesh()
        {
            var mesh = m_MeshPool.Get();
            m_AllocatedMeshes.Add(mesh);
            return mesh;
        }

        public void ReleaseAll()
        {
            foreach (var mesh in m_AllocatedMeshes)
                m_MeshPool.Release(mesh);
            m_AllocatedMeshes.Clear();

            foreach (var propertyBlock in m_AllocatedPropertyBlocks)
                m_PropertyBlockPool.Release(propertyBlock);
            m_AllocatedPropertyBlocks.Clear();
        }
    }
}