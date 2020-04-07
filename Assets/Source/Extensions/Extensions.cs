using UnityEngine;

public static class Extensions
{
    public static T Random<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
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
            dot = 0f;

        Vector3 projection = dot * normal; // (Skalärprodukten mellan vektorn velocity och normaliserade vektorn normal) * normaliserade vektorn normal
        return -projection; // The normal force returned
    } // Calculation of the normal force
}
