using System.Collections;
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
            var activated = JutsuEntry.local.root.Then(() => GetSeals().HandDistance(GetActivated()) && CheckSpellType()).Do(() => JutsuEntry.local.root.Reset());
            activated.Then(GetSeals().RamSeal)
                .Then(GetSeals().SnakeSeal)
                .Then(GetSeals().TigerSeal)
                .Do(() => SetActivated(true));
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                JutsuEntry.local.root.Update();
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
                        var playerPos = Player.currentCreature.transform.position;
                        Vector3 position = new Vector3(Random.Range(playerPos.x - 1, playerPos.x + 1), playerPos.y,
                            Random.Range(playerPos.z - 1, playerPos.z + 1));

                        creatureData =
                            (CreatureData) Player.currentCreature
                                .data.Clone(); //Clone data object, so the Player creature data isnt affected. Without a deep copy, it will update the player to act like an npc
                        creatureData.containerID = "Empty";
                        creatureData.brainId = "HumanMedium";
                        creatureData.SpawnAsync(Player.local.creature.transform.TransformPoint(0,0,2), 0, null, false, null, creature =>
                        {
                            this.creature = creature;
                            creature.OnDamageEvent += OnDamageEvent;
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
                            ClonePlayer.SetCreatureEquipment(creature); //Set the creature's equipment to match player
                            SetActivated(false);
                        });
                    }
                }

                else
                {
                    if (creatureSpawned)
                    {
                        SetJutsuTimerActivated(true);
                        StopJutsuActiveTimer();
                        SpellWheelReset();
                    }
                }

                yield return null;
            }
        }
        private void OnDamageEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            if (!hit && !damageStarted)
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
                hit = !hit;
                damageStarted = false;
        }
    }
}