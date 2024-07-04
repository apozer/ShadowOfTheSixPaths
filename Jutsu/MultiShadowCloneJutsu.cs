using System.Collections;
using System.Collections.Generic;

namespace Jutsu
{
    using UnityEngine;
    using ThunderRoad;
    public class MultiShadowCloneJutsu : JutsuSkill
    {
          private GameObject vfx;
        private GameObject sfxSpawn;
        private GameObject sfxDeath;
        private string spellInstanceId = "YangInit";
        private bool startedSpawningCreatures = false;
        private CreatureData creatureData;
        private Creature creature;
        private bool hit = false;
        private Dictionary<Creature, bool> creatures = new Dictionary<Creature, bool>();
        private int currentMax = 0;

        internal override void CustomStartData()
        {
            damageStarted = false;
            SetSpellInstanceID(spellInstanceId);
            var activated = GetRoot().Then(() =>
                GetSeals().HandDistance(GetActivated()) && CheckSpellType() && creatures.Count == 0);
            activated.Then(GetSeals().HareSeal)
                .Then(GetSeals().SnakeSeal)
                .Then(GetSeals().TigerSeal)
                .Do(() =>
                {
                    SetActivated(true);
                });
        }

        internal override void CustomEndData()
        {
            creatures = new Dictionary<Creature, bool>();
            
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                GetRoot().Update();
                SpellWheelCheck(true);
                if (GetActivated())
                {
                    if (!GetJutsuTimerActivated())
                    {
                        Debug.Log("Started timer and activated true");
                        SetJutsuTimerActivated(true);
                        SetJutsuTimerActivatedCoroutine(GameManager.local.StartCoroutine(JutsuActive()));
                    }
                     if (!startedSpawningCreatures && creatures.Count == 0)
                    {
                        startedSpawningCreatures = true;
                        var playerPos = Player.currentCreature.transform.position;
                        Vector3 position = new Vector3(Random.Range(playerPos.x - 2, playerPos.x + 2), playerPos.y,
                            Random.Range(playerPos.z - 2, playerPos.z + 2));

                        creatureData =
                            (CreatureData) Player.currentCreature
                                .data.Clone(); //Clone data object, so the Player creature data isnt affected. Without a deep copy, it will update the player to act like an npc
                        creatureData.containerID = "Empty";
                        creatureData.brainId = "HumanMedium";
                        for (int i = Random.Range(0, 6); i < JutsuEntry.local.multiShadowCloneMax; i++)
                        {
                            yield return new WaitForSeconds(0.5f);
                            creatureData.SpawnAsync(Player.local.creature.transform.TransformPoint(position),
                                Player.local.creature.transform.rotation.eulerAngles.y, null, false, null, creature =>
                                {
                                    if (currentMax == 0)
                                    {
                                        currentMax = JutsuEntry.local.multiShadowCloneMax - i;
                                    }
                                    creatures.Add(creature, false);
                                    creature.OnDamageEvent += OnDamageEvent;
                                    vfx = JutsuEntry.local.shadowCloneVFX.DeepCopyByExpressionTree();
                                    vfx.transform.position = creature.ragdoll.targetPart.transform.position;
                                    Object.Instantiate(vfx);
                                    GameObject spawnSound = JutsuEntry.local.shadowCloneSpawnSFX;
                                    spawnSound.transform.position = creature.ragdoll.targetPart.transform.position;
                                    Object.Instantiate(spawnSound);
                                    /* Check for HitReaction class, set the target of creature to the closest enemy*/
                                    if (creature.GetComponent<BrainModuleHitReaction>() is BrainModuleHitReaction
                                        reaction)
                                    {
                                        bool started = false;
                                        float shortestDistance = 0f;
                                        Creature selected = null;
                                        foreach (var active in Creature.allActive)
                                        {
                                            if (!active.isPlayer)
                                            {
                                                if (!started)
                                                {
                                                    shortestDistance = (creature.transform.position -
                                                                        active.transform.position)
                                                        .sqrMagnitude;
                                                    selected = active;
                                                    started = true;
                                                }
                                                else
                                                {
                                                    if ((creature.transform.position - active.transform.position)
                                                        .sqrMagnitude <
                                                        shortestDistance)
                                                    {
                                                        selected = active;
                                                        shortestDistance = (creature.transform.position -
                                                                            active.transform.position)
                                                            .sqrMagnitude;
                                                    }
                                                }
                                            }
                                        }

                                        reaction.lastHitSource = selected;
                                    }

                                    ClonePlayer.SetCreatureLooks(creature); //Set creature looks to match player
                                    ClonePlayer.SetCreatureEquipment(
                                        creature); //Set the creature's equipment to match player
                                });
                            
                            if(i == JutsuEntry.local.multiShadowCloneMax - 1) SetActivated(false);
                        }
                    }
                }

                else
                {
                    if (creatures.Count == currentMax - 1 && GetJutsuTimerActivated())
                    {
                        SetJutsuTimerActivated(false);
                        StopJutsuActiveTimer(true);
                    }
                }

                yield return null;
            }
        }
        private void OnDamageEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            var local = collisioninstance.targetCollider.GetComponentInParent<Creature>();
            var damaged = true;
            if (creatures[local] != null)
            {
                damaged = creatures[local];
            }

            if (!damaged)
            {
                creatures[local] = true;
                GameManager.local.StartCoroutine(timeAfter(local)); // async coroutine
            }
        }

        private bool damageStarted = false;
        IEnumerator timeAfter(Creature creature)
        {
                yield return new WaitForSeconds(0.7f); // wait 0.7 seconds
                var vfxTimeAfter = JutsuEntry.local.shadowCloneVFX;
                vfxTimeAfter.transform.position = creature.ragdoll.targetPart.transform.position;
                Object.Instantiate(vfxTimeAfter);
                GameObject tempSound = JutsuEntry.local?.shadowCloneDeathSFX;
                var refSound = Object.Instantiate(tempSound);
                if (refSound) refSound.transform.position = creature.ragdoll.targetPart.transform.position;
                startedSpawningCreatures = false;
                creatures.Remove(creature);
                creature.Despawn();
                SetActivated(false);
        }
    }
}