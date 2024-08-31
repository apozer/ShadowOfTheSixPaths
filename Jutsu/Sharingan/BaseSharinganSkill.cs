using UnityEngine;
using ThunderRoad;

namespace Jutsu
{
    public class BaseSharinganSkill : SharinganSkills
    {
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetSharinganAbilities(baseSharinganAbilities);
            base.CustomStartData();
        }
    }
}