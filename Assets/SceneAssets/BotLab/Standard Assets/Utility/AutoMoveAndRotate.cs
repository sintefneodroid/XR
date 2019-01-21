using System;
using UnityEngine;

namespace SceneAssets.BotLab.Standard_Assets.Utility {
  public class AutoMoveAndRotate : MonoBehaviour {
    public bool ignoreTimescale;
    float m_LastRealTime;
    public Vector3andSpace moveUnitsPerSecond;
    public Vector3andSpace rotateDegreesPerSecond;

    void Start() { this.m_LastRealTime = Time.realtimeSinceStartup; }

    // Update is called once per frame
    void Update() {
      var deltaTime = Time.deltaTime;
      if (this.ignoreTimescale) {
        deltaTime = Time.realtimeSinceStartup - this.m_LastRealTime;
        this.m_LastRealTime = Time.realtimeSinceStartup;
      }

      this.transform.Translate(this.moveUnitsPerSecond.value * deltaTime, this.moveUnitsPerSecond.space);
      this.transform.Rotate(this.rotateDegreesPerSecond.value * deltaTime, this.moveUnitsPerSecond.space);
    }

    [Serializable]
    public class Vector3andSpace {
      public Space space = Space.Self;
      public Vector3 value;
    }
  }
}
