using System.Speech.Recognition;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class SharinganSkills : JutsuSkill
    {
        private string skillId;
        public Choices sharinganOptions = new Choices();
        SpeechRecognitionEngine recognizer;

        internal override void CustomStartData()
        {
            /*sharinganOptions.Add("Sharingan");*/
            sharinganOptions.Add("Rinnaygan");
            sharinganOptions.Add("Disable");
            if (DojutsuTracking.mInstance.mangekyoActivated || DojutsuTracking.mInstance.devMode) sharinganOptions.Add("Mangekyo Sharingan");
            DojutsuTracking.mInstance.mangekyoActive += () =>
            {
                sharinganOptions.Add("Mangekyo Sharingan");
                DojutsuTracking.mInstance.state = EyeMaterialState.MangekyoSharingan;
            }; 
            recognizer = new SpeechRecognitionEngine();
            Grammar servicesGrammar = new Grammar(new GrammarBuilder(sharinganOptions));
            recognizer.RequestRecognizerUpdate();
            recognizer.LoadGrammarAsync(servicesGrammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
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