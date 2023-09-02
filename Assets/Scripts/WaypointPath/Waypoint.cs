using UnityEngine;

public class Waypoint : MonoBehaviour
{
	// Uso GameObj para poder tener gizmos y modificarlos en runtime 
	public Color colorGizmo;
	public float sphereSize = 1;
	public bool showGizmos = true;
	
	private void OnValidate() => sphereSize = Mathf.Abs(sphereSize);

	private void OnDrawGizmos()
	{
		if(!showGizmos) return;
		Gizmos.color = colorGizmo;
		Gizmos.DrawWireSphere(transform.position, sphereSize);
	}
}
