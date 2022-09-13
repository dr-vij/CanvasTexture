using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ViJApps.Utils
{
    public static class MathUtils
    {
        public static readonly float2 Floa2One = new float2(1, 1);

        public static readonly float2 Float2MinusOne = new float2(-1, -1);

        public static float2 RotateVectorCWHalfPi(this float2 vector) => new float2(vector.y, -vector.x);

        public static float2 RotateVectorCCWHalfPi(this float2 vector) => new float2(-vector.y, vector.x);

        public static float3 ToFloat3(this float2 val, float z = 0) => new float3(val.x, val.y, z);

        public static float2 RemapFromPercentToTexture(this float2 percentPosition) => math.remap(float2.zero, Floa2One, Float2MinusOne, Floa2One, percentPosition);

        public static float2 RemapFromPixelsToTexture(this float2 pixelPosition, float2 textureSize) => math.remap(float2.zero, textureSize, Float2MinusOne, Floa2One, pixelPosition);

        #region transformations2d

        /// <summary>
        /// Transforms point from local 2d space to world 2d space
        /// </summary>
        /// <param name="point"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float2 TransformPoint(this float2 point, float3x3 matrix) => math.mul(matrix, point.ToFloat3(1)).xy;

        /// <summary>
        /// Transforms vector from local 2d space to world 2d space
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float2 TransformDirection(this float2 direction, float3x3 matrix) => math.mul(matrix, direction.ToFloat3(0)).xy;

        /// <summary>
        /// Transforms point from world 2d space to local 2d space
        /// </summary>
        /// <param name="point"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float2 InverseTransformPoint(this float2 point, float3x3 matrix) => math.mul(math.inverse(matrix), point.ToFloat3(1)).xy;

        /// <summary>
        /// Transforms vector from world 2d space to local 2d space
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float2 InverseTransformDirection(this float2 direction, float3x3 matrix) => math.mul(math.inverse(matrix), direction.ToFloat3(0)).xy;

        #endregion

        #region 2d matrices

        /// <summary>
        /// Creates remap matrix, that will remap from min..max space to -1..1 space
        /// </summary>
        /// <param name="initialMin">initial min</param>
        /// <param name="initialMax"></param>
        /// <returns></returns>
        public static float3x3 CreateMatrix3d_RemapToOneMinusOne(float2 initialMin, float2 initialMax) =>
            CreateMatrix2d_RemapSpace(initialMin, initialMax, new float2(-1, -1), new float2(1, 1));

        /// <summary>
        /// Creates remap matrix, that will remap from minA..maxA space to minB..maxB space
        /// </summary>
        /// <param name="minA"></param>
        /// <param name="maxA"></param>
        /// <param name="minB"></param>
        /// <param name="maxB"></param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_RemapSpace(float2 minA, float2 maxA, float2 minB, float2 maxB)
        {
            var sizeA = maxA - minA;
            var scaleA = new float2(1, 1) / sizeA;

            var sizeB = maxB - minB;
            var scaleB = new float2(1, 1) / sizeB;

            var fromAto01 = math.mul(CreateMatrix2d_S(scaleA), CreateMatrix2d_T(-minA));
            var fromBto01 = math.mul(CreateMatrix2d_S(scaleB), CreateMatrix2d_T(-minB));
            var from01toB = math.inverse(fromBto01);
            return math.mul(from01toB, fromAto01);
        }
          
        /// <summary>
        /// Creates translation matrix for 2d
        /// </summary>
        /// <param name="translation">translation vector</param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_T(float2 translation) =>
            new float3x3(
                new float3(1, 0, 0),
                new float3(0, 1, 0),
                new float3(translation.x, translation.y, 1));

        /// <summary>
        /// Creates rotation matrix for 2d
        /// </summary>
        /// <param name="rotation">Rotation in radians</param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_R(float rotation) =>
            new float3x3(
                new float3(math.cos(rotation), math.sin(rotation), 0),
                new float3(math.sin(rotation), math.cos(rotation), 0),
                new float3(0, 0, 1));

        /// <summary>
        /// Creates scale matrix for 2d
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_S(float2 scale) =>
            new float3x3(
                new float3(scale.x, 0, 0),
                new float3(0, scale.y, 0),
                new float3(0, 0, 1));

        /// <summary>
        /// Creates translation and rotation and scale matrix for 2d
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotationRadians"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_TRS(float2 translation, float rotationRadians, float2 scale) =>
             math.mul(CreateMatrix2d_T(translation), math.mul(CreateMatrix2d_R(rotationRadians), CreateMatrix2d_S(scale)));

        /// <summary>
        /// Creates translation and scale matrix for 2d
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static float3x3 CreateMatrix2d_TS(float2 translation, float2 scale) =>
            new float3x3(
                new float3(scale.x, 0, 0),
                new float3(0, scale.y, 0),
                new float3(translation.x, translation.y, 1));

        #endregion
    }
}