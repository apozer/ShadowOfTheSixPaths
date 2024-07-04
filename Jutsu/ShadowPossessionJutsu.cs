
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine.VFX.Utility;

namespace Jutsu
{
    using UnityEngine;
    using ThunderRoad;
    using UnityEngine.VFX;

    /**
     * Shadow Possession Jutsu VFX, SFX and functionality
     */
    public class ShadowPossessionJutsu : JutsuSkill
    {

        //Shadow VFX and SFX
        private GameObject shadow;
        private GameObject shadowUpdate;
        private VisualEffect shadowVFX;
        private VisualEffect shadowEffect;
        private GameObject shadowSFX;
        private GameObject shadowSFXUpdate;
        private AudioSource shadowAudio;

        private bool instantiated = false;

        //Transforms for VFX bezier curve
        private Transform A;
        private Transform B;
        private Transform C;
        private Transform D;
        private bool playing = false;

        //References to original positions
        private Vector3 ogBPos;
        private Vector3 currentBPos;
        private Vector3 ogCPos;
        private Vector3 currentCPos;

        private bool returnedTo = false;
        private bool soundPlayed = false;



        //Hand signs
        private string spellId = "YinInit";
        
        //Used for Lerps
        private float elapsedTime = 0f;
        private float returnElapsedTime = 0f;
        private Creature selectedCreature;
        private bool shadowAttached = false;
        private bool aPosSet = false;
        private Vector3 apos;
        
        //Timer info
        private bool timerStarted = false;

        internal override void CustomStartData()
        {
            SetSpellInstanceID(spellId);
            shadow = JutsuEntry.local.shadow;
            shadowSFX = JutsuEntry.local.shadowSFX;
            var activated = JutsuEntry.local.root.Then(() => GetSeals().HandDistance(GetActivated()) && CheckSpellType()).Do(() => JutsuEntry.local.root.Reset());
            activated
                .Then(GetSeals().RatSeal)
                .Do(() => SetActivated(true));
        }

