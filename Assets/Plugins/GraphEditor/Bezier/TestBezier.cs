using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestBezier : MonoBehaviour
{
    [SerializeField] private LineRenderer m_LineRendererQuadratic = default;
    [SerializeField] private LineRenderer m_LineRendererCubic = default;

    void Start()
    {
        var bezier = new Bezier();

        //https://pomax.github.io/bezierinfo/#flattening example from here to visually check its ok
        var controlPointsQuadratic = new float2[]
        {
            new float2(0.7f, 2.5f),
            new float2(0.2f, 1.1f),
            new float2(2.2f, 0.6f)
        };

        var controlPointsCubic = new float2[]
        {
            new float2(1.1f, 1.5f),
            new float2(0.25f, 1.9f),
            new float2(2.1f, 2.5f),
            new float2(2.1f, 0.3f),
        };

        var resultQuadratic = bezier.GetPoints2D(controlPointsQuadratic, 16);
        m_LineRendererQuadratic.positionCount = resultQuadratic.Count;
        for(int i =0; i< resultQuadratic.Count;i++)
            m_LineRendererQuadratic.SetPosition(i, new Vector3(resultQuadratic[i].x, resultQuadratic[i].y, 0));

        var resultCubic = bezier.GetPoints2D(controlPointsCubic, 24);
        m_LineRendererCubic.positionCount = resultCubic.Count;
        for (int i = 0; i < resultCubic.Count; i++)
            m_LineRendererCubic.SetPosition(i, new Vector3(resultCubic[i].x, resultCubic[i].y, 0));
    }
}
