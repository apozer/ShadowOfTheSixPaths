﻿using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
namespace Jutsu.Kamui
{
    class Attractor : MonoBehaviour
    {

        const float gravConstant = 6.84f;
        public Rigidbody rb;
        internal List<Attractor> foundAttractors = new List<Attractor>();
        internal bool mainAttractor;
        internal bool attractorOn;
        bool isSucked = true;

        internal void RemoveReduceSizeScript()
        {
            if (!mainAttractor)
            {
                ReduceSizeOverTime script;
                gameObject.TryGetComponent(out script);

                if (script)
                {
                    script.ResetSize();
                    GameObject.Destroy(script);
                }
            }
        }

        void Update() {

            if (mainAttractor != null && rb != null) {
                if (mainAttractor && attractorOn)
                {
                    if (foundAttractors != null)
                    {
                        for (int i = 0; i < foundAttractors.Count;i++)
                        {
                            if (!foundAttractors[i])
                            {
                                foundAttractors.RemoveAt(i);
                                foundAttractors.TrimExcess();
                                i -= 1;
                                continue;
                            }
                            var attractor = foundAttractors[i];
                            Attract(attractor);
                            if (attractor.gameObject.GetComponent<ReduceSizeOverTime>() is ReduceSizeOverTime time && attractor != null && attractor != this)
                            {
                                time.isSucked = true;
                            }
                            else
                            {
                                var overTime = attractor.gameObject.AddComponent<ReduceSizeOverTime>();
                                overTime.isSucked = true;
                            }

                        }
                    }
                }
            
            }
        
        }

        void Attract(Attractor objToAttract)
        {
            
            if (objToAttract == null) return;
            Rigidbody rbToAttract = objToAttract.rb;

            Vector3 direction = rb.transform.position - rbToAttract.transform.position;
            rb.transform.position = rb.transform.root.position;
            float distance = Vector3.Distance(rb.position, rbToAttract.position);


            if (distance > 0.3f)
            {

                float forceMagnitude = gravConstant * (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, 2);

                Vector3 force = direction * forceMagnitude;
                rbToAttract.AddForce(force);
            }
            else
            {
                foundAttractors.Remove(objToAttract);
                foundAttractors.TrimExcess();
                GameObject.Destroy(objToAttract.transform.root.gameObject);
            }


        }



        public void SetFoundAttractor(List<Attractor> attractorsList) {

            foreach (Attractor attractor in attractorsList) {

                foundAttractors.Add(attractor);
            
            }
        
        }


    }
}