using UnityEngine;

namespace SceneAssets.Room.Scripts.Hand
{
    public class HandAnimation : MonoBehaviour{
        private Animator _anim;
        private HandGrabbing _handGrab;
        private static readonly int IsGrabbing = Animator.StringToHash("IsGrabbing");

        void Start(){
            _anim = GetComponentInChildren<Animator>();
            _handGrab = GetComponent<HandGrabbing>();
        }

        void Update(){
            if (_handGrab.ShouldGrab){
                if (!_anim.GetBool(IsGrabbing)){
                    _anim.SetBool(IsGrabbing, true);
                }
            }else{
                if (_anim.GetBool(IsGrabbing)){
                    _anim.SetBool(IsGrabbing, false);
                }
            }
        }
    }
}