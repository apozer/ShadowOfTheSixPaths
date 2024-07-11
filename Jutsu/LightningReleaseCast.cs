using System.Collections.Generic;
using ThunderRoad;

namespace Jutsu
{
    public class LightningReleaseCast : SpellCastCharge
    {
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            foreach (var root in JutsuEntry.local.activeRoots)
            {
                root.Reset();
            }
        }
    }
}