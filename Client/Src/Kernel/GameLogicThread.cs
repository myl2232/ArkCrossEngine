﻿using System;
using ArkCrossEngine.Network;
using ScriptableData;

namespace ArkCrossEngine
{
    internal class GameLogicThread : MyClientThread
    {
        protected override void OnStart()
        {
            // initialize lua env on logic thread
        }

        protected override void OnTick()
        {
            //这里是在逻辑线程执行的tick，渲染线程的在GameControler.cs:TickGame里。
            try
            {
                TimeUtility.SampleClientTick();

                long curTime = TimeUtility.GetLocalMilliseconds();
                if (m_LastLogTime + 10000 < curTime)
                {
                    m_LastLogTime = curTime;
#if DEBUG
                    if (WorldSystem.Instance.IsPvpScene() || WorldSystem.Instance.IsMultiPveScene())
                    {
                        GfxSystem.GfxLog("AverageRoundtripTime:{0}", TimeUtility.AverageRoundtripTime);
                    }

                    if (this.CurActionNum > 10)
                    {
                        GfxSystem.GfxLog("LogicThread.Tick actionNum {0}", this.CurActionNum);
                    }

                    DebugPoolCount((string msg) =>
                    {
                        GfxSystem.GfxLog("LogicActionQueue {0}", msg);
                    });
#endif
                    ClearPool(1024);
                }

                if (!GameControler.IsPaused)
                {
                    NetworkSystem.Instance.Tick();
                    LobbyNetworkSystem.Instance.Tick();
                    PlayerControl.Instance.Tick();
                    WorldSystem.Instance.Tick();
                    ScriptManager.Instance.Tick(false);
                }
                GameControler.LogicLoggerInstance.Tick();
            }
            catch (Exception ex)
            {
                LogSystem.Error("GameLogicThread.Tick throw Exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        protected override void OnQuit()
        {
            ScriptManager.Instance.Destroy(false);
        }

        private long m_LastLogTime = 0;
    }
}
