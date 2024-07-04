
using System;
using System.Linq;
using UnityEngine;
using ThunderRoad;
using ThunderRoad.AI;
using ThunderRoad.Manikin;

namespace Jutsu
{
    public class WaterCloneActive : MonoBehaviour
    {
        
        Material[] waterMats;
        private Material[] originals;
        private float displacement = 1f;
        private float height = 0f;
        private float maxheight = 1f;
        private Creature creature;
        private bool start = false;
        private bool startnext = false;
        private float elapsedTime = 0f;
        private Vector3 ogScale;
        private Renderer renderer;
        
        private void Start()
        {
            //Reset all water material vertexDisplacement values
            foreach (Material waterMat in waterMats)
            {
                    waterMat.SetFloat("_vertexDisplacement", displacement);
            }
            start = !start; // invert for Update start
        }

        void Update()
        {
            if (start)
            {
                /* Loop through every material in the renderer*/
                foreach (Material waterMat in waterMats)
                {
                    
                    float vertex = waterMat.GetFloat("_vertexDisplacement");
                    if (vertex > 0.01f)
                        waterMat.SetFloat("_vertexDisplacement", vertex - (0.006f));
                    else
                    {
                        waterMat.SetFloat("_vertexDisplacement", 0f); // set float to 0
                        renderer.materials = originals; // replace with original material array
                        start = !start; // invert to stop update code
                    }
                }
            }
        }

        public void Setup(Renderer part, Material[] original, Material[] material, Creature creature)
        {
            this.waterMats = material;
            originals = original;
            this.renderer = part;
            this.creature = creature;
        }
    }

    public class WaterCloneSizing : ThunderBehaviour
    {
        private float height = 0f;
        private float maxheight = 1f;
        private Creature creature;
        private bool start = false;
        private bool ended = false;
        private float elapsedTime = 0f;
        private Vector3 ogScale;
        
        private void Start()
        {
            start = !start; // invert to start update code
        }
        void Update()
        {
            if (start)
            {
                    elapsedTime += Time.deltaTime; // time since started
                    float percentageComplete = elapsedTime / 10f;
                    creature.gameObject.transform.localScale = Vector3.Lerp(creature.gameObject.transform.localScale,
                        ogScale, Mathf.SmoothStep(0, 1, percentageComplete)); // lerp between values at the percentage completed over time

                    if (percentageComplete >= 0.21f)
                    {
                        start = !start;
                        creature.gameObject.transform.localScale = ogScale; // reset to original scale
                        creature.brain.SetState(Brain.State.Alert); // turn creature back on
                        creature.ragdoll.SetState(Ragdoll.State.Standing); // turn off disable creature
                        
                        /* Check for HitReaction class, set the target of creature to the closest enemy*/
                        if (creature.GetComponent<BrainModuleHitReaction>() is BrainModuleHitReaction reaction)
                        {
                            bool started = false;
                            float shortestDistance = 0f;
                            Creature selected = null;
                            foreach (var active in Creature.allActive)
                            {
                                if (!active.isPlayer)
                                {
                                    if (!started)
                                    {
                                        shortestDistance = (creature.transform.position - active.transform.position)
                                            .sqrMagnitude;
                                        selected = active;
                                        started = true;
                                    }
                                    else
                                    {
                                        if ((creature.transform.position - active.transform.position).sqrMagnitude <
                                            shortestDistance)
                                        {
                                            selected = active;
                                            shortestDistance = (creature.transform.position - active.transform.position)
                                                .sqrMagnitude;
                                        }
                                    }
                                }
                            }
                            reaction.lastHitSource = selected;
                        }
                        ClonePlayer.SetCreatureEquipment(creature); // Sets gear to players gear
                    }
            }
        }
        public void Setup(Creature creature, Vector3 scale)
        {
            this.creature = creature;
            this.ogScale = scale;
        }
    }
}