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
    public float TargetTime = 5000;
    private float TimeElapsed;
    private int EnemiesKilled;
    private int Points;

    [Header("UI")]
    public GameScreen gameScreen;

    private GameState gameState = GameState.PLAYING;

    public void Start(){
        CurrentLives = StartingLives;
        TimeElapsed = 0;
    }

    public void Update(){
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
    }

    public void EndGame(bool wasWin){
        gameState = wasWin ? GameState.VICTORY : GameState.DEFEAT;
    }

    public enum GameState{
        PLAYING,
        VICTORY,
        DEFEAT,
        PAUSED
    }

}
