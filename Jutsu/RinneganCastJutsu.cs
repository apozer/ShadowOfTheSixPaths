using ThunderRoad;
using UnityEngine;
namespace Jutsu
{
    public class RinneganCastJutsu : SpellCastCharge
    {
        public bool spawned = false;
        private bool grabbed = false;
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            foreach (var root in JutsuEntry.local.activeRoots)
            {
                root.Reset();
            }
        }

        public override void Fire(bool active)
        {
            base.Fire(active);
            if (!active) return;
            if (!spawned)
            {
                spawned = true;
                Catalog.GetData<ItemData>("ChakraReceiver").SpawnAsync(data =>
                {
                    data.transform.localScale = new Vector3(0, 0, 0);
                    data.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                    var transform = Player.currentCreature.handRight.caster.transform;
                    data.transform.position = transform.position;
                    data.transform.rotation = transform.rotation;
                    data.gameObject.AddComponent<ReceiverSizing>().Setup(spellCaster, this);
                });
            }
        }
    }
}