//Matrix operations
float3x3 ColumnsToMatrices(in float3 c1, in float3 c2, in float3 c3)
{
    return float3x3(c1, c2, c3);
}

float3x3 ScaleMatrixFromAspect(in float aspect)
{
    return float3x3(float3(aspect, 0, 0), float3(0, 1, 0), float3(0, 0, 1));
}

float3x3 InverseScaleMatrixFromAspect(in float aspect)
{
    return float3x3(float3(1.0 / aspect, 0, 0), float3(0, 1, 0), float3(0, 0, 1));
}

float2 TransformPoint(in float3x3 m, in float2 point2d)
{
    return mul(m, float3(point2d.x, point2d.y, 1)).xy;
}

float2 TransformDirection(in float3x3 m, in float2 point2d)
{
    return mul(m, float3(point2d.x, point2d.y, 0)).xy;
}

//SDF LINE
float line_segment(in half2 p, in half2 a, in half2 b)
{
    half2 ba = b - a;
    half2 pa = p - a;
    half h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - h * ba);
}

//SDF CIRCLE
float circle(in half2 p, in half2 c)
{
    return length(p - c);
}

//SDF CIRCLE
// float circle(in half2 p, in half2 c, in half r)
// {
//     return length(p - c) - r;
// }