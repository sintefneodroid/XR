using UnityEngine;
using UnityEngine.EventSystems;

namespace Robolab.Standard_Assets.Utility {
  public class EventSystemChecker : MonoBehaviour {
    //public GameObject eventSystem;

    // Use this for initialization
    void Awake() {
      if (!FindObjectOfType<EventSystem>()) {
        //Instantiate(eventSystem);
        var obj = new GameObject("EventSystem");
        obj.AddComponent<EventSystem>();
        obj.AddComponent<StandaloneInputModule>().forceModuleActive = true;
      }
    }
  }
}
