using System;
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
        private static System.Random random = new System.Random();

        private GameObject jutsuVFX;

        internal override void CustomStartData()
        {
            jutsuVFX = JutsuEntry.local.shadowCloneVFX.DeepCopyByExpressionTree();
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
            currentMax = 0;

        }

        private int previousValue = 0;
        int GetRandomValue()
        {
            var current = random.Next(-3, 3);
            if (previousValue == current)
            {
                return GetRandomValue();
            }
            previousValue = current;

            return current;
        }
        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                GetRoot().Update();
                SpellWheelCheck(true, !GetActivated());
                if (GetActivated())
                {
                    if (!GetJutsuTimerActivated())
                    {
                        Debug.Log("Started timer and activated true");
                        SetJutsuTimerActivated(true);
                    }
                     if (!startedSpawningCreatures && creatures.Count == 0)
                    {
                        startedSpawningCreatures = true;
                        creatureData =
                            (CreatureData) Player.currentCreature
                                .data.Clone(); //Clone data object, so the Player creature data isnt affected. Without a deep copy, it will update the player to act like an npc
                        creatureData.containerID = "Empty";
                        creatureData.brainId = "HumanMedium";
                        List<Item> currentItems = Player.currentCreature.equipment.GetAllHolsteredItems();
                        ;
                        for (int i = Random.Range(0, 6); i < JutsuEntry.local.multiShadowCloneMax; i++)
                        {
                            var playerPos = Player.currentCreature.transform.position;
                            Vector3 position = new Vector3(playerPos.x + Random.Range(-0.5f,0.5f) , playerPos.y,
                                playerPos.z + Random.Range(-0.5f,0.5f));
                            yield return new WaitForSeconds(0.3f);
                            creatureData.SpawnAsync(Player.currentCreature.transform.TransformPoint(position),
                                Player.currentCreature.transform.rotation.eulerAngles.y, null, false, null, creature =>
                                {
                                    GameManager.local.StartCoroutine(ShadowCloneTimer(creature));
                                    creatures.Add(creature, false);
                                    creature.OnDamageEvent += OnDamageEvent;
                                    creature.gameObject.AddComponent<CloneDespawnEvent>().Setup(this.creatures);
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
                                        creature,currentItems); //Set the creature's equipment to match player
                                });

                            if (i == JutsuEntry.local.multiShadowCloneMax - 1)
                            {
                                SetActivated(false);
                            }
                        }
                    }
                }

                else
                {
                    if (prevCount != creatures.Count)
                    {
                        Debug.Log("Creature count: " + creatures.Count);
                        prevCount = creatures.Count;
                    }
                    if (creatures.Count <= 0 && GetJutsuTimerActivated())
                    {
                        Debug.Log("Hit creature count less than or equal to");
                        SetJutsuTimerActivated(false);
                        SpellWheelReset();
                        startedSpawningCreatures = false;
                        ResetAllRootsExcludingThis();
                    }
                }

                yield return null;
            }
        }

        private int prevCount = 0;
        IEnumerator ShadowCloneTimer(Creature creature)
        {
            yield return new WaitForSeconds(19.3f);
            if(creature) GameManager.local.StartCoroutine(timeAfter(creature, true));
        }
        private void OnDamageEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            bool damaged = true;
            var local = collisioninstance?.targetCollider?.GetComponentInParent<Creature>() ?? null;
            if (local)
            {
                if (creatures.ContainsKey(local))
                {
                    if (creatures[local] == true) return;
                    creatures[local] = true;
                    GameManager.local.StartCoroutine(timeAfter(local)); // async coroutine
                }
            }
        }

        private bool damageStarted = false;
        IEnumerator timeAfter(Creature creature, bool despawning = false)
        {
            yield return new WaitForSeconds(0.7f); // wait 0.7 seconds
            var vfxTimeAfter = jutsuVFX.DeepCopyByExpressionTree();
            vfxTimeAfter.transform.position = creature.ragdoll.targetPart.transform.position;
            Object.Instantiate(vfxTimeAfter);
            GameObject tempSound = JutsuEntry.local?.shadowCloneDeathSFX;
            var refSound = Object.Instantiate(tempSound);
            if (refSound) refSound.transform.position = creature.ragdoll.targetPart.transform.position;
            creatures.Remove(creature);
            creature.Despawn();
            if (!GetActivated()) SetActivated(false);
        }
    }

    public class CloneDespawnEvent : MonoBehaviour
    {
        private Creature creature;
        private Dictionary<Creature, bool> creatures;

        private void Start()
        {
            creature = GetComponent<Creature>();
            creature.OnDespawnEvent += OnDespawn;
        }

        private void OnDespawn(EventTime eventtime)
        {
            this.creatures.Remove(creature);
        }

        public void Setup(Dictionary<Creature, bool> creatures)
        {
            this.creatures = creatures;
        }
    }
}