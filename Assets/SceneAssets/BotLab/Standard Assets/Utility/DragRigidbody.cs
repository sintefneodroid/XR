using System.Collections;
using UnityEngine;

namespace SceneAssets.BotLab.Standard_Assets.Utility {
  public class DragRigidbody : MonoBehaviour {
    const float k_Spring = 50.0f;
    const float k_Damper = 5.0f;
    const float k_Drag = 10.0f;
    const float k_AngularDrag = 5.0f;
    const float k_Distance = 0.2f;
    const bool k_AttachToCenterOfMass = false;

    SpringJoint m_SpringJoint;

    void Update() {
      // Make sure the user pressed the mouse down
      if (!Input.GetMouseButtonDown(0)) {
        return;
      }

      var mainCamera = this.FindCamera();

      // We need to actually hit an object
      var hit = new RaycastHit();
      if (!Physics.Raycast(
              mainCamera.ScreenPointToRay(Input.mousePosition).origin,
              mainCamera.ScreenPointToRay(Input.mousePosition).direction,
              out hit,
              100,
              Physics.DefaultRaycastLayers)) {
        return;
      }

      // We need to hit a rigidbody that is not kinematic
      if (!hit.rigidbody || hit.rigidbody.isKinematic) {
        return;
      }

      if (!this.m_SpringJoint) {
        var go = new GameObject("Rigidbody dragger");
        var body = go.AddComponent<Rigidbody>();
        this.m_SpringJoint = go.AddComponent<SpringJoint>();
        body.isKinematic = true;
      }

      this.m_SpringJoint.transform.position = hit.point;
      this.m_SpringJoint.anchor = Vector3.zero;

      this.m_SpringJoint.spring = k_Spring;
      this.m_SpringJoint.damper = k_Damper;
      this.m_SpringJoint.maxDistance = k_Distance;
      this.m_SpringJoint.connectedBody = hit.rigidbody;

      this.StartCoroutine("DragObject", hit.distance);
    }

    IEnumerator DragObject(float distance) {
      var oldDrag = this.m_SpringJoint.connectedBody.drag;
      var oldAngularDrag = this.m_SpringJoint.connectedBody.angularDrag;
      this.m_SpringJoint.connectedBody.drag = k_Drag;
      this.m_SpringJoint.connectedBody.angularDrag = k_AngularDrag;
      var mainCamera = this.FindCamera();
      while (Input.GetMouseButton(0)) {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        this.m_SpringJoint.transform.position = ray.GetPoint(distance);
        yield return null;
      }

      if (this.m_SpringJoint.connectedBody) {
        this.m_SpringJoint.connectedBody.drag = oldDrag;
        this.m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
        this.m_SpringJoint.connectedBody = null;
      }
    }

    Camera FindCamera() { return this.GetComponent<Camera>() ? this.GetComponent<Camera>() : Camera.main; }
  }
}
