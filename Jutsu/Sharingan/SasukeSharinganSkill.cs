using System.Collections.Generic;
using UnityEngine;

namespace Jutsu
{
    public class SasukeSharinganSkill : SharinganSkills
    {
        public string susanoo;
        internal override void CustomStartData()
        {
            Debug.Log("hit custom start for sasukes mangekyo");
            DojutsuTracking.mInstance.SetMangekyoAbilities(mangekyoAbilities);
            DojutsuTracking.mInstance.SetSharinganAbilities(baseSharinganAbilities);
            base.CustomStartData();
        }
    }
}