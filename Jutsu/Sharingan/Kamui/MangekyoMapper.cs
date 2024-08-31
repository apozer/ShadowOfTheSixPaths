using Newtonsoft.Json;
using ThunderRoad;

namespace Jutsu.Kamui
{
    public class DojutsuMapper
    {
        public string id;
        public string mangekyoSharingan;
        public string sharinganTier;
        public bool rinnegan;
        
        public string ToJson() =>JsonConvert.SerializeObject(this, Catalog.GetJsonNetSerializerSettings());
    }
}