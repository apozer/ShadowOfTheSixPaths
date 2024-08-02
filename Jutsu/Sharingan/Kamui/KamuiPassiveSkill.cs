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
        internal override void CustomStartData()
        {
            if (Creature.allActive.Count > 1)
            {
                foreach (var creature in Creature.allActive)
                {
                    if (!creature.isPlayer)
                    {
                        AddEvents(creature);
                    }
                }
            }
            EventManager.onCreatureSpawn += CreatureSpawnEvent;
        }

        void CreatureSpawnEvent(Creature creature)
        {
            Debug.Log("Creature spawned");
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
            }
        }

        internal override void CustomEndData()
        {
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

            EventManager.onCreatureSpawn -= CreatureSpawnEvent;
        }
    }
}