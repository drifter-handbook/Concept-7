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

   public GameObject SettingsButton;
   public GameObject BackButton;
   public GameObject ExitButton;

   public GameObject SettingsPanel;
   public GameObject TitleImagePanel;
   public LevelSelectGridPanel LevelSelectGridPanel;

   public TextMeshProUGUI ChapterStoryText;
   public List<ChapterNode> ChapterNodes;
    
   private List<LevelUIInfo> allLevels;
   private TitleScreenState state = TitleScreenState.INTRO;

    public enum TitleScreenState{
        INTRO,
        TITLE_SCREEN,
        GRID,
        CHAPTER_LAYOUT,
    }

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
       if(state == TitleScreenState.INTRO){
            
            tapTextAnim.SetTrigger("End");
            ToTitle();
       }
   }

   public void ToTitle(){
    
     state = TitleScreenState.TITLE_SCREEN;
     anim.SetTrigger("ToTitleOptions");
     SettingsButton.SetActive(true);
     ExitButton.SetActive(true);
     BackButton.SetActive(false);
     TitleImagePanel.SetActive(true);
   }

   public void ToGame(int gameMode){
     //continue game, new game, multiplayer, etc.
     //but for now, just load the game
    
   }

    public void ToGrid(){
        anim.SetTrigger("ToGrid");
        SettingsButton.SetActive(true);
        ExitButton.SetActive(true);
        BackButton.SetActive(true);
        state = TitleScreenState.GRID;
    }

   public void ToggleSettings(){
       SettingsPanel.SetActive(!SettingsPanel.activeSelf);
   }

   public void PopulateLevelGrid(){
        LevelSelectGridPanel.SetupUI();
   }

   public void ToChapter(int chapterNum){
        state = TitleScreenState.CHAPTER_LAYOUT;
        anim.SetTrigger("ToChapterScreen");
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

        SettingsButton.SetActive(true);
        ExitButton.SetActive(true);
        BackButton.SetActive(true);
        TitleImagePanel.SetActive(false);
   }


   public void BackButtonPressed(){
        switch(state){
            case TitleScreenState.CHAPTER_LAYOUT:
            case TitleScreenState.GRID:
                ToTitle();
            break;
            default: break;
        }
   }

   public void ExitButtonPressed(){
      Application.Quit();
   }
}
