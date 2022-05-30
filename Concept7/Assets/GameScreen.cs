using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameScreen : MonoBehaviour
{
    public Game Game;
    
    public GameObject LifePrefab;
    public Transform LifebarParent;

    public GameObject PauseScreen;
    public GameObject EndScreen;

    public List<GameObject> VictoryUIObjs = new List<GameObject>();
    public List<GameObject> DefeatUIObjs = new List<GameObject>();

    public TextMeshProUGUI EndScreenHeaderText;
    public TextMeshProUGUI EndScreenHeaderTextShadow;
    public TextMeshProUGUI EnemiesKilledLabel;
    public TextMeshProUGUI TimeElapsedLabel;
    public TextMeshProUGUI ScoreLabel;

    public TextMeshProUGUI QuitContinueButton;

    public void UpdateComboQueue(){

    }

    public void UpdateLives(){
        if(LifebarParent.childCount != Game.CurrentLives){
            while(LifebarParent.childCount > Game.CurrentLives){
                Destroy(LifebarParent.GetChild(0).gameObject);
            }

            while(LifebarParent.childCount < Game.CurrentLives){
                Instantiate(LifePrefab, LifebarParent);
            }
        }
    }

    public void ShowEndScreen(bool wasWin){
        List<GameObject> objsToEnable = DefeatUIObjs;
        List<GameObject> objsToDisable = VictoryUIObjs;
        EndScreenHeaderText.text = "Defeat";
        EndScreenHeaderTextShadow.text = "Defeat";
        QuitContinueButton.text = "Quit";

        if(wasWin){
            objsToEnable = VictoryUIObjs;
            objsToDisable = DefeatUIObjs;
            EndScreenHeaderText.text = "Victory";
            EndScreenHeaderTextShadow.text = "Victory";
            QuitContinueButton.text = "Continue";

            EnemiesKilledLabel.text = Game.EnemiesKilled.ToString();
            TimeElapsedLabel.text = Game.TimeElapsed.ToString();
            ScoreLabel.text = Game.Points.ToString();
        }

    }
}
