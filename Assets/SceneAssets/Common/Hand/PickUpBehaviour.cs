using System;
using UnityEngine;

namespace SceneAssets.Common.Hand
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class PickUpBehaviour : MonoBehaviour
    {
        Rigidbody _body;
        float _original_body_angular_drag;

        GameObject _picked_up_object;

        RaycastHit? _raycast;
        [SerializeField] Camera camera;
        [SerializeField] float followStrength = 10f;

        [SerializeField] float holdingDistance = 3;
        //readonly VectorPid angularVelocityController = new VectorPid (30.7766f, 0, 0.2553191f);
        //readonly VectorPid headingController = new VectorPid (2.244681f, 0, 0.1382979f);

        [SerializeField] Vector3 holdingPosition;
        [SerializeField] Rigidbody leftArm;
        [SerializeField] float maxPickUpDistance = 10;

        [SerializeField] GameObject player;
        [SerializeField] Rigidbody rightArm;

        // Maximum distance from the camera at which the object can be picked up
        [SerializeField] float throwingStrength = 10;

        void Start()
        {
            this.player = this.gameObject;
            if (!this.camera)
            {
                this.camera = this.GetComponent<Camera>();
            }
        }

        void Update()
        {
            this.Raycast();
            var transform1 = this.camera.transform;
            if (this._raycast.HasValue)
            {
                if (this._raycast.Value.distance < this.holdingDistance)
                {
                    this.holdingPosition = this._raycast.Value.point;
                }
                else
                {
                    this.holdingPosition = transform1.position
                                           + transform1.forward * this.holdingDistance;
                }
            }
            else
            {
                this.holdingPosition =
                    transform1.transform.position + transform1.forward * this.holdingDistance;
            }

            var scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta * scrollDelta > 0f)
            {
                this.holdingDistance += scrollDelta;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!this._picked_up_object)
                {
                    if (this._raycast.HasValue)
                    {
                        this.TryPickUpObject();
                    }
                }
                else
                {
                    this.ReleaseObject();
                }
            }

            if (this._picked_up_object && Input.GetMouseButtonDown(0))
            {
                this.ThrowObject();
            }

            if (this._picked_up_object && Input.GetMouseButtonDown(1))
            {
                this.FreezeObject();
            }
        }

        void FixedUpdate()
        {
            if (this._picked_up_object){
                this.UpdateHoldableObject();
                this.UpdateArm(this.leftArm, this._picked_up_object);
                this.UpdateArm(this.rightArm, this._picked_up_object);
            }
        }

        void Raycast()
        {
            this._raycast = null;
            //const int layerMask = 1 << 8;
            //Debug.DrawLine (_camera.transform.position, _camera.transform.forward * _max_pick_up_distance);
            var raycastHits = Physics.RaycastAll(this.camera.transform.position,
                this.camera.transform.forward,
                this.maxPickUpDistance); //, ~layerMask);
            foreach (var hit in raycastHits)
            {
                if (this._picked_up_object)
                {
                    if (hit.collider == this._picked_up_object.GetComponent<Collider>())
                    {
                        continue;
                    }
                }

                if (hit.collider == this.player.GetComponent<Collider>() || !hit.collider.GetComponent<Rigidbody>())
                {
                    continue;
                }

                this._raycast = hit;
            }
        }

        void UpdateArm(Rigidbody arm, GameObject target)
        {
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

        void UpdateHoldableObject(){
            //_body.velocity = Vector3.Lerp (_picked_up_object.transform.position, _pivot_position, _follow_strength); 
            //_body.velocity = (_holding_position - _picked_up_object.transform.position) * _follow_strength;// + ((1 - _follow_strength) * _body.velocity);
            //_body.velocity = Vector3.SmoothDamp (_picked_up_object.transform.position, _pivot_position, _pivot_position- _picked_up_object.transform.position, _follow_strength);
            //_body.velocity = Vector3.Lerp (_body.velocity, _pivot_position - _picked_up_object.transform.position, .9f);

            var position = this._picked_up_object.transform.position;
            
            var distance = Vector3.Distance(this.holdingPosition, position);
            var direction = (this.holdingPosition - position).normalized;
            this._body.MovePosition(position + direction * distance * this.followStrength * Time.deltaTime);
        }

        void TryPickUpObject(){
            if (this._raycast != null){
                this._body = this._raycast.Value.rigidbody;
            }

            this._body.transform.position = this.holdingPosition;
            this._body.useGravity = false;

            this._original_body_angular_drag = this._body.angularDrag;
            this._body.angularDrag = 1;

            this._picked_up_object = this._body.gameObject;
            Physics.IgnoreCollision(this._picked_up_object.GetComponent<Collider>(),
                this.GetComponent<Collider>(),
                true);
        }

        void ReleaseObject(Action onRelease = null){
            Physics.IgnoreCollision(this._picked_up_object.GetComponent<Collider>(),
                this.GetComponent<Collider>(),
                false);
            this._body.isKinematic = false;
            this._body.useGravity = true;
            this._body.angularDrag = this._original_body_angular_drag;
            onRelease?.Invoke();

            this.ClearPickedUp();
        }

        void ClearPickedUp(){
            this._body = null;
            this._picked_up_object = null;
        }

        void FreezeObject(){
            this._body.isKinematic = true;
            this.ClearPickedUp();
        }

        void ThrowObject(){
            this.ReleaseObject(
                () => this._body.AddForce(
                    this.camera.transform.forward * this.throwingStrength,
                    ForceMode.Impulse));
        }
    }

    public class VectorPid
    {
        Vector3 _integral;
        Vector3 _last_error;

        public float P, I, D;

        public VectorPid(float p, float i, float d)
        {
            this.P = p;
            this.I = i;
            this.D = d;
        }

        public Vector3 Update(Vector3 currentError, float timeFrame)
        {
            this._integral += currentError * timeFrame;
            var derivative = (currentError - this._last_error) / timeFrame;
            this._last_error = currentError;
            return currentError * this.P + this._integral * this.I + derivative * this.D;
        }
    }
}