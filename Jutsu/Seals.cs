using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Jutsu
{
    using ThunderRoad;
    using UnityEngine;
    public class Seals
    {
        internal void ReleaseHand()
        {
            Player.local.handRight.ragdollHand.playerHand.ReleaseCreature();
            Player.local.handLeft.ragdollHand.playerHand.ReleaseCreature();
        }
        void ReturnHand()
        {
            Player.local.handRight.ragdollHand.playerHand.SetCreature(Player.local.handRight.ragdollHand);
            Player.local.handLeft.ragdollHand.playerHand.SetCreature(Player.local.handLeft.ragdollHand);
        }
        internal bool HandDistance(bool activated)
        {
            return Vector3.Distance(Player.local.handRight.ragdollHand.caster.transform.position,
                Player.local.handLeft.ragdollHand.caster.transform.position) < 0.1f && !activated;
        }
        
        internal bool HandDistance()
        {
            return Vector3.Distance(Player.local.handRight.ragdollHand.caster.transform.position,
                Player.local.handLeft.ragdollHand.caster.transform.position) < 0.1f;
        }

        internal bool TigerSeal()
        {
            if(Player.local.handLeft.ragdollHand.playerHand.controlHand.alternateUsePressed && !Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed) Debug.Log("Tiger");
            return Player.local.handLeft.ragdollHand.playerHand.controlHand.alternateUsePressed && !Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed;
        }

        internal bool MonkeySeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed) Debug.Log("Monkey");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed;
        }

        IEnumerator TraverseTransforms(List<Transform> array, Transform t)
        {
            TransformNamesRecursion(array, t);
            yield return null;
        }
        
        private List<string> transformNames = new List<string>();

        internal void TransformNamesRecursion(List<Transform> array, Transform transform)
        {
            if(transform != null)
                foreach (Transform t in transform)
                {
                    Debug.Log(t);
                    array.Add(t);
                    TransformNamesRecursion(array, t);
                }

        }

        private Transform returnerLeft;
        private Transform returnerRight;
        IEnumerator CompareTransforms()
        {
            List<Transform> newLeft = new List<Transform>();
            List<Transform> newRight = new List<Transform>();
            Debug.Log("starting new left");
            TransformNamesRecursion(newLeft, JutsuEntry.local.monkeySealLeftTransform.transform);
            Debug.Log("starting new right");
            TransformNamesRecursion(newRight, JutsuEntry.local.monkeySealRightTransform.transform);
            Debug.Log("starting right transform");
            RecurseForShoulder("right", Player.local.creature.ragdoll.animatorRig);
            Debug.Log("starting left transform");
            RecurseForShoulder("left", Player.local.creature.ragdoll.animatorRig);
            List<Transform> currentRight = new List<Transform>();
            List<Transform> currentLeft = new List<Transform>();
            Debug.Log("starting current left");
            TransformNamesRecursion(currentRight, returnerRight);
            Debug.Log("starting current right");
            TransformNamesRecursion(currentLeft, returnerLeft);
            
            if (newLeft.Count == currentLeft.Count)
            {
                for (int i = 0; i < currentLeft.Count; i++)
                {
                    currentLeft[i].rotation = newLeft[i].rotation;
                }
            }
            if (newRight.Count == currentRight.Count)
            {
                for (int i = 0; i < currentRight.Count; i++)
                {
                    currentRight[i].rotation = newRight[i].rotation;
                }
            }
            yield return null;
        }
        internal void RecurseForShoulder(string side, Transform transform)
        {
            
            foreach (Transform t in transform)
            {
                Debug.Log(t.name);
                if (side.Equals("left") && transform.name.Contains("ShoulderLeft"))
                {
                    Debug.Log(t.name);
                    returnerLeft =  t;
                    break;
                }
                else if (side.Equals("right") && transform.name.Contains("ShoulderRight"))
                {
                    Debug.Log(t.name);
                    returnerRight =  t;
                    break;
                }

                RecurseForShoulder(side, t);
            }
        }
        internal void MonkeySealAnimation()
        {
            GameManager.local.StartCoroutine(CompareTransforms());
        }

        internal bool SnakeSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
               Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed) Debug.Log("Snake");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                   Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed;
        }

        internal bool OxSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
               !Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed) Debug.Log("Ox");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                !Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed;
        }

        internal bool DogSeal()
        {
            if(Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
               !Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed) Debug.Log("Dog");
            return Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
                   !Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed;
        }

        internal bool RatSeal()
        {
            if(Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed &&
               !Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed) Debug.Log("Rat");
            return Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed &&
                   !Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed;
        }

        internal bool HorseSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
               !Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed) Debug.Log("Horse");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
                   !Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed;
        }

        internal bool BoarSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
               Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed) Debug.Log("Boar");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
                   Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed;
        }

        internal bool HareSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
               Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
               !(Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
                 Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed)) Debug.Log("Hare");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                   Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed &&
                   !(Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
                     Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed);
        }

        internal bool RamSeal()
        {
            if(Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
               Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed &&
               !(Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                 Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed)) Debug.Log("Ram");
            return Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
                   Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed &&
                   !(Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                     Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed);
        }

        internal bool BirdSeal()
        {
            if(Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
               Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed
               && Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
               Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed) Debug.Log("Bird");
            return Player.local.handLeft.ragdollHand.playerHand.controlHand.usePressed &&
                   Player.local.handLeft.ragdollHand.playerHand.controlHand.gripPressed
                   && Player.local.handRight.ragdollHand.playerHand.controlHand.usePressed &&
                   Player.local.handRight.ragdollHand.playerHand.controlHand.gripPressed;
        }

        internal bool DragonSeal()
        {
            if(Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed && Player.local.handLeft.ragdollHand.playerHand.controlHand.alternateUsePressed) Debug.Log("Dragon");
            return Player.local.handRight.ragdollHand.playerHand.controlHand.alternateUsePressed && Player.local.handLeft.ragdollHand.playerHand.controlHand.alternateUsePressed;
        }

    }
}