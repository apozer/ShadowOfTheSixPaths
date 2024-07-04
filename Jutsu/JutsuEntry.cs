using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using static UnityEngine.AddressableAssets.Addressables;
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
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowClone.SFX.Spawn", obj => { shadowCloneSpawnSFX = obj; },
                "ShadowCloneSpawnSFX");
            Catalog.LoadAssetAsync<GameObject>("SOTSP.Jutsu.YangRelease.ShadowClone.SFX.Death", obj => { shadowCloneDeathSFX = obj; },
                "ShadowCloneDeathSFX");
            
            return base.LoadAddressableAssetsCoroutine();
        }

        async void AsyncSetup()
        {
            await Task.Run(() =>
            {
                SequenceManagement();
                //coroutineManager = coroutine.AddComponent<CoroutineManager>();
                //Add new component of Coroutine Manager to coroutine manager reference
                //Prevents game from getting hung up when using speech recognition engine.
                Application.quitting += () => Process.GetCurrentProcess().Kill();
                
                //get GameObjects for VFX or SFX for jutsus
                /*logData = Catalog.GetData<ItemData>("JutsuLog");
                Catalog.LoadAssetAsync<GameObject>("apoz123.Jutsu.YinRelease.VFX.ShadowPosession",
                    gameobject => { shadow = gameobject;}, "ShadowPossesion");
                Catalog.LoadAssetAsync<GameObject>("apoz123.Jutsu.YinRelease.VFX.ShadowPossessionJutsu.SFX",
                    go => { shadowSFX = go;}, "ShadowPossessionSFX");*/
                //Catalog.LoadAssetAsync<GameObject>("apoz123.LightningStyle.Chidori", go => { chidori = go;}, "ChidoriVFX");
                //GameManager.local.StartCoroutine(Catalog.LoadAddressableAssetsCoroutine());

               // GameManager.local.StartCoroutine(loadAssetCoroutine);
                //GameManager.local.StartCoroutine(assetCoroutine);
                /*Catalog.LoadAssetAsync<GameObject>("apoz123.Jutsu.WindRelease.VFX.VacuumBlade", data =>{this.vacuumBlade = data;}, "VacuumBladeVFX");*/
                /*Catalog.LoadAssetAsync<GameObject>("debugObject", data =>{this.debugObject = data;}, "DebugObject");*/
            });
        }

        public Step root;
        public float jutsuActiveTime = 10f;
        public int multiShadowCloneMax = 10;
        public Dictionary<JutsuSkill, bool> canBeActiveJutsu = new Dictionary<JutsuSkill, bool>();
        public void SequenceManagement()
        {
            root = Step.Start();
        }
    }
    
    
}