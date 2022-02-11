

using UnityEngine;

namespace PM {

public class TargetTileVisualiser : MonoBehaviour
{
  private Sprite sprite;
  private SpriteRenderer spriteRenderer;

  public void Initialize(string imgPath, Color color, Vector2 position) {

    // load texture2D
    byte[] imgData = System.IO.File.ReadAllBytes(imgPath);    
    Texture2D texture = new Texture2D(1, 1);
    texture.LoadImage(imgData);

    spriteRenderer = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
    spriteRenderer.color = color;
    transform.position = position;
    sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width,
      texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    spriteRenderer.sprite = sprite;
  }

  public void SetPosition(Vector2 position) {
    transform.position = position;
  }

}

}
