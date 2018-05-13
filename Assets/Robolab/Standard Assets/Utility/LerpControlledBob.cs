using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Utility {
  [Serializable]
  public class LerpControlledBob {
    public float BobAmount;
    public float BobDuration;

    float m_Offset;

    // provides the offset that can be used
    public float Offset() { return this.m_Offset; }

    public IEnumerator DoBobCycle() {
      // make the camera move down slightly
      var t = 0f;
      while (t < this.BobDuration) {
        this.m_Offset = Mathf.Lerp(0f, this.BobAmount, t / this.BobDuration);
        t += Time.deltaTime;
        yield return new WaitForFixedUpdate();
      }

      // make it move back to neutral
      t = 0f;
      while (t < this.BobDuration) {
        this.m_Offset = Mathf.Lerp(this.BobAmount, 0f, t / this.BobDuration);
        t += Time.deltaTime;
        yield return new WaitForFixedUpdate();
      }

      this.m_Offset = 0f;
    }
  }
}
