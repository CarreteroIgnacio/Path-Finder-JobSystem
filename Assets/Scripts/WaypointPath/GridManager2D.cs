using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Exuli
{
    /*
     * El objetivo de esta clase es poder crear comodamente una grilla que detecte colisiones para invalidad los puntos
     * y exponerlos es un native array para que el path job pueda tomarlos facil, (muchos tutoriales la rehacen a al grilla cada vez)
     * A su vez tiene la capacidad de dado un vector 3 darte el punto en la grilla mas cercano. L
     * Lo cual te permite una gran independencia del tamaño de la grilla, segun su nodeDistance
     * como tambien tenes el NodeSize para determinar las colisiones, entonces dependiendo la situacion puede ser al maximo
     * o solo del tamaño del agent
     *
     * Por ultimo la gran maravilla los gizmos! y que esta grilla tiene la capacidad de recalcularse en runtime!
     * donde se puede ver claramente el tamaño de esta como tambien los puntos validos e invalidos por separado
     *
     * PD: Se llama 2D xq simplifique el codigo del juego de Motores donde tengo una A* en 3D en DOTS
    */
    public class GridManager2D : MonoBehaviour
    {

        #region Attributes

            [SerializeField] private int2 gridSize;

            [Tooltip("si esta activo recalcula la grilla con cada modificacion")]
            [SerializeField] private bool recalculateGrid;

            [Header("TestNodes")]
            [SerializeField] private float nodeDistance;
            [SerializeField] private float nodeSize;
            [SerializeField] private LayerMask collideWith;

        #endregion

        #region Serialized Vars
        
            public static GridManager2D Instance { get; private set; }
            public Node[][] GridNodes { get; private set; }
            public int2 GridSize => gridSize;
            public Vector3 BoxSize { get; private set; }
            public Vector3 StartPoint { get; private set; }
            
            public float NodeDistance => nodeDistance;

            public NativeArray<Node> PathNodeArray;
            
        #endregion
        
        
        private float _mHalfTestNode;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            InitializeGrid();
        }


        private void InitializeGrid()
        {
            var position = transform.position;
            //new int3((int)position.x, (int)position.y, (int)position.z);
            
            // inicializo las cosas aca para que se refresquen con el Recaluate Grid
            BoxSize = new Vector3(gridSize.x, 1,gridSize.y) * nodeDistance;
            _mHalfTestNode = nodeDistance / 2;
            StartPoint =
                position - BoxSize / 2f +
                Vector3.one * nodeDistance / 2; // De esta manera, la grilla tiene el pivot en el centro

            if (PathNodeArray.IsCreated) PathNodeArray.Dispose();
            
            PathNodeArray = new NativeArray<Node>(
                GridSize.x + gridSize.x * GridSize.y,
                Allocator.Persistent);
            
            //print("Native Array Lenght of: " + PathNodeArray.Length);
            
            
            GridNodes = new Node[gridSize.x][];
            for (var x = 0; x < gridSize.x; x++)
            {
                GridNodes[x] = new Node[gridSize.y];

                for (var y = 0; y < gridSize.y; y++)
                {
                        var pos = StartPoint + new Vector3(x, 0, y) * nodeDistance;
                        var index = x + y * gridSize.x;

                        var node = new Node(new int2(x, y), pos , index);

                        if (Physics.CheckSphere(pos, nodeSize / 2, collideWith))
                            node.SetIsWalkable(false);

                        GridNodes[x][y] = node;
                        PathNodeArray[node.Index] = node;
                }

            }
        }

        private void OnDisable() => PathNodeArray.Dispose();
        
        private void OnValidate()
        {
            if (gridSize.x < 1) gridSize.x = 1;
            if (gridSize.y < 1) gridSize.y = 1;


            if (nodeDistance < 0) nodeDistance = 0;
            if (nodeSize < 0) nodeSize = 0;


            if (nodeSize > nodeDistance)
                nodeSize = nodeDistance;

            if (recalculateGrid && GridNodes != null) InitializeGrid();
        }

        #region Gizmos




        /*
         maravillosa cajita en vez de hacer la clasica de hacer un box por cada punto, que equiaelent por ej si fuera 20x20x20 a 8000 box!!!
         te explota el editor, y enicmano se ve una raja
         hago box del tamaño maximo, entonces la grid solo se ve en las caras del cubo, es facil de ver, y encima solo usaria +- 30 box :3 
         
         PD: Soy demaciado bueno para hacer gizmos (?
         
         */



        [Header("Box Gizmos")] [Header("")] [Tooltip("High Performance Impact :c")]
        public bool showBoxGizmo;

        [SerializeField] private Color32 boxColor = new Color32(255, 0, 255, 255);
        [SerializeField] private Color32 gridColor = new Color32(255, 255, 255, 10);

        private void OnDrawGizmos()
        {
            if (!showBoxGizmo) return;
            Gizmos.color = boxColor;
            var boxSize = (new Vector3(gridSize.x,0, gridSize.y)) * nodeDistance;
            var position = transform.position;
            Gizmos.DrawWireCube(position, boxSize + Vector3.up);

            //---- ---- ---- ---- ----//

            Gizmos.color = gridColor;

            _mHalfTestNode = nodeDistance / 2;
            var tf = position;

            tf.x -= gridSize.x * _mHalfTestNode - _mHalfTestNode;


            var cubeSize = new Vector3(nodeDistance,1, boxSize.y);

            for (var x = 0; x < gridSize.x; x += 2)
            {
                Gizmos.DrawWireCube(tf, cubeSize);
                tf.x += nodeDistance * 2;
            }

            //----
            tf = transform.position;
            tf.z -= gridSize.y * _mHalfTestNode - _mHalfTestNode;

            cubeSize = new Vector3(boxSize.x,1, nodeDistance);

            for (var y = 0; y < gridSize.y; y += 2)
            {
                Gizmos.DrawWireCube(tf, cubeSize);
                tf.z += nodeDistance * 2;
            }

            //----
            /*
            tf = transform.position;
            tf.z -= gridSize.z * _mHalfTestNode - _mHalfTestNode;

            cubeSize = new Vector3(boxSize.x, boxSize.y, nodeDistance);

            for (var z = 0; z < gridSize.z; z += 2)
            {
                Gizmos.DrawWireCube(tf, cubeSize);
                tf.z += nodeDistance * 2;
            }*/
        }

        [Header("View TestNodes")] [Tooltip("High Performance Impact :c")]
        public bool seeValids;

        public Color32 validColor = new Color32(255, 255, 255, 50);

        public bool seeInvalids;
        public Color32 invalidColor = new Color32(255, 0, 0, 150);

        private void OnDrawGizmosSelected()
        {
            if (!seeInvalids && !seeValids) return;
            for (var x = 0; x < gridSize.x; x++)
            for (var y = 0; y < gridSize.y; y++)
            {
                if (GridNodes == null) return;
                var nya = GridNodes[x][y];

                if (nya.IsWalkable)
                {
                    if (!seeValids) continue;
                    Gizmos.color = validColor;
                    Gizmos.DrawSphere(nya.WorldPosition, nodeSize / 2);
                }
                else
                {
                    if (!seeInvalids) continue;
                    Gizmos.color = invalidColor;
                    Gizmos.DrawSphere(nya.WorldPosition, nodeSize / 2);
                }
            }
        }

        #endregion

        public int2 GetClosestPointWorldSpace(Vector3 position)
        {
            
            var pos = position - StartPoint;
            var percentageX = Mathf.Clamp01(pos.x / BoxSize.x);
            var percentageY = Mathf.Clamp01(pos.z / BoxSize.z);
            var x = Mathf.Clamp(Mathf.RoundToInt(percentageX * GridSize.x), 0, GridSize.x - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(percentageY * GridSize.y), 0, GridSize.y - 1);
            var result = GridNodes[x][y];
            while (!result.IsWalkable)
            {
                var freePoints = new List<Node>();
                for (var p = -1; p <= 1; p++)
                for (var q = -1; q <= 1; q++)
                {
                    if (x == p && y == q) continue;

                    var i = x + p;
                    var j = y + q;
                    if (i > -1 && i < GridSize.x &&
                        j > -1 && j < GridSize.y)
                    {
                        if (GridNodes[x + p][y + q].IsWalkable) 
                            freePoints.Add(GridNodes[x + p][y + q]);
                    }
                }

                var distance = Mathf.Infinity;
                foreach (var t in freePoints)
                {
                    var dist = (t.WorldPosition - position).sqrMagnitude;
                    if (!(dist < distance)) continue;
                        result = t;
                        distance = dist;
                }
            }
            return result.Coords;
        }
    }
}