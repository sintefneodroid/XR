using UnityEngine;
using UnityEngine.Serialization;

namespace SceneAssets.Common.Interaction{
  [ExecuteInEditMode]
  public class ThrowableObject : MonoBehaviour {
    [SerializeField] string awakeTag = "Grapper";

    [SerializeField] Rigidbody rb;

    [SerializeField] private bool sticky = false;
    private void Awake(){
      this.rb = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other){
      if (sticky){
        if (other.gameObject.CompareTag(awakeTag))
        {

          rb.useGravity = true;
          rb.WakeUp();
        }
        else
        {
          rb.Sleep();
          rb.useGravity = false;
        }
      }
    }

    private void OnTriggerEnter(Collider other){
      if (sticky){
        if (other.gameObject.CompareTag(awakeTag)){
          rb.useGravity = true;
          rb.WakeUp();
        }
      }
    }

    private void OnCollisionExit(Collision other){
      if (sticky){
        rb.useGravity = true;
        rb.WakeUp();
      }
    }
  }
}



