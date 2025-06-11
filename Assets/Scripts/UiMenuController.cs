
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiMenuController : MonoBehaviour {

    public GameObject panelTutorial;

    public void playGame() {
        SceneManager.LoadScene("Game");
    }

    public void exitGame() {
        Application.Quit();
    }


    public void toggleTutorial() {
        if(panelTutorial.activeSelf) {
            panelTutorial.SetActive(false);
        } else {
            panelTutorial.SetActive(true);
        }
    }

}
