using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System;
using System.Numerics;

//The ideas of some algorithms take start here:
//https://hal.inria.fr/file/index/docid/518379/filename/Xiao-DiaoChen2007c.pdf
//https://stackoverflow.com/questions/2742610/closest-point-on-a-cubic-bezier-curve
//http://jazzros.blogspot.com/2011/03/projecting-point-on-bezier-curve.html

namespace ViJ.GraphEditor.MathUtils
{
    public struct BezierSamplePoint
    {
        public float2 Position;
        public float T;
    }

    public class CubicBezierSegment2D
    {
        private float2 m_P0;
        private float2 m_P1;
        private float2 m_P2;
        private float2 m_P3;

        //First derivativer
        private float2 m_Der1A;
        private float2 m_Der1B;
        private float2 m_Der1C;

        //Second derivative
        private float2 der2A;
        private float2 der2B;

        //Buffer for calculations
        private float[] m_Roots = new float[3];

        public CubicBezierSegment2D(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            m_P0 = p0;
            m_P1 = p1;
            m_P2 = p2;
            m_P3 = p3;

            CalcDerivativePointsForBoundsCalculation();
        }

        public float2 Sample(float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            return m_P0 * mt3 + 3 * m_P1 * mt2 * t + 3 * m_P2 * mt * t2 + m_P3 * t3;
        }

        /// <summary>
        /// Gets distance from point to curve
        /// </summary>
        /// <param name="point"></param>
        /// <param name="nearestPoint"></param>
        /// <returns></returns>
        public float DistanceTo(float2 point, out float2 nearestPoint)
        {
            var complexRoots = new List<Complex>();

            var p0 = m_P0 - point;
            var p1 = m_P1 - point;
            var p2 = m_P2 - point;
            var p3 = m_P3 - point;

            var A = p3 - 3 * p2 + 3 * p1 - p0;
            var B = 3 * p2 - 6 * p1 + 3 * p0;
            var C = 3 * (p1 - p0);

            var q5 = 3 * A * A;
            var q4 = 5 * A * B;
            var q3 = 4 * A * C + 2 * B * B;
            var q2 = 3 * B * C + 3 * A * p0;
            var q1 = C * C + 2 * B * p0;
            var q0 = C * p0;

            nearestPoint = float2.zero;
            var minSqDistance = float.PositiveInfinity;

            //First lets check the first and the last points
            var distToFirst = math.distancesq(m_P0, point);
            if (math.distance(m_P0, point) < minSqDistance)
            {
                minSqDistance = distToFirst;
                nearestPoint = m_P0;
            }
            var distToLast = math.distancesq(m_P3, point);
            if (math.distance(m_P3, point) < minSqDistance)
            {
                minSqDistance = distToLast;
                nearestPoint = m_P3;
            }

            //Than try to solve equation and get real roots
            var roots = RealPolynomialRootFinder.FindRoots(complexRoots, ComponentalSum(q5), ComponentalSum(q4), ComponentalSum(q3), ComponentalSum(q2), ComponentalSum(q1), ComponentalSum(q0));
            var filtered = roots.Where(c => c.Imaginary == 0 && c.Real <= 1 && c.Real >= 0).Select(c => (float)c.Real);
            foreach (var root in filtered)
            {
                var valueAtRoot = Sample(root);
                var distanceSq = math.distancesq(valueAtRoot, point);
                if (distanceSq < minSqDistance)
                {
                    minSqDistance = distanceSq;
                    nearestPoint = valueAtRoot;
                }
            }

            return math.sqrt(minSqDistance);
        }

        /// <summary>
        /// Calculates arc length with given thresh
        /// </summary>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public float CalculateArcLength(float epsilon = 1e-2f) => CalculateArcLength(epsilon, out _);

        /// <summary>
        /// Calculates arc length with given thresh
        /// </summary>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public float CalculateArcLength(float epsilon, out int segmentCount)
        {
            var lastCalculationResult = CalculateArcLength(10);
            var currentCalculationResult = CalculateArcLength(20);
            var error = math.abs(lastCalculationResult - currentCalculationResult);
            var segCount = 40;
            while (error > epsilon)
            {
                lastCalculationResult = currentCalculationResult;
                currentCalculationResult = CalculateArcLength(segCount);
                error = math.abs(lastCalculationResult - currentCalculationResult);
                segCount *= 2;
            }
            segmentCount = segCount;
            return currentCalculationResult;
        }

