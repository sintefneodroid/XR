using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Utility {
  public class ObjectResetter : MonoBehaviour {
    Vector3 originalPosition;
    Quaternion originalRotation;
    List<Transform> originalStructure;

    Rigidbody Rigidbody;

    // Use this for initialization
    void Start() {
      this.originalStructure = new List<Transform>(this.GetComponentsInChildren<Transform>());
      this.originalPosition = this.transform.position;
      this.originalRotation = this.transform.rotation;

      this.Rigidbody = this.GetComponent<Rigidbody>();
    }

    public void DelayedReset(float delay) { this.StartCoroutine(this.ResetCoroutine(delay)); }

    public IEnumerator ResetCoroutine(float delay) {
      yield return new WaitForSeconds(delay);

      // remove any gameobjects added (fire, skid trails, etc)
      foreach (var t in this.GetComponentsInChildren<Transform>()) {
        if (!this.originalStructure.Contains(t))
          t.parent = null;
      }

      this.transform.position = this.originalPosition;
      this.transform.rotation = this.originalRotation;
      if (this.Rigidbody) {
        this.Rigidbody.velocity = Vector3.zero;
        this.Rigidbody.angularVelocity = Vector3.zero;
      }

      this.SendMessage("Reset");
    }
  }
}
