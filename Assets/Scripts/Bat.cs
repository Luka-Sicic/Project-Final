using UnityEngine;

public class Bat : Weapon
{
    public float damage = 10f;
    public float attackRange = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public string attackTrigger = "batAttack";

    private Animator animator;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    public override void Fire()
    {
        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(attackTrigger);
        }

        // Melee logic
        Debug.Log("Bat swung!");
        // For now, just a log. Collision logic can be added later.
    }
}
