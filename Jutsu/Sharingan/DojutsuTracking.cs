using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderRoad;
using Random = UnityEngine.Random;

namespace Jutsu
{
    public enum EyeMaterialState{
        NotActive,
        Disabled,
        Sharingan,
        MangekyoSharingan,
        Rinnegan
    }
    
    public class DojutsuTracking : ThunderScript
    {

        public static DojutsuTracking mInstance;
        public Material lerpMaterial;
        public Texture2D sharinganBase;
        public Texture2D sasukeMangekyoSharingan;
        public Texture2D kamuiMangekyoSharingan;
        public Texture2D mangekyoTexture;
        public Texture2D rinneganBase;
        public Material defaultColor;
        public Texture2D defaultNormalMap;
        public Texture2D defaultMetallic;
        public Texture2D defaultEmission;
        private EyeMaterialState lastState = EyeMaterialState.NotActive;
        public EyeMaterialState state = EyeMaterialState.NotActive;
        internal SkillData rinneganData;
        private AudioSource mangekyoSound;

        public bool transitionActive = false;
        public string lastActive = "";
        public bool mangekyoActivated = false;
        public bool devMode = false;
        
        public delegate void MangekyoActive();

        public event MangekyoActive mangekyoActive;
        

        private List<string> allMangekyoAblities = new List<string> { "SasukeSharinganSkills, KamuiSharinganSkills" };

        internal static  List<string> activeDojutsuSkills = new List<string>();

        private List<string> mangekyoAbilities;
        private List<string> rinnegan = new List<string>{"RinneganInit"};
        
        private bool raceInstantiated = false;

        public void SetMangekyoAbilities(List<string> list)
        {
            mangekyoAbilities = list;
        }
        
        IEnumerator WaitForRenderers(Creature creature)
        {
            yield return new WaitUntil(() => creature.renderers.Count > 0);
            if (mInstance != null)
            {
                mInstance.sharinganBase =  JutsuEntry.local.threeTomoeSharingan;
                mInstance.lerpMaterial =  JutsuEntry.local.lerpMaterial;
                mInstance.rinneganBase = JutsuEntry.local.rinneganBase;
                mInstance.sasukeMangekyoSharingan =
                    JutsuEntry.local.sasukeMangekyoSharingan;
                mInstance.kamuiMangekyoSharingan = JutsuEntry.local.kamuiMangekyoSharingan;
            }
            foreach (var renderer in creature.renderers)
            {
                if (renderer.renderer.name.Equals("Eyes_LOD0"))
                {
                    Material original = renderer.renderer.materials[0].DeepCopyByExpressionTree();
                    mInstance.defaultColor = original.DeepCopyByExpressionTree();
                    mInstance.defaultNormalMap = (Texture2D) original.GetTexture("_BumpMap");
                    mInstance.defaultMetallic = (Texture2D) original.GetTexture("_MetallicGlossMap");
                    mInstance.lerpMaterial.SetTexture("_OriginalTexture", original.GetTexture("_BaseMap").DeepCopyByExpressionTree());
                    mInstance.lerpMaterial.SetTexture("_normalOriginal", DojutsuTracking.mInstance.defaultNormalMap);
                    mInstance.lerpMaterial.SetTexture("_metallicOriginal", DojutsuTracking.mInstance.defaultMetallic);
                    mInstance.lerpMaterial.SetFloat("_transition", 0f);
                    renderer.renderer.material = mInstance.lerpMaterial;
                }
            }
        }
        
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            if (mInstance != null) return;
            mInstance = this;
            EventManager.onLevelLoad += (data, mode, time) => { this.raceInstantiated = false;};
            Player.onSpawn += playerSpawned =>
            {
                SkillData value;
                playerSpawned.onCreaturePossess += creaturePosses =>
                {
                    if (creaturePosses.currentEthnicGroup.id.ToLower().Equals("uchiha"))
                    {
                        if (raceInstantiated) return;
                        GameManager.local.StartCoroutine(WaitForRenderers(creaturePosses));
                        //works in last iteration
                        /*foreach (var mangekyo in allMangekyoAblities)
                        {
                            mangekyoActivated =
                                playerSpawned.creature.container.TryGetSkillContent(mangekyo, out value);
                            if (mangekyoActivated) break;
                        }*/
                        if (MangekyoTracker.local.mangekyoAbility != null && !devMode)
                        {
                            playerSpawned.creature.container.AddSkillContent(MangekyoTracker.local.mangekyoAbility);
                            mangekyoActivated = true;
                        }
                        else
                        {
                            
                            mangekyoActivated = false;
                        }
                        if (!devMode && !mangekyoActivated)
                        {
                            EventManager.onCreatureKill += (creature, player, instance, time) =>
                            {
                                if (!creature.isPlayer)
                                {
                                    var random = Random.Range(0, 1000);
                                    if (random / 1000 < 0.99f)
                                    {
                                        var ability = ReturnRandomMangekyoAbility();
                                        creaturePosses.container.AddSkillContent(ability);
                                        MangekyoTracker.local.mangekyoAbility = ability;
                                        mangekyoActivated = true;
                                        GameManager.local.StartCoroutine(MangekyoTracker.local.SaveJsonData());
                                        mangekyoActive?.Invoke();
                                    }
                                }
                            };
                        }
                        else if(devMode) playerSpawned.creature.container.AddSkillContent("KamuiSharinganSkills");

                    }
                    raceInstantiated = true;
                };
                
            };

