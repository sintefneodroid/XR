using UnityEngine;
using UnityEngine.UI;

namespace SceneAssets.BotLab.Standard_Assets.Utility {
  public class AlphaButtonClickMask : MonoBehaviour,
                                      ICanvasRaycastFilter {
    protected Image _image;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera) {
      Vector2 localPoint;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
          this._image.rectTransform,
          sp,
          eventCamera,
          out localPoint);

      var pivot = this._image.rectTransform.pivot;
      var normalizedLocal = new Vector2(
          pivot.x + localPoint.x / this._image.rectTransform.rect.width,
          pivot.y + localPoint.y / this._image.rectTransform.rect.height);
      var uv = new Vector2(
          this._image.sprite.rect.x + normalizedLocal.x * this._image.sprite.rect.width,
          this._image.sprite.rect.y + normalizedLocal.y * this._image.sprite.rect.height);

      uv.x /= this._image.sprite.texture.width;
      uv.y /= this._image.sprite.texture.height;

      //uv are inversed, as 0,0 or the rect transform seem to be upper right, then going negativ toward lower left...
      var c = this._image.sprite.texture.GetPixelBilinear(uv.x, uv.y);

      return c.a > 0.1f;
    }

    public void Start() {
      this._image = this.GetComponent<Image>();

      var tex = this._image.sprite.texture;

      var isInvalid = false;
      if (tex != null) {
        try {
          tex.GetPixels32();
        } catch (UnityException e) {
          Debug.LogError(e.Message);
          isInvalid = true;
        }
      } else {
        isInvalid = true;
      }

      if (isInvalid) {
        Debug.LogError("This script need an Image with a readbale Texture2D to work.");
      }
    }
  }
}
