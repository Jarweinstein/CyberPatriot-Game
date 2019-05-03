using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
            if(health == 0f)
            {
                killEnemy();
            }
        }
    }
    [SerializeField] float dmgDealt, attackCooldown;
    public float maxHealth;
    bool isAttacking = false;
    float health;
    void killEnemy()
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        health = maxHealth;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isAttacking = true;
            StartCoroutine(dealDmg(collision.gameObject));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            isAttacking = false;
        }
    }

    IEnumerator dealDmg(GameObject player)
    {
        PlayerController playerScript = player.GetComponent<PlayerController>();
        while(isAttacking)
        {
            playerScript.Health -= dmgDealt;
            yield return new WaitForSeconds(attackCooldown);
        }
        yield return null;
    }
}
