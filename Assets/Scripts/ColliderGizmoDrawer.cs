using UnityEngine;

[ExecuteAlways]
public class ColliderGizmoDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var collider = GetComponent<Collider>();
        if (collider is BoxCollider box)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}