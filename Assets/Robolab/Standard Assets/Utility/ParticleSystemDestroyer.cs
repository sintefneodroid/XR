using System.Collections;
using UnityEngine;

namespace Robolab.Standard_Assets.Utility {
  public class ParticleSystemDestroyer : MonoBehaviour {
    bool m_EarlyStop;

    float m_MaxLifetime;

    public float maxDuration = 10;
    // allows a particle system to exist for a specified duration,
    // then shuts off emission, and waits for all particles to expire
    // before destroying the gameObject

    public float minDuration = 8;

    IEnumerator Start() {
      var systems = this.GetComponentsInChildren<ParticleSystem>();

      // find out the maximum lifetime of any particles in this effect
      foreach (var system in systems) {
        this.m_MaxLifetime = Mathf.Max(system.main.startLifetime.constant, this.m_MaxLifetime);
      }

      // wait for random duration

      var stopTime = Time.time + Random.Range(this.minDuration, this.maxDuration);

      while (Time.time < stopTime && !this.m_EarlyStop) {
        yield return null;
      }

      Debug.Log("stopping " + this.name);

      // turn off emission
      foreach (var system in systems) {
        var emission = system.emission;
        emission.enabled = false;
      }

      this.BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);

      // wait for any remaining particles to expire
      yield return new WaitForSeconds(this.m_MaxLifetime);

      Destroy(this.gameObject);
    }

    public void Stop() {
      // stops the particle system early
      this.m_EarlyStop = true;
    }
  }
}
