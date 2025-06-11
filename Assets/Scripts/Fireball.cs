using UnityEngine;

public class Fireball : MonoBehaviour
{
    public bool isMooving;
    public int damage = 15;

    public bool isEnemyDamage;

    public int fireballCount = 0;

    void Start()
    {
        fireballCount = 0;
        isMooving = false;
        isEnemyDamage = false;
    }

    void OnTriggerEnter(Collider other) {
        if(other.CompareTag("enemy")) {
            this.isEnemyDamage = true;
            other.GetComponent<Unit>().TakeDamage(damage, true);
            fireballCount++;
            damage += 5;
            if (other.GetComponent<Unit>().currentHealth <= 0 || fireballCount == 4) {
                isMooving = true;
                isEnemyDamage = false;
                this.transform.position = new Vector3(0, -100, 0);

                Destroy(this.gameObject, 1f);
            }
        }
        if(other.CompareTag("Player")) {
            isMooving = false;
            isEnemyDamage = false;
            this.transform.position = new Vector3(0, -100, 0);
            other.GetComponent<Unit>().TakeDamage(damage, true);
            Destroy(this.gameObject, 1f);
        }
    }

    void OnTriggerExit(Collider other) {
         if(other.CompareTag("enemy")) {
            this.isEnemyDamage = false;
        }
    }
}
