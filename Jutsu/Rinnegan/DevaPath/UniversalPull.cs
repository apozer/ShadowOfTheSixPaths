using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using System.Speech.Recognition;
using Jutsu.Managed;

namespace Jutsu.Rinnegan.DevaPath
{
    public class UniversalPull : JutsuSkill
    {
        private SpeechRecognitionEngine _speechRecognizer;
        private bool activateUniversalPull;
        private AudioSource banshoTenin;

        internal override void CustomStartData()
        {
            Debug.Log(_speechRecognizer);
            if (_speechRecognizer == null)
            { _speechRecognizer = new SpeechRecognitionEngine();
                Choices almightyPush = new Choices();
                almightyPush.Add("Universal Pull");
                almightyPush.Add("Bansho Ten'in");
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
                if (e.Result.Text == "Universal Pull" || e.Result.Text == "Bansho Ten'in")
                {
                    activateUniversalPull = true;
                }
            }
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (activateUniversalPull)
                {
                    if (!banshoTenin)
                    {
                        var reference = Object.Instantiate(JutsuEntry.local.shinraTenseiSFX);
                        banshoTenin = reference.gameObject.GetComponent<AudioSource>();
                        banshoTenin.transform.position = Player.local.head.transform.position;
                        banshoTenin.transform.parent = Player.local.head.transform;
                    }
                    else banshoTenin.Play();
                    activateUniversalPull = false;
                    Collider[] colliders = Physics.OverlapSphere(Player.local.creature.transform.position, 10f);

                    List<Collider> validColliders = new List<Collider>();
                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature &&
                            !creature.isPlayer)
                        {
                            validColliders.Add(collider);
                        }
                    }

                    foreach (var col in validColliders)
                    {
                        if (col.gameObject.GetComponentInParent<Rigidbody>() is Rigidbody rb)
                        {
                            Vector3 target = Player.local.creature.transform.position;
                            if (Vector3.Distance(Player.local.handRight.transform.position, col.transform.position) <
                                Vector3.Distance(Player.local.handLeft.transform.position, col.transform.position))
                            {
                                target = Player.local.handRight.transform.position;
                            }
                            else target = Player.local.handLeft.transform.position;
                            var direction = target - col.transform.position;
                            var distance = Vector3.Distance(Player.local.creature.transform.position,
                                col.transform.position);
                            if (rb.GetComponentInParent<Creature>() is Creature creature)
                            {
                                if (creature.ragdoll.state != Ragdoll.State.Destabilized)
                                {
                                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                }
                            }
                            rb.AddForce(((direction.normalized * (distance * ModOptions._instance._pullModifier))  * 2f) * rb.mass, ForceMode.Impulse);
                        }
                        else if (col.gameObject.GetComponent<Rigidbody>() is Rigidbody rb2)
                        {
                            var direction = Player.local.creature.transform.position - col.transform.position;
                            var distance = Vector3.Distance(Player.local.creature.transform.position,
                                col.transform.position);
                            if (rb2.GetComponentInParent<Creature>() is Creature creature)
                            {
                                if (creature.ragdoll.state != Ragdoll.State.Destabilized)
                                {
                                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                }
                            }

                            rb2.AddForce(((direction.normalized * (distance * ModOptions._instance._pullModifier)) * 2f) * rb2.mass, ForceMode.Impulse);
                        }
                    }
                }
                yield return null;
            }
        }
    }
}