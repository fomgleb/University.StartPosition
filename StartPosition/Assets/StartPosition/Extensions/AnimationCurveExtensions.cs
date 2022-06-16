using UnityEngine;

namespace StartPosition.Extensions
{
    public static class AnimationCurveExtensions
    {
        public static void CorrectKeys(this AnimationCurve curve, Keyframe minInclusive, Keyframe maxInclusive)
        {
            for (var i = 0; i < curve.length; i++)
            {
                var key = curve.keys[i];

                if (key.value > maxInclusive.value)
                    curve.MoveKey(i, new Keyframe(key.time, maxInclusive.value, key.inTangent, key.outTangent));
                else if (key.value < minInclusive.value)
                    curve.MoveKey(i, new Keyframe(key.time, minInclusive.value, key.inTangent, key.outTangent));

                if (key.time > maxInclusive.time)
                    curve.MoveKey(i, new Keyframe(maxInclusive.time, key.value, key.inTangent, key.outTangent));
                else if (key.time < minInclusive.time)
                    curve.MoveKey(i, new Keyframe(minInclusive.time, key.value, key.inTangent, key.outTangent));
            }
        }
        
        public static float GetAreaUnderCurve(this AnimationCurve curve, float width, float height)
        {
            var areaUnderCurve = 0f;
            var keys = curve.keys;
     
            for (var i = 0; i < keys.Length - 1; i++)
            {
                // Calculate the 4 cubic Bezier control points from Unity AnimationCurve (a hermite cubic spline) 
                var K1 = keys[i];
                var K2 = keys[i + 1];
                var A = new Vector2(K1.time * width, K1.value * height);
                var D = new Vector2(K2.time * width, K2.value * height);
                var e = (D.x - A.x) / 3.0f;
                var f = height / width;
                var B = A + new Vector2(e, e * f * K1.outTangent);
                var C = D + new Vector2(-e, -e * f * K2.inTangent);

                var a = -A.y + 3.0f * B.y - 3.0f * C.y + D.y;
                var b = 3.0f * A.y - 6.0f * B.y + 3.0f * C.y;
                var c = -3.0f * A.y + 3.0f * B.y;
                var d = A.y;
     
                var t = (K2.time - K1.time) * width;
     
                var area = (a / 4.0f + b / 3.0f + c / 2.0f + d) * t;
     
                areaUnderCurve += area;
            }
            return areaUnderCurve;
        }
    }
}