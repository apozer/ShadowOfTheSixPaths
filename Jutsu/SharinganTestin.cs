using System.Collections;
using System.Collections.Generic;

namespace Jutsu
{
    using ThunderRoad;
    using UnityEngine;
    public class SharinganTestin : ThunderScript
    {
        private int count = 0;
        private bool raceInstantiated = false;
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
            Player.onSpawn += OnSpawn;
            EventManager.onLevelLoad += (data, mode, time) => { this.raceInstantiated = false;};
        }

        private void OnSpawn(Player player)
        {
            if (raceInstantiated) return;
            player.onCreaturePossess += creature =>
            {
                if (raceInstantiated) return;
                Debug.Log(creature.currentEthnicGroup.id);
                if (creature.currentEthnicGroup.id.ToLower().Equals("uchiha"))
                {
                    Debug.Log("Before renderer");
                    GameManager.local.StartCoroutine(WaitForRenderers(creature));
                }

                raceInstantiated = true;
            };
        }
        
        IEnumerator WaitForRenderers(Creature creature)
        {
            yield return new WaitUntil(() => creature.renderers.Count > 0);
            if (!JutsuEntry.local._lerpMaterialChanges)
            {
                JutsuEntry.local._lerpMaterialChanges = new GameObject().AddComponent<LerpMaterialChanges>();
                JutsuEntry.local._lerpMaterialChanges.sharinganBase =  JutsuEntry.local.threeTomoeSharingan;
                JutsuEntry.local._lerpMaterialChanges.lerpMaterial =  JutsuEntry.local.lerpMaterial;
                JutsuEntry.local._lerpMaterialChanges.rinneganBase = JutsuEntry.local.rinneganBase;
                JutsuEntry.local._lerpMaterialChanges.sasukeMangekyoSharingan =
                    JutsuEntry.local.sasukeMangekyoSharingan;
            }
            foreach (var renderer in creature.renderers)
            {
                if (renderer.renderer.name.Equals("Eyes_LOD0"))
                {
                    Material original = renderer.renderer.materials[0].DeepCopyByExpressionTree();
                    JutsuEntry.local._lerpMaterialChanges.defaultColor = original.DeepCopyByExpressionTree();
                    JutsuEntry.local._lerpMaterialChanges.defaultNormalMap = (Texture2D) original.GetTexture("_BumpMap");
                    JutsuEntry.local._lerpMaterialChanges.defaultMetallic = (Texture2D) original.GetTexture("_MetallicGlossMap");
                    JutsuEntry.local._lerpMaterialChanges.lerpMaterial.SetTexture("_OriginalTexture", original.GetTexture("_BaseMap").DeepCopyByExpressionTree());
                    JutsuEntry.local._lerpMaterialChanges.lerpMaterial.SetTexture("_normalOriginal", JutsuEntry.local._lerpMaterialChanges.defaultNormalMap);
                    JutsuEntry.local._lerpMaterialChanges.lerpMaterial.SetTexture("_metallicOriginal", JutsuEntry.local._lerpMaterialChanges.defaultMetallic);
                    JutsuEntry.local._lerpMaterialChanges.lerpMaterial.SetFloat("_transition", 0f);
                    renderer.renderer.material = JutsuEntry.local._lerpMaterialChanges.lerpMaterial;
                }
            }
        }
    }
}