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

        if (collision.collider.CompareTag("Bird"))
        {
            damage = rb.mass;
            Bird bird = collision.collider.GetComponent<Bird>();

            // Ajout d'une force artificielle pour simuler l'impact de l'oiseau
            Vector2 direction = (transform.position - collision.transform.position).normalized;
            float pushForce = rb.mass * 3 * bird.velocity; // Ajuste le facteur selon le ressenti
            GetComponent<Rigidbody2D>()?.AddForce(direction * pushForce, ForceMode2D.Impulse);

            // Appel des fonctions de rebond
            if (bird != null)
            {
                // Déterminer la direction de l'impact
                Vector2 impactDirection = collision.contacts[0].normal;
                if (Mathf.Abs(impactDirection.x) > Mathf.Abs(impactDirection.y))
                {
                    bird.BlockImpactHorizontal();
                }
                else
                {
                    bird.BlockImpactVertical();
                }
            }
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
        else
        {

        }
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

        if((int)newState - 1 < damageSprites.Length)
        {
            spriteRenderer.sprite = damageSprites[(int)newState ];
        }
    }
}
