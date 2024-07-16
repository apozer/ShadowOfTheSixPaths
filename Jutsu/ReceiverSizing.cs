using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace Jutsu
{
    public class ReceiverSizing : MonoBehaviour
    {
        private Item item;
        private bool ended = false;
        private bool setupDone = false;
        private float elapsedTime = 0f;
        private Vector3 endSize;
        private float maxLength = 3f;
        private bool debugPierce = false;
        private Dictionary<Damager, LineRenderer> renderers = new Dictionary<Damager, LineRenderer>();
        private void Start()
        {
            item = GetComponent<Item>();
            Debug.Log("Item position: " + item.transform.position);
            Debug.Log("Caster position: " + Player.currentCreature.handRight.caster.transform.position);
            item.OnGrabEvent += OnGrab;
            item.transform.localScale = new Vector3(1, 1, 0);
            endSize = new Vector3(1, 1, maxLength);
            setupDone = true;
            Debug.Log("Completed startup");
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
            }
        }

        private void Update()
        {
            if (!ended && setupDone)
            {
                item.transform.position = Player.currentCreature.handRight.caster.magicSource.transform.position -
                                          Player.currentCreature.handRight.PalmDir * 0.05f;
                item.transform.rotation = Player.currentCreature.handRight.caster.magicSource.transform.rotation;
                if (Player.currentCreature.handRight.caster.isFiring)
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
                        Player.currentCreature.handRight.Grab(item.mainHandleRight);
                    }
                }
            }
        }
    }
}