            GameManager.local.StartCoroutine(Update());
            base.ScriptLoaded(modData);
        }

        string ReturnRandomMangekyoAbility()
        {
            var random = Random.Range(0, 100);

            if (random % 5 == 0)
            {
                return "SasukeSharinganSkills";
            }

            return "KamuiSharinganSkills";
        }

        public void Execute(string lastActive, string nextActive)
        {
            SetMaterialData(GetTransitionStart(lastActive), GetTransitionStart(nextActive));
        }
        
        Texture GetTransitionStart(string lastActive)
        {
            switch (lastActive)
            {
                case "":
                    return defaultColor.GetTexture("_BaseMap");
                case "sharinganBase":
                    return sharinganBase;
                case "mangekyoSharingan":
                    return mangekyoTexture;
                case "rinnayganBase":
                    return rinneganBase;
                default:
                    return defaultColor.GetTexture("_BaseMap");
            }
        }
        public void SetMaterialData(Texture texture1, Texture texture2)
        {
            lerpMaterial.SetFloat("_transition", 0f);
            lerpMaterial.SetTexture("_OriginalTexture", texture1);
            lerpMaterial.SetTexture("_normalOriginal", defaultNormalMap);
            lerpMaterial.SetTexture("_metallicOriginal", defaultMetallic);
            lerpMaterial.SetTexture("_normalSharingan", defaultNormalMap);
            lerpMaterial.SetTexture("_metallicSharingan", defaultMetallic);
            lerpMaterial.SetTexture("_sharinganTexture", texture2);
            GameManager.local.StartCoroutine(TransitionEyeMaterial());
        }
        
        public IEnumerator TransitionEyeMaterial()
        {
            bool active = true;
            while (active)
            {
                if (!lerpMaterial) yield return null;
                if (lerpMaterial.GetFloat("_transition") < 1f)
                {
                    transitionActive = true;
                    lerpMaterial.SetFloat("_transition",
                        lerpMaterial.GetFloat("_transition") + 0.02f);
                }
                else
                {
                    transitionActive = false;
                    lerpMaterial.SetFloat("_transition", 1f);
                    active = false;
                }

                yield return Yielders.EndOfFrame;
            }
        }

        private bool stateChanged;
        

