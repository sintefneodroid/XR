using System;
using UnityEngine;

namespace Robolab.Scripts {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  public class PickUpBehaviour : MonoBehaviour {
    Rigidbody _body;
    float _original_body_angular_drag;

    GameObject _picked_up_object;

    RaycastHit? _raycast;
    public Camera _Camera;
    public float _FollowStrength = 10f;

    public float _HoldingDistance = 3;
    //readonly VectorPid angularVelocityController = new VectorPid (30.7766f, 0, 0.2553191f);
    //readonly VectorPid headingController = new VectorPid (2.244681f, 0, 0.1382979f);

    public Vector3 _HoldingPosition;
    public Rigidbody _LeftArm;
    public float _MaxPickUpDistance = 10;

    public GameObject _Player;
    public Rigidbody _RightArm;

    // Maximum distance from the camera at which the object can be picked up
    public float _ThrowingStrength = 10;

    void Start() {
      this._Player = this.gameObject;
      if (!this._Camera) {
        this._Camera = this.GetComponent<Camera>();
      }
    }

    void Update() {
      this.Raycast();
      if (this._raycast.HasValue) {
        if (this._raycast.Value.distance < this._HoldingDistance) {
          this._HoldingPosition = this._raycast.Value.point;
        } else {
          this._HoldingPosition = this._Camera.transform.position
                                 + this._Camera.transform.forward * this._HoldingDistance;
        }
      } else {
        this._HoldingPosition =
            this._Camera.transform.position + this._Camera.transform.forward * this._HoldingDistance;
      }

      var scroll_delta = Input.GetAxis("Mouse ScrollWheel");
      if (scroll_delta * scroll_delta > 0f) {
        this._HoldingDistance += scroll_delta;
      }

      if (Input.GetKeyDown(KeyCode.E)) {
        if (!this._picked_up_object) {
          if (this._raycast.HasValue) {
            this.TryPickUpObject();
          }
        } else {
          this.ReleaseObject();
        }
      }

      if (this._picked_up_object && Input.GetMouseButtonDown(0)) {
        this.ThrowObject();
      }

      if (this._picked_up_object && Input.GetMouseButtonDown(1)) {
        this.FreezeObject();
      }
    }

    void FixedUpdate() {
      if (this._picked_up_object) {
        this.UpdateHoldableObject();
        this.UpdateArm(this._LeftArm, this._picked_up_object);
        this.UpdateArm(this._RightArm, this._picked_up_object);
      }
    }

    void Raycast() {
      this._raycast = null;
      //const int layerMask = 1 << 8;
      //Debug.DrawLine (_camera.transform.position, _camera.transform.forward * _max_pick_up_distance);
      var raycast_hits = Physics.RaycastAll(
          this._Camera.transform.position,
          this._Camera.transform.forward,
          this._MaxPickUpDistance); //, ~layerMask);
      foreach (var hit in raycast_hits) {
        if (this._picked_up_object) {
          if (hit.collider == this._picked_up_object.GetComponent<Collider>()) {
            continue;
          }
        }

        if (hit.collider == this._Player.GetComponent<Collider>() || !hit.collider.GetComponent<Rigidbody>()) {
          continue;
        }

        this._raycast = hit;
      }
    }

    void UpdateArm(Rigidbody arm, GameObject target) {
      //var angularVelocityError = arm.angularVelocity * -1;
      //Debug.DrawRay (transform.position, LeftArm.angularVelocity * 10, Color.black);

      //var angularVelocityCorrection = angularVelocityController.Update (angularVelocityError, Time.deltaTime);
      //Debug.DrawRay (transform.position, angularVelocityCorrection, Color.green);

      //arm.AddTorque (angularVelocityCorrection);

      //var desiredHeading = target.transform.position - transform.position;
      //Debug.DrawRay (transform.position, desiredHeading, Color.magenta);

      //var currentHeading = -transform.up;
      //Debug.DrawRay (transform.position, currentHeading * 15, Color.blue);

      //var headingError = Vector3.Cross (currentHeading, desiredHeading);
      //var headingCorrection = headingController.Update (headingError, Time.deltaTime);

      //arm.AddTorque (headingCorrection);
      arm.transform.LookAt(target.transform);
    }

    void UpdateHoldableObject() {
      //_body.velocity = Vector3.Lerp (_picked_up_object.transform.position, _pivot_position, _follow_strength); 
      //_body.velocity = (_holding_position - _picked_up_object.transform.position) * _follow_strength;// + ((1 - _follow_strength) * _body.velocity);
      //_body.velocity = Vector3.SmoothDamp (_picked_up_object.transform.position, _pivot_position, _pivot_position- _picked_up_object.transform.position, _follow_strength);
      //_body.velocity = Vector3.Lerp (_body.velocity, _pivot_position - _picked_up_object.transform.position, .9f);
      var distance = Vector3.Distance(this._HoldingPosition, this._picked_up_object.transform.position);
      var direction = (this._HoldingPosition - this._picked_up_object.transform.position).normalized;
      this._body.MovePosition(
          this._picked_up_object.transform.position
          + direction * distance * this._FollowStrength * Time.deltaTime);
    }

    void TryPickUpObject() {
      if (this._raycast != null) {
        this._body = this._raycast.Value.rigidbody;
      }

      this._body.transform.position = this._HoldingPosition;
      this._body.useGravity = false;

      this._original_body_angular_drag = this._body.angularDrag;
      this._body.angularDrag = 1;

      this._picked_up_object = this._body.gameObject;
      Physics.IgnoreCollision(
          this._picked_up_object.GetComponent<Collider>(),
          this.GetComponent<Collider>(),
          true);
    }

    void ReleaseObject(Action on_release = null) {
      Physics.IgnoreCollision(
          this._picked_up_object.GetComponent<Collider>(),
          this.GetComponent<Collider>(),
          false);
      this._body.isKinematic = false;
      this._body.useGravity = true;
      this._body.angularDrag = this._original_body_angular_drag;
      if (on_release != null) {
        on_release();
      }

      this.ClearPickedUp();
    }

    void ClearPickedUp() {
      this._body = null;
      this._picked_up_object = null;
    }

    void FreezeObject() {
      this._body.isKinematic = true;
      this.ClearPickedUp();
    }

    void ThrowObject() {
      this.ReleaseObject(
          () => this._body.AddForce(
              this._Camera.transform.forward * this._ThrowingStrength,
              ForceMode.Impulse));
    }
  }

  public class VectorPid {
    Vector3 _integral;
    Vector3 _last_error;

    public float P, I, D;

    public VectorPid(float p, float i, float d) {
      this.P = p;
      this.I = i;
      this.D = d;
    }

    public Vector3 Update(Vector3 current_error, float time_frame) {
      this._integral += current_error * time_frame;
      var deriv = (current_error - this._last_error) / time_frame;
      this._last_error = current_error;
      return current_error * this.P + this._integral * this.I + deriv * this.D;
    }
  }
}
