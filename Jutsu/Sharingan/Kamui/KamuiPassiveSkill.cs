using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Jutsu.Kamui
{
    public class KamuiPassiveSkill : JutsuSkill
    {
        private List<Item> handledItems = new List<Item>();
        private List<Creature> ignoringCreatures = new List<Creature>();
        private bool dataStarted = false;
        internal override void CustomStartData()
        {
            StartData();
        }

        void StartData()
        {
            dataStarted = true;
            foreach (var creature in Creature.allActive)
            {
                if (!creature.isPlayer)
                {
                    AddEvents(creature);
                }
            }
            EventManager.onCreatureSpawn += CreatureSpawnEvent;
        }

        void CreatureSpawnEvent(Creature creature)
        {
            if (!creature.isPlayer)
            {
                AddEvents(creature);
            }
        }

        void AddEvents(Creature creature)
        {
            if (!ignoringCreatures.Contains(creature))
            {
                creature.ragdoll.IgnoreCollision(Player.local.creature.ragdoll, true);
                creature.locomotion.collideWithPlayer = false;
                ignoringCreatures.Add(creature);
            }

            creature.handRight.OnGrabEvent += GrabEventFunction;
            creature.handLeft.OnGrabEvent += GrabEventFunction;
            creature.handRight.OnUnGrabEvent += UnGrabEvent;
            creature.handLeft.OnUnGrabEvent += UnGrabEvent;
        }
        void UnAddEvents(Creature creature)
        {
            creature.handRight.OnGrabEvent -= GrabEventFunction;
            creature.handLeft.OnGrabEvent -= GrabEventFunction;
            creature.handRight.OnUnGrabEvent -= UnGrabEvent;
            creature.handLeft.OnUnGrabEvent -= UnGrabEvent;
        }

        private void UnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventtime)
        {
            handledItems.Remove(handle.item);
            handledItems.TrimExcess();
        }

        private void GrabEventFunction(Side side, Handle handle, float axisposition, HandlePose orientation, EventTime eventtime)
        {
            if (!handledItems.Contains(handle.item))
            {
                handledItems.Add(handle.item);
                handle.item.IgnoreRagdollCollision(Player.local.creature.ragdoll);
            }
        }

        internal override void CustomEndData()
        {
            ResetData();
        }

        void ResetData()
        {
            dataStarted = false;
            for (int y = 0; y < handledItems.Count; y++)
            {
                var item = handledItems[y];
                item.ResetRagdollCollision();
            }

            foreach (var creature in ignoringCreatures)
            {
                creature.ragdoll.IgnoreCollision(Player.local.creature.ragdoll, false);
                creature.locomotion.collideWithPlayer = true;
                UnAddEvents(creature);
            }

            handledItems.Clear();
            handledItems.TrimExcess();
            ignoringCreatures.Clear();
            ignoringCreatures.TrimExcess();
            
            EventManager.onCreatureSpawn -= CreatureSpawnEvent;
        }

        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (Player.local.handRight.isFist || Player.local.handLeft.isFist)
                {
                    if(dataStarted)
                        ResetData();
                }
                else
                {
                    if(!dataStarted)
                        StartData();
                }

                yield return null;
            }
        }
    }
}