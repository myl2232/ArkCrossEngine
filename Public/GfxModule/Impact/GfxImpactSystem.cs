﻿using System;
using System.Collections.Generic;
using ArkCrossEngine;
using UnityEngine;
using UnityEngine.Profiling;

namespace GfxModule.Impact
{
    public sealed class GfxImpactSystem
    {
        private MyDictionary<int, ImpactCacheData> impactLogicDataCacheCont = null;
        private ObjectPool<ImpactLogicInfo> m_ImpactLogicPool = null;
        private ObjectPool<EffectInfo> m_EffectInfoPool = null;

        public void Init()
        {
            impactLogicDataCacheCont = new MyDictionary<int, ImpactCacheData>();
            MyDictionary<int, object> datacont = SkillConfigProvider.Instance.impactLogicDataMgr.GetData();
            foreach (KeyValuePair<int, object> pair in datacont)
            {
                int id = pair.Key;
                ImpactLogicData ild = pair.Value as ImpactLogicData;
                ImpactCacheData idlc = new ImpactCacheData();
                idlc.CurveAnimationInfo = new CurveInfo(ild.AnimationInfo);
                idlc.CurveLockFrameInfo = new CurveInfo(ild.LockFrameInfo);
                idlc.CurveMovementInfo = new CurveMoveInfo(ild.CurveMoveInfo);
                idlc.MoveInfolist = Converter.ConvertNumericList<float>(ild.CurveMoveInfo);
                if (!String.IsNullOrEmpty(ild.AdjustPoint))
                {
                    idlc.AdjustPointV3 = ImpactUtility.ConvertVector3D(ild.AdjustPoint);
                }
                impactLogicDataCacheCont.Add(id, idlc);
            }

            m_ImpactLogicPool = new ObjectPool<ImpactLogicInfo>();
            m_ImpactLogicPool.Init(128);

            m_EffectInfoPool = new ObjectPool<EffectInfo>();
            m_EffectInfoPool.Init(c_EffectPoolCapacity);

            GfxImpactSoundManager.Instacne.Init();
        }

        public void Reset()
        {
            for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
            {
                ImpactLogicInfo info = m_ImpactLogicInfos[i];
                if (null != info)
                {
                    info.Recycle();
                    m_ImpactLogicInfos.RemoveAt(i);
                }
            }
            m_ImpactLogicInfos.Clear();
        }

