using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed = 1;
    public float maxSpeed = 1;
    private Rigidbody _rb;
    
    public Slider speedControl;
    public TextMeshProUGUI textMesh;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    // Update is called once per frame
    private void Update()
    {
        speed = speedControl.value * maxSpeed;
        textMesh.text = "Speed: " + speed.ToString("0.00");
        Vector3 input = new Vector2();
        if (Input.GetKey(KeyCode.D)) input.x += 1;
        if (Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.W)) input.z += 1;
        if (Input.GetKey(KeyCode.S)) input.z -= 1;
        var currentY = _rb.velocity.y;
        input *= speed;
        _rb.velocity = new Vector3(input.x,currentY,input.z);
    }

}
