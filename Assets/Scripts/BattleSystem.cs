using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleState { START, PLAYER1TURN, CHOOSEENEMY, ENEMYATTACK, WAITSUPER, PLAYER2TURN, WAIT, WAITENEMYATTACK, PLAYERATTACK, ENEMYTURN, WON, LOST, SUPERPOWER }
public class BattleSystem : MonoBehaviour {
    [SerializeField] private Transform player1Transform;
    [SerializeField] private Transform player2Transform;

    [SerializeField] private List<Transform> enemyTransforms = new List<Transform>();

    [SerializeField] private List<GameObject> playerPrefab = new List<GameObject>();
    [SerializeField] public List<GameObject> players = new List<GameObject>();
    [SerializeField] private List<GameObject> enemyPrefab = new List<GameObject>();
    [SerializeField] private List<GameObject> playersHud = new List<GameObject>();

    public List<GameObject> enemies = new List<GameObject>();
    int indexActiveEnemy = 0;
    int indexActivePlayer = 0;

    private float attackTimer = 0f;

    private int indexEnemyAtack = 0;
    private int indexChoose = 0;

    [SerializeField] private bool isSuperPower;

    [SerializeField] private GameObject fireball;

    private int indexPlayerFireball;

    public BattleState state;

    public Animator wipeAnimator;

    public GameObject gameHud;

    public int numberOfWave;

    public int numberOfMana;

    public bool spaceShow;

    public bool lostGameBool;

    void Start() {
        isSuperPower = false;
        state = BattleState.WAIT;
        numberOfMana = 10;
        numberOfWave = 0;
        spaceShow = false;
        lostGameBool = false;
        StartCoroutine(setupBattle());

    }