        public void Tick()
        {
            try
            {
                Profiler.BeginSample("GfxImpactSystem.Tick");
                for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
                {
                    ImpactLogicInfo info = m_ImpactLogicInfos[i];
                    if (null != info)
                    {
                        if (info.IsActive)
                        {
                            IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                            if (null != logic)
                            {
                                if (null != info.Target)
                                {
                                    if (info.Target.activeSelf)
                                    {
                                        logic.Tick(info);
                                    }
                                    else
                                    {
                                        info.Recycle();
                                        m_ImpactLogicInfos.RemoveAt(i);
                                    }
                                }
                            }
                        }
                        else
                        {
                            info.Recycle();
                            m_ImpactLogicInfos.RemoveAt(i);
                        }
                    }
                }
                GfxImpactSoundManager.Instacne.UpdateSoundInfos();
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        public void StopGfxImpact(int objId, int impactId)
        {
            GameObject obj = LogicSystem.GetGameObject(objId);
            if (null != obj)
            {
                ImpactLogicInfo info = GetImpactInfoById(objId, impactId);
                if (null != info && info.IsActive)
                {
                    IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                    logic.OnInterrupted(info);
                    info.IsActive = false;
                }
            }
        }
        public void StopAllGfxImpact(int objId)
        {
            for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
            {
                ImpactLogicInfo info = m_ImpactLogicInfos[i];
                if (null != info)
                {
                    if (info.IsActive)
                    {
                        GameObject target = info.Target;
                        if (target != null)
                        {
                            SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(target);
                            if (shareInfo.m_ActorId == objId)
                            {
                                IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                                logic.OnInterrupted(info);
                                info.IsActive = false;
                            }
                        }
                    }
                }
            }
        }

        public ImpactLogicInfo GetImpactInfoById(int objId, int impactId)
        {
            for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
            {
                ImpactLogicInfo info = m_ImpactLogicInfos[i];
                if (null != info)
                {
                    if (info.IsActive && info.ImpactId == impactId)
                    {
                        GameObject target = info.Target;
                        if (target != null)
                        {
                            SharedGameObjectInfo shareInfo = LogicSystem.GetSharedGameObjectInfo(target);
                            if (shareInfo.m_ActorId == objId)
                            {
                                return info;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void SendImpactToCharacter(int sender, int target, int impactId, float x, float y, float z, float dir, int forceLogicId)
        {
            GameObject senderObj = LogicSystem.GetGameObject(sender);
            GameObject targetObj = LogicSystem.GetGameObject(target);
            if (null == senderObj || null == targetObj)
            {
                LogSystem.Error("null obj");
            }
            SharedGameObjectInfo targetInfo = LogicSystem.GetSharedGameObjectInfo(targetObj);
            if (null == targetInfo || targetInfo.IsDead)
            {
                return;
            }
            GfxImpactSoundManager.Instacne.TryPlaySound(target, impactId);
            ImpactLogicData config = (ImpactLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactId);
            if (null != config)
            {
                if (config.MoveMode == (int)ImpactMovementType.Inherit)
                {
                    GetPreImpactMoveInfo(targetObj, out x, out y, out z, out dir);
                }
                bool needSendImpact = true;
                int logicId = config.ImpactGfxLogicId;
                if (-1 != forceLogicId)
                {
                    logicId = forceLogicId;
                }
                for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
                {
                    ImpactLogicInfo info = m_ImpactLogicInfos[i];
                    if (null != info)
                    {
                        if (info.IsActive)
                        {
                            if (info.Target.GetInstanceID() == targetObj.GetInstanceID())
                            {
                                if (info.LogicId == (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default || logicId == (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default)
                                {
                                    if (info.ImpactId != impactId)
                                    {
                                        continue;
                                    }
                                }
                                IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                                if (null != logic)
                                {
                                    needSendImpact = logic.OnOtherImpact(logicId, info, (info.ImpactId == impactId));
                                    if (needSendImpact)
                                    {
                                        info.Recycle();
                                        m_ImpactLogicInfos.RemoveAt(i);
                                    }
                                }
                            }
                        }
                    }
                }
                if (needSendImpact)
                {
                    SendImpactToCharacterImpl(senderObj, targetObj, impactId, x, y, z, dir, forceLogicId);
                }
                else
                {
                    LogicSystem.NotifyGfxStopImpact(senderObj, impactId, targetObj);
                }
            }
        }

        private void GetPreImpactMoveInfo(GameObject targetObj, out float x, out float y, out float z, out float dir)
        {
            x = 0; y = 0; z = 0; dir = 0;
            for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
            {
                ImpactLogicInfo info = m_ImpactLogicInfos[i];
                if (null != info)
                {
                    if (info.IsActive)
                    {
                        if (info.Target.GetInstanceID() == targetObj.GetInstanceID())
                        {
                            if (info.LogicId != (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default)
                            {
                                dir = info.ImpactSrcDir;
                                x = info.ImpactSrcPos.x;
                                y = info.ImpactSrcPos.y;
                                z = info.ImpactSrcPos.z;
                            }
                        }
                    }
                }
            }
        }
        public void SendDeadImpact(int targetId)
        {
            GameObject targetObj = LogicSystem.GetGameObject(targetId);
            if (null != targetObj)
            {
                bool needSendImpact = true;
                UnityEngine.Vector3 srcPos = UnityEngine.Vector3.zero;
                float srcDir = 0.0f;
                for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
                {
                    ImpactLogicInfo info = m_ImpactLogicInfos[i];
                    if (null != info)
                    {
                        if (info.IsActive)
                        {
                            if (info.Target.GetInstanceID() == targetObj.GetInstanceID())
                            {
                                if (info.LogicId != (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default)
                                {
                                    IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                                    if (null != logic)
                                    {
                                        logic.OnInterrupted(info);
                                    }
                                    needSendImpact = true;
                                    srcDir = info.ImpactSrcDir;
                                    srcPos = info.ImpactSrcPos;
                                    info.Recycle();
                                    m_ImpactLogicInfos.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
                if (needSendImpact)
                {
                    SendImpactToCharacterImpl(targetObj, targetObj, m_DeadImpactId, srcPos.x, srcPos.y, srcPos.z, srcDir, -1);
                }
            }
        }

        private void SendImpactToCharacterImpl(GameObject senderObj, GameObject targetObj, int impactId, float x, float y, float z, float dir, int forceLogicId)
        {
            ImpactLogicData config = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactId) as ImpactLogicData; if (null != config)
            {

                ImpactLogicInfo info = m_ImpactLogicPool.Alloc();
                //  ImpactLogicInfo info = new ImpactLogicInfo();
                info.Sender = senderObj;
                info.Target = targetObj;
                info.ImpactId = impactId;
                info.StartTime = Time.time;
                info.LogicId = config.ImpactGfxLogicId;
                if (-1 != forceLogicId)
                {
                    info.LogicId = forceLogicId;
                }
                info.IsActive = true;
                ImpactCacheData impactcachevalue;
                if (impactLogicDataCacheCont.TryGetValue(impactId, out impactcachevalue))
                {
                    info.AnimationInfo = impactcachevalue.CurveAnimationInfo;
                    info.LockFrameInfo = impactcachevalue.CurveLockFrameInfo;
                    info.MovementInfo = impactcachevalue.CurveMovementInfo;
                    info.ConfigCacheData = impactcachevalue;
                }
                else
                {
                    info.AnimationInfo = new CurveInfo(config.AnimationInfo);
                    info.LockFrameInfo = new CurveInfo(config.LockFrameInfo);
                    info.MovementInfo = new CurveMoveInfo(config.CurveMoveInfo);
                }
                info.AdjustPoint = UnityEngine.Quaternion.Euler(0, ImpactUtility.RadianToDegree(dir), 0) * info.ConfigCacheData.AdjustPointV3 + new UnityEngine.Vector3(x, y, z);
                info.AdjustAppend = config.AdjustAppend;
                info.AdjustDegreeXZ = config.AdjustDegreeXZ;
                info.AdjustDegreeY = config.AdjustDegreeY;
                info.ConfigData = config;
                info.Duration = config.ImpactTime / 1000;
                info.ImpactSrcPos = new UnityEngine.Vector3(x, y, z);
                info.ImpactSrcDir = dir;
                info.NormalEndPoint = GetImpactEndPos(info.Target.transform.position, UnityEngine.Quaternion.Euler(0, ImpactUtility.RadianToDegree(info.ImpactSrcDir), 0), info);
                info.NormalPos = info.Target.transform.position;
                info.OrignalPos = info.Target.transform.position;
                ShowDebugObject(info.AdjustPoint, 0);
                ShowDebugObject(info.NormalEndPoint, 1);
                for (int i = 0; i < config.EffectList.Count; i++)
                {
                    if (config.EffectList[i].Count > 0)
                    {
                        if (c_EffectPoolCapacity - m_EffectInfoPool.Count < c_MaxHitEffect)
                        {
                            EffectInfo effectinfo = m_EffectInfoPool.Alloc();
                            info.AddEffectDataByinfo(config.EffectList[i][UnityEngine.Random.Range(0, config.EffectList[i].Count)], effectinfo);
                        }
                        else
                        {
                        }
                    }
                }
                AddImpactInfo(info);
                IGfxImpactLogic logic = GfxImpactLogicManager.Instance.GetGfxImpactLogic(info.LogicId);
                if (logic != null)
                {
                    logic.StartImpact(info);
                }
                else
                {
                    LogSystem.Debug("SendImpactToCharacter: Can't find impact logic {0}", info.LogicId);
                }
            }
        }

        private void AddImpactInfo(ImpactLogicInfo addInfo)
        {
            for (int i = m_ImpactLogicInfos.Count - 1; i >= 0; --i)
            {
                ImpactLogicInfo info = m_ImpactLogicInfos[i];
                if (null != info && null != info.Target && null != addInfo && null != addInfo.Target)
                {
                    if (info.Target.GetInstanceID() == addInfo.Target.GetInstanceID() && addInfo.ImpactId == info.ImpactId)
                    {
                        if (info.LogicId == (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default || addInfo.LogicId == (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_Default)
                        {
                            info.Recycle();
                            m_ImpactLogicInfos.RemoveAt(i);
                        }
                    }
                }
            }
            m_ImpactLogicInfos.Add(addInfo);
        }
        public bool ShowDebug
        {
            get { return m_ShowDebug; }
        }

        private void ShowDebugObject(UnityEngine.Vector3 pos, int type)
        {
            if (true == m_ShowDebug)
            {
                string res = "";
                if (type == 0)
                {
                    res = "BlueCylinder";
                }
                else
                {
                    res = "RedCylinder";
                }
                GameObject obj = ResourceSystem.NewObject(res, 1.0f) as GameObject;
                if (null != obj)
                {
                    obj.transform.position = pos;
                }
            }
        }
        private UnityEngine.Vector3 GetImpactEndPos(UnityEngine.Vector3 startPos, UnityEngine.Quaternion q, ImpactLogicInfo info)
        {
            //"starttime, [movetime, speedx, speedy, speedz, accelx, accely, accelz]+"
            UnityEngine.Vector3 result = UnityEngine.Vector3.zero;
            List<float> list = null;
            if (info.ConfigCacheData == null)
            {
                list = Converter.ConvertNumericList<float>(info.ConfigData.CurveMoveInfo);
            }
            else
            {
                list = info.ConfigCacheData.MoveInfolist;
            }
            for (int i = 1; i < list.Count - 6; i += 7)
            {
                float time = list[i];
                float sx = list[i + 1];
                float sy = list[i + 2];
                float sz = list[i + 3];
                float ax = list[i + 4];
                float ay = list[i + 5];
                float az = list[i + 6];
                float timeSqr_div_2 = time * time / 2;
                result += q * new UnityEngine.Vector3(sx * time + timeSqr_div_2 * ax, sy * time + timeSqr_div_2 * ay, sz * time + timeSqr_div_2 * az);
            }
            return result + startPos;
        }

        public UnityEngine.Vector3 GetAdjustPoint(UnityEngine.Vector3 curPos, ImpactLogicInfo info)
        {
            UnityEngine.Vector3 result = new UnityEngine.Vector3(curPos.x, curPos.y, curPos.z);
            UnityEngine.Vector3 finalPoint = GetImpactRealEndPos(info);
            UnityEngine.Vector3 fromDirection = info.NormalEndPoint - info.OrignalPos;
            UnityEngine.Vector3 toDirection = finalPoint - info.OrignalPos;
            fromDirection.y = 0;
            toDirection.y = 0;
            if (info.AdjustDegreeXZ > c_Precision || info.AdjustDegreeXZ < c_Precision)
            {
                UnityEngine.Quaternion q = UnityEngine.Quaternion.FromToRotation(fromDirection, toDirection);
                float eulerAngleY = q.eulerAngles.y;
                if (eulerAngleY > 180)
                {
                    eulerAngleY = eulerAngleY - 360;
                }
                else if (eulerAngleY < -180)
                {
                    eulerAngleY = eulerAngleY + 360;
                }
                q = UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(0, eulerAngleY, 0) * info.AdjustDegreeXZ);
                float scale = 0.0f;
                if (fromDirection.magnitude < c_Precision)
                {
                    if (toDirection.magnitude < c_Precision)
                    {
                        scale = 1.0f;
                    }
                    else
                    {
                        scale = toDirection.magnitude;
                    }
                }
                else
                {
                    scale = (fromDirection.magnitude + (toDirection.magnitude - fromDirection.magnitude) * info.AdjustDegreeXZ) / fromDirection.magnitude;
                }

                result = (q * curPos) * scale;
            }
            if (info.LogicId == (int)GfxImpactLogicManager.GfxImpactLogicId.GfxImpactLogic_HitFly && Math.Abs(curPos.y) > c_Precision)
            {
                if (info.AdjustDegreeY > c_Precision || info.AdjustDegreeY < -1 * c_Precision)
                {
                    if (Math.Abs(finalPoint.y - info.NormalEndPoint.y) > c_Precision &&
                       Math.Abs(info.NormalEndPoint.y - info.OrignalPos.y) > c_Precision)
                    {
                        float scale = ((finalPoint.y - info.NormalEndPoint.y) * info.AdjustDegreeY) / (info.NormalEndPoint.y - info.OrignalPos.y);
                        result.y = curPos.y * Math.Abs(scale);
                    }
                    else
                    {
                        result.y = curPos.y;
                    }
                }
                else
                {
                    result.y = curPos.y;
                }
            }
            else
            {
                result.y = curPos.y;
            }
            return result;
            //ShowDebugObject(curPos + info.OrignalPos, 1);
            //ShowDebugObject((q * curPos) * scale + info.OrignalPos, 0);
        }

        private UnityEngine.Vector3 GetImpactRealEndPos(ImpactLogicInfo info)
        {
            UnityEngine.Vector3 dir = (info.AdjustPoint - info.NormalEndPoint).normalized;
            dir.y = 0;
            return dir * info.AdjustAppend + info.AdjustPoint;
        }

        private ImpactLogicInfo m_CurImpactLogicInfo = new ImpactLogicInfo();
        private List<ImpactLogicInfo> m_ImpactLogicInfos = new List<ImpactLogicInfo>();
        private bool m_ShowDebug = false;
        private const int m_DeadImpactId = 88888;
        public static GfxImpactSystem Instance
        {
            get
            {
                return s_Instance;
            }
        }
        private static GfxImpactSystem s_Instance = new GfxImpactSystem();
        private const float c_Precision = 0.000001f;
        private const float c_MaxHitEffect = 6;
        private const int c_EffectPoolCapacity = 10;
    }

}
