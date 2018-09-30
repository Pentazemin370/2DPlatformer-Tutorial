using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 6;
	float dashSpeed = 30;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;
	public bool dashing;
	Controller2D controller;

	Vector2 directionalInput;
	bool wallSliding;
	int jumpCounter;
	int wallDirX;
	Animator animator;

	void Start() {
		controller = GetComponent<Controller2D> ();
		animator = GetComponent<Animator> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	void Update() {
		CalculateVelocity ();
		HandleWallSliding ();
		UpdateAnimator ();

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
			if (!controller.collisions.above) {
				jumpCounter = 2;
			}
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}
		if (dashing && Mathf.Abs(dashSpeed)-Mathf.Abs(velocity.x)<=0.1f) {
			dashing = false;
		}

	}

	public void SetDirectionalInput (Vector2 input) {
		directionalInput = input;
	}

	public void OnJumpInputDown() {
		if (!controller.collisions.below && (controller.collisions.left || controller.collisions.right)) {
			jumpCounter = 1;

				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
		}
		else if(jumpCounter>0) {
			
			if (controller.collisions.slidingDownMaxSlope) {
				if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else {
				jumpCounter--;
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}
		

	public void Dash(){
		dashSpeed = 15*directionalInput.x;
		dashing = true;
	}

	void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left  || controller.collisions.right) 
			&& !controller.collisions.below && velocity.y < 0
			&& directionalInput.x==wallDirX) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}

	}

	void CalculateVelocity() {
		float targetVelocityX = directionalInput.x * moveSpeed;
		if (!dashing) {
			velocity.y += gravity * Time.deltaTime;
			velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		} else {
			velocity.y = 0;
			velocity.x = Mathf.SmoothDamp (velocity.x, dashSpeed, ref velocityXSmoothing, 0.05f);
		}

	}

	void UpdateAnimator(){
		transform.localScale = new Vector3(-controller.collisions.faceDir,1,1);
		int state = 0;
		if (controller.collisions.below) {
			if (directionalInput.x != 0)
				state = 1;
		} else {
			state = (velocity.y > 0) ? 2 : 3;
		}
		animator.SetInteger ("playerState", state);
	}

}
