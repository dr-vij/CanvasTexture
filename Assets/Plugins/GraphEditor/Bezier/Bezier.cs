using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

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

    public int BinominalBruteforce(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

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

}
