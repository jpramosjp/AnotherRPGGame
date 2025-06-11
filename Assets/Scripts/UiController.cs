using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;


public class UiController : MonoBehaviour {
    public BattleSystem battleSystem;

    public TextMeshProUGUI waveText;

    public List<TextMeshProUGUI> playerNameText;
    public TextMeshProUGUI powerText;

    public GameObject imageSpace;

    public GameObject panelLost;

    public GameObject panelPause;

    public TextMeshProUGUI higScoreText;

    public TextMeshProUGUI scoreText;


    public Sprite iconSound;
    public Sprite iconSoundOff;

    public AudioSource music;

    public UnityEngine.UI.Image soundButton;

    public bool soundOn = true;



    void Start() {
        imageSpace.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        waveText.text = "Wave: " + battleSystem.numberOfWave.ToString();
        for (int i = 0; i < battleSystem.players.Count; i++) {
            playerNameText[i].text = battleSystem.players[i].GetComponent<Unit>().currentHealth + "/" + battleSystem.players[i].GetComponent<Unit>().maxHealth;
        }
        powerText.text = "Power: " + battleSystem.numberOfMana.ToString() + "/" + 10.ToString();
        imageSpace.SetActive(battleSystem.spaceShow);

        if (battleSystem.lostGameBool) {
            panelLost.SetActive(true);
            higScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
            scoreText.text = battleSystem.numberOfWave.ToString();
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            pauseGame();

        }
    }

    public void pauseGame() {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (panelPause.activeSelf) {
            panelPause.SetActive(false);
            canvasGroup.blocksRaycasts = false;
            Time.timeScale = 1f;
            return;
        }
        panelPause.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        Time.timeScale = 0f;
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleSound() {
        soundOn = !soundOn;
        if (soundOn) {
            music.Play();
            soundButton.sprite = iconSound;
            return;
        }
        music.Pause();
        soundButton.sprite = iconSoundOff;
    }
    

    public void menuGame() {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }
}
