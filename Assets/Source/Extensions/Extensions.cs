using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
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
        // Om vår hastighet och normal pekar åt samma håll (dot = positiv), bör det inte finnas någon normalkraft.
        if (dot > 0f)
            return Vector3.zero;

        Vector3 projection = dot * normal; // (Skalärprodukten mellan vektorn velocity och normaliserade vektorn normal) * normaliserade vektorn normal
        return -projection; // The normal force returned
    } // Calculation of the normal force
    public static Vector3 ToInput(this Vector3 position, Transform origin)
    {
        return new Vector3(
            Vector3.Dot(origin.transform.right, (position - origin.transform.position)),
            0f,
            Vector3.Dot(origin.transform.forward, (position - origin.transform.position)));
    }

    public static Vector3 DirectionTo(this Vector3 origin, Vector3 target)
    {
        return target - origin;
    }
    public static Vector3 DirectionFrom(this Vector3 origin, Vector3 target)
    {
        return origin - target;
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
