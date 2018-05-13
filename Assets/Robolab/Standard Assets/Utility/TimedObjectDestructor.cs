using UnityEngine;

namespace UnityStandardAssets.Utility {
  public class TimedObjectDestructor : MonoBehaviour {
    [SerializeField] readonly float m_TimeOut = 1.0f;
    [SerializeField] bool m_DetachChildren;

    void Awake() { this.Invoke("DestroyNow", this.m_TimeOut); }

    void DestroyNow() {
      if (this.m_DetachChildren) this.transform.DetachChildren();
      Object.Destroy(this.gameObject);
    }
  }
}
