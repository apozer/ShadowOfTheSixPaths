using ThunderRoad;

namespace Jutsu.Managed
{
    public class ModOptions : ThunderScript
    {
        public static ModOptions _instance;
        public float _pushModifier = 0.6f;
        public float _pullModifier = 1f;
        
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            if (_instance != null) return;
            _instance = this;
            base.ScriptLoaded(modData);
        }
    }
}