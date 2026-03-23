using UnityEngine;

public class TintForUnclaimedSprite : MonoBehaviour
{
    [SerializeField] private Color unclaimedTint = new Color(0.6862745f, 0.6666667f, 0.7254902f);

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = unclaimedTint;

        transform.GetComponentInParent<Tile>().OnTileClaimed += (tile) => spriteRenderer.color = Color.white;
    }
}
