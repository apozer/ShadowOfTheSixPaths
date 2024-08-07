using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jutsu.Kamui;
using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class MangekyoTracker : ThunderScript
    {
        public static MangekyoTracker local;
        public string mangekyoAbility;
        private bool saveExists = false;
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            if (local != null) return;
            local = this;
            GameManager.local.StartCoroutine(GetJsonData());
            base.ScriptLoaded(modData);
        }

        IEnumerator  GetJsonData()
        {
            yield return new WaitUntil(() => Player.characterData != null);
            List<PlatformBase.Save> saves = null;
            yield return GameManager.platform.ReadSavesCoroutine("mangekyo", value =>
            {
                Debug.Log("Saves exist");
                saves = value;
            });

            
            foreach (var save in saves)
            {
                if (save.id.Equals(Player.characterData.ID))
                {
                    MangekyoMapper mapper = JsonConvert.DeserializeObject<MangekyoMapper>(save.data);
                    Debug.Log(mapper.id);
                    if(mapper != null) mangekyoAbility = mapper.mangekyoSharingan;
                    saveExists = true;
                    break;
                }
            }
            
        }
        public  IEnumerator SaveJsonData()
        {
            MangekyoMapper mapper = new MangekyoMapper();
            mapper.id = Player.characterData.ID;
            mapper.mangekyoSharingan = local.mangekyoAbility;

            var json = mapper.ToJson();
            PlatformBase.Save save = new PlatformBase.Save(mapper.id, "mangekyo", json);
            yield return GameManager.platform.WriteSaveCoroutine(save);

        }
    }
}