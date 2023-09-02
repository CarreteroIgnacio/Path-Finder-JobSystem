using Unity.Mathematics;
using UnityEngine;

/*
 * Quisas algunas cosas parescan raras, pero estan asi para lograr el mayor rendimiento posible. Dado q los nodos estan un native array persitens
 * no se estan inicializando todo el tiempo
 */
public struct Node
{
    public int2 Coords { get; }
    public Vector3 WorldPosition { get; }
    public bool IsWalkable { get; private set; }

    public int Index { get; }
    public Node(int2 cor, Vector3 pos, int index)
    {
        Coords = cor;
        WorldPosition = pos;
        Index = index;
        
        IsWalkable = true;
        
        gCost = int.MaxValue;
        hCost = 0;
        fCost = 0;
        CameFromNodeIndex = -1;
    }


    public void SetIsWalkable(bool isWalkable) => IsWalkable = isWalkable;

    public int CameFromNodeIndex { get; private set; }
    
    public int gCost { get; private set; }

    public int hCost { get; private set; }

    public int fCost { get; private set; }

    public void CalculateFCost() => fCost = gCost + hCost;

    public void ResetNode()
    {
        gCost = int.MaxValue;
        fCost = 0;
        hCost = 0;
        CameFromNodeIndex = -1;
    }
    public void SetGcost(int cost)
    {
        gCost = cost;
        CalculateFCost();
    }

    public void SetHcost(int cost)
    {
        hCost = cost;
        CalculateFCost();
    }

    public void SetComeIndex(int nya) => CameFromNodeIndex = nya;
}