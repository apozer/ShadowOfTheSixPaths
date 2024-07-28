using System.Collections.Generic;
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

        void Update() {

            if (mainAttractor != null) {
                Debug.Log("Main attractor is not null");
                if (mainAttractor && attractorOn)
                {
                    Debug.Log("Main attractor is true");
                    if (foundAttractors != null)
                    {
                        Debug.Log("foundAttractors is not null");
                        foreach (Attractor attractor in foundAttractors)
                        {
                            Debug.Log("Attracting");
                            Attract(attractor);
                            if (attractor.gameObject.GetComponent<ReduceSizeOverTime>() == null && attractor != this)
                            {
                                attractor.gameObject.AddComponent<ReduceSizeOverTime>();
                                attractor.gameObject.GetComponent<ReduceSizeOverTime>().isSucked = true;
                            }

                        }
                    }
                }
            
            }
        
        }

        void Attract(Attractor objToAttract) {


            Rigidbody rbToAttract = objToAttract.rb;


            Vector3 direction = rb.position - rbToAttract.position;

            float distance = direction.magnitude;

            float forceMagnitude = gravConstant * (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, 2);

            Vector3 force = direction.normalized * forceMagnitude;

            rbToAttract.AddForce(force);
        
        }



        public void SetFoundAttractor(List<Attractor> attractorsList) {

            foreach (Attractor attractor in attractorsList) {

                foundAttractors.Add(attractor);
            
            }
        
        }


    }
}