using ArkCrossEngine;
using SkillSystem;

namespace GfxModule.Skill.Trigers
{
    public class ExchangePositionTrigger : AbstractSkillTriger
    {
        public override ISkillTriger Clone()
        {
            ExchangePositionTrigger copy = new ExchangePositionTrigger();
            copy.m_StartTime = m_StartTime;
            return copy;
        }

        public override void Reset()
        {
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 1)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
            }
        }

        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            if (curSectionTime < m_StartTime)
            {
                return true;
            }
            UnityEngine.GameObject obj = sender as UnityEngine.GameObject;
            if (obj == null)
            {
                return false;
            }
            SharedGameObjectInfo owner_info = LogicSystem.GetSharedGameObjectInfo(obj);
            if (owner_info.Summons.Count <= 0)
            {
                return false;
            }
            UnityEngine.GameObject first_summon = LogicSystem.GetGameObject(owner_info.Summons[0]);
            if (first_summon == null)
            {
                return false;
            }
            UnityEngine.Vector3 summon_pos = first_summon.transform.position;
            first_summon.transform.position = obj.transform.position;
            obj.transform.position = summon_pos;
            LogicSystem.NotifyGfxUpdatePosition(obj, obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
            LogicSystem.NotifyGfxUpdatePosition(first_summon, first_summon.transform.position.x,
                first_summon.transform.position.y, first_summon.transform.position.z);
            return false;
        }
    }
}
