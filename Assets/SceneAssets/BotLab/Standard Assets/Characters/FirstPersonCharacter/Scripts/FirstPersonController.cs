using SceneAssets.BotLab.Standard_Assets.CrossPlatformInput.Scripts;
using SceneAssets.BotLab.Standard_Assets.Utility;
using UnityEngine;

namespace SceneAssets.BotLab.Standard_Assets.Characters.FirstPersonCharacter.Scripts {
  [RequireComponent(typeof(CharacterController))]
  [RequireComponent(typeof(AudioSource))]
  public class FirstPersonController : MonoBehaviour {
    [SerializeField] readonly FOVKick m_FovKick = new FOVKick();

    [SerializeField] readonly CurveControlledBob m_HeadBob = new CurveControlledBob();

    [SerializeField] readonly LerpControlledBob m_JumpBob = new LerpControlledBob();

    AudioSource m_AudioSource;
    // the sound played when character touches back on ground.

    Camera m_Camera;
    CharacterController m_CharacterController;
    CollisionFlags m_CollisionFlags;

    [SerializeField] AudioClip[] m_FootstepSounds;

    [SerializeField] float m_GravityMultiplier;

    Vector2 m_Input;

    [SerializeField] bool m_IsWalking;

    bool m_Jump;

    bool m_Jumping;

    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] AudioClip m_JumpSound;

    [SerializeField] float m_JumpSpeed;

    // the sound played when character leaves the ground.
    [SerializeField] AudioClip m_LandSound;

    [SerializeField] MouseLook m_MouseLook;

    Vector3 m_MoveDir = Vector3.zero;
    float m_NextStep;
    Vector3 m_OriginalCameraPosition;
    bool m_PreviouslyGrounded;

    [SerializeField] float m_RunSpeed;

    [SerializeField] [Range(0f, 1f)] float m_RunstepLenghten;

    float m_StepCycle;

    [SerializeField] float m_StepInterval;

    [SerializeField] float m_StickToGroundForce;

    [SerializeField] bool m_UseFovKick;

    [SerializeField] bool m_UseHeadBob;

    [SerializeField] float m_WalkSpeed;

    float m_YRotation;

    // Use this for initialization
    void Start() {
      this.m_CharacterController = this.GetComponent<CharacterController>();
      this.m_Camera = Camera.main;
      this.m_OriginalCameraPosition = this.m_Camera.transform.localPosition;
      this.m_FovKick.Setup(this.m_Camera);
      this.m_HeadBob.Setup(this.m_Camera, this.m_StepInterval);
      this.m_StepCycle = 0f;
      this.m_NextStep = this.m_StepCycle / 2f;
      this.m_Jumping = false;
      this.m_AudioSource = this.GetComponent<AudioSource>();
      this.m_MouseLook.Init(this.transform, this.m_Camera.transform);
    }

    // Update is called once per frame
    void Update() {
      this.RotateView();
      // the jump state needs to read here to make sure it is not missed
      if (!this.m_Jump) {
        this.m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
      }

      if (!this.m_PreviouslyGrounded && this.m_CharacterController.isGrounded) {
        this.StartCoroutine(this.m_JumpBob.DoBobCycle());
        this.PlayLandingSound();
        this.m_MoveDir.y = 0f;
        this.m_Jumping = false;
      }

      if (!this.m_CharacterController.isGrounded && !this.m_Jumping && this.m_PreviouslyGrounded) {
        this.m_MoveDir.y = 0f;
      }

      this.m_PreviouslyGrounded = this.m_CharacterController.isGrounded;
    }

    void PlayLandingSound() {
      this.m_AudioSource.clip = this.m_LandSound;
      this.m_AudioSource.Play();
      this.m_NextStep = this.m_StepCycle + .5f;
    }

    void FixedUpdate() {
      float speed;
      this.GetInput(out speed);
      // always move along the camera forward as it is the direction that it being aimed at
      var desiredMove = this.transform.forward * this.m_Input.y + this.transform.right * this.m_Input.x;

      // get a normal for the surface that is being touched to move along it
      RaycastHit hitInfo;
      Physics.SphereCast(
          this.transform.position,
          this.m_CharacterController.radius,
          Vector3.down,
          out hitInfo,
          this.m_CharacterController.height / 2f,
          Physics.AllLayers,
          QueryTriggerInteraction.Ignore);
      desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

      this.m_MoveDir.x = desiredMove.x * speed;
      this.m_MoveDir.z = desiredMove.z * speed;

      if (this.m_CharacterController.isGrounded) {
        this.m_MoveDir.y = -this.m_StickToGroundForce;

        if (this.m_Jump) {
          this.m_MoveDir.y = this.m_JumpSpeed;
          this.PlayJumpSound();
          this.m_Jump = false;
          this.m_Jumping = true;
        }
      } else {
        this.m_MoveDir += Physics.gravity * this.m_GravityMultiplier * Time.fixedDeltaTime;
      }

      this.m_CollisionFlags = this.m_CharacterController.Move(this.m_MoveDir * Time.fixedDeltaTime);

      this.ProgressStepCycle(speed);
      this.UpdateCameraPosition(speed);

      this.m_MouseLook.UpdateCursorLock();
    }

