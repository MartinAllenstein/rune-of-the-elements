using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    public Transform GetWaypoint(int index) => waypoints[index];
    public int WaypointCount => waypoints.Length;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
    }
}
