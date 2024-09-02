﻿using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using System.Speech.Recognition;
using Jutsu.Managed;

namespace Jutsu.Rinnegan.DevaPath
{
    public class AlmightyPush : JutsuSkill
    {
        private SpeechRecognitionEngine _speechRecognizer;
        private bool activateAlmightyPush;
        private AudioSource shinraTensei;

        internal override void CustomStartData()
        {
            Debug.Log(_speechRecognizer);
            if (_speechRecognizer == null)
            { _speechRecognizer = new SpeechRecognitionEngine();
                Choices almightyPush = new Choices();
                almightyPush.Add("Almighty Push");
                almightyPush.Add("Shinra Tensei");
                Grammar servicesGrammar = new Grammar(new GrammarBuilder(almightyPush));
                _speechRecognizer.SetInputToDefaultAudioDevice();
                _speechRecognizer.RequestRecognizerUpdate();
                _speechRecognizer.LoadGrammarAsync(servicesGrammar);
                _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
                _speechRecognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            }
            else
            {
                _speechRecognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            }
        }
        internal override void CustomEndData()
        {
            if(_speechRecognizer != null) _speechRecognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
        }
        
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.Log("Recognized: " + e.Result);
            if (e.Result.Confidence > 0.93f)
            {
                if (e.Result.Text == "Almighty Push" || e.Result.Text == "Shinra Tensei")
                {
                    activateAlmightyPush = true;
                }
            }
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (activateAlmightyPush)
                {
                    activateAlmightyPush = false;
                    if (!shinraTensei)
                    {
                        var reference = Object.Instantiate(JutsuEntry.local.shinraTenseiSFX);
                        shinraTensei = reference.gameObject.GetComponent<AudioSource>();
                        shinraTensei.transform.position = Player.local.head.transform.position;
                        shinraTensei.transform.parent = Player.local.head.transform;
                    }
                    else shinraTensei.Play();
                    Collider[] colliders = Physics.OverlapSphere(Player.local.creature.transform.position, 6f);

                    List<Collider> validColliders = new List<Collider>();
                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature &&
                            !creature.isPlayer)
                        {
                            validColliders.Add(collider);
                        }
                        else if (collider.gameObject.GetComponentInParent<Item>() is Item item)
                        {
                            if (!Player.local.creature.equipment.GetAllHolsteredItems().Contains(item))
                            {
                                validColliders.Add(collider);
                            }
                        }
                    }

                    foreach (var col in validColliders)
                    {
                        if (col.gameObject.GetComponentInParent<Rigidbody>() is Rigidbody rb)
                        {
                            var direction = col.transform.position - Player.local.creature.transform.position;
                            var distance = Vector3.Distance(Player.local.creature.transform.position,
                                col.transform.position);
                            if (rb.GetComponentInParent<Creature>() is Creature creature)
                            {
                                if (creature.ragdoll.state != Ragdoll.State.Destabilized)
                                {
                                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                }
                            }
                            rb.AddForce(((direction.normalized / (distance * ModOptions._instance._pushModifier))  * 15f) * rb.mass, ForceMode.Impulse);
                        }
                        else if (col.gameObject.GetComponent<Rigidbody>() is Rigidbody rb2)
                        {
                            var direction = col.transform.position - Player.local.creature.transform.position;
                            var distance = Vector3.Distance(Player.local.creature.transform.position,
                                col.transform.position);
                            if (rb2.GetComponentInParent<Creature>() is Creature creature)
                            {
                                if (creature.ragdoll.state != Ragdoll.State.Destabilized)
                                {
                                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                }
                            }
                            rb2.AddForce(((direction.normalized / (distance * ModOptions._instance._pushModifier))  * 15f) * rb2.mass, ForceMode.Impulse);
                        }
                    }
                }
                yield return null;
            }
        }
    }
}