    void PlayJumpSound() {
      this.m_AudioSource.clip = this.m_JumpSound;
      this.m_AudioSource.Play();
    }

    void ProgressStepCycle(float speed) {
      if (this.m_CharacterController.velocity.sqrMagnitude > 0
          && (this.m_Input.x != 0 || this.m_Input.y != 0)) {
        this.m_StepCycle +=
            (this.m_CharacterController.velocity.magnitude
             + speed * (this.m_IsWalking ? 1f : this.m_RunstepLenghten))
            * Time.fixedDeltaTime;
      }

      if (!(this.m_StepCycle > this.m_NextStep)) {
        return;
      }

      this.m_NextStep = this.m_StepCycle + this.m_StepInterval;

      this.PlayFootStepAudio();
    }

    void PlayFootStepAudio() {
      if (!this.m_CharacterController.isGrounded) {
        return;
      }

      // pick & play a random footstep sound from the array,
      // excluding sound at index 0
      var n = Random.Range(1, this.m_FootstepSounds.Length);
      this.m_AudioSource.clip = this.m_FootstepSounds[n];
      this.m_AudioSource.PlayOneShot(this.m_AudioSource.clip);
      // move picked sound to index 0 so it's not picked next time
      this.m_FootstepSounds[n] = this.m_FootstepSounds[0];
      this.m_FootstepSounds[0] = this.m_AudioSource.clip;
    }

    void UpdateCameraPosition(float speed) {
      Vector3 newCameraPosition;
      if (!this.m_UseHeadBob) {
        return;
      }

      if (this.m_CharacterController.velocity.magnitude > 0 && this.m_CharacterController.isGrounded) {
        this.m_Camera.transform.localPosition = this.m_HeadBob.DoHeadBob(
            this.m_CharacterController.velocity.magnitude
            + speed * (this.m_IsWalking ? 1f : this.m_RunstepLenghten));
        newCameraPosition = this.m_Camera.transform.localPosition;
        newCameraPosition.y = this.m_Camera.transform.localPosition.y - this.m_JumpBob.Offset();
      } else {
        newCameraPosition = this.m_Camera.transform.localPosition;
        newCameraPosition.y = this.m_OriginalCameraPosition.y - this.m_JumpBob.Offset();
      }

      this.m_Camera.transform.localPosition = newCameraPosition;
    }

    void GetInput(out float speed) {
      // Read input
      var horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
      var vertical = CrossPlatformInputManager.GetAxis("Vertical");

      var waswalking = this.m_IsWalking;

      #if !MOBILE_INPUT
      // On standalone builds, walk/run speed is modified by a key press.
      // keep track of whether or not the character is walking or running
      this.m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
      #endif
      // set the desired speed to be walking or running
      speed = this.m_IsWalking ? this.m_WalkSpeed : this.m_RunSpeed;
      this.m_Input = new Vector2(horizontal, vertical);

      // normalize input if it exceeds 1 in combined length:
      if (this.m_Input.sqrMagnitude > 1) {
        this.m_Input.Normalize();
      }

      // handle speed change to give an fov kick
      // only if the player is going to a run, is running and the fovkick is to be used
      if (this.m_IsWalking != waswalking
          && this.m_UseFovKick
          && this.m_CharacterController.velocity.sqrMagnitude > 0) {
        this.StopAllCoroutines();
        this.StartCoroutine(!this.m_IsWalking ? this.m_FovKick.FOVKickUp() : this.m_FovKick.FOVKickDown());
      }
    }

    void RotateView() { this.m_MouseLook.LookRotation(this.transform, this.m_Camera.transform); }

    void OnControllerColliderHit(ControllerColliderHit hit) {
      var body = hit.collider.attachedRigidbody;
      //dont move the rigidbody if the character is on top of it
      if (this.m_CollisionFlags == CollisionFlags.Below) {
        return;
      }

      if (body == null || body.isKinematic) {
        return;
      }

      body.AddForceAtPosition(this.m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
  }
}
