using System;
using System.Collections;
using System.Collections.Generic;
using System.Speech.Recognition;
using UnityEngine;
using ThunderRoad;
using UnityEngine.VFX;

namespace Jutsu.Susanoo
{
    public class SusanooSkill : JutsuSkill
    {
        private GameObject susanooType;
        SpeechRecognitionEngine _speechRecognizer;
        private bool activateSusanoo;
        private Material shader;
        private GameObject susanoo;
        private Collider groundCollider;
        private VisualEffect vfx;
        private Coroutine timer;
        private bool skilledEnabled = false;

        GameObject GetSusanooKind()
        {
            switch (MangekyoTracker.local.mangekyoAbility)
            {
                case "SasukeSharinganSkills":
                    return JutsuEntry.local.sasukeSusanooRibcage.DeepCopyByExpressionTree();
                default:
                    return null;
            }
        }

        internal override void CustomStartData()
        {
            skilledEnabled = true;
            susanooType = GetSusanooKind();
            _speechRecognizer = new SpeechRecognitionEngine();
            Choices susanoo = new Choices();
            susanoo.Add("Soosahno");
            Grammar servicesGrammar = new Grammar(new GrammarBuilder(susanoo));
            _speechRecognizer.RequestRecognizerUpdate();
            _speechRecognizer.LoadGrammarAsync(servicesGrammar);
            _speechRecognizer.SetInputToDefaultAudioDevice();
            _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            _speechRecognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }
        
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.93f)
            {
                if (e.Result.Text == "Soosahno" && !susanoo && skilledEnabled)
                {
                    if (Physics.Raycast(new Vector3(0,1,0), Vector3.down, out RaycastHit hit,float.MaxValue, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore))
                    {
                        Debug.Log(hit.collider.name);
                        groundCollider = hit.collider;
                    }
                    activateSusanoo = true;
                }
            }
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (activateSusanoo)
                {
                    if (!susanoo)
                    {
                        susanoo = GameObject.Instantiate(susanooType);
                        StopCollision();
                        susanoo.AddComponent<CollisionTest>().Setup(susanoo.GetComponent<Collider>());
                        shader = susanoo.gameObject.GetComponentInChildren<MeshRenderer>().material;
                        shader.SetFloat("_cutOffHeight", -1f);
                        vfx = susanoo.GetComponentInChildren<VisualEffect>();
                        vfx.Stop();
                    }
                    else
                    {
                        susanoo.gameObject.transform.position = Player.local.creature.transform.position;
                        susanoo.gameObject.transform.rotation = Player.local.creature.transform.rotation;
                    }

                    ChangeValuesOverTime();
                }
                if (susanoo && !activateSusanoo)
                {
                    susanoo.gameObject.transform.position = Player.local.creature.transform.position;
                    susanoo.gameObject.transform.rotation = Player.local.creature.transform.rotation;
                }

