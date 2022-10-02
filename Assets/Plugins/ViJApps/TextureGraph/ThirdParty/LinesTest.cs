using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using LibTessDotNet;
using UnityEngine;
using ViJApps.TextureGraph.Utils;
using Mesh = UnityEngine.Mesh;

namespace ViJApps.TextureGraph.ThirdParty
{
    public class LinesTest : MonoBehaviour
    {
        [SerializeField] private List<Transform> mPoints;

        [SerializeField] private MeshFilter mMeshFilter;
        [Range(0, 1)] [SerializeField] private double mPercent = 0.5f;
        [SerializeField] private JoinType m_joinType;

        private void Start()
        {
            mMeshFilter.mesh = new Mesh();
        }

        void Update()
        {
            var points = mPoints.Select(c => c.position);
            var clipperPoints = ClipperConverters. UnityToClipper(points);

            Paths64 pp = new();
            Path64 p = new Path64(clipperPoints);

            Clipper64 clipper = new Clipper64();
            clipper.AddSubject(p);
            clipper.Execute(ClipType.Union, FillRule.EvenOdd, pp);
            
            //Inset
            double width = 200;
            
            var innerResult = new Paths64();
            ClipperOffset co = new ClipperOffset();
            co.AddPaths(pp, m_joinType, EndType.Polygon);
            innerResult.AddRange(co.Execute(-width*mPercent));
            
            //Outset
            var outerResult = new Paths64();
            co = new ClipperOffset();
            co.AddPaths(pp, m_joinType, EndType.Polygon);
            outerResult.AddRange(co.Execute(width*(1-mPercent)));

            clipper.Clear();
            clipper.AddSubject(outerResult);
            clipper.AddClip(innerResult);
            Paths64 final = new Paths64();
            clipper.Execute(ClipType.Difference, FillRule.EvenOdd, final);

            Tess tess = new Tess();
            foreach (var contour in final)
            {
                var float2Contour = ClipperConverters.ClipperToUnityFloat2(contour);
                tess.AddContour(float2Contour.ToContourVertices());
            }

            tess.Tessellate();
            tess.TessToUnityMesh(mMeshFilter.mesh);
        }
    }
}
