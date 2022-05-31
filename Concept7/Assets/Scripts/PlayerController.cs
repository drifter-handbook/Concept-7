using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputHandler input;
    private Game game;
    [SerializeField] private MovementController movement;
    [SerializeField] private WeaponController weapon;

    public int PlayerID = -1;

    public static PlayerController Instance { get; private set; }
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

        game = FindObjectOfType<Game>();
        game.NumPlayers++;
        PlayerID = game.NumPlayers;

        weapon.Initialize(game.GameScreen);
    }

    void FixedUpdate()
    {
        if (input.move.pressed || input.move.released)
            movement.ChangeDir(input.dir);

        if (input.action1.pressed){
            weapon.TryAddAlchemy(WeaponType.PRIMARYRED);
        }
        if (input.action1.down)
            weapon.Fire(WeaponType.PRIMARYRED);

        if (input.action2.pressed){
            weapon.TryAddAlchemy(WeaponType.PRIMARYYELLOW);
        }
        if (input.action2.down)
            weapon.Fire(WeaponType.PRIMARYYELLOW);

        if (input.action3.pressed){
            weapon.TryAddAlchemy(WeaponType.PRIMARYBLUE);
        }
        if (input.action3.down)
            weapon.Fire(WeaponType.PRIMARYBLUE);
            
        if (input.action4.pressed)
            weapon.TryFireAlchemy();
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
