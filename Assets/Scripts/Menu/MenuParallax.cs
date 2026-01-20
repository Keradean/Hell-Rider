using UnityEngine;

public class MenuBackground : MonoBehaviour
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
    }

    private void Update()
    {
        float speedMultiplier = 1f;

        transform.position += new Vector3(moveSpeed * speedMultiplier * Time.deltaTime, 0, 0);

        if (Mathf.Abs(transform.position.x) >= backgroundImageWidth)
        {
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }
    }
}