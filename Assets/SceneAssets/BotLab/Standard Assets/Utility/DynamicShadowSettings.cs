using UnityEngine;

namespace SceneAssets.BotLab.Standard_Assets.Utility {
  public class DynamicShadowSettings : MonoBehaviour {
    public float adaptTime = 1;
    float m_ChangeSpeed;
    float m_OriginalStrength = 1;

    float m_SmoothHeight;
    public float maxHeight = 1000;
    public float maxShadowBias = 0.1f;
    public float maxShadowDistance = 10000;
    public float minHeight = 10;
    public float minShadowBias = 1;
    public float minShadowDistance = 80;
    public Light sunLight;

    void Start() { this.m_OriginalStrength = this.sunLight.shadowStrength; }

    // Update is called once per frame
    void Update() {
      var ray = new Ray(Camera.main.transform.position, -Vector3.up);
      RaycastHit hit;
      var height = this.transform.position.y;
      if (Physics.Raycast(ray, out hit)) {
        height = hit.distance;
      }

      if (Mathf.Abs(height - this.m_SmoothHeight) > 1) {
        this.m_SmoothHeight = Mathf.SmoothDamp(
            this.m_SmoothHeight,
            height,
            ref this.m_ChangeSpeed,
            this.adaptTime);
      }

      var i = Mathf.InverseLerp(this.minHeight, this.maxHeight, this.m_SmoothHeight);

      QualitySettings.shadowDistance = Mathf.Lerp(this.minShadowDistance, this.maxShadowDistance, i);
      this.sunLight.shadowBias = Mathf.Lerp(this.minShadowBias, this.maxShadowBias, 1 - (1 - i) * (1 - i));
      this.sunLight.shadowStrength = Mathf.Lerp(this.m_OriginalStrength, 0, i);
    }
  }
}
