using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelSelectGridPanel;


public class TitleScreenUI : MonoBehaviour
{
   public Animator tapTextAnim;
   private Animator anim;

   public GameObject SettingsPanel;
   public LevelSelectGridPanel LevelSelectGridPanel;

     


   void Start(){
       anim = GetComponent<Animator>();

        //Initialize level select Grid Panel
        if(LevelSelectGridPanel != null){
            //fake
            List<LevelUIInfo> levels = new List<LevelUIInfo>();
            for(int i = 0; i< 20; i++){
                LevelUIInfo lv = new LevelUIInfo();
                lv.levelIndex = i;
                lv.chapterIndex = i/5;
                lv.boss = i%5 == 0;
                lv.points = Random.Range(1,99999);
                levels.Add(lv);
            }
            LevelSelectGridPanel.Initialize(levels);
        }


   }

   public void TapToStart(){
       anim.SetTrigger("ToTitleOptions");
       tapTextAnim.SetTrigger("End");

        /*
        If save file exists, show continue button
        */

   }

   public void ToGame(int gameMode){
     //continue game, new game, multiplayer, etc.
     //but for now, just load the game
    SceneManager.LoadScene("GameScene");
   }

    public void ToGrid(){
        anim.SetTrigger("ToGrid");
    }


   public void ToggleSettings(){
       SettingsPanel.SetActive(!SettingsPanel.activeSelf);
   }

   public void PopulateLevelGrid(){
        LevelSelectGridPanel.SetupUI();
   }
}
