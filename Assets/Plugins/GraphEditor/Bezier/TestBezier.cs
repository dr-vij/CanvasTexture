using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestBezier : MonoBehaviour
{
    [SerializeField] private LineRenderer m_LineRendererQuadratic = default;
    [SerializeField] private LineRenderer m_LineRendererCubic = default;

    private float2 m_MinQuad;
    private float2 m_MaxQuad;

    private float2 m_MinCubic;
    private float2 m_MaxCubic;

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
        for (int i = 0; i < resultQuadratic.Count; i++)
            m_LineRendererQuadratic.SetPosition(i, new Vector3(resultQuadratic[i].x, resultQuadratic[i].y, 0));
        var minMaxQuad = bezier.GetMinMaxQuadraticBezier2D(controlPointsQuadratic);
        m_MinQuad = minMaxQuad.min;
        m_MaxQuad = minMaxQuad.max;

        var resultCubic = bezier.GetPoints2D(controlPointsCubic, 24);
        m_LineRendererCubic.positionCount = resultCubic.Count;
        for (int i = 0; i < resultCubic.Count; i++)
            m_LineRendererCubic.SetPosition(i, new Vector3(resultCubic[i].x, resultCubic[i].y, 0));
        var minMaxCoub = bezier.GetMinMaxCubicBezier2D(controlPointsCubic);
        m_MinCubic = minMaxCoub.min;
        m_MaxCubic = minMaxCoub.max;
    }

    private void OnDrawGizmos()
    {
        var sizeQuad = m_MaxQuad - m_MinQuad;
        var centerQuad = (m_MinQuad + m_MaxQuad) * 0.5f;

        var sizeCube = m_MaxCubic - m_MinCubic;
        var centerCube = (m_MinCubic + m_MaxCubic) * 0.5f;

        Gizmos.DrawWireCube((Vector2)centerQuad, (Vector2)sizeQuad);
        Gizmos.DrawWireCube((Vector2)centerCube, (Vector2)sizeCube);
    }
}
