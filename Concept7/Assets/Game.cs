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

        //

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

}
