using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.DebugViz;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Jutsu
{
    /**
     * Jutsu Entry - Entry point to the Jutsu mod
     * CustomData class from ThunderRoad
     * Use this to import VFX GameObjects, or for more complex things such as voice activation
     */
    public class JutsuEntry : CustomData
    {
        //static reference for Singleton structure
        public static JutsuEntry local;
        
        //Reference to global coroutine manager (Useful for non monbehaviour classes)
        //public GameObject coroutine = new GameObject();
        //public CoroutineManager coroutineManager;
        public SDFBakeTool bakeTool; // = new GameObject().AddComponent<SDFBakeTool>();*/
        
        //Speech Recognition Engine object
        //private SpeechRecognitionEngine recognizer;
        
        //Gameobject for Substitution Jutsu
        internal ItemData logData;

        internal ItemData chidoriItemData;
        
        //GameObjects for Shadow Possession Jutsu
        public GameObject shadow;
        public GameObject shadowSFX;
        
        //GameObjects for Chidori 
        //public GameObject chidori;
        public GameObject chidoriStartSFX;
        public GameObject chidoriLoopSFX;
        
        //GameObjects for Rasengan
        public GameObject rasenganStartSFX;
        public GameObject rasenganLoopSFX;
        
        //VFX for Vacuum Blade
        public GameObject vacuumBlade;
        public GameObject debugObject;
        
        //Hand Signs
        public GameObject monkeySealRightTransform;
        public GameObject monkeySealLeftTransform;
        public bool spellWheelDisabled = false;
        
        //Water Clone
        public string waterMaterialAddress = "SOTSP.Jutsu.WaterRelease.VFX.WaterMaterial";
        public GameObject waterVFX;
        public GameObject sound1;
        public GameObject sound2;
        public Material waterMaterial;
        
        //Shadow Clone
        public GameObject shadowCloneVFX;
        public GameObject shadowCloneSpawnSFX;
        public GameObject shadowCloneDeathSFX;
        
        //Shadow Shuriken
        public GameObject shadowShurikenJutsu;


        public Material lerpMaterial;
        //Sharingan Eye Materials
        public Texture2D threeTomoeSharingan;
        
        //Rinnegan Eye Materials
        public Texture2D rinneganBase;
        
        
        //Tracking Eye Materials
        internal string lastActive = "";
        private string currentlyActive = "";
       
        public override void OnCatalogRefresh()
        {
            //Only want one instance of the loader running
            if (local != null) return;
            local = this;
            AsyncSetup();
            
        }

        internal GameObject lerpReferenceGO = new GameObject();
        internal LerpMaterialChanges _lerpMaterialChanges;
        public override IEnumerator LoadAddressableAssetsCoroutine()
        {
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.LightningRelease.Chidori.SFX.start", go => { chidoriStartSFX = go;}, "ChidoriStartSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.LightningRelease.Chidori.SFX.loop",
                go => { chidoriLoopSFX = go;}, "ChidoriLoopSFX");
            
            //Rasengan Audio
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Chakra.Rasengan.SFX.Start", go => { rasenganStartSFX = go;}, "RasenganStartSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Chakra.Rasengan.SFX.Loop",
                go => { rasenganLoopSFX = go;}, "RasenganLoopSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.HandSigns.MonkeyLeft", go => { monkeySealLeftTransform = go;}, "MonkeySealLeftTransform");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.HandSigns.MonkeyRight", go => { monkeySealRightTransform = go;}, "MonkeySealRightTransform");
            
            //Shadow Possesion
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YinRelease.ShadowPossession", go => { shadow = go;}, "ShadowVFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YinRelease.SFX.ShadowPossession", go => { shadowSFX = go;}, "ShadowSFX");
            
            //Water Clone jutsu
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.WaterRelease.WaterClone.VFX.Waterfall", gameobject => { waterVFX = gameobject; },
                "WaterFallEffect");

            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.WaterRelease.WaterClone.SFX.Spawn",
                gameobject => { sound1 = gameobject; }, "WaterSFX1");

            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.WaterRelease.WaterClone.SFX.Splash", obj => { sound2 = obj; },
                "SplashSFX");

            Catalog.LoadAssetAsync<Material>(waterMaterialAddress,
                waterMaterial => { this.waterMaterial = waterMaterial; },
                "WaterMaterial");
            
            
            //Shadow Clone Jutsu data
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowClone.VFX", obj => { shadowCloneVFX = obj; },
                "ShadowCloneVFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowShuriken.VFX", obj => { shadowShurikenJutsu = obj;},
                "ShadowShurikenVFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowClone.SFX.Spawn", obj => { shadowCloneSpawnSFX = obj; },
                "ShadowCloneSpawnSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowClone.SFX.Death", obj => { shadowCloneDeathSFX = obj; },
                "ShadowCloneDeathSFX");
            
            //Lerp Material
            Catalog.LoadAssetAsync<Material>("SOTSP.Jutsu.Materials.Lerp", obj => { 
                lerpMaterial = obj;
            }, "Lerp Material");
            //Sharingan Eye Materials
            Catalog.LoadAssetAsync<Texture2D>("SOTSP.Jutsu.Sharingan.BaseSharingan.Texture", obj =>
            {
                threeTomoeSharingan = obj;
            }, "Three Tomoe Sharingan");
            
            //Rinnegan Eye Materials
            Catalog.LoadAssetAsync<Texture2D>("SOTSP.Jutsu.Rinnegan.BaseRinnegan.Texture", obj =>
            {
                rinneganBase = obj;
            }, "Rinnegan");
            return base.LoadAddressableAssetsCoroutine();
        }

        
        public Choices sharinganOptions = new Choices();
        SpeechRecognitionEngine recognizer;
        async void AsyncSetup()
        {
            await Task.Run(() =>
            {
                _lerpMaterialChanges = lerpReferenceGO.AddComponent<LerpMaterialChanges>();
                _lerpMaterialChanges.sharinganBase = threeTomoeSharingan;
                _lerpMaterialChanges.lerpMaterial = this.lerpMaterial;
                _lerpMaterialChanges.rinneganBase = rinneganBase;
                Debug.Log("Setting lerp changes");
                SequenceManagement();
                //Prevents game from getting hung up when using speech recognition engine.
                Application.quitting += () => Process.GetCurrentProcess().Kill();
                
                sharinganOptions.Add("Sharingan");
                sharinganOptions.Add("Rinnaygan");
                sharinganOptions.Add("Disable");
                recognizer = new SpeechRecognitionEngine();
                Grammar servicesGrammar = new Grammar(new GrammarBuilder(sharinganOptions));
                recognizer.RequestRecognizerUpdate();
                recognizer.LoadGrammarAsync(servicesGrammar);
                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
                recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            });
        }

        internal EyeMaterialState state = EyeMaterialState.NotActive;
        internal enum EyeMaterialState{
            NotActive,
            Disabled,
            Sharingan,
            Rinnegan
        }
        
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
                Debug.Log(e.Result.Text);
                if (e.Result.Confidence < 0.93f) return;
                if (!transitionActive)
                {
                    Debug.Log("Lepr Material Changes: " + _lerpMaterialChanges);
                    Debug.Log("Last active in async: " + lastActive);
                    if (lastActive.ToLower().Contains(e.Result.Text.ToLower())) return;

                    if (e.Result.Text.ToLower().Equals("disable"))
                    {
                        state = EyeMaterialState.Disabled;
                    }
                    if (e.Result.Text.ToLower().Equals("sharingan"))
                    {
                        Debug.Log("last active: " + lastActive);
                        state = EyeMaterialState.Sharingan;
                    }
                    if (e.Result.Text.ToLower().Equals("rinnaygan"))
                    {
                        state = EyeMaterialState.Rinnegan;
                    }
                }
        }

        internal bool transitionActive = false;

        public Step root;
        public float jutsuActiveTime = 10f;
        public int multiShadowCloneMax = 10;
        public List<Step> activeRoots = new List<Step>();
        public Dictionary<JutsuSkill, bool> canBeActiveJutsu = new Dictionary<JutsuSkill, bool>();
        public void SequenceManagement()
        {
            root = Step.Start();
        }
    }

    internal class LerpMaterialChanges : MonoBehaviour
    {
        public Material lerpMaterial;
        public Texture2D sharinganBase;
        public Texture2D rinneganBase;
        public Material defaultColor;
        public Texture2D defaultNormalMap;
        public Texture2D defaultMetallic;
        public Texture2D defaultEmission;
        private JutsuEntry.EyeMaterialState lastState = JutsuEntry.EyeMaterialState.NotActive;
        internal SkillData rinneganData;

        public void Execute(string lastActive, string nextActive)
        {
            Debug.Log("Executing code");
            
            Debug.Log("Thing 1 is: " + sharinganBase);
            Debug.Log("Thing 2 is: " + rinneganBase);
            Debug.Log("Thing 1 is: " + GetTransitionStart(lastActive));
            Debug.Log("Thing 2 is: " + GetTransitionStart(nextActive));
            SetMaterialData(GetTransitionStart(lastActive), GetTransitionStart(nextActive));
        }
        
        Texture GetTransitionStart(string lastActive)
        {
            switch (lastActive)
            {
                case "":
                    Debug.Log(defaultColor);
                    return defaultColor.GetTexture("_BaseMap");
                case "sharinganBase":
                    Debug.Log(sharinganBase);
                    return sharinganBase;
                case "rinnayganBase":
                    return rinneganBase;
                default:
                    return defaultColor.GetTexture("_BaseMap");
            }
        }
        public void SetMaterialData(Texture texture1, Texture texture2)
        {
            
            Debug.Log("Textures adding");
            lerpMaterial.SetFloat("_transition", 0f);
            lerpMaterial.SetTexture("_OriginalTexture", texture1);
            lerpMaterial.SetTexture("_normalOriginal", defaultNormalMap);
            lerpMaterial.SetTexture("_metallicOriginal", defaultMetallic);
            lerpMaterial.SetTexture("_normalSharingan", defaultNormalMap);
            lerpMaterial.SetTexture("_metallicSharingan", defaultMetallic);
            lerpMaterial.SetTexture("_sharinganTexture", texture2);
            Debug.Log("Textures added");
            GameManager.local.StartCoroutine(TransitionEyeMaterial());
        }
        
        public IEnumerator TransitionEyeMaterial()
        {
            Debug.Log("Hit the coroutine");
            bool active = true;
            while (active)
            {
                if (!lerpMaterial) yield return null;
                if (lerpMaterial.GetFloat("_transition") < 1f)
                {
                    JutsuEntry.local.transitionActive = true;
                    lerpMaterial.SetFloat("_transition",
                        lerpMaterial.GetFloat("_transition") + 0.02f);
                }
                else
                {
                    JutsuEntry.local.transitionActive = false;
                    lerpMaterial.SetFloat("_transition", 1f);
                    Debug.Log("ended transitions");
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
                case JutsuEntry.EyeMaterialState.NotActive:
                    return "";
                case JutsuEntry.EyeMaterialState.Disabled:
                    return "";
                case JutsuEntry.EyeMaterialState.Sharingan:
                    return "sharinganBase";
                case JutsuEntry.EyeMaterialState.Rinnegan:
                    return "rinnayganBase";
                default:
                    return "";
            }
        }
        private void Update()
        {
            if (!JutsuEntry.local.transitionActive  && lastState != JutsuEntry.local.state && defaultColor)
            {
                Debug.Log("Checked into update method in LerpMaterialChanges");
                switch (JutsuEntry.local.state)
                {
                    case JutsuEntry.EyeMaterialState.Disabled:
                        Execute(GetLastActive(), "");
                        Player.local.creature.container.RemoveContent("RinneganInit");
                        lastState = JutsuEntry.EyeMaterialState.Disabled;
                        JutsuEntry.local.lastActive = "";
                        break;
                    case JutsuEntry.EyeMaterialState.Sharingan:
                        Execute(GetLastActive(), "sharinganBase");
                        lastState = JutsuEntry.EyeMaterialState.Sharingan;
                        JutsuEntry.local.lastActive = "sharinganBase";
                        break;
                    case JutsuEntry.EyeMaterialState.Rinnegan:
                        Execute(GetLastActive(), "rinnayganBase");
                        lastState = JutsuEntry.EyeMaterialState.Rinnegan;
                        Player.local.creature.container.AddSkillContent("RinneganInit");
                        JutsuEntry.local.lastActive = "rinnayganBase";
                        break;
                }
            }
        }
    }
    
}