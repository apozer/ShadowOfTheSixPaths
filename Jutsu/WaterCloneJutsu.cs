using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using ThunderRoad.Manikin;
using ThunderRoad.Skill.Spell;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

namespace Jutsu
{
    public class WaterCloneJutsu : JutsuSkill
    {
        private CreatureData creatureData;
        private Material[] originalMaterials;
        private bool hit = false;
        private readonly string spellId = "WaterInit";
        private bool creatureSpawned = false;
        private Creature creature;

        internal override void CustomStartData()
        {
            SetSpellInstanceID(spellId);
            var activate = JutsuEntry.local.root.Then(() => GetSeals().HandDistance(GetActivated()) && CheckSpellType()).Do(() => JutsuEntry.local.root.Reset());
            activate
                .Then(() => !waitALittleBit && GetSeals().TigerSeal())
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
                           var ogScale = creature.gameObject.transform.localScale; // cache original scale
                            creature.gameObject.transform.localScale = new Vector3(creature.gameObject.transform.localScale.x, 0f, creature.gameObject.transform.localScale.z); //flatten creauture size
                            this.creature = creature;
                            //GameManager.local.StartCoroutine(CheckIsGrounded());
                            GameObject temporary =
                                GameObject.Instantiate(JutsuEntry.local.sound1); //Instantiate sfx object
                            temporary.transform.position =
                                creature.transform.position; // update position (Spatial audio)
                            temporary.GetComponent<AudioSource>().Play(); // Play sfx

                            creature.brain.SetState(Brain.State
                                .Idle); // Prevents brain from acting on ragdoll parts, this can be buggy if it does.
                            creature.ragdoll.SetState(Ragdoll.State
                                .Disabled); // Disables the mesh, so it cannot be affected by physics.

                            ClonePlayer.SetCreatureLooks(creature);

                            /*Loop over all renderers to update materials*/
                            foreach (Creature.RendererData data in creature.renderers)
                            {
                                if (data.renderer)
                                {
                                    Material temp =
                                        JutsuEntry.local.waterMaterial
                                            .DeepCopyByExpressionTree(); // Deep copy of material
                                    Material[] materials =
                                        data.renderer.materials
                                            .DeepCopyByExpressionTree(); // original materials on creature renderer
                                    Material[] tempArray = new Material[materials.Length]; // Water material array
                                    for (int i = 0; i < tempArray.Length; i++)
                                    {
                                        tempArray[i] = temp.DeepCopyByExpressionTree();
                                    }

                                    data.renderer.materials =
                                        tempArray
                                            .DeepCopyByExpressionTree(); // set renderer material array to deep copy of the water materials array

                                    /*Add transitioning class to creature*/
                                    creature.gameObject.AddComponent<WaterCloneActive>().Setup(data.renderer, materials,
                                        data.renderer.materials.DeepCopyByExpressionTree(), creature);
                                }
                            }

                            //Only need one of this class type to be active
                            creature.gameObject.AddComponent<WaterCloneSizing>().Setup(creature, ogScale);
                            SetActivated(false);
                        });
                    }
                }
                else
                {
                    StopJutsuActiveTimer();
                    if (!waitALittleBit && creatureSpawned)
                    {
                        GameManager.local.StartCoroutine(WaitALittleBit());
                    }
                }
                yield return null;
            }
        }

        private bool waitALittleBit = false;
        IEnumerator WaitALittleBit()
        {
            Debug.Log("waiting a few");
            waitALittleBit = true;
            yield return new WaitForSeconds(3f);
            creatureSpawned = false;
            SpellWheelReset();
            waitALittleBit = false;
        }
        IEnumerator CheckIsGrounded()
        {
            yield return new WaitUntil(() => this.creature.locomotion.isGrounded);
            creature.OnDamageEvent += OnDamageEvent;
        }
        
        /*
         * On damage event when creature is hit
         */
        private void OnDamageEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            if (!hit)
            {
                Creature creature = collisioninstance.targetCollider.GetComponentInParent<Creature>(); //hit creature
                GameManager.local.StartCoroutine(timeAfter(creature)); // async coroutine
                hit = !hit; // invert for timeAfter method
            }
        }

        IEnumerator timeAfter(Creature creature)
        {
            yield return new WaitForSeconds(0.7f); // wait 0.7 seconds
            VisualEffect vfx = JutsuEntry.local.waterVFX.GetComponentInChildren<VisualEffect>();
            VisualEffect go = Object.Instantiate(vfx);
            go.transform.position = creature.ragdoll.headPart.transform.position;
            go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y - creature.GetHeight(),
                go.transform.position.z);
            GameObject tempSound = Object.Instantiate(JutsuEntry.local.sound2);
            tempSound.GetComponent<AudioSource>().Play();
            creatureSpawned = false;
            creature.Despawn();
            hit = !hit;
        }
    }
}