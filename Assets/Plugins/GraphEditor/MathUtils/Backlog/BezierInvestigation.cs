using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Experimental
{

    // usefull links:
    // some cool stuff about bezier: https://pomax.github.io/bezierinfo/ 
    // Binominals explained: https://ru.wikipedia.org/wiki/%D0%A2%D1%80%D0%B5%D1%83%D0%B3%D0%BE%D0%BB%D1%8C%D0%BD%D0%B8%D0%BA_%D0%9F%D0%B0%D1%81%D0%BA%D0%B0%D0%BB%D1%8F
    public class Bezier
    {
        private List<int[]> m_LookupTable = new List<int[]>();

        public Bezier()
        {
            Test();
        }

        public void Test()
        {
            //Check that bruteforce and lookup table gives the same result.
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    var lookup = BinominalLookup(i, j);
                    var bruteforce = BinominalBruteforce(i, j);
                    if (lookup != bruteforce)
                        throw new System.Exception("Error in binominals");
                }
            }
        }

        /// <summary>
        /// Gets binominal from lookup table and fills it if table is not completed
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public int BinominalLookup(int n, int k)
        {
            while (n >= m_LookupTable.Count)
            {
                var size = m_LookupTable.Count;
                var nextRow = new int[size + 1];
                nextRow[0] = 1;
                for (int i = 1, prev = size - 1; i < size; i++)
                    nextRow[i] = m_LookupTable[prev][i - 1] + m_LookupTable[prev][i];
                nextRow[size] = 1;
                m_LookupTable.Add(nextRow);
            }

            return m_LookupTable[n][k];
        }

        /// <summary>
        /// Calculates binominal (really heavy function, dont recommend to use it)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public int BinominalBruteforce(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        /// <summary>
        /// Recursive version of Factorial
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public int Factorial(int x)
        {
            if (x > 12)
                throw new System.Exception("Integer is not enought for this factorial");

            if (x == 0)
                return 1;
            else
                return x * Factorial(x - 1);
        }

        #region basis function

        public float CalcBezierWithBruteforce(int n, float t)
        {
            var sum = 0f;
            for (int k = 0; k <= n; k++)
                sum += BinominalBruteforce(n, k) * math.pow(1 - t, n - k) * math.pow(t, k);
            return sum;
        }

        public float CalcBezierWithLookup(int n, float t)
        {
            var sum = 0f;
            for (int k = 0; k <= n; k++)
                sum += BinominalLookup(n, k) * math.pow(1 - t, n - k) * math.pow(t, k);
            return sum;
        }

        public float CalcBezierQuadratic(float t)
        {
            var t2 = t * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            return mt2 * 2 * mt * t + t2;
        }

        public float CalcBezierCubic(float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            return mt3 + 3 * mt2 * t + 3 * mt * t2 + t3;
        }

        #endregion;

        #region weighted basis functions

        //1D:

        public float CalcBezier1D(int n, float t, float[] w)
        {
            var sum = 0f;
            for (var k = 0; k <= n; k++)
                sum += w[k] * BinominalLookup(n, k) * math.pow(1 - t, n - k) * math.pow(t, k);
            return sum;
        }

        public float CalcBezierQuadratic1D(float t, float[] w)
        {
            var t2 = t * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            return w[0] * mt2 + w[1] * 2 * mt * t + w[2] * t2;
        }

        public float CalcBezierCubic1D(float t, float[] w)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            return w[0] * mt3 + 3 * w[1] * mt2 * t + 3 * w[2] * mt * t2 + w[3] * t3;
        }

        //2D:

        public float2 CalcBezier2D(int n, float t, float2[] w)
        {
            var sum = float2.zero;
            for (var k = 0; k <= n; k++)
                sum += w[k] * BinominalLookup(n, k) * math.pow(1 - t, n - k) * math.pow(t, k);
            return sum;
        }

        public float2 CalcBezierQuadratic2D(float t, float2[] w)
        {
            var t2 = t * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            return w[0] * mt2 + w[1] * 2 * mt * t + w[2] * t2;
        }

        public float2 CalcBezierCubic2D(float t, float2[] w)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            return w[0] * mt3 + 3 * w[1] * mt2 * t + 3 * w[2] * mt * t2 + w[3] * t3;
        }

        //3D:

        public float3 CalcBezier3D(int n, float t, float3[] w)
        {
            var sum = float3.zero;
            for (var k = 0; k <= n; k++)
                sum += w[k] * BinominalLookup(n, k) * math.pow(1 - t, n - k) * math.pow(t, k);
            return sum;
        }

        public float3 CalcBezierQuadratic3D(float t, float3[] w)
        {
            var t2 = t * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            return w[0] * mt2 + w[1] * 2 * mt * t + w[2] * t2;
        }

        public float3 CalcBezierCubic3D(float t, float3[] w)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            return w[0] * mt3 + 3 * w[1] * mt2 * t + 3 * w[2] * mt * t2 + w[3] * t3;
        }

        #endregion

        #region Casteljau's algorithm

        public float CalcBezierCasteljau1D(float t, float[] points)
        {
            if (points.Length == 1)
                return (points[0]);

            var newPoints = new float[points.Length - 1];
            for (int i = 0; i < newPoints.Length; i++)
                newPoints[i] = (1 - t) * points[i] + t * points[i + 1];
            return CalcBezierCasteljau1D(t, newPoints);
        }

        public float2 CalcBezierCasteljau2D(float t, float2[] points)
        {
            if (points.Length == 1)
                return (points[0]);

            var newPoints = new float2[points.Length - 1];
            for (int i = 0; i < newPoints.Length; i++)
                newPoints[i] = (1 - t) * points[i] + t * points[i + 1];
            return CalcBezierCasteljau2D(t, newPoints);
        }

        public float3 CalcBezierCasteljau3D(float t, float3[] points)
        {
            if (points.Length == 1)
                return (points[0]);

            var newPoints = new float3[points.Length - 1];
            for (int i = 0; i < newPoints.Length; i++)
                newPoints[i] = (1 - t) * points[i] + t * points[i + 1];
            return CalcBezierCasteljau3D(t, newPoints);
        }

        #endregion

        #region Flattening curve

        public List<float> GetPoints1D(float[] points, int segmentCount, List<float> result = null)
        {
            if (result == null)
                result = new List<float>(segmentCount + 1);
            else
                result.Clear();

            var step = 1f / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                var t = i * step;
                result.Add(CalcBezier1D(points.Length - 1, t, points));
            }
            result.Add(CalcBezier1D(points.Length - 1, 1f, points));

            return result;
        }

        public List<float2> GetPoints2D(float2[] points, int segmentCount, List<float2> result = null)
        {
            if (result == null)
                result = new List<float2>(segmentCount + 1);
            else
                result.Clear();

            var step = 1f / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                var t = i * step;
                result.Add(CalcBezier2D(points.Length - 1, t, points));
            }
            result.Add(CalcBezier2D(points.Length - 1, 1f, points));

            return result;
        }

        public List<float3> GetPoints3D(float3[] points, int segmentCount, List<float3> result = null)
        {
            if (result == null)
                result = new List<float3>(segmentCount + 1);
            else
                result.Clear();

            var step = 1f / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                var t = i * step;
                result.Add(CalcBezier3D(points.Length - 1, t, points));
            }
            result.Add(CalcBezier3D(points.Length - 1, 1f, points));

            return result;
        }

        #endregion

        #region Derivatives and root finding helpers

        public float[] GetFirstDerivativePointsForCubic1D(float[] points)
        {
            var A = points[0];
            var B = points[1];
            var C = points[2];
            var D = points[3];
            var newPoints = new float[] { 3 * (B - A), 3 * (C - B), 3 * (D - C) };
            return newPoints;
        }

        public float[] GetFirstDerivativePointsForQuadratic1D(float[] points)
        {
            var A = points[0];
            var B = points[1];
            var C = points[2];
            var newPoints = new float[] { 2 * (B - A), 2 * (C - B) };
            return newPoints;
        }

        public List<float> GetLinearRoots(float[] points, List<float> result = null)
        {
            if (result == null)
                result = new List<float>();
            else
                result.Clear();

            var A = points[0];
            var B = points[1];
            if (B - A != 0)
            {
                var root = -A / (B - A);
                if (root >= 0 && root <= 1)
                    result.Add(root);
            }

            return result;
        }

        public List<float> GetQuadraticRoots(float[] points, List<float> result = null)
        {
            if (result == null)
                result = new List<float>();
            else
                result.Clear();

            var v1 = points[0];
            var v2 = points[1];
            var v3 = points[2];

            var a = v1 - 2 * v2 + v3;
            var b = 2 * (v2 - v1);
            var c = v1;

            var b2min4ac = b * b - 4 * a * c;

            if (b2min4ac >= 0)
            {
                var root1 = (-b - math.sqrt(b2min4ac)) / (2 * a);
                var root2 = (-b + math.sqrt(b2min4ac)) / (2 * a);

                if (root1 >= 0 && root1 <= 1)
                    result.Add(root1);
                if (root2 >= 0 && root2 <= 1)
                    result.Add(root2);
            }

            return result;
        }

        public float FirstDerivativeCubic1D(float t, float[] points) => CalcBezierQuadratic1D(t, GetFirstDerivativePointsForCubic1D(points));

        public float FirstDerivativeQuadratic1D(float t, float[] points) => CalcBezier1D(2, t, GetFirstDerivativePointsForQuadratic1D(points));

        #endregion

        #region BezierCurve Min Max helpers

        /// <summary>
        /// Get min max for quadratic bezier 3D
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float3 min, float3 max) GetMinMaxQuadraticBezier3D(float3[] controlPoints)
        {
            var minMaxX = GetMinMaxQuadraticBezier1D(controlPoints.Select(c => c.x).ToArray());
            var minMaxY = GetMinMaxQuadraticBezier1D(controlPoints.Select(c => c.y).ToArray());
            var minMaxZ = GetMinMaxQuadraticBezier1D(controlPoints.Select(c => c.z).ToArray());

            return (new float3(minMaxX.min, minMaxY.min, minMaxZ.min), new float3(minMaxX.max, minMaxY.max, minMaxZ.max));
        }

        /// <summary>
        /// Get min max for cubic bezier 3D
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float3 min, float3 max) GetMinMaxCubicBezier3D(float3[] controlPoints)
        {
            var minMaxX = GetMinMaxCubicBezier1D(controlPoints.Select(c => c.x).ToArray());
            var minMaxY = GetMinMaxCubicBezier1D(controlPoints.Select(c => c.y).ToArray());
            var minMaxZ = GetMinMaxCubicBezier1D(controlPoints.Select(c => c.z).ToArray());

            return (new float3(minMaxX.min, minMaxY.min, minMaxZ.min), new float3(minMaxX.max, minMaxY.max, minMaxZ.max));
        }

        /// <summary>
        /// Get min max for quadratic bezier 2D
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float2 min, float2 max) GetMinMaxQuadraticBezier2D(float2[] controlPoints)
        {
            var minMaxX = GetMinMaxQuadraticBezier1D(controlPoints.Select(c => c.x).ToArray());
            var minMaxY = GetMinMaxQuadraticBezier1D(controlPoints.Select(c => c.y).ToArray());
            return (new float2(minMaxX.min, minMaxY.min), new float2(minMaxX.max, minMaxY.max));
        }

        /// <summary>
        /// Get min max for cubic bezier 2D
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float2 min, float2 max) GetMinMaxCubicBezier2D(float2[] controlPoints)
        {
            var minMaxX = GetMinMaxCubicBezier1D(controlPoints.Select(c => c.x).ToArray());
            var minMaxY = GetMinMaxCubicBezier1D(controlPoints.Select(c => c.y).ToArray());
            return (new float2(minMaxX.min, minMaxY.min), new float2(minMaxX.max, minMaxY.max));
        }

        /// <summary>
        /// Get min max for quadratic bezier
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float min, float max) GetMinMaxQuadraticBezier1D(float[] controlPoints)
        {
            //We need just the first derivative
            var derivativePoints = GetFirstDerivativePointsForQuadratic1D(controlPoints);
            var roots = GetLinearRoots(derivativePoints);
            var zeroT = CalcBezierQuadratic1D(0, controlPoints);
            var oneT = CalcBezierQuadratic1D(1, controlPoints);

            //calculate values at roots
            for (int i = 0; i < roots.Count; i++)
                roots[i] = CalcBezierQuadratic1D(roots[i], controlPoints);

            return GetMinMax(zeroT, oneT, roots);
        }

        /// <summary>
        /// Get min max for cubic bezier
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <returns></returns>
        public (float min, float max) GetMinMaxCubicBezier1D(float[] controlPoints)
        {
            //Find roots for first derivative
            var derivativePoints = GetFirstDerivativePointsForCubic1D(controlPoints);
            var roots = GetQuadraticRoots(derivativePoints);
            var zeroT = CalcBezierCubic1D(0, controlPoints);
            var oneT = CalcBezierCubic1D(1, controlPoints);

            //Find roots for second derivative
            var secondDerivativePoints = GetFirstDerivativePointsForQuadratic1D(derivativePoints);
            var secondRoots = GetLinearRoots(secondDerivativePoints);
            roots.AddRange(secondRoots);

            //calculate values at roots
            for (int i = 0; i < roots.Count; i++)
                roots[i] = CalcBezierQuadratic1D(roots[i], controlPoints);

            return GetMinMax(zeroT, oneT, roots);
        }

        //Just a helper function to get min and max from given val0 val1 and list :)
        private (float min, float max) GetMinMax(float val0, float val1, List<float> others)
        {
            var min = math.min(val0, val1);
            var max = math.max(val0, val1);
            foreach (var root in others)
            {
                min = math.min(min, root);
                max = math.max(max, root);
            }

            return (min, max);
        }

        #endregion
    }




}