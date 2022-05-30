using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("Debug Cheats")]
    public bool Invulnerable = false;

    [Header("Game Info")]
    public int CurrentLives;
    public int StartingLives = 3; //Also doubles as Max Lives
    //Queue

    [Header("Point Information")]
    public int PointsPerEnemy = 10;
    public float MaxTimePoints = 180;
    [HideInInspector] public float TimeElapsed;
    [HideInInspector] public float  EnemiesKilled;
    [HideInInspector] public float  Points;

    [Header("UI")]
    public GameScreen GameScreen;

    private GameState gameState = GameState.PLAYING;

    public void Start(){
        CurrentLives = StartingLives;
        TimeElapsed = 0;
        GameScreen.Game = this;
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
        if(CurrentLives <= 0){
            EndGame(false);
        }
        GameScreen.UpdateLives();
    }

    public void EndGame(bool wasWin){
        gameState = GameState.ENDED;
        Points = Points += (MaxTimePoints -= TimeElapsed);
        GameScreen.ShowEndScreen(wasWin);
    }
    

    public enum GameState{
        PLAYING,
        ENDED,
        PAUSED
    }

}
