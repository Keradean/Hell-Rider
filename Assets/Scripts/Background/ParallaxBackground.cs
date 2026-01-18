using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] float moveSpeed;

    [Header("Components")]
    private float backgroundImageWidth;
    
    void Start()
    {
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        backgroundImageWidth = sprite.texture.width / sprite.pixelsPerUnit;
    }

    void Update()
    {
        float speedMultiplier = 1f;

        if (PlayerController.localInstance != null)
        {
            speedMultiplier = PlayerController.localInstance.boostBackgroundSpeed;
        }

        transform.position += new Vector3(moveSpeed * speedMultiplier * Time.deltaTime, 0, 0);

        if(Mathf.Abs(transform.position.x) - backgroundImageWidth > 0) { 
            transform.position = new Vector3(0, transform.position.y,0f);
        }
    }
}
