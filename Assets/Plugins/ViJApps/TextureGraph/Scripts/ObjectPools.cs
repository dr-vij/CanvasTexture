using UnityEngine;
using UnityEngine.Pool;

namespace ViJApps.TextureGraph
{
    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool() : base(() => new Mesh(), null, (c) => c.Clear(), Object.Destroy) { }
    }

    public class PropertyBlockPool : ObjectPool<MaterialPropertyBlock>
    {
        public PropertyBlockPool() : base(() => new MaterialPropertyBlock(), null, c => c.Clear(), null, true, 10, 1000) { }
    }
}