        /// <summary>
        /// Calculates arc length for given points cound
        /// </summary>
        /// <param name="segmentCount"></param>
        /// <returns></returns>
        public float CalculateArcLength(int segmentCount)
        {
            var step = 1f / segmentCount;
            var prevPoint = Sample(0);
            var sum = 0f;
            for (int i = 1; i <= segmentCount; i++)
            {
                var newPoint = Sample(i * step);
                sum += math.distance(prevPoint, newPoint);
                prevPoint = newPoint;
            }
            return sum;
        }

        /// <summary>
        /// Flattens spline with given count of segments
        /// </summary>
        /// <param name="segmentCount"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public List<BezierSamplePoint> FlattenSpline(int segmentCount, List<BezierSamplePoint> result = null)
        {
            if (result == null)
                result = new List<BezierSamplePoint>(segmentCount + 1);
            else
                result.Clear();

            var step = 1f / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                var t = i * step;
                result.Add(new BezierSamplePoint()
                {
                    Position = Sample(t),
                    T = t,
                });
            }
            result.Add(new BezierSamplePoint()
            {
                Position = Sample(1f),
                T = 1,
            });
            return result;
        }

        public (float2 min, float2 max) GetMinMax()
        {
            var min = math.min(m_P0, m_P3);
            var max = math.max(m_P0, m_P3);

            for (int i = 0; i < 2; i++)
            {
                GetAllDimensionRoots(i, m_Roots);
                foreach (var root in m_Roots)
                {
                    if (root != 0)
                    {
                        var val = Sample(root);
                        min = math.min(min, val);
                        max = math.max(max, val);
                    }
                }
            }

            return (min, max);
        }

        /// <summary>
        /// Finds roots for first and second derivatives of the curve.
        /// It is used to find bounding box of the curve
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="roots"></param>
        private void GetAllDimensionRoots(int dimension, float[] roots)
        {
            SearchQuadRoots(dimension, out var t1, out var t2);
            var t3 = SearchLinearRoots(dimension);
            roots[0] = t1;
            roots[1] = t2;
            roots[2] = t3;
        }

        /// <summary>
        /// Finds roots for square equation
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="resultT1"></param>
        /// <param name="resultT2"></param>
        private void SearchQuadRoots(int dimension, out float resultT1, out float resultT2)
        {
            resultT1 = 0;
            resultT2 = 0;

            var a = m_Der1A[dimension] - 2 * m_Der1B[dimension] + m_Der1C[dimension];
            var b = 2 * (m_Der1B[dimension] - m_Der1A[dimension]);
            var c = m_Der1A[dimension];

            var b2min4ac = b * b - 4 * a * c;
            if (b2min4ac >= 0)
            {
                var aBy2 = 2 * a;
                var sqrt = math.sqrt(b2min4ac);
                var root1 = (-b - sqrt) / (aBy2);
                var root2 = (-b + sqrt) / (aBy2);

                if (root1 >= 0 && root1 <= 1)
                    resultT1 = root1;
                if (root2 >= 0 && root2 <= 1)
                    resultT2 = root2;
            }
        }

        /// <summary>
        /// Finds roots for linear equation
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        private float SearchLinearRoots(int dimension)
        {
            var a = der2A[dimension];
            var b = der2B[dimension];
            var bSubA = b - a;
            if (b - a != 0)
            {
                var root = -a / (bSubA);
                if (root >= 0 && root <= 1)
                    return root;
            }
            return 0f;
        }

        private void CalcDerivativePointsForBoundsCalculation()
        {
            m_Der1A = 3 * (m_P1 - m_P0);
            m_Der1B = 3 * (m_P2 - m_P1);
            m_Der1C = 3 * (m_P3 - m_P2);

            der2A = 2 * (m_Der1B - m_Der1A);
            der2B = 2 * (m_Der1C - m_Der1B);
        }

        private float ComponentalSum(float2 point)
        {
            return point.x + point.y;
        }
    }
}