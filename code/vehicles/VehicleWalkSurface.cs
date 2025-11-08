using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VehicleWalkSurface : MonoBehaviour
{
    [Tooltip("Optional. If left null, will use the BoxCollider on the same GameObject.")]
    public BoxCollider surface;

    [Tooltip("Tag this GameObject with this tag to make it discoverable via raycast.")]
    public string surfaceTag = "VehicleWalkSurface";

    void Reset()
    {
        var bc = GetComponent<BoxCollider>();
        bc.isTrigger = true; // we only use bounds; collisions come from the player’s capsule
        gameObject.tag = surfaceTag;
        surface = bc;
    }

    public Bounds WorldBounds
    {
        get
        {
            if (!surface) surface = GetComponent<BoxCollider>();
            var b = surface.bounds;
            return b;
        }
    }

    //public Vector3 ClampWorldPosition(Vector3 worldPos, float margin = 0.05f)
    //{
    //    var b = WorldBounds;
    //    float x = Mathf.Clamp(worldPos.x, b.min.x + margin, b.max.x - margin);
    //    float z = Mathf.Clamp(worldPos.z, b.min.z + margin, b.max.z - margin);
    //    // keep current Y so the player stays on the deck height managed by your controller
    //    return new Vector3(x, worldPos.y, z);
    //}

    public Vector3 ClampWorldPosition(Vector3 worldPos, float margin = 0.05f)
    {
        if (!surface) surface = GetComponent<BoxCollider>();

        // Work in the collider's local space (handles rotation/scale properly)
        Transform t = surface.transform;

        // Convert the world point into the surface's local space
        Vector3 local = t.InverseTransformPoint(worldPos);

        // BoxCollider center/size are in local space
        Vector3 c = surface.center;
        Vector3 e = surface.size * 0.5f; // half extents

        // Clamp only X/Z (top-down footprint). Keep local Y unchanged.
        float lx = Mathf.Clamp(local.x, c.x - e.x + margin, c.x + e.x - margin);
        float lz = Mathf.Clamp(local.z, c.z - e.z + margin, c.z + e.z - margin);

        Vector3 clampedLocal = new Vector3(lx, local.y, lz);

        // Back to world space
        Vector3 clampedWorld = t.TransformPoint(clampedLocal);

        // Preserve the caller's Y (your controller manages deck height)
        clampedWorld.y = worldPos.y;

        return clampedWorld;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!surface) surface = GetComponent<BoxCollider>();
        if (!surface) return;

        Gizmos.color = Color.cyan;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = surface.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(surface.center, surface.size);
        Gizmos.matrix = oldMatrix;
    }

    private void OnDrawGizmosSelected()
    {
        if (!surface) surface = GetComponent<BoxCollider>();
        if (!surface) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // semi-transparent green
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = surface.transform.localToWorldMatrix;
        Gizmos.DrawCube(surface.center, surface.size);
        Gizmos.matrix = oldMatrix;
    }
#endif
}
