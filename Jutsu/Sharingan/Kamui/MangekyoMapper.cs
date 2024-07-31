using Newtonsoft.Json;
using ThunderRoad;

namespace Jutsu.Kamui
{
    public class MangekyoMapper
    {
        public string id;
        public string mangekyoSharingan;
        
        public string ToJson() =>JsonConvert.SerializeObject(this, Catalog.GetJsonNetSerializerSettings());
    }
}