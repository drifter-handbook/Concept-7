using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenUI : MonoBehaviour
{
   public Animator tapTextAnim;
   private Animator anim;

   void Start(){
       anim = GetComponent<Animator>();
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
}
