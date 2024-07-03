﻿using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Jutsu
{
    public class JutsuSkill : SkillData
    {
        private SpellCaster casterLeft;
        private SpellCaster casterRight;
        private string spellInstanceId = "";
        private bool jutsuActivated;
        private bool jutsuTimerActivated;
        private Seals seals;
        private Coroutine activeJutsuCoroutine;
        private Coroutine activeJutsuTimerCoroutine;

        public void SetActivated(bool state)
        {
            this.jutsuActivated = state;
        }
        public bool GetActivated()
        {
            return this.jutsuActivated;
        }
        public Seals GetSeals()
        {
            return this.seals;
        }
        public void SetSpellInstanceID(string id)
        {
            this.spellInstanceId = id;
        }
        public string GetSpellInstanceID()
        {
            return this.spellInstanceId;
        }
        public void SetSpellCasters(SpellCaster left, SpellCaster right)
        {
            this.casterLeft = left;
            this.casterRight = right;
        }
        public SpellCaster GetSpellCasterLeft()
        {
            return this.casterLeft;
        }
        public SpellCaster GetSpellCasterRight()
        {
            return this.casterRight;
        }
        public bool SetJutsuTimerActivated(bool state)
        {
            return this.jutsuTimerActivated = state;
        }
        public bool GetJutsuTimerActivated()
        {
            return this.jutsuTimerActivated;
        }

        public void SetJutsuTimerActivatedCoroutine(Coroutine coroutine)
        {
            this.activeJutsuTimerCoroutine = coroutine;
        }
        
        
        

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            seals = new Seals();
            GameManager.local.StartCoroutine(WaitForPlayer());
            jutsuActivated = false;
            jutsuTimerActivated = false;

        }

        private IEnumerator WaitForPlayer()
        {
            yield return new WaitUntil(() => Player.local != null);
            Debug.Log("Set up for after player is done");
            this.casterLeft = Player.local.handLeft.ragdollHand.caster;
            this.casterRight = Player.local.handRight.ragdollHand.caster;
            CustomStartData();
            activeJutsuCoroutine = GameManager.local.StartCoroutine(JutsuStart());
        }
        
        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);
            GameManager.local.StopCoroutine(this.activeJutsuCoroutine);
        }

        internal virtual IEnumerator JutsuStart()
        {
            yield return null;
        }

        internal IEnumerator JutsuActive()
        {
            yield return new WaitForSeconds(JutsuEntry.local.jutsuActiveTime);
            Debug.Log("Jutsu Timer ended");
            SetActivated(false);
            SetJutsuTimerActivated(false);
            Debug.Log(JutsuEntry.local.spellWheelDisabled);
            SpellWheelReset();
        }

        internal void StopJutsuActiveTimer()
        {
            if (this.activeJutsuTimerCoroutine != null)
            {
                GameManager.local.StopCoroutine(this.activeJutsuTimerCoroutine);
            }
        }

        internal void SpellWheelCheck(bool ignoreReset = false)
        {
            if (GetSeals().HandDistance())
            {
                if (CheckSpellType())
                {
                    if (!JutsuEntry.local.spellWheelDisabled)
                    {
                        GetSpellCasterLeft().DisableSpellWheel(this);
                        GetSpellCasterRight().DisableSpellWheel(this);
                        JutsuEntry.local.spellWheelDisabled = true;
                    }
                }
            }
            else
            {
                if (CheckSpellType() && !GetActivated())
                {
                    if (ignoreReset)
                    {
                        if (JutsuEntry.local.spellWheelDisabled)
                        {
                            GetSpellCasterLeft().AllowSpellWheel(this);
                            GetSpellCasterRight().AllowSpellWheel(this);
                            JutsuEntry.local.spellWheelDisabled = false;
                        }

                    }
                    else
                    {
                        if (JutsuEntry.local.spellWheelDisabled)
                        {
                            JutsuEntry.local.root.Reset();
                            GetSpellCasterLeft().AllowSpellWheel(this);
                            GetSpellCasterRight().AllowSpellWheel(this);
                            JutsuEntry.local.spellWheelDisabled = false;
                        }
                    }
                }
            }
        }
        internal void SpellWheelReset()
        {
            if (JutsuEntry.local.spellWheelDisabled)
            {
                JutsuEntry.local.root.Reset();
                GetSpellCasterLeft().AllowSpellWheel(this);
                GetSpellCasterRight().AllowSpellWheel(this);
                JutsuEntry.local.spellWheelDisabled = false;
            }
        }

        internal virtual void CustomStartData()
        {
            
        }
        internal bool CheckSpellType()
        {
            return this.spellInstanceId != "" && ((GetSpellCasterLeft().spellInstance != null && GetSpellCasterLeft().spellInstance.id.Equals(this.spellInstanceId)) ||
                   (GetSpellCasterRight().spellInstance != null && GetSpellCasterRight().spellInstance.id.Equals(this.spellInstanceId)));
        }
    }
}