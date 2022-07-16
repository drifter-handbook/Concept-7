using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static LevelSelectGridPanel;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI LevelNum;
    public TextMeshProUGUI Score;

    LevelUIInfo info;

    public void Initialize(LevelUIInfo info){
        this.info = info;
        if(LevelNum != null){
            LevelNum.text = info.levelIndex+"";
        }

        if(Score != null){
            Score.gameObject.SetActive(info.points >= 0);
            Score.text = info.points +"";
        }
    }

    public void Pressed(){
        SceneManager.LoadScene("GameScene");
    }
}
