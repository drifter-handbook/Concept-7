using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static LevelSelectGridPanel;


public class TitleScreenUI : MonoBehaviour
{
   public Animator tapTextAnim;
   private Animator anim;

   public GameObject SettingsPanel;
   public GameObject TitleImagePanel;
   public LevelSelectGridPanel LevelSelectGridPanel;

   public TextMeshProUGUI ChapterStoryText;
   public List<ChapterNode> ChapterNodes;
    
   private List<LevelUIInfo> allLevels;
   


   void Start(){
       anim = GetComponent<Animator>();

        //Initialize level select Grid Panel
        if(LevelSelectGridPanel != null){
            //fake
             allLevels = new List<LevelUIInfo>();
            for(int i = 0; i< 20; i++){
                LevelUIInfo lv = new LevelUIInfo();
                lv.levelIndex = i;
                lv.chapterIndex = i/5;
                lv.boss = i%5 == 0;
                lv.points = Random.Range(1,99999);
                allLevels.Add(lv);
            }
            LevelSelectGridPanel.Initialize(allLevels);
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

   public void ToChapter(int chapterNum){
        List<LevelUIInfo> levels = new List<LevelUIInfo>();
        foreach(LevelUIInfo level in allLevels){
            if(level.chapterIndex == chapterNum){
                levels.Add(level);
            }
        }

        //Setup chapter text
        ChapterStoryText.text = "Chapter "+ chapterNum + ": " + "And here's where the story would go";

        //Setup chapter nodes
        for(int i = 0; i < 5; i++){
            ChapterNodes[i].Initialize(levels[i]);
        }

        TitleImagePanel.SetActive(false);
   }
}
