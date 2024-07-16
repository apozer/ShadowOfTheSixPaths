using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class ChakraReceiver : JutsuSkill
    {
        private string spellInstanceId = "RinneganInit";
        private bool spawned = false;
        internal override void CustomStartData()
        {

            return;
            SetSpellInstanceID(spellInstanceId);
            var activated = GetRoot().Then(() => GetSeals().HandDistance(GetActivated()) && (CheckSpellType()));
            activated.Then(GetSeals().DragonSeal)
                .Then(GetSeals().BirdSeal)
                .Do(() => SetActivated(true));
        }


        internal override IEnumerator JutsuStart()
        {
            while (false)
            {
                GetRoot().Update();
                if (JutsuEntry.local.root.AtEnd()) JutsuEntry.local.root.Reset();
                SpellWheelCheck(GetActivated());

                if (GetActivated() && Player.currentCreature.handRight.caster.isFiring && CheckSpellType())
                {
                    if (!spawned)
                    {
                        spawned = true;
                        Catalog.GetData<ItemData>("ChakraReceiver").SpawnAsync(data =>
                        {
                            Debug.Log("Rinnegan spawn called");
                            data.transform.localScale = new Vector3(0, 0, 0);
                            data.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                            var transform = Player.currentCreature.handRight.caster.transform;
                            data.transform.position = transform.position;
                            data.transform.rotation = transform.rotation;
                            data.gameObject.AddComponent<ReceiverSizing>();
                            SetActivated(false);
                            spawned = false;
                        });
                    }
                }
                yield return null;
            }
        }
    }
}