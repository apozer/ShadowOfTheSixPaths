using System.Collections.Generic;
using UnityEngine;

namespace Jutsu.Rinnegan.DevaPath
{
    public class  DevaSkills : JutsuSkill
    {
        public List<string> devaPathAbilities;
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetPathAbilities(GetType().ToString(), devaPathAbilities);
            DojutsuTracking.mInstance.ActivateDojustu(devaPathAbilities);
            base.CustomStartData();
        }

        internal override void CustomEndData()
        {
            DojutsuTracking.mInstance.RemoveRinneganAbility(this.GetType().ToString());
        }
    }
}