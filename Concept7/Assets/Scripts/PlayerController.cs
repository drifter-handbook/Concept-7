using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private InputHandler input;
    private Game game;
    [SerializeField] private MovementController movement;
    [SerializeField] private WeaponController weapon;

    int blockWeapon;

    public int PlayerID = -1;

    Coroutine unpauseCoroutine;

    bool pausePressed;

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
        RefreshGameScreen();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // replace gamescreen reference on each scene load
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshGameScreen();
    }

    void RefreshGameScreen()
    {
        game = FindObjectOfType<Game>();
        game.NumPlayers++;
        PlayerID = game.NumPlayers;
    }

    public void EnableWeapon()
    {
        blockWeapon--;
    }
    public void DisableWeapon()
    {
        blockWeapon++;
    }


    void FixedUpdate()
    {
        if (!Game.Instance.Paused)
        {
            if (input.move.pressed || input.move.released)
                movement.ChangeDir(input.dir);

            if (input.action4.pressed)
                weapon.TryFireAlchemy();
            if (blockWeapon > 0)
            {
                return;
            }
            if (input.action1.pressed)
            {
                weapon.TryAddAlchemy(WeaponType.PRIMARYRED);
            }
            if (input.action1.down)
                weapon.Fire(WeaponType.PRIMARYRED);

            if (input.action2.pressed)
            {
                weapon.TryAddAlchemy(WeaponType.PRIMARYYELLOW);
            }
            if (input.action2.down)
                weapon.Fire(WeaponType.PRIMARYYELLOW);

            if (input.action3.pressed)
            {
                weapon.TryAddAlchemy(WeaponType.PRIMARYBLUE);
            }
            if (input.action3.down)
                weapon.Fire(WeaponType.PRIMARYBLUE);
        }
    }

    void Update()
    {
        pausePressed = input.pause.pressed;
        if (pausePressed && unpauseCoroutine == null)
        {
            Game.Instance.Paused = true;
            game.GameScreen.ShowPauseScreen();
            unpauseCoroutine = StartCoroutine(WaitForUnpause());
        }
    }

    IEnumerator WaitForUnpause()
    {
        yield return null;
        while (!pausePressed && Game.Instance.Paused) {
            yield return null;
        }
        Game.Instance.Paused = false;
        game.GameScreen.HidePauseScreen();
        yield return null;
        unpauseCoroutine = null;
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
