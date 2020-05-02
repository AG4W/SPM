using UnityEngine;

using System.Collections;
using System.Collections.Generic;
public static class Extensions
{
    //floats
    //try using these when lerping to improve feels
    //linear interpolation feels boring af
    //glorious curves superior
    public static float Smoothstep(this float t)
    {
        return t * t * (3f - 2f * t);
    }
    public static float Smootherstep(this float t)
    {
        return t * t * t * (t * (6f * t - 15f) + 10f);
    }
    public static float EaseOut(this float t)
    {
        return Mathf.Sin(t * Mathf.PI * .5f);
    }
    public static float EaseIn(this float t)
    {
        return 1f - Mathf.Cos(t * Mathf.PI * .5f);
    }
    public static float Interpolate(this float t, InterpolationMode mode)
    {
        switch (mode)
        {
            case InterpolationMode.Linear:
                return t;
            case InterpolationMode.Smoothstep:
                return t.Smoothstep();
            case InterpolationMode.Smootherstep:
                return t.Smootherstep();
            case InterpolationMode.EaseIn:
                return t.EaseIn();
            case InterpolationMode.EaseOut:
                return t.EaseOut();
            default:
                return t;
        }
    }

    //arrays
    public static T Random<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length - 1)];
    }
    public static T First<T>(this T[] array)
    {
        return array[0];
    }
    public static T Last<T>(this T[] array)
    {
        return array[array.Length - 1];
    }
    public static T Random<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count - 1)];
    }
    public static T First<T>(this List<T> list)
    {
        return list[0];
    }
    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    //Vectors
    public static Vector3 TransformToLocalDirection(this Vector3 worldDirection, Transform origin)
    {
        return (origin.right * worldDirection.x) + (origin.forward * worldDirection.z);
    }
    public static Vector3 GetNormalForce(this Vector3 velocity, Vector3 normal)
    {
        // Skalärprodukten mellan vektorn velocity och (normaliserade) vektorn normal
        float dot = Vector3.Dot(velocity, normal);
        
        if (dot > 0f) // Om vår hastighet och normal pekar åt samma håll (dot = positiv), bör det inte finnas någon normalkraft.
            return Vector3.zero;

        Vector3 projection = dot * normal; // (Skalärprodukten mellan vektorn velocity och normaliserade vektorn normal) * normaliserade vektorn normal

        return -projection; // The normal force returned
    }
    public static Vector3 ToInput(this Vector3 position, Transform origin)
    {
        return new Vector3(
            Vector3.Dot(origin.transform.right, (position - origin.transform.position)),
            0f,
            Vector3.Dot(origin.transform.forward, (position - origin.transform.position)));
    }
    public static Vector3 PointOnCircle(this Vector3 center, float radius, float angleInDegrees)
    {
        float a = angleInDegrees * Mathf.PI / 180f;
        return center + (new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius);
    }

    public static Vector3 ToXZ(this Vector3 vector, float y = 0f)
    {
        vector.y = y;

        return vector;
    }

    public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
    {
        return target - origin;
    }
    public static Vector3 DirectionFrom(this Vector3 origin, Vector3 target)
    {
        return origin - target;
    }
    public static float Dot(this Vector3 lhs, Vector3 rhs)
    {
        return Vector3.Dot(lhs, rhs);
    }

    public static float DistanceTo(this Vector3 origin, Vector3 target)
    {
        return Vector3.Distance(origin, target);
    }

    //transform
    public static Transform FindRecursively(this Transform origin, string name)
    {
        if (origin.Find(name) != null)
            return origin.Find(name);

        foreach (Transform child in origin)
        {
            if (child.FindRecursively(name) != null)
                return child.FindRecursively(name);
        }

        return null;
    }
}
public enum InterpolationMode
{
    Linear,
    Smoothstep,
    Smootherstep,
    EaseIn,
    EaseOut,
}