        string GetLastActive()
        {
            switch (lastState)
            {
                case EyeMaterialState.NotActive:
                    return "";
                case EyeMaterialState.Disabled:
                    return "";
                case EyeMaterialState.Sharingan:
                    return "sharinganBase";
                case EyeMaterialState.MangekyoSharingan:
                    return "mangekyoSharingan";
                case EyeMaterialState.Rinnegan:
                    return "rinnayganBase";
                default:
                    return "";
            }
        }

        static void DisableActiveDojutsu()
        {
            if (activeDojutsuSkills.Count < 1) return;
            foreach (var reference in activeDojutsuSkills)
            {
                Player.local.creature.container.RemoveContent(reference);
            }

            activeDojutsuSkills.RemoveAll(data => data.GetType() is string);
            activeDojutsuSkills.TrimExcess();
        }

        public void SetMangekyoTexture()
        {
            if (mangekyoAbilities.Contains("Amaterasu"))
            {
                mangekyoTexture = sasukeMangekyoSharingan.DeepCopyByExpressionTree();
            }

            if (mangekyoAbilities.Contains("Kamui"))
            {
                mangekyoTexture = kamuiMangekyoSharingan;
            }
        }
        List<string> SelectedActiveDojutsu(EyeMaterialState type)
        {
            switch (type)
            {
                case EyeMaterialState.Sharingan:
                    break;
                case EyeMaterialState.MangekyoSharingan:
                    return mangekyoAbilities;
                case EyeMaterialState.Rinnegan:
                    return rinnegan;
                default:
                    return null;
            }

            return null;
        }

        void ActivateDojustu(List<string> toActivate)
        {
            foreach (var skill in toActivate)
            {
                if (skill.Contains("Init")) Player.local.creature.container.AddSpellContent(skill);
                else  Player.local.creature.container.AddSkillContent(skill);
                activeDojutsuSkills.Add(skill);
            }
        }
        
        
        IEnumerator Update()
        {
            while (true)
            {
                if (!transitionActive && lastState != state && defaultColor)
                {
                    Debug.Log("State changed to: " + state);
                    switch (state)
                    {
                        case EyeMaterialState.Disabled:
                            DisableActiveDojutsu();
                            Execute(GetLastActive(), "");
                            lastState = EyeMaterialState.Disabled;
                            JutsuEntry.local.lastActive = "";
                            break;
                        case EyeMaterialState.Sharingan:
                            DisableActiveDojutsu();
                            Execute(GetLastActive(), "sharinganBase");
                            lastState = EyeMaterialState.Sharingan;
                            JutsuEntry.local.lastActive = "sharinganBase";
                            break;
                        case EyeMaterialState.MangekyoSharingan:
                            DisableActiveDojutsu();
                            ActivateDojustu(SelectedActiveDojutsu(EyeMaterialState.MangekyoSharingan));
                            SetMangekyoTexture();
                            if (!mangekyoSound)
                            {
                                var reference = GameObject.Instantiate(JutsuEntry.local.mangekyoSFX);
                                mangekyoSound = reference.gameObject.GetComponent<AudioSource>();
                                mangekyoSound.transform.position = Player.local.head.transform.position;
                                mangekyoSound.transform.parent = Player.local.head.transform;
                            }
                            else mangekyoSound.Play();

                            Execute(GetLastActive(), "mangekyoSharingan");
                            lastState = EyeMaterialState.MangekyoSharingan;
                            JutsuEntry.local.lastActive = "mangekyoSharingan";
                            break;
                        case EyeMaterialState.Rinnegan:
                            DisableActiveDojutsu();
                            ActivateDojustu(SelectedActiveDojutsu(EyeMaterialState.Rinnegan));
                            Execute(GetLastActive(), "rinnayganBase");
                            lastState = EyeMaterialState.Rinnegan;
                            JutsuEntry.local.lastActive = "rinnayganBase";
                            break;
                    }
                }

                yield return null;
            }
        }
    }
}