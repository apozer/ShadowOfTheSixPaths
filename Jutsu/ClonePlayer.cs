using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace Jutsu
{
    using ThunderRoad;
    using UnityEngine;

    public class ClonePlayer
    {
        internal static void SetCreatureEquipment(Creature creature, List<Item> items)
        {
            //check the creature exists
            Creature playerCreature = Player.currentCreature;
            if (playerCreature == null) return;
            
            Debug.Log("Checking creature items and wardrobe");

            //try catch
            try
            {
                foreach (ItemContent itemContent in
                         Player.local.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true,
                                 content =>
                                 {
                                     ItemModuleWardrobe module;
                                     ItemModule itemModule;
                                     if (content.data.TryGetModule(out module))
                                         return true;
                                     if (content.data.TryGetModule(out itemModule))
                                     {
                                         Debug.Log("Holsterable: " + itemModule.itemData.displayName);
                                         return true;
                                     }

                                     return false;
                                 }))
                {
                    if (itemContent.HasState<ContentStateWorn>())
                    {
                        Debug.Log("Wardrobe: " + itemContent.data.displayName);
                        creature.equipment.EquipWardrobe(itemContent);
                    }
                }
                List<(ItemData, Holder)> dataHolders = new List<(ItemData, Holder)>();
                foreach (var item in items)
                {
                    dataHolders.Add((item.data, item.holder));
                }
                foreach (var (data, holder) in dataHolders)
                {
                    data.SpawnAsync(item =>
                    {
                        foreach (var creatureHolder in creature.holders)
                        {
                            if (creatureHolder.name != holder.name)
                                continue;
                            creatureHolder.Snap(item);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Something went wrong when setting the creatures equipment {e}");
            }
        }

        internal static void SetCreatureLooks(Creature creature)
        {
            //Used because SetHeight causes some issues when trying to hard set a position with SpellCastProjectile
            try
            {
                creature.SetHeight(Player.currentCreature.GetHeight());
                creature.SetColor(Player.characterData.customization.hairColor, Creature.ColorModifier.Hair);
                creature.SetColor(Player.characterData.customization.hairSecondaryColor,
                    Creature.ColorModifier.HairSecondary);
                creature.SetColor(Player.characterData.customization.hairSpecularColor,
                    Creature.ColorModifier.HairSpecular);
                creature.SetColor(Player.characterData.customization.skinColor, Creature.ColorModifier.Skin);
                creature.SetColor(Player.characterData.customization.eyesIrisColor, Creature.ColorModifier.EyesIris);
                creature.SetColor(Player.characterData.customization.eyesScleraColor,
                    Creature.ColorModifier.EyesSclera);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Something went wrong when setting the creatures height and colours"); //e}");
            }
        }
    }
}