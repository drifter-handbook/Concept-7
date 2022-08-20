using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorUseHP : MonoBehaviour, IActorCollisionHandler
{
    public int Order => 2;

    public float startingHealth;
    public float health;
    bool ReadyToDie = false;
    public string DeathNoise = "enemy_death_med";
    private bool dyingAnim = false;
    private float dyingAnimTimer = 0;
    

    public void Initialize(StageData.Actor actor)
    {
        health = actor.Hp ?? 1;
        startingHealth = health;
    }

    void Update()
    {
        if (ReadyToDie && !dyingAnim)
        {
            Game.Instance.PlaySFX(DeathNoise, 1f, UnityEngine.Random.Range(0.5f, 1));
            Game.Instance.ShowFlyoutText("DEATH", transform.position);
            ParticleSystem deathJuice = transform.GetComponentInChildren<ParticleSystem>();
            deathJuice.transform.SetParent(null);
            if(deathJuice.gameObject.GetComponent<KillAfterSeconds>() != null){
                deathJuice.gameObject.GetComponent<KillAfterSeconds>().timerStart = true;
            }
            
            if(deathJuice != null){
                deathJuice.Play();
            }

            dyingAnim = true;
            dyingAnimTimer=0;

            // disable colliders
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }
            // call actor destroy handlers when dying anim starts rather than on actual destroy
            foreach (var handler in gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Health);
            }
        }

        if(dyingAnim){
            dyingAnimTimer += Time.deltaTime*15;
            Material mat = GetComponent<SpriteRenderer>().material;
            mat.SetFloat("_FadeOut", dyingAnimTimer);

            if(dyingAnimTimer > 5){
                 Die();
                 dyingAnim = false;
            }
        }
    }

    public void HandleCollision(GameObject other)
    {
        if (other.tag == "PlayArea")
        {
            return;
        }
        StageActor actor = GetComponent<StageActor>();
        if (actor != null)
        {
            ActorSuppressOtherUseHP suppress = other.GetComponent<ActorSuppressOtherUseHP>();
            if (suppress == null || !suppress.Classifications.Contains(actor.Classification))
            {
                if (other.tag == "PlayerWeapon")
                {
                    float damage = (float?)other.GetComponent<PlayerWeapon>()?.weaponData.damage ?? 0;
                    if (damage == 0)
                    {
                        // Debug.Log($"Warning: GameObject {other} of actor {actor.ActorType} deals no damage: {other.GetComponent<PlayerWeapon>()?.weaponData.damage}");
                    }
                    health -= (float?)other.GetComponent<PlayerWeapon>()?.weaponData.damage ?? 0;
                    Game.Instance.PlaySFX("impact", 0.1f, UnityEngine.Random.Range(0.5f, 1));
                }
                if (health <= 0)
                {
                    PrepareToDie();
                }
            }
        }
    }

    void PrepareToDie()
    {
        ReadyToDie = true;
    }

    void Die()
    {
        Game.Instance.EnemyKilled((int)startingHealth);
        StageActor actor = GetComponent<StageActor>();
        if (gameObject != null && actor != null)
        {
            Destroy(gameObject);
        }
    }
}
