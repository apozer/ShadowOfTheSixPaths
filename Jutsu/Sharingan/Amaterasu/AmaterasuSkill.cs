using System.Collections;
using System.Speech.Recognition;
using ThunderRoad;
using ThunderRoad.Skill.Spell;
using UnityEngine;
using UnityEngine.VFX;

namespace Jutsu
{
    public class AmaterasuSkill: JutsuSkill
    {
        private SpeechRecognitionEngine _speechRecognizer;
        private bool activateAmaterasu;
        private AudioSource mangekyoSound;

        internal override void CustomStartData()
        {
            Debug.Log(_speechRecognizer);
            if (_speechRecognizer == null)
            { _speechRecognizer = new SpeechRecognitionEngine();
                Choices amaterasu = new Choices();
                amaterasu.Add("Amaterasu");
                Grammar servicesGrammar = new Grammar(new GrammarBuilder(amaterasu));
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

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.Log("Recognized: " + e.Result);
            if (e.Result.Confidence > 0.93f)
            {
                if (e.Result.Text == "Amaterasu")
                {
                    activateAmaterasu = true;
                }
            }
        }
        
        internal override void CustomEndData()
        {
            if(_speechRecognizer != null) _speechRecognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
        }

        internal override IEnumerator JutsuStart()
        {
            Debug.Log("Jutsu Start Loop in Amaterasu Skill");
            while (true)
            {
                if (activateAmaterasu)
                {
                    RaycastHit hit;
                    activateAmaterasu = false;
                    if (Physics.Raycast(Player.local.head.transform.position, Player.local.head.transform.forward, out hit,float.MaxValue, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore))
                    {
                        if ((hit.collider.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer))
                        {
                            creature.Inflict("AmaterasuBurning", this, 300f, parameter: 100f);
                            if (!mangekyoSound)
                            {
                                var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                mangekyoSound.transform.position = Player.local.head.transform.position;
                                mangekyoSound.transform.parent = Player.local.head.transform;
                            }
                            else mangekyoSound.Play();
                        }
                        else if (hit.collider.GetComponentInParent<Item>() is Item item &&
                                 item.mainHandler?.creature is Creature fromItem && !fromItem.isPlayer)
                        {
                            fromItem.Inflict("AmaterasuBurning", this, 300f, parameter: 100f);
                            if (!mangekyoSound)
                            {
                                var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                mangekyoSound.transform.position = Player.local.head.transform.position;
                                mangekyoSound.transform.parent = Player.local.head.transform;
                            }
                            else mangekyoSound.Play();
                        }
                        else if (hit.collider.GetComponentInParent<Item>() is Item hitItem &&
                                 hitItem.mainHandler?.creature is Creature heldItemCreature &&
                                 !heldItemCreature.equipment.GetAllHolsteredItems().Contains(hitItem))
                        {
                            if (hitItem.mainHandler.creature.isPlayer)
                            {
                                ColliderGroup c = null;
                                foreach (var collider in hitItem.colliderGroups)
                                {
                                    if (collider.name == "Blades")
                                    {
                                        c = collider;
                                    }
                                }

                                if (c != null)
                                    c.imbue.Transfer(Catalog.GetData<SpellCastCharge>("AmaterasuFire"),
                                        c.imbue.maxEnergy);
                                if (!mangekyoSound)
                                {
                                    var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                    mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                    mangekyoSound.transform.position = Player.local.head.transform.position;
                                    mangekyoSound.transform.parent = Player.local.head.transform;
                                }
                                else mangekyoSound.Play();
                            }
                        }
                        else
                        {
                            
                            if (hit.collider.transform.root.GetComponentInParent<Creature>() is Creature playerCreature && playerCreature.isPlayer)
                            {
                                    if (JutsuEntry.local.activeChidori)
                                    {
                                        JutsuEntry.local.activeChidori.GetComponentInChildren<VisualEffect>()
                                            .SetBool("amaterasu", true);
                                        JutsuEntry.local.activeChidori.gameObject.transform.GetComponentInChildren<ParticleSystem>().Play();
                                        var trigger = JutsuEntry.local.activeChidori.gameObject.transform.FindChildRecursiveTR(
                                            "AmaterasuTrigger");
                                        trigger.gameObject.GetComponent<SphereCollider>().enabled = true;
                                        trigger.gameObject.AddComponent<AmaterasuBurn>();
                                        if (!mangekyoSound)
                                        {
                                            var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                            mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                            mangekyoSound.transform.position = Player.local.head.transform.position;
                                            mangekyoSound.transform.parent = Player.local.head.transform;
                                        }
                                        else mangekyoSound.Play();

                                    }
                            }
                            else if (hit.collider.transform.root.name.Contains("Chidori 1 2"))
                            {
                                if (JutsuEntry.local.activeChidori)
                                {
                                    JutsuEntry.local.activeChidori.GetComponentInChildren<VisualEffect>()
                                        .SetBool("amaterasu", true);
                                    ParticleSystem ps = JutsuEntry.local.activeChidori.gameObject.transform
                                        .GetComponentInChildren<ParticleSystem>();
                                    ps.Play();
                                    var trigger = JutsuEntry.local.activeChidori.gameObject.transform.FindChildRecursiveTR(
                                        "AmaterasuTrigger");
                                    trigger.gameObject.GetComponent<SphereCollider>().enabled = true;
                                    trigger.gameObject.AddComponent<AmaterasuBurn>();
                                    if (!mangekyoSound)
                                    {
                                        var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                        mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                        mangekyoSound.transform.position = Player.local.head.transform.position;
                                        mangekyoSound.transform.parent = Player.local.head.transform;
                                    }
                                    else mangekyoSound.Play();

                                }
                            }
                            else
                            {
                                Vector3 position = hit.point;
                                var vfx = GameObject.Instantiate(JutsuEntry.local.amaterasuVFX.DeepCopyByExpressionTree());
                                vfx.AddComponent<AmaterasuBurn>();
                                vfx.transform.position = position;
                                if (!mangekyoSound)
                                {
                                    var reference = Object.Instantiate(JutsuEntry.local.mangekyoSFX);
                                    mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                    mangekyoSound.transform.position = Player.local.head.transform.position;
                                    mangekyoSound.transform.parent = Player.local.head.transform;
                                }
                                else mangekyoSound.Play();

                                GameManager.local.StartCoroutine(AmaterasuTimer(vfx));
                            }
                        }
                    }
                }
                yield return null;
            }
        }


        IEnumerator AmaterasuTimer(GameObject reference)
        {
            yield return new WaitForSeconds(10f);
            reference.GetComponent<ParticleSystem>().Stop();
            yield return new WaitForSeconds(2f);
            GameObject.Destroy(reference);
        }
    }
}