    void Update() {
        configureEnemies();
        fireballFinish();
        if (validEnemies() && state != BattleState.WON && state != BattleState.START && state != BattleState.WAIT && state != BattleState.LOST) {
            state = BattleState.WON;
            return;
        }
        if (validPlayer() && state != BattleState.WON && state != BattleState.START && state != BattleState.WAIT && state != BattleState.LOST) {
            state = BattleState.LOST;
            lostGame();
            return;
        }
        if (state == BattleState.CHOOSEENEMY) {
            if (enemies.Count > 0) {
                spaceShow = true;
                playerChoose();
            }
        }
        if (state == BattleState.PLAYERATTACK) {
            spaceShow = true;
            attackTimer += Time.deltaTime;
            bool colorResult = players[indexActivePlayer].GetComponent<Unit>().changeColor(attackTimer);
            if (Input.GetKeyDown(KeyCode.Space) || !colorResult) {
                attackTimer = 0f;
                players[indexActivePlayer].GetComponent<Unit>().resetColor();
                state = BattleState.WAIT;
                players[indexActivePlayer].GetComponent<Unit>().attack(enemies[indexActiveEnemy].GetComponent<Unit>(), () => comeBackPosition(true));
                spaceShow = false;
            }
        }
        if (state == BattleState.ENEMYTURN) {

            if (indexEnemyAtack > enemies.Count - 1) {
                if (indexActivePlayer == 0) {
                    indexActivePlayer++;
                    state = BattleState.PLAYER2TURN;
                }
                else {
                    indexActivePlayer = 0;
                    state = BattleState.PLAYER1TURN;
                }
                indexEnemyAtack = 0;
                player1Turn();
                player2Turn();
                return;
            }
            indexChoose = UnityEngine.Random.Range(0, players.Count);
            GameObject player = players[indexChoose];
            if (player.GetComponent<Unit>().currentHealth <= 0) {
                if (indexChoose == 0) {
                    indexChoose = 1;
                    player = players[1];
                }
                else {
                    indexChoose = 0;
                    player = players[0];
                }
            }

            if (enemies[indexEnemyAtack].GetComponent<Unit>().currentHealth <= 0) {
                return;
            }

            state = BattleState.WAIT;
            enemies[indexEnemyAtack].GetComponent<Unit>().moveTo(player.transform.Find("PositionPlayer").position, () => setEnemyAttack(), false);

        }

        if (state == BattleState.ENEMYATTACK) {
            spaceShow = false;
            attackTimer += Time.deltaTime;
            players[indexChoose].GetComponent<Unit>().canDefend = true;
            if (attackTimer >= enemies[indexEnemyAtack].GetComponent<Unit>().attackWindow) {
                attackTimer = 0f;
                state = BattleState.WAIT;
                enemies[indexEnemyAtack].GetComponent<Unit>().attack(players[indexChoose].GetComponent<Unit>(), () => comeBackPosition(false));
                spaceShow = false;
            }

        }
        if (players.Count > 0 && players[indexChoose].GetComponent<Unit>().canDefend) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                players[indexChoose].GetComponent<Unit>().callDefendPlayer();
            }
        }

        if (state == BattleState.SUPERPOWER) {
            if (players[indexActivePlayer].GetComponent<Unit>().fireballReference == null) {
                return;
            }
            spaceShow = true;
            if (!players[indexActivePlayer].GetComponent<Unit>().getIsMoovingFireball() && Input.GetKeyDown(KeyCode.Space)) {
                attackTimer = 0;
                players[indexActivePlayer].GetComponent<Unit>().setIsMoovingFireball(true);
                players[indexActivePlayer].GetComponent<Unit>().moveTo(enemies[indexActiveEnemy].transform.position, null, false, true);
            }
            if (players[indexActivePlayer].GetComponent<Unit>().getIsMoovingFireball() && players[indexActivePlayer].GetComponent<Unit>().getEnemyDamage()) {
                players[indexActivePlayer].GetComponent<Unit>().incrementSpeedFireball();
                nextPlayer();
                attackTimer = 0;

            }

        }

        if (state == BattleState.WAITSUPER) {
            attackTimer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) && attackTimer > 0.5f) {
                attackTimer = 0f;
                Debug.Log(players[indexPlayerFireball].GetComponent<Unit>().unitName + " " + attackTimer);
                if (players[indexPlayerFireball].GetComponent<Unit>().validCollider(players[indexActivePlayer].GetComponent<Unit>())) {
                    players[indexActivePlayer].GetComponent<Unit>().moveTo(enemies[indexActiveEnemy].transform.position, null, false, true);
                    state = BattleState.SUPERPOWER;
                }
            }
        }

        if (state == BattleState.WON) {
            StartCoroutine(finishBattle());
            // wipeAnimator.SetTrigger("circleOut");
            state = BattleState.WAIT;
        }
        if (state == BattleState.START) {
            StartCoroutine(setupBattle());
            state = BattleState.WAIT;
        }
    }

    public void nextPlayer() {
        indexPlayerFireball = (indexPlayerFireball == 0) ? 1 : 0;
        Vector3 targetPosition = (indexPlayerFireball == 0) ? player1Transform.position : player2Transform.position;
        players[indexActivePlayer].GetComponent<Unit>().moveTo(targetPosition, null, false, true);
        state = BattleState.WAITSUPER;
    }

    public void configureEnemies() {
        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] == null) {
                enemies.RemoveAt(i);
                continue;
            }
        }
    }


    public void setAttackTimer(float attackTimer) {
        this.attackTimer = attackTimer;
    }

    public Boolean validEnemies() {
        bool isValid = false;
        if (enemies.Count == 0) {
            isValid = true;
            return isValid;
        }
        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i].GetComponent<Unit>().currentHealth <= 0) {
                isValid = true;
                continue;
            }
            isValid = false;
            break;
        }
        return isValid;
    }

    public Boolean validPlayer(bool isCheckAlive = false) {
        bool isValid = false;

        for (int i = 0; i < players.Count; i++) {
            if (players[i].GetComponent<Unit>().currentHealth <= 0) {
                isValid = true;
                if(isCheckAlive) {
                    break;
                }
                continue;
            }
            isValid = false;
            if( isCheckAlive) {
                continue;
            }
            break;
        }
        return isValid;
    }

    public void fireballFinish() {
        if (players.Count == 0) {
            return;
        }

        if (players[indexActivePlayer].GetComponent<Unit>().fireballReference == null && (state == BattleState.SUPERPOWER || state == BattleState.WAITSUPER) && isSuperPower) {
            players[indexActivePlayer].GetComponent<Unit>().speedFirabll = 5f;
            players[0].GetComponent<Unit>().setTurnOfAnimationEspecial(false);
            players[1].GetComponent<Unit>().setTurnOfAnimationEspecial(false);
            players[indexActivePlayer].GetComponent<Unit>().fireballIsActive = false;
            spaceShow = false;
            changeTurnPlayer();
            return;
        }
    }


    public void enemyAttackCallback() {
        indexEnemyAtack++;
        state = BattleState.ENEMYTURN;
    }


    public void playerChoose() {

        if (Input.GetKeyDown(KeyCode.DownArrow) ||  Input.GetKeyDown(KeyCode.S) ) {
            changeEnemy(1);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            changeEnemy(-1 + enemies.Count);
        }
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            spaceShow = false;
            for (int i = 0; i < enemies.Count; i++) {
                enemies[i].GetComponent<Unit>().hud.SetActive(false);
            }
            setSuperPower(false);
            state = indexActivePlayer == 1 ? BattleState.PLAYER2TURN : BattleState.PLAYER1TURN;
            player1Turn();
            player2Turn();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !isSuperPower) {
            state = BattleState.WAIT;
            spaceShow = false;
            enemies[indexActiveEnemy].GetComponent<Unit>().hud.SetActive(false);
            players[indexActivePlayer].GetComponent<Unit>().moveTo(enemies[indexActiveEnemy].transform.Find("PositionEnemy").position, () => setPlayerAttack());

        }

        if (Input.GetKeyDown(KeyCode.Space) && isSuperPower && !players[indexActivePlayer].GetComponent<Unit>().fireballIsActive) {
            state = BattleState.WAIT;
            spaceShow = false;
            numberOfMana--;
            indexPlayerFireball = indexActivePlayer;
            enemies[indexActiveEnemy].GetComponent<Unit>().hud.SetActive(false);
            players[indexActivePlayer].GetComponent<Unit>().fireballIsActive = true;
            players[indexActivePlayer].GetComponent<Unit>().callSuperPowerRotine(fireball, () => setSuperPowerAttackState());
        }

    }

    public void setSuperPowerAttackState() {
        state = BattleState.SUPERPOWER;
    }
    public void setPlayerAttack() {
        state = BattleState.PLAYERATTACK;

    }
    public void setWait() {
        state = BattleState.WAIT;

    }
    public void setEnemyAttack() {
        state = BattleState.ENEMYATTACK;
        enemies[indexEnemyAtack].GetComponent<Unit>().setAttackWindow(UnityEngine.Random.Range(1f, 6f));
        enemies[indexEnemyAtack].GetComponent<Unit>().resetAttackPower();
    }

    public void comeBackPosition(bool isPlayer) {
        if (isPlayer) {
            players[indexActivePlayer].GetComponent<Unit>().inverRotation();
            Vector3 targetPosition = (indexActivePlayer == 0) ? player1Transform.position : player2Transform.position;
            players[indexActivePlayer].GetComponent<Unit>().moveTo(targetPosition, () => callbackTurn(isPlayer), true);
            return;
        }
        players[indexChoose].GetComponent<Unit>().canDefend = false;
        enemies[indexEnemyAtack].GetComponent<Unit>().inverRotation();
        Vector3 targetPositionEnemy = enemies[indexEnemyAtack].transform.parent.position;
        enemies[indexEnemyAtack].GetComponent<Unit>().moveTo(targetPositionEnemy, () => callbackTurn(isPlayer), true);
    }
    public void changeTurnPlayer() {
        state = indexActivePlayer == 0 ? BattleState.ENEMYTURN : BattleState.PLAYER1TURN;
        if (indexActivePlayer == 1 && players[0].GetComponent<Unit>().currentHealth <= 0) {
            state = BattleState.ENEMYTURN;
            return;
        }
        player1Turn();
        player2Turn();
        return;
    }
    public void callbackTurn(bool isPlayer) {
        if (isPlayer) {
            enemies[indexActiveEnemy].GetComponent<Unit>().cancelLaugh();
            players[indexActivePlayer].GetComponent<Unit>().inverRotation();
            changeTurnPlayer();
            return;
        }
        enemies[indexEnemyAtack].GetComponent<Unit>().inverRotation();
        indexEnemyAtack++;
        state = BattleState.ENEMYTURN;
    }

    public void changeEnemy(int choose) {
        if (indexActiveEnemy > enemies.Count - 1 || enemies[indexActiveEnemy] == null) {
            if (enemies.Count > 0) {
                indexActiveEnemy = 0;
            }
        }
        enemies[indexActiveEnemy].GetComponent<Unit>().hud.SetActive(false);
        indexActiveEnemy = (indexActiveEnemy + choose) % enemies.Count;
        enemies[indexActiveEnemy].GetComponent<Unit>().hud.SetActive(true);
    }

    IEnumerator setupBattle() {
        
        numberOfWave++;
        bool upgradeEnemy = false;
        if(numberOfWave > 1 ) {
            upgradeEnemy = true;
        }
        playersHud[0].SetActive(false);
        playersHud[1].SetActive(false);
        if (players.Count == 0) {
            GameObject player1 = Instantiate(playerPrefab[0], player1Transform.position, player1Transform.rotation);
            player1.transform.SetParent(player1Transform);
            players.Add(player1);
            GameObject player2 = Instantiate(playerPrefab[1], player2Transform.position, player2Transform.rotation);
            player2.transform.SetParent(player2Transform);
            players.Add(player2);
        }

        players[0].transform.position = GetEdgeXPosition(Camera.main, players[0].transform.position.y, players[0].transform.position.z, 0, true);
        players[1].transform.position = GetEdgeXPosition(Camera.main, players[1].transform.position.y, players[1].transform.position.z, 0);

        for (int i = 0; i <= 2; i++) {
            int randomEnemyIndex = UnityEngine.Random.Range(0, enemyPrefab.Count);
            
            int indexPosition = i;
            GameObject enemy = Instantiate(enemyPrefab[randomEnemyIndex], enemyTransforms[indexPosition].position, enemyTransforms[indexPosition].rotation);
            if(upgradeEnemy) {
                enemy.GetComponent<Unit>().upgradeEnemy(numberOfWave);
            }
            enemy.transform.SetParent(enemyTransforms[indexPosition]);
            
            enemies.Add(enemy);
        }
        enemyTransforms.RemoveAll(x => x == null);
        players[0].GetComponent<Unit>().moveTo(player1Transform.position, null, true, false);
        players[1].GetComponent<Unit>().moveTo(player2Transform.position, null, true, false);
        yield return new WaitForSeconds(1f);
        wipeAnimator.SetTrigger("circleIn");
        yield return new WaitForSeconds(1.5f);
        gameHud.SetActive(true);

        state = BattleState.PLAYER1TURN;
        player1Turn();
        player2Turn();
    }


    void player1Turn() {
        if (state != BattleState.PLAYER1TURN) {
            return;
        }
        if (players[0].GetComponent<Unit>().currentHealth <= 0) {
            state = BattleState.PLAYER2TURN;
            player2Turn();
            return;
        }
        playersHud[0].SetActive(true);
        playersHud[1].SetActive(false);
    }

    void player2Turn() {
        if (state != BattleState.PLAYER2TURN) {
            return;
        }
        Debug.Log(players[1].GetComponent<Unit>().currentHealth);
        if (players[1].GetComponent<Unit>().currentHealth <= 0) {
            state = BattleState.PLAYER1TURN;
            player1Turn();
            return;
        }
        playersHud[0].SetActive(false);
        playersHud[1].SetActive(true);
    }

    public void chooseEnemey(int playerActive) {
        if (isSuperPower && (numberOfMana <= 0 || validPlayer(true)) ) {
            return;
        }
        playersHud[playerActive].SetActive(false);
        state = BattleState.CHOOSEENEMY;
        if (enemies.Count > 0) {
            this.changeEnemy(-1 + enemies.Count);
        }
        indexActivePlayer = playerActive;
    }

    public void setSuperPower(bool isSuperPower) {
        this.isSuperPower = isSuperPower;
    }



    public Vector3 GetEdgeXPosition(Camera cam, float fixedY, float fixedZ, float x, bool player1 = false) {

        Vector3 viewportPoint = new Vector3(x, 0.5f, Mathf.Abs(cam.transform.position.z - fixedZ));


        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPoint);
        if (x == 0) {
            worldPos.x += player1 ? 5 : 10;
        }
        else {
            worldPos.x -= 5;
        }


        return new Vector3(worldPos.x, fixedY, fixedZ);
    }


    public IEnumerator finishBattle() {
        playersHud[0].SetActive(false);
        playersHud[1].SetActive(false);
        players[0].GetComponent<Unit>().animationDieOff();
        players[1].GetComponent<Unit>().animationDieOff();
        for (int i = 0; i < players.Count; i++) {
            if (players[i].GetComponent<Unit>().currentHealth <= 0) {
                players[i].GetComponent<Unit>().addLife(10);
            }
        }
        yield return new WaitForSeconds(1.5f);
        Vector3 targetPosition = GetEdgeXPosition(Camera.main, players[0].transform.position.y, players[0].transform.position.z, 1);
        //players[0].transform.position = Vector3.MoveTowards(transform.position, , 5f * Time.deltaTime);
        players[0].GetComponent<Unit>().moveTo(targetPosition, null, true, false);
        Vector3 targetPosition2 = GetEdgeXPosition(Camera.main, players[1].transform.position.y, players[1].transform.position.z, 1);
        //players[0].transform.position = Vector3.MoveTowards(transform.position, , 5f * Time.deltaTime);
        players[1].GetComponent<Unit>().moveTo(targetPosition2, null, true, false);
        yield return new WaitForSeconds(1.5f);
        gameHud.SetActive(false);
        wipeAnimator.SetTrigger("circleOut");
        yield return new WaitForSeconds(1.5f);
        state = BattleState.START;

    }

    public void lostGame() {
        spaceShow = false;
        gameHud.SetActive(false);
        if(numberOfWave > PlayerPrefs.GetInt("highScore", 0)) {
            PlayerPrefs.SetInt("HighScore", numberOfWave);
        }
        
       
        lostGameBool = true;
    }

}
