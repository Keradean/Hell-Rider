using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float moveSpeed;

    [Header("Components")]
    private float backgroundImageWidth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Sprite sprite = spriteRenderer.sprite;
            backgroundImageWidth = sprite.texture.width / sprite.pixelsPerUnit;
        }
        else
        {
            Debug.LogWarning("ParallaxBackground: SpriteRenderer or Sprite is missing!");
        }
    }

    private void Update()
    {
        float speedMultiplier = 1f;

        // Prüfe ob der lokale Player existiert
        if (PlayerController.localInstance != null)
        {
            speedMultiplier = PlayerController.localInstance.boostBackgroundSpeed;
        }

        transform.position += new Vector3(moveSpeed * speedMultiplier * Time.deltaTime, 0, 0);

        if (Mathf.Abs(transform.position.x) >= backgroundImageWidth)
        {
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }
    }
}