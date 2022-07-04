using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static LevelSelectGridPanel;
using TMPro;

public class ChapterNode : MonoBehaviour
{
    public Image NodeImage;
    public Color LockedColor;
    public Color CurrentColor;
    public Color CompletedColor;
    public TextMeshProUGUI Score;
    

    public GameObject CurrentVFX;

    private LevelUIInfo levelInfo; 
    private LevelState state;

    public enum LevelState{
        LOCKED,
        CURRENT,
        COMPLETED
    }


    public void Initialize(LevelUIInfo levelInfo){

        if(!PlayerPrefs.HasKey("FurthestCompletedLevel")){
            PlayerPrefs.SetInt("FurthestCompletedLevel", 0);
        }

        int furthestCompletedLevel = PlayerPrefs.GetInt("FurthestCompletedLevel");
        if(furthestCompletedLevel <= levelInfo.levelIndex){
            state = LevelState.COMPLETED;
            NodeImage.color = CompletedColor;
            Score.gameObject.SetActive(true);
            Score.text = levelInfo.points+"";
            CurrentVFX.SetActive(false);

        } else if (furthestCompletedLevel == levelInfo.levelIndex -1 ){
            state = LevelState.CURRENT;
            NodeImage.color = CurrentColor;
            Score.gameObject.SetActive(false);
            CurrentVFX.SetActive(true);

        } else {
            state = LevelState.LOCKED;
            NodeImage.color = LockedColor;
            Score.gameObject.SetActive(false);
            CurrentVFX.SetActive(true);
        }
    }

    public void Select(){
        if(state == LevelState.CURRENT || state == LevelState.COMPLETED){
            SceneManager.LoadScene("GameScene");
            //Select which yaml to pass
        }
    }

}
