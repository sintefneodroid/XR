using UnityEngine;
using UnityEngine.UI;

namespace Robolab.Standard_Assets.Utility {
  [RequireComponent(typeof(Text))]
  public class FPSCounter : MonoBehaviour {
    const float fpsMeasurePeriod = 0.5f;
    const string display = "{0} FPS";
    int m_CurrentFps;
    int m_FpsAccumulator;
    float m_FpsNextPeriod;
    Text m_Text;

    void Start() {
      this.m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
      this.m_Text = this.GetComponent<Text>();
    }

    void Update() {
      // measure average frames per second
      this.m_FpsAccumulator++;
      if (Time.realtimeSinceStartup > this.m_FpsNextPeriod) {
        this.m_CurrentFps = (int)(this.m_FpsAccumulator / fpsMeasurePeriod);
        this.m_FpsAccumulator = 0;
        this.m_FpsNextPeriod += fpsMeasurePeriod;
        this.m_Text.text = string.Format(display, this.m_CurrentFps);
      }
    }
  }
}
