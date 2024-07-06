using ThunderRoad;
using ThunderRoad.Skill.Spell;

namespace Jutsu
{
    public class SpellCastChakra : SpellCastCharge
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