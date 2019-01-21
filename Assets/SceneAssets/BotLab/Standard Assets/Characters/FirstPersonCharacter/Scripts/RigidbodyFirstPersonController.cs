using System;
using SceneAssets.BotLab.Standard_Assets.CrossPlatformInput.Scripts;
using UnityEngine;

namespace SceneAssets.BotLab.Standard_Assets.Characters.FirstPersonCharacter.Scripts {
  [RequireComponent(typeof(Rigidbody))]
  [RequireComponent(typeof(CapsuleCollider))]
  public class RigidbodyFirstPersonController : MonoBehaviour {
    public AdvancedSettings advancedSettings = new AdvancedSettings();

    public Camera cam;
    CapsuleCollider m_Capsule;
    Vector3 m_GroundContactNormal;

    bool m_Jump, m_PreviouslyGrounded;

    Rigidbody m_RigidBody;
    float m_YRotation;
    public MouseLook mouseLook = new MouseLook();
    public MovementSettings movementSettings = new MovementSettings();

    public Vector3 Velocity { get { return this.m_RigidBody.velocity; } }

    public bool Grounded { get; private set; }

    public bool Jumping { get; private set; }

    public bool Running {
      get {
        #if !MOBILE_INPUT
        return this.movementSettings.Running;
        #else
	            return false;
                                                                                        #endif
      }
    }

    void Start() {
      this.m_RigidBody = this.GetComponent<Rigidbody>();
      this.m_Capsule = this.GetComponent<CapsuleCollider>();
      this.mouseLook.Init(this.transform, this.cam.transform);
    }

    void Update() {
      this.RotateView();

      if (CrossPlatformInputManager.GetButtonDown("Jump") && !this.m_Jump) {
        this.m_Jump = true;
      }
    }

    void FixedUpdate() {
      this.GroundCheck();
      var input = this.GetInput();

      if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon)
          && (this.advancedSettings.airControl || this.Grounded)) {
        // always move along the camera forward as it is the direction that it being aimed at
        var desiredMove = this.cam.transform.forward * input.y + this.cam.transform.right * input.x;
        desiredMove = Vector3.ProjectOnPlane(desiredMove, this.m_GroundContactNormal).normalized;

        desiredMove.x = desiredMove.x * this.movementSettings.CurrentTargetSpeed;
        desiredMove.z = desiredMove.z * this.movementSettings.CurrentTargetSpeed;
        desiredMove.y = desiredMove.y * this.movementSettings.CurrentTargetSpeed;
        if (this.m_RigidBody.velocity.sqrMagnitude
            < this.movementSettings.CurrentTargetSpeed * this.movementSettings.CurrentTargetSpeed) {
          this.m_RigidBody.AddForce(desiredMove * this.SlopeMultiplier(), ForceMode.Impulse);
        }
      }

      if (this.Grounded) {
        this.m_RigidBody.drag = 5f;

        if (this.m_Jump) {
          this.m_RigidBody.drag = 0f;
          this.m_RigidBody.velocity = new Vector3(
              this.m_RigidBody.velocity.x,
              0f,
              this.m_RigidBody.velocity.z);
          this.m_RigidBody.AddForce(new Vector3(0f, this.movementSettings.JumpForce, 0f), ForceMode.Impulse);
          this.Jumping = true;
        }

        if (!this.Jumping
            && Mathf.Abs(input.x) < float.Epsilon
            && Mathf.Abs(input.y) < float.Epsilon
            && this.m_RigidBody.velocity.magnitude < 1f) {
          this.m_RigidBody.Sleep();
        }
      } else {
        this.m_RigidBody.drag = 0f;
        if (this.m_PreviouslyGrounded && !this.Jumping) {
          this.StickToGroundHelper();
        }
      }

