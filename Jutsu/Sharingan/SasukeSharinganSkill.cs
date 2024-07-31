using System.Collections.Generic;

namespace Jutsu
{
    public class SasukeSharinganSkill : SharinganSkills
    {
        public List<string> mangekyoAbilities;
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetMangekyoAbilities(mangekyoAbilities);
            base.CustomStartData();
        }
    }
}