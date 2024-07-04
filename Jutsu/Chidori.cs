using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.AI.Action;
using UnityEngine.VFX;

namespace Jutsu
{
    using UnityEngine;
    using ThunderRoad;

    /**
     * Class for Chidori spell
     */
    public class Chidori : JutsuSkill
    {
        private ItemData chidoriData;
        private Item chidori;
        private VisualEffect chidoriVFX;
        private GameObject chidoriStart;
        private GameObject chidoriLoop;
        private AudioSource chidoriStartSFX;
        private AudioSource chidoriLoopSFX;
        private bool startSoundPlayed = false;
        private Damager pierce;
        private GameObject pierceGO;
        private bool chidoriStarted = false;
        private bool vfxStarted = false;
        private readonly string spellId = "LightningInit";

        internal override void CustomStartData()
        {
            SetSpellInstanceID(spellId);
            var activated = JutsuEntry.local.root.Then(() => GetSeals().HandDistance(GetActivated()) && (CheckSpellType())).Do(() => JutsuEntry.local.root.Reset());
            activated.Then(GetSeals().MonkeySeal)
                .Then(GetSeals().DragonSeal)
                .Then(GetSeals().RatSeal)
                .Then(GetSeals().BirdSeal)
                .Then(GetSeals().OxSeal)
                .Then(GetSeals().SnakeSeal)
                .Then(GetSeals().DogSeal)
                .Then(GetSeals().TigerSeal)
                .Then(GetSeals().MonkeySeal)
                .Do(() => SetActivated(true));
        }
        
        private FixedJoint joint;
        private bool hasPenetrated = false;
        private Creature penetrated;


        internal override IEnumerator JutsuStart()
        {
            yield return new WaitForSeconds(2f);
            while (true)
            {
                JutsuEntry.local.root.Update();
                if (JutsuEntry.local.root.AtEnd()) JutsuEntry.local.root.Reset();
                bool value;
                SpellWheelCheck(GetActivated());
                
                if (GetActivated())
                {
                    if (!GetJutsuTimerActivated())
                    {
                        SetJutsuTimerActivated(true);
                        SetJutsuTimerActivatedCoroutine(GameManager.local.StartCoroutine(JutsuActive()));
                    }
                    if (!chidori && !chidoriStarted)
                    {
                        chidoriStarted = true;
                        //Instantiate all Chidori GameObjects
                        //Set up references for Chidori VFX and SFX
                        Catalog.TryGetData("ChidoriItem", out chidoriData);
                        if (chidoriData != null)
                        {
                            chidoriData.SpawnAsync(item =>
                            {
                                chidori = item;
                                chidori.physicBody.useGravity = false;
                                
                                chidori.transform.position =
                                    Player.local.handRight.ragdollHand.transform.position;
                                chidori.transform.rotation =
                                    Player.local.handRight.ragdollHand.transform.rotation;
                                
                                chidori.IgnoreRagdollCollision(Player.local.creature.ragdoll);
                                
                                joint = Player.local.handRight.ragdollHand.gameObject.AddComponent<FixedJoint>();
                                joint.breakForce = Mathf.Infinity;
                                joint.breakTorque =  Mathf.Infinity;
                                joint.connectedBody = chidori.physicBody.rigidBody;
                                
                                foreach (Creature c in Creature.allActive)
                                {
                                    if (!c.isPlayer)
                                    {
                                        Player.local.creature.ragdoll.rightUpperArmPart.ragdoll.IgnoreCollision(
                                            c.ragdoll, true);
                                        Player.local.creature.handRight.ragdoll.IgnoreCollision(c.ragdoll, true);
                                    }
                                }
                                chidoriVFX = chidori.gameObject.GetComponentInChildren<VisualEffect>();
                                chidoriStart = GameObject.Instantiate(JutsuEntry.local.chidoriStartSFX);
                                chidoriStartSFX = chidoriStart.GetComponent<AudioSource>();
                                chidoriLoop = GameObject.Instantiate(JutsuEntry.local.chidoriLoopSFX);
                                chidoriLoopSFX = chidoriLoop.GetComponent<AudioSource>();
                                chidoriVFX.Stop();
                            });
                        }
                        else
                        {
                            chidoriStarted = false;
                        }

                    }

                    else if(chidori)
                    {
                        //While spell active and holding trigger
                            //Check to see if sound is played
                            if (!startSoundPlayed)
                            {
                                chidoriStartSFX.Play();
                                startSoundPlayed = true;
                            }

                            //Check to see if sound can begin looping
                            if ((startSoundPlayed && !chidoriStartSFX.isPlaying) && !chidoriLoopSFX.isPlaying)
                            {
                                chidoriLoopSFX.Play();
                            }

                            //Get SpellCaster magic transform position and set chidori to that (Adjusted some to go into the hand, rather than in the palm)
                            //Update position and rotation every frame
                            if (!vfxStarted)
                            {
                                chidoriVFX.Play();
                                vfxStarted = true;
                            }

                            if (chidori.isPenetrating)
                            {
                                penetrated = chidori.GetComponentInParent<Creature>();
                                hasPenetrated = true;
                            }
                            if (!chidori.isPenetrating && hasPenetrated)
                            {
                                GameObject.DestroyImmediate(joint);
                                chidori.transform.position =
                                    Player.local.handRight.ragdollHand.transform.position;
                                chidori.transform.rotation =
                                    Player.local.handRight.ragdollHand.transform.rotation;
                                chidori.IgnoreRagdollCollision(Player.local.creature.ragdoll);
                                if(penetrated) chidori.IgnoreRagdollCollision(penetrated.ragdoll);
                                joint = Player.local.handRight.ragdollHand.gameObject.AddComponent<FixedJoint>();
                                joint.breakForce = Mathf.Infinity;
                                joint.breakTorque =  Mathf.Infinity;
                                joint.connectedBody = chidori.physicBody.rigidBody;
                                chidori.ResetRagdollCollision();
                                chidori.IgnoreRagdollCollision(Player.local.creature.ragdoll);
                                hasPenetrated = false;
                            }
                    }
                }
                else
                {
                    //Reset all values to default while still updating Chidori position
                    if (chidori && vfxStarted)
                    {
                        if (chidoriStartSFX.isPlaying)
                        {
                            chidoriStartSFX.Stop();
                        }
                        else if (chidoriLoopSFX.isPlaying)
                        {
                            chidoriLoopSFX.Stop();
                        }
                        startSoundPlayed = false;
                        chidoriVFX.Stop();
                        chidoriStarted = false;
                        vfxStarted = false;
                        foreach (Creature c in Creature.allActive)
                        {
                            if (!c.isPlayer)
                            {
                                Player.local.creature.ragdoll.rightUpperArmPart.ragdoll
                                    .IgnoreCollision(c.ragdoll, false);
                                Player.local.creature.handRight.ragdoll.IgnoreCollision(c.ragdoll, false);
                            }
                        }
                        chidori.Despawn();
                        SetJutsuTimerActivated(false);
                    }
                }
                yield return null;
            }
        }
    }
}