      this.m_Jump = false;
    }

    float SlopeMultiplier() {
      var angle = Vector3.Angle(this.m_GroundContactNormal, Vector3.up);
      return this.movementSettings.SlopeCurveModifier.Evaluate(angle);
    }

    void StickToGroundHelper() {
      RaycastHit hitInfo;
      if (Physics.SphereCast(
          this.transform.position,
          this.m_Capsule.radius * (1.0f - this.advancedSettings.shellOffset),
          Vector3.down,
          out hitInfo,
          this.m_Capsule.height / 2f
          - this.m_Capsule.radius
          + this.advancedSettings.stickToGroundHelperDistance,
          Physics.AllLayers,
          QueryTriggerInteraction.Ignore)) {
        if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f) {
          this.m_RigidBody.velocity = Vector3.ProjectOnPlane(this.m_RigidBody.velocity, hitInfo.normal);
        }
      }
    }

    Vector2 GetInput() {
      var input = new Vector2 {
          x = CrossPlatformInputManager.GetAxis("Horizontal"),
          y = CrossPlatformInputManager.GetAxis("Vertical")
      };
      this.movementSettings.UpdateDesiredTargetSpeed(input);
      return input;
    }

    void RotateView() {
      //avoids the mouse looking if the game is effectively paused
      if (Mathf.Abs(Time.timeScale) < float.Epsilon) {
        return;
      }

      // get the rotation before it's changed
      var oldYRotation = this.transform.eulerAngles.y;

      this.mouseLook.LookRotation(this.transform, this.cam.transform);

      if (this.Grounded || this.advancedSettings.airControl) {
        // Rotate the rigidbody velocity to match the new direction that the character is looking
        var velRotation = Quaternion.AngleAxis(this.transform.eulerAngles.y - oldYRotation, Vector3.up);
        this.m_RigidBody.velocity = velRotation * this.m_RigidBody.velocity;
      }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    void GroundCheck() {
      this.m_PreviouslyGrounded = this.Grounded;
      RaycastHit hitInfo;
      if (Physics.SphereCast(
          this.transform.position,
          this.m_Capsule.radius * (1.0f - this.advancedSettings.shellOffset),
          Vector3.down,
          out hitInfo,
          this.m_Capsule.height / 2f - this.m_Capsule.radius + this.advancedSettings.groundCheckDistance,
          Physics.AllLayers,
          QueryTriggerInteraction.Ignore)) {
        this.Grounded = true;
        this.m_GroundContactNormal = hitInfo.normal;
      } else {
        this.Grounded = false;
        this.m_GroundContactNormal = Vector3.up;
      }

      if (!this.m_PreviouslyGrounded && this.Grounded && this.Jumping) {
        this.Jumping = false;
      }
    }

    [Serializable]
    public class MovementSettings {
      // Speed when walking forward
      public float BackwardSpeed = 4.0f;

      [HideInInspector] public float CurrentTargetSpeed = 8f;

      public float ForwardSpeed = 8.0f;

      public float JumpForce = 30f;

      // Speed when sprinting
      public KeyCode RunKey = KeyCode.LeftShift;

      // Speed when walking sideways
      public float RunMultiplier = 2.0f;

      public AnimationCurve SlopeCurveModifier = new AnimationCurve(
          new Keyframe(-90.0f, 1.0f),
          new Keyframe(0.0f, 1.0f),
          new Keyframe(90.0f, 0.0f));

      // Speed when walking backwards
      public float StrafeSpeed = 4.0f;

      #if !MOBILE_INPUT
      public bool Running { get; private set; }
      #endif

      public void UpdateDesiredTargetSpeed(Vector2 input) {
        if (input == Vector2.zero) {
          return;
        }

        if (input.x > 0 || input.x < 0) {
          this.CurrentTargetSpeed = this.StrafeSpeed;
        }

        if (input.y < 0) {
          this.CurrentTargetSpeed = this.BackwardSpeed;
        }

        if (input.y > 0) {
          this.CurrentTargetSpeed = this.ForwardSpeed;
        }
        #if !MOBILE_INPUT
        if (Input.GetKey(this.RunKey)) {
          this.CurrentTargetSpeed *= this.RunMultiplier;
          this.Running = true;
        } else {
          this.Running = false;
        }
        #endif
      }

      #if !MOBILE_INPUT
      #endif
    }

    [Serializable]
    public class AdvancedSettings {
      // rate at which the controller comes to a stop when there is no input
      public bool airControl;

      public float groundCheckDistance = 0.01f;

      // can the user control the direction that is being moved in the air
      [Tooltip("set it to 0.1 or more if you get stuck in wall")]
      public float shellOffset;

      // stops the character
      public float slowDownRate = 20f;

      // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
      public float stickToGroundHelperDistance = 0.5f;
      //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    }
  }
}
