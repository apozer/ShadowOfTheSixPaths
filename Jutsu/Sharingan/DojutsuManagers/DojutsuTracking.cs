using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public AudioSource sharinganSound;
        public AudioSource sharinganDisableSound;
        public AudioSource rinneganSound;

        public bool transitionActive = false;
        public string lastActive = "";
        public bool mangekyoActivated = false;
        public bool sharinganActivated = false;
        public bool rinneganActivated = false;
        public bool devMode = false;
        private string devModeMangekyo = "KamuiSharinganSkills";
        
        public delegate void MangekyoActive();

        public delegate void SharinganActive();

        public delegate void RinneganActive();

        public event MangekyoActive mangekyoActive;
        public event SharinganActive sharinganActive;
        public event RinneganActive rinneganActive;

        internal static  List<string> activeDojutsuSkills = new List<string>();

        private List<string> mangekyoAbilities;
        private List<string> sharinganAbilities;
        private List<string> rinneganAbilties;
        
        //tracker for Sharingan Slow active creature
        public Creature activeCreature;
        
        private bool raceInstantiated = false;

        public void SetMangekyoAbilities(List<string> list)
        {
            mangekyoAbilities = list;
        }

        public void SetSharinganAbilities(List<string> list)
        {
            sharinganAbilities = list;
        }
        public void SetRinneganAbilities(List<string> list)
        {
            rinneganAbilties = list;
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
                this.state = EyeMaterialState.Disabled;
                if (MangekyoTracker.local.rinnegan && !devMode)
                {
                    if (creature.container.HasSkillContent("BaseRinnegan"))
                    {
                        creature.container.RemoveContent("BaseRinnegan");
                        if (creature.container.HasSkillContent("BaseSharinganSkill"))
                        {
                            creature.container.RemoveContent("BaseSharinganSkill");
                        }
                        else if (creature.container.HasSkillContent(MangekyoTracker.local.mangekyoAbility))
                        {
                            creature.container.RemoveContent(MangekyoTracker.local.mangekyoAbility);
                        }
                        
                    }
                    creature.container.AddSkillContent("BaseRinnegan");
                    mangekyoActivated = true;
                    sharinganActivated = true;
                    rinneganActivated = true;
                }
                else if (MangekyoTracker.local.mangekyoAbility != null  && !devMode)
                {
                    if (creature.container.HasSkillContent(MangekyoTracker.local.mangekyoAbility))
                    {
                        creature.container.RemoveContent(MangekyoTracker.local.mangekyoAbility);
                        if (creature.container.HasSkillContent("BaseSharinganSkill"))
                        {
                            creature.container.RemoveContent("BaseSharinganSkill");
                        }
                    }

                    creature.container.AddSkillContent(MangekyoTracker.local.mangekyoAbility);

                    mangekyoActivated = true;
                    sharinganActivated = true;
                }
                else if(MangekyoTracker.local.sharinganTier != null && !devMode)
                {
                    if (creature.container.HasSkillContent("BaseSharinganSkill"))
                    {
                        creature.container.RemoveContent("BaseSharinganSkill");
                    }
                    creature.container.AddSkillContent("BaseSharinganSkill");
                    sharinganActivated = true;
                }
                else
                {
                    mangekyoActivated = false;
                    sharinganActivated = false;
                    rinneganActivated = false;
                }

                if (!devMode && !sharinganActivated)
                {
                    EventManager.onCreatureKill += (creatureKilled, player, instance, time) =>
                    {
                        if (!creature.isPlayer)
                        {
                            var random = Random.Range(0, 1000);
                            if (random / 1000 < 0.99f)
                            {
                                creature.container.AddSkillContent("BaseSharinganSkill");
                                sharinganActivated = true;
                                MangekyoTracker.local.mangekyoAbility = "ThreeTomoe";
                                GameManager.local.StartCoroutine(MangekyoTracker.local.SaveJsonData());
                                sharinganActive?.Invoke();
                            }
                        }
                    };
                }
                if (!devMode && !mangekyoActivated && sharinganActivated)
                {
                    EventManager.onCreatureKill += (creatureKilled, player, instance, time) =>
                    {
                        if (!creature.isPlayer)
                        {
                            var random = Random.Range(0, 1000);
                            if (random / 1000 < 0.99f)
                            {
                                var ability = ReturnRandomMangekyoAbility();
                                creature.container.RemoveContent("BaseSharinganSkill");
                                creature.container.AddSkillContent(ability);
                                MangekyoTracker.local.mangekyoAbility = ability;
                                mangekyoActivated = true;
                                GameManager.local.StartCoroutine(MangekyoTracker.local.SaveJsonData());
                                mangekyoActive?.Invoke();
                            }
                        }
                    };
                }

                if (!devMode && !rinneganActivated && mangekyoActivated)
                {
                    EventManager.onCreatureKill += (creatureKilled, player, instance, time) =>
                    {
                        if (!creature.isPlayer)
                        {
                            var random = Random.Range(0, 1000);
                            if (random / 1000 < 0.99f)
                            {
                                creature.container.RemoveContent("BaseRinnegan");
                                creature.container.AddSkillContent("BaseRinnegan");
                                MangekyoTracker.local.rinnegan = true;
                                rinneganActivated = true;
                                GameManager.local.StartCoroutine(MangekyoTracker.local.SaveJsonData());
                                rinneganActive?.Invoke();
                            }
                        }
                    };
                }
                else if(devMode) creature.container.AddSkillContent(devModeMangekyo);
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

        private bool resetDisable = false;
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            if (mInstance != null) return;
            mInstance = this;
            Application.quitting += () =>
            {
                DisableActiveDojutsu();
                resetDisable = false;
            };
            EventManager.onLevelLoad += (data, mode, time) =>
            {
                this.raceInstantiated = false;
                resetDisable = true;
            };
            Player.onSpawn += playerSpawned =>
            {
                playerSpawned.onCreaturePossess += creaturePosses =>
                {
                    if (creaturePosses.currentEthnicGroup.id.ToLower().Equals("uchiha"))
                    {
                        if (raceInstantiated) return;
                        GameManager.local.StartCoroutine(WaitForRenderers(creaturePosses));

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
            foreach (var value in Player.local.creature.container.contents)
            {
                if (value.GetType() == typeof(SharinganSkills))
                {
                    Debug.Log(value.referenceID);
                }
            }
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
                    return sharinganAbilities;
                case EyeMaterialState.MangekyoSharingan:
                    return mangekyoAbilities;
                case EyeMaterialState.Rinnegan:
                    return rinneganAbilties;
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
                            if (!sharinganDisableSound)
                            {
                                var reference = GameObject.Instantiate(JutsuEntry.local.sharinganDisableSFX);
                                sharinganDisableSound = reference.gameObject.GetComponent<AudioSource>();
                                sharinganDisableSound.transform.position = Player.local.head.transform.position;
                                sharinganDisableSound.transform.parent = Player.local.head.transform;
                            }
                            else sharinganDisableSound.Play();
                            Execute(GetLastActive(), "");
                            lastState = EyeMaterialState.Disabled;
                            JutsuEntry.local.lastActive = "";
                            break;
                        case EyeMaterialState.Sharingan:
                            DisableActiveDojutsu();
                            ActivateDojustu(SelectedActiveDojutsu(EyeMaterialState.Sharingan));
                            if (!sharinganSound)
                            {
                                var reference = GameObject.Instantiate(JutsuEntry.local.sharinganSFX);
                                sharinganSound = reference.gameObject.GetComponent<AudioSource>();
                                sharinganSound.transform.position = Player.local.head.transform.position;
                                sharinganSound.transform.parent = Player.local.head.transform;
                            }
                            else sharinganSound.Play();
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
                            if (!rinneganSound)
                            {
                                var reference = GameObject.Instantiate(JutsuEntry.local.rinneganStartSFX);
                                rinneganSound = reference.gameObject.GetComponent<AudioSource>();
                                rinneganSound.transform.position = Player.local.head.transform.position;
                                rinneganSound.transform.parent = Player.local.head.transform;
                            }
                            else rinneganSound.Play();
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