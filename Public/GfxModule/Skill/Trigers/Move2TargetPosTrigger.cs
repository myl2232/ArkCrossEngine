using ArkCrossEngine;
using SkillSystem;
using UnityEngine;

namespace GfxModule.Skill.Trigers
{
    public class Move2TargetPosTrigger : AbstractSkillTriger
    {
        enum TargetPosType
        {
            kNone,
            kOwnerLastTouchPos,
            kOwnerRandomPos
        }

        public override ISkillTriger Clone()
        {
            Move2TargetPosTrigger copy = new Move2TargetPosTrigger();
            copy.m_StartTime = m_StartTime;
            copy.m_MoveTime = m_MoveTime;
            copy.m_TargetPosType = m_TargetPosType;
            copy.m_RelativeCenter = m_RelativeCenter;
            copy.m_Radius = m_Radius;
            return copy;
        }

        public override void Reset()
        {
            m_IsInited = false;
        }

        protected override void Load(ScriptableData.CallData callData)
        {
            if (callData.GetParamNum() >= 2)
            {
                m_StartTime = long.Parse(callData.GetParamId(0));
                m_MoveTime = long.Parse(callData.GetParamId(1));
            }
        }

        protected override void Load(ScriptableData.FunctionData funcData)
        {
            ScriptableData.CallData callData = funcData.Call;
            if (callData != null)
            {
                Load(callData);
                for (int i = 0; i < funcData.Statements.Count; i++)
                {
                    ScriptableData.CallData stCall = funcData.Statements[i] as ScriptableData.CallData;
                    if (stCall == null)
                    {
                        continue;
                    }
                    if (stCall.GetId() == "ownerlasttouchpos")
                    {
                        LoadOwnerLastTouchPosConfig(stCall);
                    }
                    else if (stCall.GetId() == "ownerrandompos")
                    {
                        LoadOwnerRandomPosConfig(stCall);
                    }
                }
                /*
                foreach (ScriptableData.ISyntaxComponent statement in funcData.Statements) {
                  ScriptableData.CallData stCall = statement as ScriptableData.CallData;
                  if (stCall == null) {
                    continue;
                  }
                  if (stCall.GetId() == "ownerlasttouchpos") {
                    LoadOwnerLastTouchPosConfig(stCall);
                  } else if (stCall.GetId() == "ownerrandompos") {
                    LoadOwnerRandomPosConfig(stCall);
                  }
                }*/
            }
        }

        public override bool Execute(object sender, SkillInstance instance, long delta, long curSectionTime)
        {
            if (curSectionTime < m_StartTime)
            {
                return true;
            }
            GameObject obj = sender as GameObject;
            if (obj == null)
            {
                return false;
            }
            if (!m_IsInited)
            {
                Init(obj, instance);
            }
            if (m_MoveTime == 0)
            {
                TriggerUtil.MoveObjTo(obj, m_TargetPos);
                return false;
            }
            long passed_time = curSectionTime - m_StartTime;
            UnityEngine.Vector3 motion_pos = UnityEngine.Vector3.Lerp(m_StartPos, m_TargetPos, passed_time * 1.0f / m_MoveTime);
            TriggerUtil.MoveObjTo(obj, motion_pos);
            TriggerUtil.UpdateObjPosition(obj);
            return true;
        }

        private void Init(GameObject obj, SkillInstance instance)
        {
            m_IsInited = true;
            m_StartPos = obj.transform.position;
            switch (m_TargetPosType)
            {
                case TargetPosType.kOwnerLastTouchPos:
                    m_TargetPos = GetOwnerLastTouchPos(obj);
                    break;
                case TargetPosType.kOwnerRandomPos:
                    m_TargetPos = GetOwnerRandomPos(obj, m_RelativeCenter, m_Radius);
                    break;
                default:
                    m_TargetPos = obj.transform.position;
                    break;
            }
            UnityEngine.Vector3 direction = m_TargetPos - m_StartPos;
            if (direction != UnityEngine.Vector3.zero)
            {
                GfxSkillSystem.ChangeDir(obj, direction);
            }
        }

        private void LoadOwnerLastTouchPosConfig(ScriptableData.CallData callData)
        {
            m_TargetPosType = TargetPosType.kOwnerLastTouchPos;
        }

        private void LoadOwnerRandomPosConfig(ScriptableData.CallData stCall)
        {
            m_TargetPosType = TargetPosType.kOwnerRandomPos;
            if (stCall.GetParamNum() >= 2)
            {
                m_RelativeCenter = ScriptableDataUtility.CalcVector3(stCall.GetParam(0) as ScriptableData.CallData);
                m_Radius = float.Parse(stCall.GetParamId(1));
            }
        }

        private UnityEngine.Vector3 GetOwnerLastTouchPos(GameObject obj)
        {
            SharedGameObjectInfo obj_info = LogicSystem.GetSharedGameObjectInfo(obj);
            if (obj_info == null || obj_info.SummonOwnerActorId < 0)
            {
                return obj.transform.position;
            }
            SharedGameObjectInfo owner_info = LogicSystem.GetSharedGameObjectInfo(obj_info.SummonOwnerActorId);
            if (owner_info == null)
            {
                return obj.transform.position;
            }
            UnityEngine.Vector3 last_touch_pos = new UnityEngine.Vector3(owner_info.LastTouchX, owner_info.LastTouchY, owner_info.LastTouchZ);
            return last_touch_pos;
        }

        private UnityEngine.Vector3 GetOwnerRandomPos(GameObject obj, UnityEngine.Vector3 center, float radius)
        {
            SharedGameObjectInfo obj_info = LogicSystem.GetSharedGameObjectInfo(obj);
            if (obj_info == null || obj_info.SummonOwnerActorId < 0)
            {
                return obj.transform.position;
            }
            GameObject owner = LogicSystem.GetGameObject(obj_info.SummonOwnerActorId);
            if (owner == null)
            {
                return obj.transform.position;
            }
            System.Random random = new System.Random();
            float random_x = (float)(random.NextDouble() * radius);
            float random_z = (float)(random.NextDouble() * radius);
            UnityEngine.Vector3 random_pos = center;
            random_pos.x += random_x;
            random_pos.z += random_z;
            UnityEngine.Vector3 world_random_pos = owner.transform.TransformPoint(random_pos);
            return CalcValidRandomPos(owner.transform.position, world_random_pos);
        }

        private UnityEngine.Vector3 CalcValidRandomPos(UnityEngine.Vector3 start_pos, UnityEngine.Vector3 random_pos)
        {
            UnityEngine.Vector3 obj_pos = start_pos;
            UnityEngine.Vector3 end_pos = random_pos;
            end_pos.y += 3;
            obj_pos.y = end_pos.y;
            UnityEngine.RaycastHit hitinfo;
            int layer = 1 << LayerMask.NameToLayer("AirWall");
            if (Physics.Linecast(obj_pos, end_pos, out hitinfo, layer))
            {
                return TriggerUtil.GetGroundPos(hitinfo.point);
            }
            else
            {
                return TriggerUtil.GetGroundPos(end_pos);
            }
        }

        private long m_MoveTime;
        private TargetPosType m_TargetPosType = TargetPosType.kNone;
        private UnityEngine.Vector3 m_RelativeCenter;
        private float m_Radius;

        private bool m_IsInited = false;
        private UnityEngine.Vector3 m_StartPos;
        private UnityEngine.Vector3 m_TargetPos;
    }
}
