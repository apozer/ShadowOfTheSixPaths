using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace Jutsu
{
    public class ReceiverSizing : MonoBehaviour
    {
        private Item item;
        private SpellCaster _spellCaster;
        private bool ended = false;
        private bool setupDone = false;
        private float elapsedTime = 0f;
        private Vector3 endSize;
        private float maxLength = 3f;
        private bool debugPierce = false;
        private RinneganCastJutsu active;
        private Dictionary<Damager, LineRenderer> renderers = new Dictionary<Damager, LineRenderer>();
        private void Start()
        {
            item = GetComponent<Item>();
            item.OnGrabEvent += OnGrab;
            item.transform.localScale = new Vector3(1, 1, 0);
            endSize = new Vector3(1, 1, maxLength);
            setupDone = true;
        }

        public void Setup(SpellCaster spellCaster, RinneganCastJutsu active)
        {
            _spellCaster = spellCaster;
            this.active = active;
        }

        private void OnGrab(Handle handle, RagdollHand ragdollhand)
        {
            if (!ended)
            {
                ended = true;
                foreach (var damager in item.gameObject.GetComponentsInChildren<Damager>())
                {
                    if (damager.type == Damager.Type.Pierce)
                    {
                        damager.penetrationDepth = 0.25f * item.transform.localScale.z;
                    }
                }
                active.spawned = false;
            }
        }

        private void Update()
        {
            if (!ended && setupDone)
            {
                item.transform.position = _spellCaster.magicSource.transform.position -
                                          _spellCaster.ragdollHand.PalmDir * 0.05f;
                item.transform.rotation = _spellCaster.magicSource.transform.rotation;
                if (_spellCaster.isFiring)
                {
                    elapsedTime += Time.deltaTime;
                    float percentageComplete = elapsedTime / (2f * (maxLength * maxLength));
                    item.transform.localScale = Vector3.Lerp(item.transform.localScale, endSize, percentageComplete);

                    foreach (var damager in item.gameObject.GetComponentsInChildren<Damager>())
                    {
                        if (damager.type == Damager.Type.Pierce)
                        {
                            if (debugPierce)
                            {
                                if (!renderers.ContainsKey(damager))
                                {
                                    LineRenderer renderer = new GameObject().AddComponent<LineRenderer>();
                                    renderers.Add(damager, renderer);
                                }

                                else
                                {
                                    var renderer = renderers[damager];

                                    renderer.startColor = Color.red;
                                    var positions = new[]
                                    {
                                        damager.transform.position,
                                        damager.transform.position +
                                        (-damager.transform.forward * damager.penetrationDepth)
                                    };
                                    renderer.SetPositions(positions);

                                }
                            }

                            damager.penetrationDepth = 0.25f * item.transform.localScale.z;
                        }
                    }
                    
                    if (item.transform.localScale.z >= maxLength)
                    {
                        ended = true;
                        elapsedTime = 0f;
                        item.transform.localScale = new Vector3(1, 1, maxLength);
                        _spellCaster.ragdollHand.Grab(item.mainHandleRight);
                        active.spawned = false;
                    }
                }
            }
        }
    }
}