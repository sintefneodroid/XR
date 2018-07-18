using UnityEngine;

namespace Robolab.Standard_Assets.Utility {
  public class SmoothFollow : MonoBehaviour {
    // The distance in the x-z plane to the target
    [SerializeField] readonly float distance = 10.0f;

    // the height we want the camera to be above the target
    [SerializeField] readonly float height = 5.0f;

    [SerializeField] float heightDamping;

    [SerializeField] float rotationDamping;

    // The target we are following
    [SerializeField] Transform target;

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void LateUpdate() {
      // Early out if we don't have a target
      if (!this.target) {
        return;
      }

      // Calculate the current rotation angles
      var wantedRotationAngle = this.target.eulerAngles.y;
      var wantedHeight = this.target.position.y + this.height;

      var currentRotationAngle = this.transform.eulerAngles.y;
      var currentHeight = this.transform.position.y;

      // Damp the rotation around the y-axis
      currentRotationAngle = Mathf.LerpAngle(
          currentRotationAngle,
          wantedRotationAngle,
          this.rotationDamping * Time.deltaTime);

      // Damp the height
      currentHeight = Mathf.Lerp(currentHeight, wantedHeight, this.heightDamping * Time.deltaTime);

      // Convert the angle into a rotation
      var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

      // Set the position of the camera on the x-z plane to:
      // distance meters behind the target
      this.transform.position = this.target.position;
      this.transform.position -= currentRotation * Vector3.forward * this.distance;

      // Set the height of the camera
      this.transform.position = new Vector3(
          this.transform.position.x,
          currentHeight,
          this.transform.position.z);

      // Always look at the target
      this.transform.LookAt(this.target);
    }
  }
}
