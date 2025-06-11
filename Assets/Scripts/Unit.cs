using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour {
    public string unitName;
    public int maxHealth;
    public int currentHealth;
    public int attackPower;
    public int referenceAttackPower;
    public bool isPlayerUnit;

    public GameObject hud;

    public Animator animator;
    public bool chargeAttack = false;
    public MeshRenderer meshRenderer;
    public GameObject indicator;

    public float attackWindow;
    bool excelentAttack = false;
    bool defendAttack = false;
    public bool canDefend = false;

    public GameObject rangeAttack;

    public GameObject fireballReference;

    public float speedFirabll;

    public bool fireballIsActive = false;

    public AudioSource footStep;
    public AudioSource threSwordAudioSource;
    public AudioSource oneSwordAudioSource; 
    public AudioSource knockAudioSource;

    public AudioSource laughAudioSource;

    public AudioSource dizzyAudioSource;

    public AudioSource hitAudioSource;

    public AudioSource hit2AudioSource;

    void Start() {

        currentHealth = maxHealth;
        speedFirabll = 5;
        animator = GetComponent<Animator>();
        if (indicator != null) {
            indicator.SetActive(false);
        }
        setAttackWindow(6f);
        referenceAttackPower = attackPower;
        hitAudioSource = GameObject.FindWithTag("Hit").GetComponent<AudioSource>();
        footStep = GameObject.FindWithTag("FootStep").GetComponent<AudioSource>();
        threSwordAudioSource = GameObject.FindWithTag("threSword").GetComponent<AudioSource>();
        oneSwordAudioSource = GameObject.FindWithTag("OneSword").GetComponent<AudioSource>();
        knockAudioSource = GameObject.FindWithTag("knock").GetComponent<AudioSource>();
        laughAudioSource = GameObject.FindWithTag("Laught").GetComponent<AudioSource>();
        dizzyAudioSource = GameObject.FindWithTag("Dizzy").GetComponent<AudioSource>();
        hit2AudioSource = GameObject.FindWithTag("Hit2").GetComponent<AudioSource>();
    }



    public void setIsMoovingFireball(bool isMooving) {
        fireballReference.GetComponent<Fireball>().isMooving = isMooving;
    }

    public bool getIsMoovingFireball() {
        if (fireballReference == null) {
            return false;
        }
        return fireballReference.GetComponent<Fireball>().isMooving;
    }

    public bool getEnemyDamage() {
        return fireballReference.GetComponent<Fireball>().isEnemyDamage;
    }

    public void setIsEnemyDamage(bool isEnemyDamage) {
        fireballReference.GetComponent<Fireball>().isEnemyDamage = isEnemyDamage;
    }


    
    public void setAttackWindow(float attackWindow) {
        this.attackWindow = attackWindow;

    }

    public void resetAttackPower() {
        attackPower = referenceAttackPower;
    }

    public void callDefendPlayer(System.Action callback = null) {
        StartCoroutine(playerDefend(callback));
    }

    public IEnumerator takeDamageAnimation() {
        animator.SetBool("damage", true);
        hitAudioSource.Play();
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("damage", false);
    }

    public void addLife(float life) {
        if(currentHealth <= 0) {
            animationDieOff();
        }
        currentHealth += (int)life;
        if (currentHealth > maxHealth) {
            currentHealth = maxHealth;
        }
        
    }



    public IEnumerator playerDefend(System.Action callback = null) {
        defendAttack = true;
        animator.SetBool("defend", true);
        yield return new WaitForSeconds(0.3f);
        animator.SetBool("defend", false);
        defendAttack = false;
        callback?.Invoke();
    }


    public void TakeDamage(int damage, bool callDamage = false) {
        if (callDamage) {
            StartCoroutine(takeDamageAnimation());
        }
        currentHealth -= damage;
        if (currentHealth <= 0) {
            currentHealth = 0;
            if (!isPlayerUnit) {
                Die();
                return;
            }
            animationDie();
        }

    }

    public void setTurnOfAnimationEspecial(bool valor) {
        animator.SetBool("superPower", valor);
    }

    void animationDie() {
        animator.SetBool("die", true);
    }
    public void animationDieOff() {
        animator.SetBool("die", false);
    }
    void Die() {
        animationDie();
        Destroy(gameObject, 5f);
    }

    public void moveTo(Vector3 targetPosition, System.Action callback = null, bool comeBackPosition = false, bool isSuperPower = false) {
        StopAllCoroutines();
        if (!isSuperPower) {
            StartCoroutine(moveToTarget(targetPosition, callback, comeBackPosition));
            return;
        }
        StartCoroutine(moveToTargetFireball(targetPosition, callback, comeBackPosition));
    }

    public void inverRotation() {
        float rotationY = transform.eulerAngles.y;
        float newRotation = (rotationY == 270) ? 90 : 270;
        transform.rotation = Quaternion.Euler(0, newRotation, 0);

    }

    public void attack(Unit target, System.Action callback = null) {
        StopAllCoroutines();

        StartCoroutine(PerformAttackPlayer(target, callback));


    }

    public bool changeColor(float attackTimer) {
        if (attackTimer < attackWindow * 0.35f) {
            meshRenderer.material.color = Color.red;
            attackPower = 0;
            excelentAttack = false;
            return true;
        }
        if (attackTimer < attackWindow * 0.55f) {
            meshRenderer.material.color = Color.yellow;
            attackPower = 5;
            excelentAttack = false;
            return true;
        }
        if (attackTimer < attackWindow * 0.75f) {
            meshRenderer.material.color = Color.blue;
            attackPower = 10;
            excelentAttack = false;
            return true;
        }
        if (attackTimer < attackWindow * 0.85f) {
            meshRenderer.material.color = Color.green;
            attackPower = 30;
            excelentAttack = true;
            return true;
        }


        attackPower = 0;
        excelentAttack = false;
        return false;

    }

    public void resetColor() {
        indicator.SetActive(false);
        meshRenderer.material.color = Color.white;
    }

    public void cancelLaugh() {
        animator.SetBool("laugth", false);
    }

    public IEnumerator moveToTargetFireball(Vector3 targetPosition, System.Action callback = null, bool comeBackPosition = false) {
        if (fireballReference == null) yield break;

        Vector3 startPosition = fireballReference.transform.position;
        Vector3 targetFlat = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        while (fireballReference != null && Vector3.Distance(fireballReference.transform.position, targetFlat) > 0.1f) {
            if (fireballReference == null) yield break;
            fireballReference.transform.position = Vector3.MoveTowards(
                fireballReference.transform.position,
                targetFlat,
                speedFirabll * Time.deltaTime
            );
            yield return null;
        }

        if (fireballReference != null && getEnemyDamage()) {

            setIsEnemyDamage(false);
        }

        callback?.Invoke();
    }

    public void incrementSpeedFireball() {
        speedFirabll += 1f;
    }

    public IEnumerator moveToTarget(Vector3 targetPosition, System.Action callback = null, bool comeBackPosition = false, bool isSuperPower = false) {
        if(footStep == null) {
            footStep = GameObject.FindWithTag("FootStep").GetComponent<AudioSource>();
        }
        footStep.volume = 1;
        animator.SetBool("goTo", true);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5 * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("goTo", false);
        if (comeBackPosition) {
            yield return new WaitForSeconds(0.5f);
        }
        if (!comeBackPosition && !isSuperPower) {
            animator.SetBool("prepareAttack", true);
            yield return new WaitForSeconds(0.3f);
            if (isPlayerUnit) {
                indicator.SetActive(true);
            }
        }
        chargeAttack = true;
        footStep.volume = 0;
        callback?.Invoke();
    }





    public IEnumerator PerformAttackPlayer(Unit unit, System.Action callback = null) {

        animator.SetBool("prepareAttack", false);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("attack", true);


        if (isPlayerUnit) {
            if (excelentAttack) {
                // yield return new WaitForSeconds(0.1f);
                threSwordAudioSource.Play();
            }
            else {
                oneSwordAudioSource.Play();
            }
            animator.SetBool("excelentAttack", excelentAttack);
        }
        yield return new WaitForSeconds(0.3f);
        bool damage = true;
        if (unit.defendAttack) {
            attackPower = 0;
            damage = false;
            knockAudioSource.Play();
            animator.SetBool("attack", false);
            animator.SetBool("dizzy", true);
            dizzyAudioSource.Play();
            yield return new WaitForSeconds(0.5f);
            animator.SetBool("dizzy", false);
        }
        if (attackPower == 0 && !unit.isPlayerUnit) {
            damage = false;

            unit.animator.SetBool("laugth", true);
            laughAudioSource.Play();
        }
        if (unit.isPlayerUnit && damage) {
            hit2AudioSource.Play();
        } else if (damage) {
            hitAudioSource.Play();
        }
        unit.animator.SetBool("damage", damage);
        if (excelentAttack) {
            yield return new WaitForSeconds(1f);
        }
        else {
            yield return new WaitForSeconds(0.5f);
        }




        animator.SetBool("attack", false);
        if (excelentAttack) {
            yield return new WaitForSeconds(0.45f);
        }


        unit.animator.SetBool("damage", false);
        unit.TakeDamage(attackPower);
        Debug.Log(unit.unitName + " take " + attackPower + " damage");
        Debug.Log(unit.currentHealth + " current health");
        yield return new WaitForSeconds(2f);
        callback?.Invoke();
    }

    public void callSuperPowerRotine(GameObject fireball, System.Action callback = null) {
        StopAllCoroutines();
        StartCoroutine(superPowerStart(fireball, callback));
    }


    public IEnumerator superPowerStart(GameObject fireball, System.Action callback = null) {
        fireballReference = Instantiate(fireball, new Vector3(rangeAttack.transform.position.x, -10, rangeAttack.transform.position.z), rangeAttack.transform.rotation);
        while (Vector3.Distance(fireballReference.transform.position, rangeAttack.transform.position) > 0.1f) {
            fireballReference.transform.position = Vector3.MoveTowards(fireballReference.transform.position, rangeAttack.transform.position, 7 * Time.deltaTime);
            yield return null;
        }
        
        callback?.Invoke();
    }

    public IEnumerator animationEspecial() {
        setTurnOfAnimationEspecial(true);
        yield return new WaitForSeconds(0.2f);
        setTurnOfAnimationEspecial(false);
    }

    public bool validCollider(Unit playerReference) {
        bool isCollider = rangeAttack.GetComponent<ColliderRangedAttack>().isCollider;
        StopCoroutine(animationEspecial());
        StartCoroutine(animationEspecial());
        return isCollider;
    }


    public void upgradeEnemy(int numberOfWave) {
        currentHealth += numberOfWave * 2;
        maxHealth += numberOfWave * 2;
        attackPower += numberOfWave * 2;
        
    }
    

}
