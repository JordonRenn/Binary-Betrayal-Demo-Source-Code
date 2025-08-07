using UnityEngine;
using System;

#region SBG Math
public static class SBGMath
{
    /// <summary>
    /// Performs smooth step interpolation between two values
    /// </summary>
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        // Clamp x to 0..1 range
        x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        // Evaluate polynomial
        return x * x * (3 - 2 * x);
    }

    /// <summary>
    /// Performs smoother step interpolation (Ken Perlin's improvement)
    /// </summary>
    public static float SmootherStep(float edge0, float edge1, float x)
    {
        // Clamp x to 0..1 range
        x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
        // Evaluate polynomial (6x^5 - 15x^4 + 10x^3)
        return x * x * x * (x * (x * 6 - 15) + 10);
    }

    /// <summary>
    /// Returns an eased value using a cubic Bezier curve
    /// </summary>
    public static float BezierInterpolation(float start, float end, float t, float control1, float control2)
    {
        float oneMinusT = 1f - t;
        float oneMinusTSquared = oneMinusT * oneMinusT;
        float tSquared = t * t;

        return oneMinusTSquared * oneMinusT * start +
               3f * oneMinusTSquared * t * control1 +
               3f * oneMinusT * tSquared * control2 +
               tSquared * t * end;
    }

    /// <summary>
    /// Hermite interpolation between two points
    /// </summary>
    public static float HermiteInterpolation(float start, float end, float startTangent, float endTangent, float t)
    {
        float tSquared = t * t;
        float tCubed = tSquared * t;

        float h1 = 2f * tCubed - 3f * tSquared + 1f;
        float h2 = -2f * tCubed + 3f * tSquared;
        float h3 = tCubed - 2f * tSquared + t;
        float h4 = tCubed - tSquared;

        return h1 * start + h2 * end + h3 * startTangent + h4 * endTangent;
    }

    /// <summary>
    /// Performs spring-damper interpolation (for smooth camera following)
    /// </summary>
    public static Vector3 SpringInterpolation(Vector3 current, Vector3 target, ref Vector3 velocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Infinity)
    {
        return Vector3.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);
    }

    /// <summary>
    /// Interpolates between quaternions with constant angular velocity
    /// </summary>
    public static Quaternion SlerpConstantVelocity(Quaternion from, Quaternion to, float angularVelocity, float deltaTime)
    {
        float angle = Quaternion.Angle(from, to);
        if (angle < 0.0001f)
            return to;

        float t = Mathf.Min(1.0f, angularVelocity * deltaTime / angle);
        return Quaternion.Slerp(from, to, t);
    }

    /// <summary>
    /// Maps a value from one range to another
    /// </summary>
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    /// <summary>
    /// Returns a vector with the specified direction and magnitude
    /// </summary>
    public static Vector3 SetVectorMagnitude(Vector3 vector, float magnitude)
    {
        return vector.normalized * magnitude;
    }

    /// <summary>
    /// Returns a random point inside a sphere
    /// </summary>
    public static Vector3 RandomPointInSphere(float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
        float randomDistance = UnityEngine.Random.Range(0f, radius);
        return randomDirection * randomDistance;
    }

    /// <summary>
    /// Returns a random point on a sphere's surface
    /// </summary>
    public static Vector3 RandomPointOnSphere(float radius)
    {
        return UnityEngine.Random.onUnitSphere * radius;
    }

    /// <summary>
    /// Calculates a point on a bezier curve
    /// </summary>
    public static Vector3 BezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)³ * P0
        p += 3f * uu * t * p1; // 3(1-t)² * t * P1
        p += 3f * u * tt * p2; // 3(1-t) * t² * P2
        p += ttt * p3; // t³ * P3

        return p;
    }

    /// <summary>
    /// Calculates the closest point on a line segment to a given point
    /// </summary>
    public static Vector3 ClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        lineDirection.Normalize();

        Vector3 pointVector = point - lineStart;
        float dot = Vector3.Dot(pointVector, lineDirection);

        // Clamp dot to line segment
        dot = Mathf.Clamp(dot, 0f, lineLength);

        return lineStart + lineDirection * dot;
    }

    /// <summary>
    /// Determines if a point is inside a triangle
    /// </summary>
    public static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Convert to 2D coordinates by discarding the smallest component
        int minComponent = MinComponentIndex(Vector3.Cross(b - a, c - a).normalized);
        Vector2 point = RemoveComponent(p, minComponent);
        Vector2 triA = RemoveComponent(a, minComponent);
        Vector2 triB = RemoveComponent(b, minComponent);
        Vector2 triC = RemoveComponent(c, minComponent);

        // Check if point is inside triangle using barycentric coordinates
        float denominator = ((triB.y - triC.y) * (triA.x - triC.x) + (triC.x - triB.x) * (triA.y - triC.y));
        float u = ((triB.y - triC.y) * (point.x - triC.x) + (triC.x - triB.x) * (point.y - triC.y)) / denominator;
        float v = ((triC.y - triA.y) * (point.x - triC.x) + (triA.x - triC.x) * (point.y - triC.y)) / denominator;
        float w = 1 - u - v;

        return u >= 0f && v >= 0f && w >= 0f;
    }

    /// <summary>
    /// Returns the index of the component with the smallest absolute value
    /// </summary>
    private static int MinComponentIndex(Vector3 v)
    {
        float absX = Mathf.Abs(v.x);
        float absY = Mathf.Abs(v.y);
        float absZ = Mathf.Abs(v.z);

        if (absX < absY && absX < absZ) return 0; // X is smallest
        if (absY < absX && absY < absZ) return 1; // Y is smallest
        return 2; // Z is smallest
    }

    /// <summary>
    /// Removes a component from a Vector3 and returns a Vector2
    /// </summary>
    private static Vector2 RemoveComponent(Vector3 v, int component)
    {
        switch (component)
        {
            case 0: return new Vector2(v.y, v.z); // Remove X
            case 1: return new Vector2(v.x, v.z); // Remove Y
            default: return new Vector2(v.x, v.y); // Remove Z
        }
    }

    /// <summary>
    /// Calculates a trajectory for a projectile to hit a target
    /// </summary>
    /// <returns>Launch velocity vector, null if no solution exists</returns>
    public static Vector3? CalculateProjectileVelocity(Vector3 startPos, Vector3 targetPos, float arcHeight, float gravity)
    {
        Vector3 toTarget = targetPos - startPos;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float xzDistance = toTargetXZ.magnitude;

        // Calculate time to reach target
        float time = Mathf.Sqrt(2f * (arcHeight + toTarget.y) / gravity) + Mathf.Sqrt(2f * arcHeight / gravity);

        // Check if target is reachable
        if (float.IsNaN(time)) return null;

        // Calculate initial velocity
        Vector3 velocity = toTargetXZ.normalized * (xzDistance / time);
        velocity.y = Mathf.Sqrt(2f * gravity * arcHeight) + toTarget.y / time;

        return velocity;
    }

    /// <summary>
    /// Performs orbit camera update (rotates around a target)
    /// </summary>
    /// <returns>New camera position and rotation</returns>
    public static (Vector3 position, Quaternion rotation) UpdateOrbitCamera(
        Vector3 targetPos, float distance, float pitch, float yaw)
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 negDistance = new Vector3(0, 0, -distance);
        Vector3 position = rotation * negDistance + targetPos;

        return (position, rotation);
    }

    /// <summary>
    /// Performs a weighted blend of multiple vectors
    /// </summary>
    public static Vector3 WeightedBlendVectors(Vector3[] vectors, float[] weights)
    {
        if (vectors.Length != weights.Length)
            throw new ArgumentException("Vectors and weights arrays must have the same length");

        Vector3 result = Vector3.zero;
        float totalWeight = 0f;

        for (int i = 0; i < vectors.Length; i++)
        {
            result += vectors[i] * weights[i];
            totalWeight += weights[i];
        }

        if (totalWeight > 0f)
            result /= totalWeight;

        return result;
    }

    /// <summary>
    /// Returns a random point within an annular region (donut shape)
    /// </summary>
    public static Vector2 RandomPointInAnnulus(float innerRadius, float outerRadius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(UnityEngine.Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

        return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    }

    /// <summary>
    /// Checks if a point is inside a convex polygon
    /// </summary>
    public static bool PointInConvexPolygon(Vector2 point, Vector2[] polygonPoints)
    {
        int numPoints = polygonPoints.Length;
        bool inside = false;

        for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
        {
            if (((polygonPoints[i].y > point.y) != (polygonPoints[j].y > point.y)) &&
                (point.x < (polygonPoints[j].x - polygonPoints[i].x) * (point.y - polygonPoints[i].y) /
                (polygonPoints[j].y - polygonPoints[i].y) + polygonPoints[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    /// <summary>
    /// Easing function with customizable curve
    /// </summary>
    public static float EaseCustom(float t, float power)
    {
        if (power < 0)
            return 1f - Mathf.Pow(1f - t, -power);
        else
            return Mathf.Pow(t, power);
    }

    public struct TargetFloatRange //create a range using floats
    {
        public float minValue { get; private set; }
        public float maxValue { get; private set; }

        public TargetFloatRange(float min, float max)
        {
            this.minValue = min;
            this.maxValue = max;
        }
    }
}
#endregion