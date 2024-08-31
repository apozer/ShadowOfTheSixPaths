using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class SharinganSlow : JutsuSkill
    {
        private bool activateSharinganSlow = true;
        private bool timeSlowed = false;
        private Object speedModifier;
        private bool chidoriFinished = false;
        private float speedMultiplier = 5f;
        private bool fellOrDead = false;
        internal override IEnumerator JutsuStart()
        {
            while (true)
            {
                if (chidoriFinished && !JutsuEntry.local.activeChidori)
                {
                        timeSlowed = false;
                        chidoriFinished = false;
                        TimeManager.SetTimeScale(1f);
                        Player.local.locomotion.globalMoveSpeedMultiplier = 1f;
                }
                if (activateSharinganSlow && !JutsuEntry.local.activeChidori)
                {
                    foreach (var creature in Creature.allActive)
                    {
                        var distance = Vector3.Distance(Player.currentCreature.transform.position,
                            creature.transform.position);
                        if (creature.brain.isAttacking && creature.brain.currentTarget.isPlayer && distance < 2f && !timeSlowed && !DojutsuTracking.mInstance.activeCreature)
                        {
                            if (DojutsuTracking.mInstance.sharinganSound)
                            {
                                DojutsuTracking.mInstance.sharinganSound.Play();
                            }
                            timeSlowed = true;
                            float speedModifier = 4f;
                            TimeManager.SetTimeScale(1/speedModifier);
                            Player.local.locomotion.globalMoveSpeedMultiplier = (int) speedModifier;
                            DojutsuTracking.mInstance.activeCreature = creature;
                            DojutsuTracking.mInstance.activeCreature.OnKillEvent += OnKillEvent;
                            DojutsuTracking.mInstance.activeCreature.OnFallEvent += OnFallEvent;
                            GameManager.local.StartCoroutine(Timer());
                        }/*
                        else if (timeSlowed)
                        {
                            activeCreature.OnKillEvent -= OnKillEvent;
                            activeCreature.OnFallEvent -= OnFallEvent;
                            fellOrDead = false;
                            TimeManager.SetTimeScale(1f);
                            Player.local.locomotion.globalMoveSpeedMultiplier = 1f;
                        }*/
                    }
                    yield return null;
                }
                else
                {
                    if (!timeSlowed)
                    {
                        timeSlowed = true;
                        float speedModifier = 4f;
                        TimeManager.SetTimeScale(1/speedModifier);
                        Player.local.locomotion.globalMoveSpeedMultiplier = (int) speedModifier;
                        chidoriFinished = true;
                    }
                    yield return null;
                }
            }
        }

        private void OnFallEvent(Creature.FallState state)
        {
            if (state == Creature.FallState.Falling || state == Creature.FallState.NearGround ||
                state == Creature.FallState.Stabilizing)
            {
                if (!fellOrDead) fellOrDead = true;
            }
        }

        private void OnKillEvent(CollisionInstance collisioninstance, EventTime eventtime)
        {
            if(!fellOrDead) fellOrDead = true;
        }

        IEnumerator Timer()
        {
            yield return new WaitUntil(() => fellOrDead);
            DojutsuTracking.mInstance.activeCreature.OnKillEvent -= OnKillEvent;
            DojutsuTracking.mInstance.activeCreature.OnFallEvent -= OnFallEvent;
            DojutsuTracking.mInstance.activeCreature = null;
            fellOrDead = false;
            TimeManager.SetTimeScale(1f);
            Player.local.locomotion.globalMoveSpeedMultiplier = 1f;
            yield return new WaitForSeconds(3f);
            timeSlowed = false;
        }
    }
}