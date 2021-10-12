using UnityEngine;
public class ExampleTopDownPlayer : MonoBehaviour {
	// Example scripts are included to demonstate how tile prefabs can be customized to behave in a game setting.
	// ExampleTopDownPlayer is a simple script to move around in the game world to test tile world behavior.

	Rigidbody2D cRigidBody2D;
	public float speed = 2;

	void Awake () {
		cRigidBody2D = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate () {
		cRigidBody2D.velocity = Movement();
		SpriteAnimation();
	}

	void SpriteAnimation(){
		if (Input.GetAxis("Horizontal") > 0) transform.localScale = new Vector2(1, transform.localScale.y);
		if (Input.GetAxis("Horizontal") < 0) transform.localScale = new Vector2(-1, transform.localScale.y);
	}

	Vector2 Movement() {
		Vector2 vel = new Vector2();
		vel.x = Input.GetAxis("Horizontal")* speed;
		vel.y = Input.GetAxis("Vertical")* speed;
		return vel;
	}


#if UNITY_EDITOR
	void OnDrawGizmos() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere((Vector2)transform.position, .15f);
	}
#endif
}
