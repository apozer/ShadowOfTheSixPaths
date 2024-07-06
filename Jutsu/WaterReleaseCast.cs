using ThunderRoad;

namespace Jutsu
{
    public class WaterReleaseCast : SpellCastCharge
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