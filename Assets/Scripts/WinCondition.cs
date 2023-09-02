using UnityEngine;

public class WinCondition : MonoBehaviour
{
    private SphereCollider _collider;
    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        if(_collider == null)
            _collider = gameObject.AddComponent<SphereCollider>();

        _collider.radius = sphereSize;
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.Win();
    }

    public bool showGizmos;
    public float sphereSize = 1;
    public Color colorGizmo;
    private void OnDrawGizmos()
    {
        if(!showGizmos) return;
        Gizmos.color = colorGizmo;
        Gizmos.DrawWireSphere(transform.position, sphereSize);
    }
}
