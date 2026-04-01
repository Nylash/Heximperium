using UnityEngine;

public class TintForUnclaimedSprite : MonoBehaviour
{
    [SerializeField] private Color unclaimedTint = new Color(0.6862745f, 0.6666667f, 0.7254902f);

    private SpriteRenderer spriteRenderer;
    private Tile tile;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tile = GetComponentInParent<Tile>();

        if (!tile.Claimed)
        {
            spriteRenderer.color = unclaimedTint;

            tile.OnTileClaimed += (tile) => spriteRenderer.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        if (tile != null)
        {
            tile.OnTileClaimed -= (tile) => spriteRenderer.color = Color.white;
        }
    }
}