        internal override IEnumerator JutsuStart()
        {
            yield return new WaitForSeconds(2f);
            while (true)
            {
                JutsuEntry.local.root.Update();
                if(JutsuEntry.local.root.AtEnd()) JutsuEntry.local.root.Reset();
                
                SpellWheelCheck(GetActivated());
                
                if (!instantiated && GetActivated())
                {
                    Debug.Log("Started Shadow Possession Setup");
                    //Instantiate VFX and SFX
                    shadowSFXUpdate = Object.Instantiate(shadowSFX);
                    shadowUpdate = Object.Instantiate(shadow);
                    shadowUpdate.transform.position = Player.local.transform.position;
                    shadowVFX = shadowUpdate.GetComponentInChildren<VisualEffect>();
                    shadowVFX.SetFloat("start", 0f);
                    shadowVFX.Play();

                    //Set references for bezier points
                    A = GameObject.Find("A").transform;
                    B = GameObject.Find("B").transform;
                    C = GameObject.Find("C").transform;
                    D = GameObject.Find("D").transform;

                    //Start position is players feet
                    shadowSFXUpdate.transform.position = A.position;
                    shadowAudio = shadowSFXUpdate.GetComponent<AudioSource>();
                    instantiated = true;
                }

                else if (instantiated && GetActivated())
                {
                    if (!GetJutsuTimerActivated()) GameManager.local.StartCoroutine(JutsuActive());
                    if (!playing)
                    {
                        Debug.Log("Instantiated and not playing");
                        playing = true;
                        shadowVFX.Play();
                        //Check if creature has the shadow possesion controller active
                        /*if (selectedCreature is Creature creature1 &&
                            creature1.gameObject.GetComponent<ShadowPossessionController>() is
                                ShadowPossessionController spc) spc.Unpossess();*/

                        //Reset values
                        selectedCreature = null;
                        D.position = Player.local.creature.transform.position;

                        //Check if all active is not null or empty and there is not currently selected creature
                        if (!Creature.allActive.IsNullOrEmpty() && !selectedCreature)
                        {

                            //Loop over active creature list
                            foreach (Creature creature in Creature.allActive)
                            {
                                //Verify creature is not the player
                                if (!creature.isPlayer)
                                {
                                    selectedCreature = creature;

                                    //Get random distance and direction for B and C positions in Bezier curve
                                    var distance = Vector3.Distance(A.position,
                                        selectedCreature.transform.position);
                                    var direction = (selectedCreature.transform.position - A.position);
                                    var perpendicular = Vector3.Cross(Vector3.up, direction).normalized;

                                    var randomBOperator = Random.Range(0, 100);
                                    var distB = Random.Range(0.8f, 2f);
                                    if (randomBOperator < 0)
                                    {
                                        distB *= -1;
                                    }

                                    var randomCOperator = Random.Range(0, 100);
                                    var distC = Random.Range(2f, 0.8f);
                                    if (randomCOperator < 0)
                                    {
                                        distC *= -1;
                                    }

                                    //Set current postions and store them for use to return to normal later
                                    ogBPos = B.position = A.position + (direction.normalized * (0.3f * distance));
                                    currentBPos = B.position += (perpendicular * distB);
                                    ogCPos = C.position = A.position + (direction.normalized * (0.6f * distance));
                                    currentCPos = C.position += (-perpendicular * distC);

                                    //Adds shadow possesion controller
                                    /*if (!selectedCreature.gameObject.GetComponent<ShadowPossessionController>())
                                        selectedCreature.gameObject.AddComponent<ShadowPossessionController>();
                                    else
                                        selectedCreature.gameObject.GetComponent<ShadowPossessionController>()
                                            .Possess();*/
                                    //break after creature is selected.
                                    break;
                                }
                            }
                        }
                    }

                    //If creature is found and shadow vfx has not started
                    if (selectedCreature && !shadowAttached)
                    {

                        //Player SFX
                        if (!soundPlayed)
                        {
                            shadowAudio.Play();
                            soundPlayed = true;
                        }

                        //Update A position to player feet
                        A.position = new Vector3(Player.local.creature.player.head.transform.position.x,
                            Player.local.creature.transform.position.y,
                            Player.local.creature.player.head.transform.position.z);
                        D.position = selectedCreature.transform.position;

                        //Get time since vfx started and track percentage between 0 and 1
                        elapsedTime += Time.deltaTime;
                        float percentageComplete = elapsedTime / ((D.position - A.position).magnitude * 0.05f);

                        //Update particle positions along the bezier curve
                        shadowVFX.SetFloat("start", percentageComplete);

                        if (percentageComplete >= 1f)
                        {

                            //Once close enough (Vector3 comparison is inaccurate) set final values
                            shadowVFX.SetFloat("start", 1f);
                            elapsedTime = 0f;
                            D.position = B.position;
                            shadowAttached = true;
                        }

                        if (!returnedTo)
                        {
                            //Moves B and C Positions back to original point on the line
                            returnElapsedTime += Time.deltaTime;
                            float percentageBComplete = returnElapsedTime / 1.2f;
                            float percentageCComplete = returnElapsedTime / 1.2f;
                            B.position = Vector3.Lerp(currentBPos, ogBPos, percentageBComplete);
                            Vector3 tempB = new Vector3(B.position.x, B.position.y + 1f, B.position.z);
                            if (Physics.Raycast(tempB, Vector3.down, out RaycastHit hitB))
                            {
                                B.position = new Vector3(B.position.x, hitB.collider.transform.position.y, B.position.z);
                            }
                            C.position = Vector3.Lerp(currentCPos, ogBPos, percentageCComplete);
                            Vector3 tempC = new Vector3(B.position.x, B.position.y + 1f, B.position.z);
                            if (Physics.Raycast(tempC, Vector3.down, out RaycastHit hitC))
                            {
                                B.position = new Vector3(B.position.x, hitC.collider.transform.position.y, B.position.z);
                            }
                            if (Vector3.Distance(B.position, ogBPos) < 0.01f)
                            {
                                if (Vector3.Distance(C.position, ogCPos) < 0.01f)
                                {
                                    returnElapsedTime = 0f;
                                    returnedTo = true;
                                }
                            }
                        }
                    }

                    //If shadow is attached to the enemy creature
                    if (shadowAttached)
                    {

                        //Update A position to the Player feet
                        A.transform.position = new Vector3(Player.local.creature.player.head.transform.position.x,
                            Player.local.creature.transform.position.y,
                            Player.local.creature.player.head.transform.position.z);
                        //Update D position to the enemy feet
                        D.position = selectedCreature.transform.position;

                        if (!returnedTo)
                        {
                            //Moves B and C Positions back to original point on the line
                            returnElapsedTime += Time.deltaTime;
                            float percentageBComplete = returnElapsedTime / 1.2f;
                            float percentageCComplete = returnElapsedTime / 1.2f;
                            B.position = Vector3.Lerp(currentBPos, ogBPos, percentageBComplete);
                            Vector3 tempB = new Vector3(B.position.x, B.position.y + 1f, B.position.z);
                            if (Physics.Raycast(tempB, Vector3.down, out RaycastHit hitB))
                            {
                                B.position = new Vector3(B.position.x, hitB.transform.position.y, B.position.z);
                            }
                            C.position = Vector3.Lerp(currentCPos, ogBPos, percentageCComplete);
                            Vector3 tempC = new Vector3(B.position.x, B.position.y + 1f, B.position.z);
                            if (Physics.Raycast(tempC, Vector3.down, out RaycastHit hitC))
                            {
                                B.position = new Vector3(B.position.x, hitC.transform.position.y, B.position.z);
                            }
                            if (Vector3.Distance(B.position, ogBPos) < 0.01f)
                            {
                                if (Vector3.Distance(C.position, ogCPos) < 0.01f)
                                {
                                    returnElapsedTime = 0f;
                                    returnedTo = true;
                                }
                            }
                        }
                    }
                }

                else if(instantiated && !GetActivated())
                    {
                        if (playing && selectedCreature)
                        {
                            //TODO: Add code to do the opposite of what the original lerp does, so vfx leaves in reverse (Cool effect)

                            //Reset all values to default
                            soundPlayed = false;
                            shadowVFX.Stop();
                            shadowVFX.SetFloat("start", 0f);
                            SetActivated(false);
                            elapsedTime = 0f;
                            returnElapsedTime = 0f;
                            D.position = Player.local.transform.position;
                            A.position = Player.local.transform.position;
                            playing = false;
                            returnedTo = false;
                            shadowAttached = false;
                            selectedCreature.brain.SetState(Brain.State.Alert);
                            SpellWheelReset();
                        }
                    }
                yield return null;
            }
        }
    }
}