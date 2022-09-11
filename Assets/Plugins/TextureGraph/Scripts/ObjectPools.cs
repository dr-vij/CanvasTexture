using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ViJApps.Utils
{
    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool() : base(() => new Mesh(), null, (c) => c.Clear(), (c) => UnityEngine.Object.Destroy(c), true, 10, 10000) { }
    }

    public class PropertyBlockPool : ObjectPool<MaterialPropertyBlock>
    {
        public PropertyBlockPool() : base(() => new MaterialPropertyBlock(), null, (c) => c.Clear(), null, true, 10, 1000) { }
    }
}