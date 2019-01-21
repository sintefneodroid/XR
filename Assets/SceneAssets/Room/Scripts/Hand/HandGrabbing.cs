using UnityEngine;
using UnityEngine.XR;

//needs to be UnityEngine.VR in Versions before 2017.2

namespace SceneAssets.Room.Scripts.Hand
{
    public class HandGrabbing : MonoBehaviour{
        public HandGrabbing OtherHandReference;
        //public XRNode NodeType;
        public float GrabDistance = 0.1f;
        public string GrabTag = "Grab";
        public float ThrowMultiplier=1.5f;

        public Transform CurrentGrabObject{
            get { return _currentGrabObject; }
            set { _currentGrabObject = value; }
        }

        private Vector3 _lastFramePosition;
        [SerializeField]private Transform _currentGrabObject;
        [SerializeField]private bool _isGrabbing;
        [SerializeField] private bool m_shouldGrab;

        public bool ShouldGrab
        {
            get => m_shouldGrab;
            set => m_shouldGrab = value;
        }

        void Start(){
            _lastFramePosition = transform.position;
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
            _currentGrabObject = null;
            _isGrabbing = false;
        }

        void Update()
        {
            //update hand position and rotation
            //transform.localPosition = InputTracking.GetLocalPosition(NodeType);
            //transform.localRotation = InputTracking.GetLocalRotation(NodeType);

            if (_currentGrabObject == null){             //check for colliders in proximity

                var colliders = Physics.OverlapSphere(transform.position, GrabDistance);
                if (colliders.Length > 0){                 //if there are colliders, take the first one if we press the grab button and it has the tag for grabbing
                    foreach (var col in colliders){
                        if (ShouldGrab && col.gameObject.CompareTag(GrabTag)){
                    
                            if(_isGrabbing){ //if we are already grabbing, return
                                return;
                            }
                    
                            _isGrabbing = true;

                            //set current object to the object we have picked up (set it as child)
                            col.transform.SetParent(transform);

                            //if there is no rigidbody to the grabbed object attached, add one
                            if(col.GetComponent<Rigidbody>() == null){
                                col.gameObject.AddComponent<Rigidbody>();
                            }

                            //set grab object to kinematic (disable physics)
                            col.GetComponent<Rigidbody>().isKinematic = true;


                            //save a reference to grab object
                            _currentGrabObject = col.transform;


                            //does other hand currently grab object? then release it!
                            if (OtherHandReference.CurrentGrabObject != null){
                                OtherHandReference.CurrentGrabObject = null;
                            }
                            break;
                        }
                
                    }
                }
            }else{         //we have object in hand, update its position with the current hand position (+defined offset from it)
                //if we we release grab button, release current object
                if (!ShouldGrab){

                    //set grab object to non-kinematic (enable physics)
                    Rigidbody _objectRGB = _currentGrabObject.GetComponent<Rigidbody>();
                    _objectRGB.isKinematic = false;
                    _objectRGB.collisionDetectionMode = CollisionDetectionMode.Continuous;

                    //calculate the hand's current velocity
                    Vector3 CurrentVelocity = (transform.position - _lastFramePosition) / Time.deltaTime;

                    //set the grabbed object's velocity to the current velocity of the hand
                    _objectRGB.velocity = CurrentVelocity * ThrowMultiplier;

                    //release the the object (unparent it)
                    _currentGrabObject.SetParent(null);

                    //release reference to object
                    _currentGrabObject = null;
                }

            }

            //release grab ?
            if (!ShouldGrab && _isGrabbing){
                _isGrabbing = false;
            }

            //save the current position for calculation of velocity in next frame
            _lastFramePosition = transform.position;
        }
    }
}
