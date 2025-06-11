using Unity.VisualScripting;
using UnityEngine;

public class ColliderRangedAttack : MonoBehaviour
{
    public bool isCollider;

    public bool isColliderAux;
    private float cooldown;

    private void Start() {
        isCollider = false;
        isColliderAux = false;
        cooldown = 0f;
    }

    private void Update() {
        if(!isColliderAux && isCollider) {
            cooldown += Time.deltaTime;
            if(cooldown > 0.3f) {
                cooldown = 0f;
                isCollider = false;
                isColliderAux = false;
            }
        }
        
    }


    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("fireball")) {
            isCollider = true;
            isColliderAux = true;
        }
        Debug.Log("isCollider" + isCollider);
        
    }


    private void OnTriggerExit(Collider other) {
        isColliderAux = false;
        Debug.Log("isColliderAux" + isColliderAux);
    }
}
