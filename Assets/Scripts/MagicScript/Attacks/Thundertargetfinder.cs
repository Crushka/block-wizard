using UnityEngine;


public static class ThunderTargetFinder
{
    public static Vector3 Find(Vector3 origin, Vector3 forward, float range, float cylinderRadius, LayerMask enemyMask)
    {
        Vector3 end = origin + forward * range;

        Collider[] hits = Physics.OverlapCapsule(origin, end, cylinderRadius, enemyMask);

        if (hits.Length == 0)
            return end;

        Collider nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var col in hits)
        {
            float dist = Vector3.Distance(origin, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = col;
            }
        }

      
        return nearest.transform.position;
    }
}