using System;
using System.Collections.Generic;
using UnityEngine;

public class WaypointList : MonoBehaviour
{
    // gran capacidad de uso en runtime, re leveldesing friendly. Pones los waypoints como hijos. Clickeas Load Children y gg
    // Overwrite los renombra y cambia el color del gizmos para que queden ordenados y bonitos
    
    [SerializeField] private List<Waypoint> waypointList = new List<Waypoint>();
    public Color lineColor = Color.white;
    public bool overwriteChildens;
    public bool showGizmos = true;

    public Waypoint currentPoint;
    private int _currentIndex;

    private void Awake()
    {
        if (waypointList.Count == 0)
            throw new NotImplementedException("No tiene puntos asignados");
        
        currentPoint = waypointList[_currentIndex];
    }

    public Waypoint GetNextPoint()
    {
        _currentIndex++;
        if (_currentIndex >= waypointList.Count) _currentIndex = 0;
        return currentPoint = waypointList[_currentIndex];
    }

    public Waypoint GetClosestWaypoint(Vector3 position)
    {
        var distance = float.MaxValue;
        for (var i = 0; i < waypointList.Count; i++)
        {
            var point = waypointList[i];
            var dist = Vector3.Distance(position, point.transform.position);
            
            if (!(dist < distance)) continue;
                currentPoint = point;
                _currentIndex = i;
                distance = dist;
        }

        // para evitar q vuelva hacia atras
        
        var nextIndex = _currentIndex+1;
        if (nextIndex >= waypointList.Count) nextIndex = 0;
        
        var currentPos = currentPoint.transform.position;
        var dirNextNode = (waypointList[nextIndex].transform.position - currentPos).normalized;
        var dirToAgent = (position - currentPos).normalized;
        if(Mathf.Abs( Vector3.Angle(dirNextNode, dirToAgent) ) < 90f)
            GetNextPoint();
        
        
        return currentPoint;
    }

    public bool loadChildrens;
    private void OnValidate()
    {
        if (loadChildrens)
        {
            loadChildrens = false;
            var waypoints = GetComponentsInChildren<Waypoint>();
            foreach (var point in waypoints)
            {
                if(!waypointList.Contains(point))
                    waypointList.Add(point);
            }
        }
        if (!overwriteChildens) return;
        for (var i = 0; i < waypointList.Count; i++)
        {
            var point = waypointList[i];
            point.colorGizmo = lineColor;
            point.showGizmos = showGizmos;
            point.name = i + "-" + name + "-Point";
        }
    }

    private void OnDrawGizmos()
    {
        if(!showGizmos) return;
        if ( waypointList.Count < 2 ) return;
        
        Gizmos.color = lineColor;
        var oldPoint = waypointList[0].transform.position;
        Vector3 mid , point;
        for ( var i = 1; i < waypointList.Count; i++ )
        {
            if(waypointList[i] == null) continue;
            point = waypointList[i].transform.position;
            
            
            Gizmos.DrawLine(oldPoint, point);
            mid = (oldPoint + point)/2;
            ArrowGizmo(mid, (point - oldPoint).normalized);
            oldPoint = point;
        }

        point = waypointList[0].transform.position;
        Gizmos.DrawLine(oldPoint, point);
        mid = (oldPoint + point)/2;
        ArrowGizmo(mid, (point - oldPoint).normalized);
    }
    
     
    private static void ArrowGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.3f, float arrowHeadAngle = 30.0f)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos, right * arrowHeadLength);
        Gizmos.DrawRay(pos, left * arrowHeadLength);
    }
}
