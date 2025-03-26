using UnityEngine;

public enum BlockType { Wood, Glass, Stone }
public enum BlockState { Intact, Cracked, Broken }

public class DestructibleBlock : MonoBehaviour
{
    public BlockType blockType;
    public Sprite[] damageSprites; // 0 = Intact, 1 = Fissuré, 2 = Brisé
    public float maxDurability;
    private float currentDurability;
    private SpriteRenderer spriteRenderer;
    private BlockState state = BlockState.Intact;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentDurability = maxDurability;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        float damage = 0f;

        // Si l'objet est un oiseau (tag "Bird"), seul la masse compte
        if (collision.collider.CompareTag("Bird"))
        {
            damage = rb.mass;
        }
        else
        {
            damage = rb.mass * rb.velocity.magnitude;
        }

        ApplyDamage(damage);
    }

    public void ApplyDamage(float damage)
    {
        currentDurability -= damage;
        UpdateBlockState();

        if (currentDurability <= 0)
            Destroy(gameObject);
    }

    private void UpdateBlockState()
    {
        float ratio = currentDurability / maxDurability;

        if (ratio > 0.66f)
            ChangeSprite(BlockState.Intact);
        else if (ratio > 0.33f)
            ChangeSprite(BlockState.Cracked);
        else
            ChangeSprite(BlockState.Broken);
    }

    private void ChangeSprite(BlockState newState)
    {
        if (state == newState) return;
        state = newState;
        spriteRenderer.sprite = damageSprites[(int)newState];
    }
}
