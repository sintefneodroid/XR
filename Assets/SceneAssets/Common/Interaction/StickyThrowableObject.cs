using UnityEngine;
using UnityEngine.Serialization;

namespace SceneAssets.Common.Interaction
{
  public class StickyThrowableObject : MonoBehaviour {
    [SerializeField] string awakeTag = "Grapper";

    [SerializeField] Rigidbody rb;

    private void Awake()
    {
      this.rb = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other){
      if (other.gameObject.CompareTag(awakeTag)){
        rb.useGravity = true;
        rb.WakeUp();
      }else{
        rb.Sleep();
        rb.useGravity = false;
      }
    }

    private void OnTriggerEnter(Collider other){
      if (other.gameObject.CompareTag(awakeTag)){
        rb.useGravity = true;
        rb.WakeUp();
      }
    }

    private void OnCollisionExit(Collision other){
      rb.useGravity = true;
      rb.WakeUp();    
    }
  }
}



