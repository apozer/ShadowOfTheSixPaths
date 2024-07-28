using ThunderRoad;
using UnityEngine;
namespace Jutsu
{
    public class SpellCastAmaterasu : SpellCastCharge
    {
        public override bool OnImbueCollisionStart(CollisionInstance collisionInstance)
        {
            if (collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart?.ragdoll?.creature is Creature creature && !creature.isPlayer)
            {
                creature.Inflict("AmaterasuBurning", this, 300f, parameter: 100f);
            }
            return base.OnImbueCollisionStart(collisionInstance);
        }
    }
}