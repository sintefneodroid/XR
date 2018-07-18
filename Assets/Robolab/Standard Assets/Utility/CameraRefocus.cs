using UnityEngine;

namespace Robolab.Standard_Assets.Utility {
  public class CameraRefocus {
    readonly Vector3 m_OrigCameraPos;
    public Camera Camera;
    public Vector3 Lookatpoint;
    bool m_Refocus;
    public Transform Parent;

    public CameraRefocus(Camera camera, Transform parent, Vector3 origCameraPos) {
      this.m_OrigCameraPos = origCameraPos;
      this.Camera = camera;
      this.Parent = parent;
    }

    public void ChangeCamera(Camera camera) { this.Camera = camera; }

    public void ChangeParent(Transform parent) { this.Parent = parent; }

    public void GetFocusPoint() {
      RaycastHit hitInfo;
      if (Physics.Raycast(
          this.Parent.transform.position + this.m_OrigCameraPos,
          this.Parent.transform.forward,
          out hitInfo,
          100f)) {
        this.Lookatpoint = hitInfo.point;
        this.m_Refocus = true;
        return;
      }

      this.m_Refocus = false;
    }

    public void SetFocusPoint() {
      if (this.m_Refocus) {
        this.Camera.transform.LookAt(this.Lookatpoint);
      }
    }
  }
}
