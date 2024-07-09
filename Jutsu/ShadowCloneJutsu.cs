using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

namespace Jutsu
{
    using ThunderRoad;
    using UnityEngine;
    public class ShadowCloneJutsu : JutsuSkill
    {
        private GameObject vfx;
        private GameObject sfxSpawn;
        private GameObject sfxDeath;
        private string spellInstanceId = "YangInit";
        private bool creatureSpawned = false;
        private CreatureData creatureData;
        private Creature creature;
        private bool hit = false;

        internal override void CustomStartData()
        {
            damageStarted = false;
            SetSpellInstanceID(spellInstanceId);
            var activated = GetRoot().Then(() => GetSeals().HandDistance(GetActivated()) && CheckSpellType());
            activated.Then(GetSeals().RamSeal)
                .Then(GetSeals().SnakeSeal)
                .Then(GetSeals().TigerSeal)
                .Do(() => SetActivated(true));
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
                     if (!creatureSpawned)
                    {
                        creatureSpawned = true;

                        creatureData =
                            (CreatureData) Player.currentCreature
                                .data.Clone(); //Clone data object, so the Player creature data isnt affected. Without a deep copy, it will update the player to act like an npc
                        creatureData.containerID = "Empty";
                        creatureData.brainId = "HumanMedium";
                        List<Item> currentItems = Player.currentCreature.equipment.GetAllHolsteredItems();
                        creatureData.SpawnAsync(Player.local.creature.transform.TransformPoint(0,0,2.5f), Player.local.creature.transform.rotation.eulerAngles.y, null, false, null, creature =>
                        {
                            GameManager.local.StartCoroutine(ShadowCloneTimer(creature));
                            this.creature = creature;
                            creature.OnDamageEvent += OnDamageEvent;
                            creature.OnDespawnEvent += OnDespawnEvent;
                            vfx = JutsuEntry.local.shadowCloneVFX.DeepCopyByExpressionTree();
                            vfx.transform.position = creature.ragdoll.targetPart.transform.position;
                            Object.Instantiate(vfx);
                            GameObject spawnSound = JutsuEntry.local.shadowCloneSpawnSFX;
                            spawnSound.transform.position = creature.ragdoll.targetPart.transform.position;
                            Object.Instantiate(spawnSound);
                            /* Check for HitReaction class, set the target of creature to the closest enemy*/
                            if (creature.GetComponent<BrainModuleHitReaction>() is BrainModuleHitReaction reaction)
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
                                            shortestDistance = (creature.transform.position - active.transform.position)
                                                .sqrMagnitude;
                                            selected = active;
                                            started = true;
                                        }
                                        else
                                        {
                                            if ((creature.transform.position - active.transform.position).sqrMagnitude <
                                                shortestDistance)
                                            {
                                                selected = active;
                                                shortestDistance = (creature.transform.position - active.transform.position)
                                                    .sqrMagnitude;
                                            }
                                        }
                                    }
                                }
                                reaction.lastHitSource = selected;
                            }
                            ClonePlayer.SetCreatureLooks(creature); //Set creature looks to match player
                            ClonePlayer.SetCreatureEquipment(creature, currentItems); //Set the creature's equipment to match player
                            SetActivated(false);
                        });
                    }
                }

                else
                {
                    if (this.creature && GetJutsuTimerActivated())
                    {
                        SetJutsuTimerActivated(false);
                        StopJutsuActiveTimer(true);
                        ResetAllRootsExcludingThis();
                    }
                }

                yield return null;
            }
        }

        IEnumerator ShadowCloneTimer(Creature creature)
        {
            yield return new WaitForSeconds(19.3f);
            if(creature) GameManager.local.StartCoroutine(timeAfter(creature));
        }

        private void OnDespawnEvent(EventTime eventtime)
        {
            SetJutsuTimerActivated(false);
            StopJutsuActiveTimer(true);
        }

        private void OnDamageEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            if (!damageStarted)
            {
                damageStarted = true;
                GameManager.local.StartCoroutine(timeAfter(creature)); // async coroutine
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
                if (refSound) refSound.transform.position = this.creature.ragdoll.targetPart.transform.position;
                creatureSpawned = false;
                creature.Despawn();
                SetActivated(false);
                damageStarted = false;
        }
    }
}