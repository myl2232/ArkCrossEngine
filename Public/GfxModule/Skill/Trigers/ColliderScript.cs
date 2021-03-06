﻿using ArkCrossEngine;

/// TODO: remove dep on unity
public class ColliderScript : UnityEngine.MonoBehaviour
{
    public ColliderScript()
    {

    }
    public void SetOnTriggerEnter(MyAction<UnityEngine.Collider> onEnter)
    {
        m_OnTrigerEnter += onEnter;
    }
    public void SetOnTriggerExit(MyAction<UnityEngine.Collider> onExit)
    {
        m_OnTrigerExit += onExit;
    }

    public void SetOnDestroy(MyAction onDestroy)
    {
        m_OnDestroy += onDestroy;
    }

    public void OnDestroy()
    {
        if (m_OnDestroy != null)
        {
            m_OnDestroy();
        }
    }

    void OnTriggerEnter(UnityEngine.Collider collider)
    {
        if (null != m_OnTrigerEnter)
        {
            m_OnTrigerEnter(collider);
        }
    }
    void OnTriggerExit(UnityEngine.Collider collider)
    {
        if (null != m_OnTrigerExit)
        {
            m_OnTrigerExit(collider);
        }
    }

    private MyAction<UnityEngine.Collider> m_OnTrigerEnter;
    private MyAction<UnityEngine.Collider> m_OnTrigerExit;
    private MyAction m_OnDestroy;
}
