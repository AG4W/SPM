using UnityEngine;

public static class Extensions
{
    //arrays
    public static T Random<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
    public static T First<T>(this T[] array)
    {
        return array[0];
    }
    public static T Last<T>(this T[] array)
    {
        return array[array.Length - 1];
    }

    //floats
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
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
