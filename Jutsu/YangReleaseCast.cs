using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Jutsu
{
    public class YangReleaseCast : SpellCastCharge
    {
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            Debug.Log("Spellcaster loaded");
            foreach (var root in JutsuEntry.local.activeRoots)
            {
                root.Reset();
            }

            spellCaster.OnTriggerImbueEvent += ImbueEvent;
        }

        public override void Load(Imbue imbue)
        {
            Debug.Log("Imbued");
            base.Load(imbue);
            imbue.OnImbueUse += ImbueUsed;
        }

        private void ImbueEvent(Collider other, bool enter)
        {
            Debug.Log("Imbue event trigger hit");
            if(other.gameObject.GetComponentInParent<Item>() is Item item) item.OnThrowEvent += OnThrowEvent;
        }

        public override void UpdateImbue(float speedRatio)
        {
            //Debug.Log("Updating imbue.");
            base.UpdateImbue(speedRatio);
        }

        private void ImbueUsed(SpellCastCharge spellCastCharge, float f, bool fired, EventTime time)
        {
            Debug.Log("Energy Filled");
                Item item = spellCastCharge.imbue.colliderGroup.RootGroup.gameObject.GetComponentInParent<Item>();
                item.OnThrowEvent += OnThrowEvent;
        }

        private bool active = false;
        private void OnThrowEvent(Item item)
        {
            if (!active)
            {
                active = true;
                Debug.Log("Start on throw event");
                int number = 4;
                ItemData itemData = item.data;
                List<Item> spawnedItems = new List<Item>();
                for (int i = 0; i < number; i++)
                {
                    itemData.SpawnAsync(spawned =>
                    {
                        Debug.Log("Item spawned");
                        spawned.IgnoreItemCollision(item);
                        Debug.Log("Spawned: " + spawned.transform.position);
                        var adjusted = new Vector3(item.transform.position.x + Random.Range(0.5f, -0.5f),
                            item.transform.position.y, item.transform.position.z + Random.Range(0.5f, -0.5f));
                        Debug.Log("Adjusted: " + adjusted);
                        spawned.transform.position = adjusted;
                        spawned.transform.rotation = item.transform.rotation;
                        spawned.physicBody.velocity = item.physicBody.velocity;
                        GameObject vfx = JutsuEntry.local.shadowShurikenJutsu;
                        GameObject spawnSfx = JutsuEntry.local.shadowCloneSpawnSFX.DeepCopyByExpressionTree();
                        vfx.transform.position = spawned.transform.position;
                        vfx.transform.rotation = spawned.transform.rotation;
                        spawnSfx.transform.position = spawned.transform.position;
                        Object.Instantiate(vfx);
                        Object.Instantiate(spawnSfx);
                        spawnedItems.Add(spawned);
                        foreach (var spawnedItem in spawnedItems)
                        {
                            if (spawnedItem.Equals(spawned)) continue;
                            spawned.IgnoreItemCollision(spawnedItem);
                        }

                        GameManager.local.StartCoroutine(ItemTimer(spawned, spawnedItems));
                    });
                }
            }

        }


        IEnumerator ItemTimer(Item item, List<Item> spawnedItems)
        {
            yield return new WaitForSeconds(10f);
            GameObject vfx = JutsuEntry.local.shadowShurikenJutsu;
            vfx.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            vfx.transform.position = item.transform.position;
            vfx.transform.rotation = item.transform.rotation;
            GameObject despawnSfx = JutsuEntry.local.shadowCloneDeathSFX.DeepCopyByExpressionTree();
            despawnSfx.transform.position = item.transform.position;
            Object.Instantiate(vfx);
            Object.Instantiate(despawnSfx);
            item.Despawn();

            if (spawnedItems.Count == 0) active = false;
        }
    }
}