using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.VFX;

namespace Jutsu
{
    public class LightningCloneJutsu : JutsuSkill
    {
        private string spellInstanceId = "LightningInit";
        private GameObject vfx;
        private bool creatureSpawned = false;
        private CreatureData creatureData;
        private Creature creature;
        private bool disabled = false;

        internal override void CustomStartData()
        {
            disabled = true;
            damageStarted = false;
            SetSpellInstanceID(spellInstanceId);
            var activated = GetRoot().Then(() => GetSeals().HandDistance(GetActivated()) && CheckSpellType());

            activated.Then(GetSeals().RamSeal)
                .Then(GetSeals().TigerSeal)
                .Then(GetSeals().SnakeSeal)
                .Then(GetSeals().MonkeySeal)
                .Do(() => SetActivated(true));
        }

        internal override IEnumerator JutsuStart()
        {
            while (!disabled)
            {
                GetRoot().Update();
                SpellWheelCheck(true);
                if (GetActivated())
                {
                    if (!GetJutsuTimerActivated())
                    {
                        SetJutsuTimerActivated(true);
                        SetJutsuTimerActivatedCoroutine(GameManager.local.StartCoroutine(JutsuActive()));
                    }

                    if (!creatureSpawned)
                    {
                        creatureSpawned = true;

                        creatureData =
                            (CreatureData)Player.currentCreature
                                .data
                                .Clone(); //Clone data object, so the Player creature data isnt affected. Without a deep copy, it will update the player to act like an npc
                        creatureData.containerID = "Empty";
                        creatureData.brainId = "HumanMedium";
                        GameManager.local.StartCoroutine(LightningEffectTimer(vfx));
                    }
                }
                yield return null;
            }
        }

        IEnumerator LightningEffectTimer(GameObject vfx)
        {
            VisualEffect effect = null;
            Creature creatureLocal = null;
            creatureData.SpawnAsync(Player.local.creature.transform.TransformPoint(0, 0, 2.5f),
                Player.local.creature.transform.rotation.eulerAngles.y, null, false, null, creature =>
                {
                    creatureLocal = creature;
                    creatureLocal.ragdoll.SetState(Ragdoll.State.NoPhysic);
                    foreach (Creature.RendererData data in creature.renderers)
                    {
                        if (data.renderer)
                        { 
                            // Deep copy of material
                            Material[] materials =
                                data.renderer.materials
                                    .DeepCopyByExpressionTree(); // original materials on creature renderer
                            Material[] tempArray = new Material[materials.Length]; // Water material array
                            for (int i = 0; i < tempArray.Length; i++)
                            {
                                if (tempArray[i]?.color == null) continue;
                                tempArray[i].color = new Color(tempArray[i].color.r, tempArray[i].color.g, tempArray[i].color.b, 0f);
                            }

                            data.renderer.materials =
                                tempArray
                                    .DeepCopyByExpressionTree(); // set renderer material array to deep copy of the water materials array

                            /*Add transitioning class to creature*/
                            creature.gameObject.AddComponent<WaterCloneActive>().Setup(data.renderer, materials,
                                data.renderer.materials.DeepCopyByExpressionTree(), creature);
                        }
                    }
                    vfx = JutsuEntry.local.lightningCloneVFX.DeepCopyByExpressionTree();
                    vfx.transform.position = creatureLocal.ragdoll.targetPart.transform.position;
                    vfx.transform.rotation = creatureLocal.ragdoll.transform.rotation;
                    effect = Object.Instantiate(vfx).GetComponentInChildren<VisualEffect>();
                });

            yield return new WaitUntil(() => creatureLocal != null);
            yield return new WaitForSeconds(0.5f);
            creatureLocal.Despawn();
            effect.Stop();
            List<Item> currentItems = Player.currentCreature.equipment.GetAllHolsteredItems();
                        creatureData.SpawnAsync(Player.local.creature.transform.TransformPoint(0, 0, 2.5f),
                            Player.local.creature.transform.rotation.eulerAngles.y, null, false, null, creature =>
                            {
                                GameManager.local.StartCoroutine(LightningCloneTimer(creature));
                                this.creature = creature;
                                creature.OnDamageEvent += OnDamageEvent;
                                creature.OnDespawnEvent += OnDespawnEvent;
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
                                ClonePlayer.SetCreatureEquipment(creature,
                                    currentItems); //Set the creature's equipment to match player
                                SetActivated(false);
                            });
        }
        
        IEnumerator LightningCloneTimer(Creature creature)
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
            var vfxTimeAfter = JutsuEntry.local.lightningCloneVFX.DeepCopyByExpressionTree();
            VisualEffect effect = vfxTimeAfter.gameObject.GetComponentInChildren<VisualEffect>();
            vfxTimeAfter.transform.position = creature.ragdoll.targetPart.transform.position;
            Object.Instantiate(vfxTimeAfter);
            creatureSpawned = false;
            creature.Despawn();
            SetActivated(false);
            damageStarted = false;
            yield return new WaitForSeconds(0.5f);
            effect.Stop();
        }
    }
}