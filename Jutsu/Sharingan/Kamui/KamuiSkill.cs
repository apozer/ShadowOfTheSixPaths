using System.Collections;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Timers;
using ThunderRoad;
using UnityEngine;
namespace Jutsu.Kamui
{
    public class KamuiSkill : JutsuSkill
    {
        GameObject kamui;
        Collider[] colliderObjects;
        List<Creature> colliderCreature = new List<Creature>();
        List<Attractor> attractors = new List<Attractor>();
        Attractor thisAttractor;
        bool attractorOn;
        float attractorToItemDistance;
        float distortionAmount;
        private float elapsedTime;
        bool isSucked;
        private Timer aTimer;
        bool startDestroy;

        private bool activateKamui;

        SpeechRecognitionEngine _speechRecognizer;
        internal override void CustomStartData()
        {
            distortionAmount = 0f;
            kamui = JutsuEntry.local.kamuiVFX;
            _speechRecognizer = new SpeechRecognitionEngine();
            Choices amaterasu = new Choices();
            amaterasu.Add("Amaterasu");
            Grammar servicesGrammar = new Grammar(new GrammarBuilder(amaterasu));
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
                if (e.Result.Text == "Amaterasu")
                {
                    activateKamui = true;
                }
            }
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (activateKamui)
                {
                    attractorOn = false;
                    startDestroy = false;
                    bool stopChecking = false;
                    activateKamui = false;
                    var kamuiRef = GameObject.Instantiate(kamui.DeepCopyByExpressionTree());
                    var attractor = kamuiRef.gameObject.AddComponent<Attractor>();
                    GameManager.local.StartCoroutine(KamuiEffectLoop(kamuiRef, attractor, attractorOn, startDestroy, stopChecking));
                }
                yield return null;
            }
        }

        IEnumerator KamuiEffectLoop(GameObject kamui, Attractor attractor, bool on, bool destroy, bool stopChecking)
        {
            bool value = true;
            while (value)
            {
                if (distortionAmount < 1 && !destroy) {

                    distortionAmount += 0.01f;
                    foreach (Material mat in kamui.gameObject.GetComponent<MeshRenderer>().materials) {
                        mat.SetFloat("_distortionAmount",distortionAmount);
                    }
                }
                    
                float distance = Vector3.Distance(Player.local.creature.transform.position, kamui.transform.position);
            
                if (distance > 7f && !stopChecking) {

                    kamui.GetComponent<Rigidbody>().isKinematic = true;
                    attractor.attractorOn = true;
                    stopChecking = true;

                    colliderObjects = Physics.OverlapSphere(kamui.transform.position, 4f);
                    var colliderItems = FindAttractors(colliderObjects);
                    CreateAttractors(colliderItems);
                    attractor.SetFoundAttractor(attractors);

                }

                if (startDestroy) {

                    if (distortionAmount > 0.01f) {
                        distortionAmount -= 0.01f;
                        foreach (Material mat in kamui.gameObject.GetComponent<MeshRenderer>().materials)
                        {
                            mat.SetFloat("_distortionAmount", distortionAmount);
                        }
                    }

                    else {
                        GameObject.Destroy(kamui);
                        value = false;
                    }
            
                }

                yield return null;
            }
        }
        
        
        private List<Item> FindAttractors(Collider[] colliderObjects)
        {

            List<Item> colliderItems = new List<Item>();
            if (colliderObjects != null)
            {

                foreach (Collider collider in colliderObjects)
                {

                    if (collider.gameObject.GetComponentInParent<Item>() != null)
                    {
                        if (!colliderItems.Contains(collider.gameObject.GetComponentInParent<Item>()))
                        {

                            colliderItems.Add(collider.gameObject.GetComponentInParent<Item>());

                        }

                    }

                   else if (collider.gameObject.GetComponentInParent<Creature>() != null) {
                        
                        if (!colliderCreature.Contains(collider.gameObject.GetComponentInParent<Creature>()))
                        {

                            colliderCreature.Add(collider.gameObject.GetComponentInParent<Creature>());

                        }
                    }
                }
            }

            return colliderItems;
        }


        private void CreateAttractors(List<Item> colliderItems) {
            
            if (colliderItems != null)
            {
                Debug.Log("Got past colliderItems null check");
                foreach (Item collideItem in colliderItems)
                {
                    if (collideItem != null)
                    {
                        Debug.Log(collideItem);

                        collideItem.gameObject.AddComponent<Attractor>();
                        Attractor added = collideItem.gameObject.GetComponent<Attractor>();

                        added.rb = collideItem.gameObject.GetComponent<Rigidbody>();
                        Debug.Log(added);
                        if (!attractors.Contains(added))
                        {
                        Debug.Log("Add to list");
                        attractors.Add(added);
                        }

                    }

                }


                foreach (Creature collideCreatures in colliderCreature)
                {
                    if (colliderCreature != null)
                    {

                        collideCreatures.gameObject.AddComponent<Attractor>();
                        Attractor added = collideCreatures.gameObject.GetComponent<Attractor>();

                        added.rb = collideCreatures.gameObject.GetComponent<Rigidbody>();
                        if (!attractors.Contains(added))
                        {
                            Debug.Log("Add to list");
                            attractors.Add(added);
                        }

                    }

                }

            }
        }
        private void SetTimer()
        {

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(10000);
            // Hook up the Elapsed event for the timer. 

            aTimer.Elapsed += OnTimedEvent;

            aTimer.AutoReset = false;
            aTimer.Enabled = true;

        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            startDestroy = true;
        }
    }
}