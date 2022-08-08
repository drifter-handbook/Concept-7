using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public Transform Pip1;
    public Transform Pip2;
    public Transform Pip3;
    public Sprite RImage;
    public Sprite BImage;
    public Sprite YImage;
    public Sprite EmptyImage;

    public List<WeaponData> weaponInfos;


    public Image AlchemyMoveIcon;
    public TextMeshProUGUI AlchemyMoveName;

    public GameObject SettingsPanel;

    void Update()
    {
    }

    public void UpdateAlchemyText(int r, int y, int b){
       
        WeaponData weaponData = null;
        foreach(WeaponData weaponInfo in weaponInfos){
            if(weaponInfo.r == r && weaponInfo.y == y && weaponInfo.b == b){
                weaponData = weaponInfo;
            }
        }



        if(weaponData == null){
            AlchemyMoveName.text = "";
            AlchemyMoveIcon.color = Color.clear;
            return;
        }
        

        if(weaponData.r > 0 || weaponData.y > 0 || weaponData.b > 0){
            AlchemyMoveName.text = weaponData.attackName;
            AlchemyMoveIcon.sprite = weaponData.icon;
            AlchemyMoveIcon.color = Color.white;
        }
    }


    public void AddQueueElement(WeaponType queueElement){
       Sprite newElem;
       switch(queueElement){
           case WeaponType.PRIMARYBLUE: newElem = BImage;  break;
           case WeaponType.PRIMARYRED: newElem = RImage;  break;
           case WeaponType.PRIMARYYELLOW:
           default: newElem = YImage; break;
       }
       
        bool setImage = false;
        if(!setImage && Pip1.GetComponent<Image>().sprite == EmptyImage){
            Pip1.GetComponent<Image>().sprite = newElem; 
            setImage = true;
        }

        if(!setImage && Pip2.GetComponent<Image>().sprite == EmptyImage){
            Pip2.GetComponent<Image>().sprite = newElem; 
            setImage = true;
        }

        if(!setImage && Pip3.GetComponent<Image>().sprite == EmptyImage){
            Pip3.GetComponent<Image>().sprite = newElem; 
            setImage = true;
        }

        if(!setImage){
            //shift
            Pip3.GetComponent<Image>().sprite =  Pip2.GetComponent<Image>().sprite;
            Pip2.GetComponent<Image>().sprite =  Pip1.GetComponent<Image>().sprite;
            Pip1.GetComponent<Image>().sprite =  newElem;
        }

    }

    public void ClearQueue(){
        Pip1.GetComponent<Image>().sprite = EmptyImage;
         Pip2.GetComponent<Image>().sprite = EmptyImage;
          Pip3.GetComponent<Image>().sprite = EmptyImage;
    }

    public void UpdateLives(){
        if(LifebarParent.childCount != Game.CurrentLives){
            for(int i = LifebarParent.childCount; i < Game.CurrentLives; i++){
                Instantiate(LifePrefab, LifebarParent);
            }
            
            for(int i = LifebarParent.childCount; i > Game.CurrentLives; i--){
                Destroy(LifebarParent.GetChild(0).gameObject);
            }
        }
    }

    public void ToggleSettings(){
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    public void QuitToMenu(){

    }

    public void ShowComboScreen(){
        
    }

    public void ShowEndScreen(bool wasWin){
        List<GameObject> objsToEnable = DefeatUIObjs;
        List<GameObject> objsToDisable = VictoryUIObjs;
        EndScreenHeaderText.text = "Defeat";
        EndScreenHeaderTextShadow.text = "Defeat";
        QuitContinueButton.text = "Quit";
        EndScreen.SetActive(true);

        if(wasWin){
            objsToEnable = VictoryUIObjs;
            objsToDisable = DefeatUIObjs;
            EndScreenHeaderText.text = "Victory";
            EndScreenHeaderTextShadow.text = "Victory";
            QuitContinueButton.text = "Continue";
        }

        EnemiesKilledLabel.text = Game.EnemiesKilled.ToString();
        TimeElapsedLabel.text = Game.TimeElapsed.ToString();
        ScoreLabel.text = Game.Points.ToString();

        foreach(GameObject objToEnable in objsToEnable){
            objToEnable.SetActive(true);
        }

         foreach(GameObject objToDisable in objsToDisable){
            objToDisable.SetActive(false);
        }

    }

    public void ShowPauseScreen(){
        PauseScreen.SetActive(true);
    }

    public void HidePauseScreen(){
        Game.Instance.Paused = false;
        PauseScreen.SetActive(false);
    }
}
