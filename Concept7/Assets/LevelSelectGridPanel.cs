using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectGridPanel : MonoBehaviour
{

    public GameObject LevelSelectPrefab;
    public Transform Grid;
  
    List<LevelUIInfo> levelInfos;

    public struct LevelUIInfo{
        public int levelIndex; //which level it is in a set
        public int chapterIndex; //which chapter the level is in
        public float points; //how many points the user has earned in the level
        public bool boss; //whether or not this is a boss level
    }

    public void Initialize(List<LevelUIInfo> levelInfos){
        this.levelInfos = levelInfos;
    }

    public void SetupUI(){
         StartCoroutine(SetupUICoroutine());
    }

    IEnumerator SetupUICoroutine()
    {
        foreach(LevelUIInfo level in levelInfos){
            LevelButton levelBtn = Instantiate(LevelSelectPrefab, Grid).GetComponent<LevelButton>();
            levelBtn.Initialize(level);
            //Initialize level select prefab
            if(level.levelIndex % 5 == 0 && level.levelIndex > 0){
                yield return new WaitForSeconds(0.1f);
            } else {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

}
