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

            var result = new Paths64();
            foreach (var path in pp)
            {
                ClipperOffset co = new ClipperOffset();
                co.AddPath(path, JoinType.Square, EndType.Polygon);
                result.AddRange(co.Execute(-100));
            }

            clipper.Clear();
            clipper.AddSubject(pp);
            clipper.AddClip(result);
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
