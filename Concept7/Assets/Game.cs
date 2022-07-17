using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;

    [Header("Debug Cheats")]
    public bool Invulnerable = false;
    public bool ContinueWithNoLives = true;
    public bool StartStage = true;
    public bool OverrideLevelID = false;

    [Header("Game Info")]
    public int CurrentLives;
    public int StartingLives = 3; //Also doubles as Max Lives
    [HideInInspector] public int NumPlayers = 0;
    //Queue

    [Header("Point Information")]
    public int PointsPerEnemy = 10;
    public float MaxTimePoints = 180;
    [HideInInspector] public float TimeElapsed;
    [HideInInspector] public float EnemiesKilled;
    [HideInInspector] public float Points;

    [Header("UI")]
    public GameScreen GameScreen;
    
    [Header("SFX")]
    public SoundLookup SoundLookup;
    public Transform MusicBox; //BGM, holds the spawned sfx
    public GameObject SFXPrefab;

    private GameState gameState = GameState.PLAYING;
    private PlayerController ClientPlayer; //the user's player
   
    public int LevelID = 0;

    public GameObject FlyoutTextPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Start(){
        CurrentLives = StartingLives;
        TimeElapsed = 0;
        GameScreen.Game = this;
        GameScreen.UpdateLives();
        if (StageEditor.Instance == null)
        {
            Load();
        }
    }

    public void Load(){
        if (OverrideLevelID)
        {
            StageDirector.StartStage(LevelID);
        } else {
            //loading in from title screen (the actual proper way)
            LevelID = PlayerPrefs.GetInt("CurrentLevel");
            StageDirector.StartStage(LevelID);
        }
    }

    public void Update(){
        if(gameState == GameState.ENDED) return;
        if(gameState == GameState.PLAYING){
            TimeElapsed += Time.deltaTime;
        }
    }

    public void EnemyKilled(){
        EnemiesKilled++;
        Points += PointsPerEnemy;
    }

    public void LivesChanged(int num){
        if(Invulnerable) return;
        CurrentLives = Mathf.Clamp(CurrentLives+num, 0, StartingLives);
        if(CurrentLives <= 0 && !ContinueWithNoLives){
            EndGame(false);
        }
        GameScreen.UpdateLives();
    }

    public void EndGame(bool wasWin){
        gameState = GameState.ENDED;
        if(wasWin){
            Points = Points += (MaxTimePoints -= TimeElapsed);
            //Save level progress
            if(SaveData.Instance.FurthestCompletedLevel < LevelID){
                SaveData.Instance.FurthestCompletedLevel = LevelID;
            }
            SaveData.Instance.Save();

            //Save points
            if(!PlayerPrefs.HasKey("Level"+LevelID+"Points") || PlayerPrefs.GetFloat("Level"+LevelID+"Points") < Points){
                PlayerPrefs.SetFloat("Level"+LevelID+"Points", Points);
            }
        }
        GameScreen.ShowEndScreen(wasWin);
    }
    
    public void PlaySFX(string clip){
        PlaySFX(clip, -1, float.MaxValue);
    }
    
    public void PlaySFX(string clip, float volume){
        PlaySFX(clip, volume, float.MaxValue);
    }

    
    public void PlaySFX(string clip, float volume, float pitch){
        GameObject obj = Instantiate(SFXPrefab, MusicBox);
        obj.name = "SFX - " + clip;
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        SoundLookup.Sound snd = SoundLookup.GetSound(clip);
        if (snd == null)
        {
            Debug.Log($"Warning: Sound Effect {clip} is not registered.");
            return;
        }
        audioSource.clip = snd.clip;

        if(volume > 0){
            audioSource.volume = volume * snd.volume;
        }

        if(pitch < float.MaxValue){
            audioSource.pitch = pitch;
        }

        audioSource.Play();
        Destroy(audioSource.gameObject,  audioSource.clip.length * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
    }


    public enum GameState{
        PLAYING,
        ENDED,
        PAUSED
    }

    public void ShowFlyoutText(string text, Vector3 pos){
        GameObject flyout = Instantiate(FlyoutTextPrefab);
        flyout.transform.position = pos;
        flyout.GetComponent<FlyoutText>().Initialize(text);
    }

}
