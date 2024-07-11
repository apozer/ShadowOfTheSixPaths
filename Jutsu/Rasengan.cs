using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using ThunderRoad.Skill.Spell;
using UnityEngine;
using UnityEngine.VFX;

namespace Jutsu
{
    public class Rasengan : JutsuSkill
    {
        private bool activated = false;
        private bool startVfx = false;
        private ItemData rasenganData;
        private Item rasenganItem;
        private VisualEffect rasenganVFXOuter;
        private VisualEffect rasenganVFXInner;
        private bool increaseVFX = false;
        private Coroutine rasenganRun;
        private VisualEffect[] rasenganVFXArray;
        private bool innerDone = false;
        private bool outerDone = false;
        private bool spinnerDone = false;
        private bool timeActive = false;
        private bool fireActive = false;
        
        //SFX objects
        public GameObject rasenganStart;
        public AudioSource rasenganStartSFX;
        public GameObject rasenganLoop;
        public AudioSource rasenganLoopSFX;

        public bool startSoundPlayed = false;

        internal override void CustomStartData()
        {
            rasenganData = Catalog.GetData<ItemData>("RasenganItem");
        }

        bool DistanceCheck()
        {
            if(Player.local.handRight.ragdollHand) return Vector3.Distance(Player.local.handRight.ragdollHand.transform.position, Player.local.handLeft.ragdollHand.transform.position) < 0.3f;
            return false;
        }

        bool VelocityCheck()
        {
            if(Player.local.handRight.ragdollHand) return Player.local.handRight.ragdollHand.Velocity().magnitude > 1f;
            return false;
        }
        string spellId = "ChakraSpell";
        bool CheckSpellSelected()
        {
            if (Player.local.handLeft.ragdollHand.caster.spellInstance != null )
                //Player.local.handRight.ragdollHand.caster.spellInstance != null)
            {

                return Player.local.handLeft.ragdollHand.caster.spellInstance.id.Equals(spellId);
                      // Player.local.handRight.ragdollHand.caster.spellInstance.id.Equals(spellId);
            }
            

            return false;
        }

        IEnumerator TimeActive()
        {
            timeActive = true;
            yield return new WaitForSeconds(10f);
            innerDone = false;
            outerDone = false;
            activated = false;
            timeActive = false;
            colliderEnabled = false;
            if (rasenganStartSFX.isPlaying)
            {
                rasenganStartSFX.Stop();
            }
            if (rasenganLoopSFX.isPlaying)
            {
                Debug.Log("Loop effect stop");
                rasenganLoopSFX.Stop();
            }
            startSoundPlayed = false;
            rasenganItem.Despawn();
        }
        private Step root;
        private Collider activeCollider;
        private bool colliderEnabled = false;
        private bool enableFire = false;
        internal override IEnumerator JutsuStart() 
        {
            while (true)
            {
                if (!activated && !startVfx && CheckSpellSelected()  && Player.local.handLeft.ragdollHand.caster.isFiring)
                {
                    startVfx = true;
                    rasenganData.SpawnAsync(item =>
                    {
                        RagdollHand handLeft = Player.local.handLeft.ragdollHand;
                        var pos = handLeft.caster.magicSource.position + (handLeft.PalmDir.normalized * 0.05f) + (handLeft.caster.magicSource.up.normalized * 0.05f);
                        rasenganItem = item;
                        rasenganItem.IgnoreRagdollCollision(Player.local.creature.ragdoll);
                        activeCollider = rasenganItem.colliderGroups[0].colliders[0];
                        activeCollider.enabled = false;
                        rasenganItem.transform.position = pos;
                        rasenganItem.transform.rotation = handLeft.caster.magicSource.rotation;
                        rasenganVFXArray = rasenganItem.GetComponentsInChildren<VisualEffect>();
                        rasenganVFXOuter = rasenganVFXArray[0];
                        rasenganVFXInner = rasenganVFXArray[1];
                        rasenganVFXOuter.SetFloat("radius", 0f);
                        rasenganVFXOuter.SetFloat("radiusInner", 0f);
                        rasenganVFXInner.SetFloat("size", 0f);
                        rasenganItem.gameObject.AddComponent<RasenganMono>();
                        rasenganStart = GameObject.Instantiate(JutsuEntry.local.rasenganStartSFX);
                        rasenganStartSFX = rasenganStart.GetComponent<AudioSource>();
                        rasenganLoop = GameObject.Instantiate(JutsuEntry.local.rasenganLoopSFX);
                        rasenganLoopSFX = rasenganLoop.GetComponent<AudioSource>();
                        increaseVFX = true;
                    });
                }
                if (!activated && increaseVFX)
                {
                    if (!startSoundPlayed)
                    {
                        rasenganStartSFX.Play();
                        startSoundPlayed = true;
                        yield return new WaitForSeconds(1f);
                    }
                    if ((startSoundPlayed && !rasenganStartSFX.isPlaying) && !rasenganLoopSFX.isPlaying)
                    {
                        rasenganLoopSFX.Play();
                    }
                    RagdollHand handLeft = Player.local.handLeft.ragdollHand;
                    var pos = handLeft.caster.magicSource.position + (handLeft.PalmDir.normalized * 0.05f) + (handLeft.caster.magicSource.up.normalized * 0.05f);
                    rasenganItem.transform.position = pos;
                    rasenganItem.transform.rotation = handLeft.caster.magicSource.rotation;
                    if (true)
                    {
                        if (rasenganVFXInner.GetFloat("size") < 0.8f)
                        {
                            rasenganVFXInner.SetFloat("size", rasenganVFXInner.GetFloat("size") + 0.01f);
                        }
                        else spinnerDone = true;
                        if (rasenganVFXOuter.GetFloat("radius") < 1f)
                        {
                            rasenganVFXOuter.SetFloat("radius", rasenganVFXOuter.GetFloat("radius") + 0.01f);
                        } else innerDone = true;
                        if (rasenganVFXOuter.GetFloat("radiusInner") > -1f)
                        {
                            rasenganVFXOuter.SetFloat("radiusInner", rasenganVFXOuter.GetFloat("radiusInner") - 0.01f);
                        }
                        else outerDone = true;
                        
                        if (innerDone && outerDone && spinnerDone) 
                        {
                            increaseVFX = false;
                            startVfx = false;
                            activated = true;
                        }
                    }
                }

                if (activated)
                {
                    
                    //Check to see if sound is played

                    //Check to see if sound can begin looping
                    if ((startSoundPlayed && !rasenganStartSFX.isPlaying) && !rasenganLoopSFX.isPlaying)
                    {
                        rasenganLoopSFX.Play();
                    }
                    if (!colliderEnabled)
                    {
                        activeCollider.enabled = true;
                        colliderEnabled = true;
                    }

                    RagdollHand handLeft = Player.local.handLeft.ragdollHand;
                    var pos = handLeft.caster.magicSource.position + (handLeft.PalmDir.normalized * 0.05f) +
                              (handLeft.caster.magicSource.up.normalized * 0.05f);
                    if (!timeActive) GameManager.local.StartCoroutine(TimeActive());
                    if (rasenganItem)
                    {
                        rasenganItem.transform.position =
                            pos;
                        rasenganItem.transform.rotation =
                            handLeft.caster.magicSource.transform.rotation;
                    }
                }
                
                yield return null;
            }
        }

        public void SetActivated()
        {
            this.activated = false;
        }
    }
}