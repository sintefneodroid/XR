using UnityEngine;
using UnityEngine.XR;

namespace SceneAssets.Room.Scripts
{
    public class SeatedRig : MonoBehaviour{
        void Start()
        {
            InputTracking.Recenter();
        }
    }
}
