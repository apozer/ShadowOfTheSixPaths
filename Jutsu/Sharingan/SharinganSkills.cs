using System.Collections.Generic;
using System.Speech.Recognition;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class SharinganSkills : JutsuSkill
    {
        public List<string> mangekyoAbilities;
        public List<string> baseSharinganAbilities;
        public List<string> baseRinneganAbilities;
        private string skillId;
        public Choices sharinganOptions = new Choices();
        SpeechRecognitionEngine recognizer;

        internal override void CustomStartData()
        {
            sharinganOptions.Add("Disable");
            if(DojutsuTracking.mInstance.rinneganActivated || DojutsuTracking.mInstance.devMode) sharinganOptions.Add("Rinnaygan");
            if(DojutsuTracking.mInstance.sharinganActivated || DojutsuTracking.mInstance.devMode) sharinganOptions.Add("Sharingan");
            if (DojutsuTracking.mInstance.mangekyoActivated || DojutsuTracking.mInstance.devMode) sharinganOptions.Add("Mangekyo Sharingan");
            DojutsuTracking.mInstance.mangekyoActive += () =>
            {
                sharinganOptions.Add("Mangekyo Sharingan");
                DojutsuTracking.mInstance.state = EyeMaterialState.MangekyoSharingan;
            };
            DojutsuTracking.mInstance.sharinganActive += () =>
            {
                sharinganOptions.Add("Sharingan");
                DojutsuTracking.mInstance.state = EyeMaterialState.Sharingan;
            };
            DojutsuTracking.mInstance.rinneganActive += () =>
            {
                sharinganOptions.Add("Rinnaygan");
                DojutsuTracking.mInstance.state = EyeMaterialState.Rinnegan;
            };
            recognizer = new SpeechRecognitionEngine();
            Grammar servicesGrammar = new Grammar(new GrammarBuilder(sharinganOptions));
            recognizer.RequestRecognizerUpdate();
            recognizer.LoadGrammarAsync(servicesGrammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }

        internal override void CustomEndData()
        {
            if(recognizer != null)  recognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < 0.93f) return;
            Debug.Log(e.Result);
            if (!DojutsuTracking.mInstance.transitionActive && !Player.local.creature.isKilled)
            {
                if (DojutsuTracking.mInstance.lastActive.ToLower().Contains(e.Result.Text.ToLower())) return;

                if (e.Result.Text.ToLower().Equals("disable"))
                {
                    DojutsuTracking.mInstance.state = EyeMaterialState.Disabled;
                }
                if (e.Result.Text.ToLower().Equals("sharingan"))
                {
                    DojutsuTracking.mInstance.state = EyeMaterialState.Sharingan;
                }
                if (e.Result.Text.ToLower().Equals("mangekyo sharingan"))
                {
                    DojutsuTracking.mInstance.state = EyeMaterialState.MangekyoSharingan;
                }
                if (e.Result.Text.ToLower().Equals("rinnaygan"))
                {
                    DojutsuTracking.mInstance.state = EyeMaterialState.Rinnegan;
                }
            }
        }
    }
}