                yield return null;
            }
        }

        private bool started = false;
        void ChangeValuesOverTime()
        {
            var cutoff = shader.GetFloat("_cutOffHeight");
            if (cutoff < 3f)
            {
                if (!started)
                {
                    started = true;
                    vfx.Play();
                }
                shader.SetFloat("_cutOffHeight", (cutoff +  0.01f));
            }
            else
            {
                activateSusanoo = false;
                timer = GameManager.local.StartCoroutine(Timer());
            }
        }

        bool ReverseValuesOverTime()
        {
            var cutoff = shader.GetFloat("_cutOffHeight");
            Debug.Log("Cutoff: " + cutoff);
            if (cutoff > -1f)
            {
                if (started)
                {
                    started = false;
                    vfx.Stop();
                }
                shader.SetFloat("_cutOffHeight", (cutoff - 0.01f));
            }
            else
            {
                GameObject.Destroy(susanoo);
                return false;
            }
            
            return true;
        }

        void StopCollision()
        {
            var collider = susanoo.GetComponent<Collider>();
            Physics.IgnoreCollision(Player.local.locomotion.capsuleCollider, collider, true);
            Physics.IgnoreCollision(Player.local.GetComponentInChildren<Collider>(), collider, true);
            Physics.IgnoreCollision(groundCollider, collider, true);
            Player.local.creature.ragdoll.IgnoreCollision(collider, true);
            Player.local.creature.equipment.OnHeldItemsChangeEvent += HeldItemEvent;
            if (Player.local.handRight.ragdollHand.grabbedHandle != null && Player.local.handRight.ragdollHand.grabbedHandle.item is Item itemRight)
            {
                itemRight.IgnoreColliderCollision(collider);
                itemsToIgnore.Add(itemRight);
            }

            if (Player.local.handLeft.ragdollHand.grabbedHandle != null && Player.local.handLeft.ragdollHand.grabbedHandle.item is Item itemLeft)
            {
                itemLeft.IgnoreColliderCollision(collider);
                itemsToIgnore.Add(itemLeft);
            }

            //AddEvents();
        }

        void ResetCollision()
        {
            var collider = susanoo.GetComponent<Collider>();
            Physics.IgnoreCollision(Player.local.locomotion.capsuleCollider, collider, false);
            Physics.IgnoreCollision(Player.local.GetComponentInChildren<Collider>(), collider, false);
            Physics.IgnoreCollision(groundCollider, collider, false);
            Player.local.creature.ragdoll.IgnoreCollision(collider, false);
            Player.local.creature.equipment.OnHeldItemsChangeEvent -= HeldItemEvent;
            foreach (var item in itemsToIgnore)
            {
                item.ResetColliderCollision();
            }

            //UnAddEvents();
        }

        private List<Item> itemsToIgnore = new List<Item>();
        private void HeldItemEvent(Item oldrighthand, Item oldlefthand, Item newrighthand, Item newlefthand)
        {
            if(oldrighthand) oldrighthand.ResetColliderCollision();
            if(oldlefthand) oldlefthand.ResetColliderCollision();
            itemsToIgnore.Remove(oldrighthand);
            itemsToIgnore.Remove(oldlefthand);
            itemsToIgnore.Add(newrighthand);
            itemsToIgnore.Add(newlefthand);
            var collider = susanoo.GetComponent<Collider>();
            newrighthand.IgnoreColliderCollision(collider);
            oldlefthand.IgnoreColliderCollision(collider);
        }

        IEnumerator Timer()
        {
            Debug.Log("Starting timer");
            yield return new WaitForSeconds(20f);
            ResetCollision();
            var destroying = true;
            while (destroying)
            {
                destroying = ReverseValuesOverTime();

                yield return null;
            }

            timer = null;
        }
        IEnumerator OnDestroy()
        {
            ResetCollision();
            var destroying = true;
            while (destroying)
            {
                destroying = ReverseValuesOverTime();

                yield return null;
            }
        }

        internal override void CustomEndData()
        {
            _speechRecognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
            if (susanoo)
            {
                if(timer != null) GameManager.local.StopCoroutine(timer);
                ResetCollision();
                itemsToIgnore.Clear();
                itemsToIgnore.TrimExcess();
                activateSusanoo = false;
                GameManager.local.StartCoroutine(OnDestroy());
            }
        }
        
        
        /*void AddEvents()
        {
            Player.local.creature.handRight.OnGrabEvent += GrabEventFunction;
            Player.local.creature.handLeft.OnGrabEvent += GrabEventFunction;
            Player.local.creature.handRight.OnUnGrabEvent += UnGrabEvent;
            Player.local.creature.handLeft.OnUnGrabEvent += UnGrabEvent;
        }
        
        void UnAddEvents()
        {
            Player.local.creature.handRight.OnGrabEvent -= GrabEventFunction;
            Player.local.creature.handLeft.OnGrabEvent -= GrabEventFunction;
            Player.local.creature.handRight.OnUnGrabEvent -= UnGrabEvent;
            Player.local.creature.handLeft.OnUnGrabEvent -= UnGrabEvent;
        }

        private List<Item> handledItems = new List<Item>();
        private void GrabEventFunction(Side side, Handle handle, float axisposition, HandlePose orientation, EventTime eventtime)
        {
            if (!handledItems.Contains(handle.item))
            {
                handledItems.Add(handle.item);
                handle.item.IgnoreRagdollCollision(Player.local.creature.ragdoll);
            }
        }
        private void UnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventtime)
        {
            handledItems.Remove(handle.item);
            handledItems.TrimExcess();
        }*/
    }

    public class CollisionTest : MonoBehaviour
    {
        private Collider collider;
        private void OnCollisionEnter(Collision other)
        {
            Debug.Log("Other Collider: " + other.collider.transform.root.name);
            if(other.collider.transform.root.name.Contains("Player")) Physics.IgnoreCollision(other.collider, collider, true);;
        }

        public void Setup(Collider collider)
        {
            this.collider = collider;
        }
    }
}