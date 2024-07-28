using System;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class AmaterasuBurn : MonoBehaviour
    {
        private ParticleSystem ps;
         private void OnTriggerEnter(Collider other)
         {
             if (other.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
             {
                 creature.Inflict("AmaterasuBurning", this, 300f, parameter: 100f);
             }
         }

         private void OnParticleCollision(GameObject other)
         {
             
             if (other.GetComponentInParent<Creature>() is Creature creature && !creature.isPlayer)
             {
                 creature.Inflict("AmaterasuBurning", this, 300f, parameter: 100f);
             }
         }
     }
 }