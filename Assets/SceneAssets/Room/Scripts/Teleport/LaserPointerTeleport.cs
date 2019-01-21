using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SceneAssets.Room.Scripts.Teleport
{
  public class LaserPointerTeleport : MonoBehaviour {
    [SerializeField] Transform cameraRigTransform;
    [SerializeField] Transform headTransform;  // The camera rig's head

    [SerializeField] float teleportOffset = 0.1f;  // Offset from the floor for the reticle to avoid z-fighting

    [SerializeField] LayerMask teleportMask;  // Mask to filter out areas where teleports are allowed

    [SerializeField] protected Transform trackedObj;

    [SerializeField] GameObject laserPrefab;  // The laser prefab

    private GameObject laser;  // A reference to the spawned laser

    private Transform laserTransform;  // The transform component of the laser for ease of use


    [SerializeField] GameObject teleportReticlePrefab;  // Stores a reference to the teleport reticle prefab.

    private GameObject reticle;  // A reference to an instance of the reticle

    private Transform teleportReticleTransform;  // Stores a reference to the teleport reticle transform for ease of use


    private Vector3 hitPoint;  // Point where the raycast hits
    private Vector3 hitNormal;

    private bool shouldTeleport;   // True if there's a valid teleport target

    [SerializeField] private bool activate;

    public bool Activate{
      get => activate;
      set => activate = value;
    }

    //new
    void Start () {
      laser = Instantiate (laserPrefab,this.transform);
      laserTransform = laser.transform;
      reticle = Instantiate (teleportReticlePrefab,this.transform);
      teleportReticleTransform = reticle.transform;
      if (trackedObj == null)
      {
        this.trackedObj = this.transform;
      }


      //this.teleportMask = LayerMask.GetMask("Everything");
      //this.teleportMask -= LayerMask.GetMask("Ignore Raycast");
    
    }

    void Update () {    // Is the touchpad held down?
      if (Activate) {
        RaycastHit hit;

        if (Physics.Raycast (trackedObj.transform.position, transform.forward, out hit, 100, teleportMask)) {
          // Send out a raycast from the controller
          hitPoint = hit.point;
          hitNormal = hit.normal;

          ShowLaser (hit);

          ShowReticle(hit);

          shouldTeleport = true;
        }
      } else { // Touchpad not held down, hide laser & teleport reticle
        laser.SetActive (false);
        reticle.SetActive (false);
      }

      if (!Activate && shouldTeleport) {    // Touchpad released this frame & valid teleport position found
        Teleport ();
      }
    }

    private void ShowReticle(RaycastHit hit){
      reticle.SetActive (true);        //Show teleport reticle
      teleportReticleTransform.position = hitPoint + hitNormal*teleportOffset;
      teleportReticleTransform.LookAt(hitPoint);
    }

    private void ShowLaser (RaycastHit hit) {
      laser.SetActive (true); //Show the laser
      laserTransform.position = Vector3.Lerp (trackedObj.transform.position, hitPoint, .5f); // Move laser to the middle between the controller and the position the raycast hit
      laserTransform.LookAt (hitPoint); // Rotate laser facing the hit point
      var localScale = laserTransform.localScale;
      localScale = new Vector3 (localScale.x, localScale.y,hit.distance); // Scale laser so it fits exactly between the controller & the hit point
      laserTransform.localScale = localScale;
    }

    private void Teleport () {
      shouldTeleport = false; // Teleport in progress, no need to do it again until the next touchpad release
      reticle.SetActive (false); // Hide reticle
      var difference = cameraRigTransform.position - headTransform.position; // Calculate the difference between the center of the virtual room & the player's head
      difference.y = 0; // Don't change the final position's y position, it should always be equal to that of the hit point

      cameraRigTransform.position = hitPoint + difference; // Change the camera rig position to where the the teleport reticle was. Also add the difference so the new virtual room position is relative to the player position, allowing the player's new position to be exactly where they pointed. (see illustration)
    }
  }
}



