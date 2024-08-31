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


        //Amaterasu VFX
        internal GameObject amaterasuVFX;
        public Material lerpMaterial;
        //Sharingan Eye Materials
        public Texture2D threeTomoeSharingan;
        public Texture2D sasukeMangekyoSharingan;
        public Texture2D kamuiMangekyoSharingan;
        
        //Rinnegan Eye Materials
        public Texture2D rinneganBase;
        
        
        //Tracking Eye Materials
        internal string lastActive = "";
        private string currentlyActive = "";
        
        
        //Sharingan SFX
        public GameObject sharinganSFX;
        public GameObject sharinganDisableSFX;
        
        //Rinnegan SFX
        public GameObject rinneganStartSFX;
        
        //Kamui
        public GameObject kamuiVFX;
        //Sharingan SFX
        public GameObject mangekyoSFX;

        public Item activeChidori;
        
        
        // Susanoo's
        public GameObject sasukeSusanooRibcage;
       
        public override void OnCatalogRefresh()
        {
            //Only want one instance of the loader running
            if (local != null) return;
            local = this;
            AsyncSetup();
        }
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
            
            Catalog.LoadAssetAsync<Texture2D>("SOTSP.Jutsu.Sharingan.Mangekyo.Sasuke",
                obj => { sasukeMangekyoSharingan = obj;}, "SasukeMangekyoSharingan");
            Catalog.LoadAssetAsync<Texture2D>("SOTSP.Jutsu.Sharingan.Mangekyo.Kamui.Texture",
                obj => { kamuiMangekyoSharingan = obj;}, "KamuiMangekyoSharingan");
            
            //Rinnegan Eye Materials
            Catalog.LoadAssetAsync<Texture2D>("SOTSP.Jutsu.Rinnegan.BaseRinnegan.Texture", obj =>
            {
                rinneganBase = obj;
            }, "Rinnegan");
            
            //Amaterasu VXF
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.MangekyoSharingan.Amaterasu.VFX", obj =>
            {
                amaterasuVFX = obj;
            }, "AmaterasuVFX");
            
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.Mangekyo.SFX", obj => { mangekyoSFX = obj;}, "MangekyoSFX");
            
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.MangekyoSharingan.Kamui.VFX", obj => { kamuiVFX = obj;}, "KamuiVFX");
            
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.Mangekyo.Susanoo.Ribcage.Sasuke",
                obj => { sasukeSusanooRibcage = obj;}, "SasukeSusanooRibcage");
            
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.SFX", obj => { sharinganSFX = obj; Debug.Log("SHARINGAN SFX IS: " + sharinganSFX);}, "SharinganSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Sharingan.Disable.SFX", obj => { sharinganDisableSFX = obj;},"SharinganDisableSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.Rinnegan.SFX", obj => { rinneganStartSFX = obj;}, "RinneganStartSFX");
            return base.LoadAddressableAssetsCoroutine();
        }

        
        async void AsyncSetup()
        {
            await Task.Run(() =>
            {
                DojutsuTracking.mInstance.sharinganBase = threeTomoeSharingan;
                DojutsuTracking.mInstance.lerpMaterial = this.lerpMaterial;
                DojutsuTracking.mInstance.rinneganBase = rinneganBase;
                DojutsuTracking.mInstance.sasukeMangekyoSharingan = sasukeMangekyoSharingan;
                SequenceManagement();
                //Prevents game from getting hung up when using speech recognition engine.
                Application.quitting += () => Process.GetCurrentProcess().Kill();
            });
        }

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
}