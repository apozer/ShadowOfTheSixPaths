﻿using System.Collections.Generic;

namespace Jutsu
{
    public class KamuiSharinganSkill : SharinganSkills
    {
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetMangekyoAbilities(mangekyoAbilities);
            DojutsuTracking.mInstance.SetSharinganAbilities(baseSharinganAbilities);
            base.CustomStartData();
        }
    }
}