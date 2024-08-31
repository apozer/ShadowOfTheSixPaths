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
        private GameObject kamuiRef;
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
            if(_speechRecognizer == null) _speechRecognizer = new SpeechRecognitionEngine();
            Choices kamuiChoice = new Choices();
            kamuiChoice.Add("Kamui");
            Grammar servicesGrammar = new Grammar(new GrammarBuilder(kamuiChoice));
            _speechRecognizer.RequestRecognizerUpdate();
            _speechRecognizer.LoadGrammarAsync(servicesGrammar);
            _speechRecognizer.SetInputToDefaultAudioDevice();
            _speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            _speechRecognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.93f && !kamuiRef)
            {
                Debug.Log(e.Result.Text);
                if (e.Result.Text == "Kamui")
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
                    SetTimer();
                    attractorOn = false;
                    startDestroy = false;
                    bool stopChecking = false;
                    activateKamui = false; 
                    if (Physics.Raycast(Player.local.head.transform.position, Player.local.head.transform.forward,
                            out RaycastHit hit, 8f, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null)
                        {
                            
                            kamuiRef = GameObject.Instantiate(JutsuEntry.local.kamuiVFX.DeepCopyByExpressionTree());
                            kamuiRef.transform.position = hit.point - (hit.point - Player.local.head.transform.position).normalized * 1f;
                            kamuiRef.transform.LookAt(Player.local.head.transform);
                            thisAttractor = kamuiRef.gameObject.AddComponent<Attractor>();
                            thisAttractor.rb = kamuiRef.gameObject.GetComponentInChildren<Rigidbody>();
                            thisAttractor.rb.transform.position = kamuiRef.transform.position;
                            thisAttractor.mainAttractor = true;
                            GameManager.local.StartCoroutine(KamuiEffectLoop(kamuiRef, thisAttractor, attractorOn, startDestroy, stopChecking));
                        }
                        else
                        {
                        
                            kamuiRef = GameObject.Instantiate(JutsuEntry.local.kamuiVFX.DeepCopyByExpressionTree());
                            kamuiRef.transform.position = Player.local.head.transform.position + (Player.local.head.transform.forward * 8f);
                            kamuiRef.transform.LookAt(Player.local.head.transform);
                            thisAttractor = kamuiRef.gameObject.AddComponent<Attractor>();
                            thisAttractor.rb = kamuiRef.gameObject.GetComponentInChildren<Rigidbody>();
                            thisAttractor.rb.transform.position = kamuiRef.transform.position;
                            thisAttractor.mainAttractor = true;
                            GameManager.local.StartCoroutine(KamuiEffectLoop(kamuiRef, thisAttractor, attractorOn, startDestroy, stopChecking));
                        }
                    }
                    else
                    {
                        
                        kamuiRef = GameObject.Instantiate(JutsuEntry.local.kamuiVFX.DeepCopyByExpressionTree());
                        kamuiRef.transform.position = Player.local.head.transform.position + (Player.local.head.transform.forward * 8f);
                        kamuiRef.transform.LookAt(Player.local.head.transform);
                        thisAttractor = kamuiRef.gameObject.AddComponent<Attractor>();
                        thisAttractor.rb = kamuiRef.gameObject.GetComponentInChildren<Rigidbody>();
                        thisAttractor.rb.transform.position = kamuiRef.transform.position;
                        thisAttractor.mainAttractor = true;
                        GameManager.local.StartCoroutine(KamuiEffectLoop(kamuiRef, thisAttractor, attractorOn, startDestroy, stopChecking));
                    }
                }
                yield return null;
            }
        }

        internal override void CustomEndData()
        {
            if(kamuiRef) GameObject.Destroy(kamuiRef);
            if(_speechRecognizer != null) _speechRecognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
            Attractor next = null;
            foreach (var attractor in attractors)
            {
                if (!attractor.mainAttractor)
                {
                    next = attractor;
                    attractor.RemoveReduceSizeScript();
                    attractors.Remove(attractor);
                    GameObject.Destroy(next);
                    next = null;
                }
            }
        }

        IEnumerator KamuiEffectLoop(GameObject kamui, Attractor attractor, bool on, bool destroy, bool stopChecking)
        {
            bool value = true;
            while (value)
            {
                if (distortionAmount < 1 && !destroy) {

                    distortionAmount += 0.01f;
                    
                    foreach (Material mat in kamui.GetComponentInChildren<MeshRenderer>().materials) {
                        mat.SetFloat("_distortionAmount",distortionAmount);
                    }
                }
            
                if (!stopChecking && !startDestroy) {
                    
                    kamui.GetComponentInChildren<Rigidbody>().isKinematic = true;
                    attractor.attractorOn = true;
                    stopChecking = true;

                    colliderObjects = Physics.OverlapSphere(kamui.transform.position, 8f);
                    var colliderItems = FindAttractors(colliderObjects);
                    CreateAttractors(colliderItems);
                    attractor.SetFoundAttractor(attractors);

                }

                if (startDestroy)
                {
                    destroy = true;
                    if (distortionAmount > 0.001f) {
                        distortionAmount -= 0.01f;
                        foreach (Material mat in kamui.GetComponentInChildren<MeshRenderer>().materials)
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
                    if (collider.gameObject.GetComponentInParent<Item>() is Item item)
                    {
                        Item sideLeft = Player.local.creature?.equipment?.GetHeldItem(Side.Left);
                        Item sideRight = Player.local.creature?.equipment?.GetHeldItem(Side.Right);
                        List<Item> holstered = Player.local.creature.equipment.GetAllHolsteredItems();
                        if ((sideLeft && !sideLeft.Equals(item)) || (sideRight && !sideRight.Equals(item)) || (holstered != null && !holstered.Contains(item)))
                        {
                            if (!colliderItems.Contains(collider.gameObject.GetComponentInParent<Item>()))
                            {
                                colliderItems.Add(collider.gameObject.GetComponentInParent<Item>());
                            }
                        }
                    }
                   else if (collider.gameObject.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer) {
                        
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
                foreach (Item collideItem in colliderItems)
                {
                    if (collideItem != null)
                    {
                        collideItem.gameObject.AddComponent<Attractor>();
                        Attractor added = collideItem.gameObject.GetComponent<Attractor>();

                        added.rb = collideItem.gameObject.GetComponent<Rigidbody>();
                        if (!attractors.Contains(added))
                        {
                            attractors.Add(added);
                        }
                    }
                }
                
                foreach (Creature collideCreatures in colliderCreature)
                {
                    if (collideCreatures != null)
                    {
                        collideCreatures.gameObject.AddComponent<Attractor>();
                        Attractor added = collideCreatures.gameObject.GetComponent<Attractor>();

                        added.rb = collideCreatures.gameObject.GetComponent<Rigidbody>();
                        if (!attractors.Contains(added))
                        {
                            attractors.Add(added);
                        }
                    }
                }
            }
        }
        private void SetTimer()
        {

            // Create a timer with a two second interval.
            aTimer = new Timer(10000);
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