using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveCamera : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float speed;
    Vector2 movement;

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

	private void FixedUpdate()
	{
        rb.MovePosition(rb.position + movement.normalized * speed);
	}
}
