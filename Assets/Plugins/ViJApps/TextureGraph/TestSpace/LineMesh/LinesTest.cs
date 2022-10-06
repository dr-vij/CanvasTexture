using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using ViJApps.TextureGraph.Utils;
using Mesh = UnityEngine.Mesh;

namespace ViJApps.TextureGraph.ThirdParty
{
    public class LinesTest : MonoBehaviour
    {
        [SerializeField] private List<Transform> mPoints;

        [SerializeField] private MeshFilter mMeshFilter;
        [SerializeField] private LineJoinType m_joinType;
        [SerializeField] private LineEndingType m_endType;
        [SerializeField] private float mThickness = 1f;

        private void Start()
        {
            mMeshFilter.mesh = new Mesh();
        }

        void Update()
        {
            var points = mPoints.Select(c => new float2(c.position.x, c.position.y) );
            mMeshFilter.mesh = MeshTools.CreatePolyLine(points.ToList(), mThickness, m_endType, m_joinType , mesh: mMeshFilter.mesh);
        }
    }
}
