using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {
	public KeyCode jumpKey;
	Player player;
	KeyCode lastKey;
	float lastPressed;
	float cooldown;
	private readonly float delay = 0.2f;


	void Start () {
		lastPressed = -1;
		player = GetComponent<Player> ();
	}

	void Update () {
		Vector2 directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		player.SetDirectionalInput (directionalInput);
		cooldown -= Time.deltaTime;
		if (DoubleTap (KeyCode.RightArrow) || DoubleTap(KeyCode.LeftArrow))
			player.Dash ();
		if (Input.GetKeyDown (jumpKey)) {
			player.OnJumpInputDown ();
		}
		if (Input.GetKeyUp (jumpKey)) {
			player.OnJumpInputUp ();
		}
	}

	bool DoubleTap(KeyCode k){
		if (Input.GetKeyDown (k)) {
			if (cooldown <= 0 && k == lastKey && Time.time - lastPressed <= delay) {
				cooldown = 0.4f;
				Debug.Log ("double tap " + Time.time);
				return true;
			} else {
				lastKey = k;
				lastPressed = Time.time;
			}
		}
		return false;
	}

}
