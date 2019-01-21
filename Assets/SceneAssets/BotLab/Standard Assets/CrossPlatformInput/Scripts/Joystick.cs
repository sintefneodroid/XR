using UnityEngine;
using UnityEngine.EventSystems;

namespace SceneAssets.BotLab.Standard_Assets.CrossPlatformInput.Scripts {
  public class Joystick : MonoBehaviour,
                          IPointerDownHandler,
                          IPointerUpHandler,
                          IDragHandler {
    public enum AxisOption {
      // Options for which axes to use
      Both, // Use both
      OnlyHorizontal, // Only horizontal
      OnlyVertical // Only vertical
    }

    public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use

    public string
        horizontalAxisName =
            "Horizontal"; // The name given to the horizontal axis for the cross platform input

    CrossPlatformInputManager.VirtualAxis
        m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input

    Vector3 m_StartPos;
    bool m_UseX; // Toggle for using UnityEngine; using UnityEditor; using the x axis
    bool m_UseY; // Toggle for using UnityEngine; using UnityEditor; using the Y axis

    CrossPlatformInputManager.VirtualAxis
        m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

    public int MovementRange = 100;

    public string
        verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

    public void OnDrag(PointerEventData data) {
      var newPos = Vector3.zero;

      if (this.m_UseX) {
        var delta = (int)(data.position.x - this.m_StartPos.x);
        delta = Mathf.Clamp(delta, -this.MovementRange, this.MovementRange);
        newPos.x = delta;
      }

      if (this.m_UseY) {
        var delta = (int)(data.position.y - this.m_StartPos.y);
        delta = Mathf.Clamp(delta, -this.MovementRange, this.MovementRange);
        newPos.y = delta;
      }

      this.transform.position = new Vector3(
          this.m_StartPos.x + newPos.x,
          this.m_StartPos.y + newPos.y,
          this.m_StartPos.z + newPos.z);
      this.UpdateVirtualAxes(this.transform.position);
    }

    public void OnPointerDown(PointerEventData data) { }

    public void OnPointerUp(PointerEventData data) {
      this.transform.position = this.m_StartPos;
      this.UpdateVirtualAxes(this.m_StartPos);
    }

    void OnEnable() { this.CreateVirtualAxes(); }

    void Start() { this.m_StartPos = this.transform.position; }

    void UpdateVirtualAxes(Vector3 value) {
      var delta = this.m_StartPos - value;
      delta.y = -delta.y;
      delta /= this.MovementRange;
      if (this.m_UseX) {
        this.m_HorizontalVirtualAxis.Update(-delta.x);
      }

      if (this.m_UseY) {
        this.m_VerticalVirtualAxis.Update(delta.y);
      }
    }

    void CreateVirtualAxes() {
      // set axes to use
      this.m_UseX = this.axesToUse == AxisOption.Both || this.axesToUse == AxisOption.OnlyHorizontal;
      this.m_UseY = this.axesToUse == AxisOption.Both || this.axesToUse == AxisOption.OnlyVertical;

      // create new axes based on axes to use
      if (this.m_UseX) {
        this.m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(this.horizontalAxisName);
        CrossPlatformInputManager.RegisterVirtualAxis(this.m_HorizontalVirtualAxis);
      }

      if (this.m_UseY) {
        this.m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(this.verticalAxisName);
        CrossPlatformInputManager.RegisterVirtualAxis(this.m_VerticalVirtualAxis);
      }
    }

    void OnDisable() {
      // remove the joysticks from the cross platform input
      if (this.m_UseX) {
        this.m_HorizontalVirtualAxis.Remove();
      }

      if (this.m_UseY) {
        this.m_VerticalVirtualAxis.Remove();
      }
    }
  }
}
