using System.Collections.Generic;

namespace Jutsu
{
    public class SasukeSharinganSkill : SharinganSkills
    {
        public List<string> mangekyoAbilities;
        public string susanoo;
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetMangekyoAbilities(mangekyoAbilities);
            base.CustomStartData();
        }
    }
}