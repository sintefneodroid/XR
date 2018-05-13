using UnityEngine;

namespace UnityStandardAssets.CrossPlatformInput {
  public class InputAxisScrollbar : MonoBehaviour {
    public string axis;

    void Update() { }

    public void HandleInput(float value) { CrossPlatformInputManager.SetAxis(this.axis, value * 2f - 1f); }
  }
}
