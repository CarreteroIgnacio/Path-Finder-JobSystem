using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Exuli
{
    public class Agent : MonoBehaviour
    {
        #region Attribs

        [Header("Stats")]
        public float speed = 1;
        public float turnSpeed = .5f;
        
        [Header("Field of View")]
        public float viewAngle = 45f;
        public float viewDist = 2;


        [Header("Waypoints")]
        public WaypointList waypointList;
        
        [Header("Stuffs xD")][Tooltip("Must select Walls and Player Layer")]
        public LayerMask levelMask;
        public float keepDistance = 1;

        

        #endregion

        #region Local Vars

            private delegate void CurrentState();
            private CurrentState _currentState;
            
            private List<Vector3> _pathList;

            private Transform _playerReference;
            private Transform _currentTarget;
            private Transform _transF;

        #endregion

            
        private void Awake()
        {
            
            #if UNITY_EDITOR
                var nya = GetComponent<MeshRenderer>();
                if(nya != null) _gizmoColor = nya.material.color;
            #endif
            _transF = GetComponent<Transform>();
        }

        private void Start()
        {
            AgentManager.Instance.AddAgent(this);
            _playerReference = AgentManager.Instance.GetPlayer();

            PatrolReturn();
        }
        private void Update() => _currentState();
        private void OnDisable() => AgentManager.Instance.RemoveAgent(this);

        private void PathJob() => _pathList = Path2D.GetPath(_transF.position, _currentTarget.position);


        #region Checks

        private bool CheckFov(ref Vector3 dir)
        {
            if (Vector3.Distance(_playerReference.position, _transF.position) > viewDist)
            {
                AgentManager.Instance.CanSeePlayer(this, false);
                return false;
            }

            dir = (_playerReference.position - _transF.position).normalized;
            var nya = Vector3.Angle(_transF.forward, dir) <= viewAngle / 2; 
            AgentManager.Instance.CanSeePlayer(this, nya);

            return nya;
        }

        private void CheckWaypoint()
        {
            if (!_currentTarget.CompareTag("Wpoint")) return;
            
            if (Vector3.Distance(_transF.position, _currentTarget.position) >= .1f) return;
            _currentTarget = waypointList.GetNextPoint().transform;
            PathJob();
        }
        
        private bool CheckPatrolReturn()
        {
            switch (_pathList.Count)
            {
                case 0:
                case 1 when Vector3.Distance(_transF.position, _pathList[0]) < 0.1f:
                    return true;
                default:
                    return false;
            }
        }
        
        #endregion
        
        private void FollowNode()
        {
            //print("FollowNode "+ _pathList.Count );
            if (_pathList.Count == 0)
            {
                PathJob();
                if (_pathList.Count == 0) return;
            }
                
            Debug.DrawLine(_pathList[0], _pathList[0] + Vector3.up *2);
            var position = _transF.position;
            var currentNode = CalculateCurrentNode();


            var direction = (currentNode - position).normalized;
            var rotGoal = Quaternion.LookRotation(direction);
            _transF.rotation = Quaternion.Slerp(_transF.rotation, rotGoal, turnSpeed);
            position += _transF.forward * (speed * Time.deltaTime);
            _transF.position = position;
        }

        private Vector3 CalculateCurrentNode()
        {
            var position = transform.position;
            Vector3 closeTarget;

            switch (_pathList.Count)
            {
                case 0:
                    closeTarget = _currentTarget.position;
                    break;
                case 1:
                    closeTarget = _pathList[0];
                    break;
                default:
                {
                    if (Vector3.Distance(position, _pathList[0]) <  0.15f)
                    {
                        closeTarget = _pathList[1];
                        _pathList.Remove(_pathList[0]);
                    }
                    else
                        closeTarget = _pathList[0];

                    break;
                }
            }

            if (Vector3.Distance(position, _currentTarget.position) < Vector3.Distance(position, closeTarget))
                closeTarget = _currentTarget.position;
            return closeTarget;
        }

        
        private void SeekPlayerFov()
        {
            Vector3 dir = default;
            if (!CheckFov(ref dir)) return;
            
            Physics.Raycast(transform.position, dir, out var hit, viewDist, levelMask);

            if (hit.collider == null) return;
                if (hit.collider.CompareTag("Player"))
                    AgentManager.Instance.AlertPlayer();
        }

        private void SeekPlayerPosition()
        {
            Vector3 dir = default;
            var position = _transF.position;
            if (CheckFov(ref dir))
            {

                Physics.Raycast(position, dir, out var hit, viewDist, levelMask);
                if (hit.collider ==  null) return;

                if (hit.collider.CompareTag("Wall"))
                {
                    if(CheckPatrolReturn())
                    {
                        PatrolReturn();
                        return;
                    }
                    FollowNode();
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    FollowPlayer();
                    #if UNITY_EDITOR
                        //solo tiene sentido limpiarla en el editor para q se vea bien el gizmos
                        _pathList.Clear();
                    #endif
                }
            }
            else
            {
                FollowNode();
                if(CheckPatrolReturn()) PatrolReturn();
            }
        }


        
        
        private void MoveToTarget()
        {
            Vector3 dir = default;
            if (CheckFov(ref dir))
            {
                Physics.Raycast(_transF.position, dir, out var hit, viewDist, levelMask);

                if (hit.collider ==  null) return;

                if (hit.collider.CompareTag("Player"))
                {
                    var position = _transF.position;
                    
                    //para evitar glichs
                    if (Vector3.Distance(position, _currentTarget.position) < keepDistance)
                        dir *= -1;
                    
                    Debug.DrawLine(position, _currentTarget.position, Color.cyan);
                    
                    _transF.LookAt(_currentTarget);


                    dir = new Vector3(dir.x, 0, dir.z);
                    _transF.position += dir * (speed * Time.deltaTime);
                }
                else
                    AlertPlayerFinded();
            }
            else AlertPlayerFinded();
        }


        //--------------- States -------------------//
        private void PatrolReturn()
        {
            if (AgentManager.Instance.AnyoneSeePlayer())
            {
                PathJob();
                return;
            }
            _currentTarget = waypointList.GetClosestWaypoint(transform.position).transform;
            PathJob();

            CurrentState nya = null;
            nya += FollowNode;
            nya += CheckWaypoint;
            nya += SeekPlayerFov;

            _currentState = nya;
        }

        public void AlertPlayerFinded()
        {
            _currentTarget = _playerReference;
            PathJob();
            
            CurrentState nya = null;
            nya += SeekPlayerPosition;
            
            _currentState = nya;
        }

        private void FollowPlayer()
        {
            _currentTarget = _playerReference;
            CurrentState nya = null;
            nya += MoveToTarget;
            
            _currentState = nya;
        }
    //--------------------------------------------------------------//
        private void OnValidate()
        {
            speed = Mathf.Abs(speed);
            turnSpeed = Mathf.Abs(turnSpeed);
            viewAngle = Mathf.Abs(viewAngle);
            viewDist = Mathf.Abs(viewDist);
        }

#if UNITY_EDITOR
        
        #region Gizmos

        private Color _gizmoColor = Color.white;

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;

            _transF = transform;
            GizmosExtensions.DrawWireArc(_transF.position, _transF.forward,viewAngle, viewDist, 10f );
            if (_pathList == null) return;
            if (_pathList.Count == 0) return;
            //if (!_currentTarget.CompareTag("Wpoint")) return;
            
            var oldPos = transform.position;
            var myan = _pathList.Count;
            
            for (var i = 0; i < myan; i++)
            {
                var nya = _pathList[i];
                Gizmos.DrawLine(oldPos, nya);
                oldPos = nya;
            }
            Gizmos.DrawWireSphere(_currentTarget.position, .25f);
        }
        
        #endregion
#endif
        
    }
}
