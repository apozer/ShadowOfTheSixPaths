using ThunderRoad;

namespace Jutsu.Rinnegan
{
    public class BaseRinnegan : SharinganSkills
    {
        internal override void CustomStartData()
        {
            DojutsuTracking.mInstance.SetRinneganAbilities(baseRinneganAbilities);
            DojutsuTracking.mInstance.SetMangekyoAbilities(mangekyoAbilities);
            DojutsuTracking.mInstance.SetSharinganAbilities(baseSharinganAbilities);
            base.CustomStartData();
        }
    }
}