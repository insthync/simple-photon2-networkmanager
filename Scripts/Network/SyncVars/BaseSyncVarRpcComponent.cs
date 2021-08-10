using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseSyncVarRpcComponent : MonoBehaviourPunCallbacks
{
    public enum SyncMode
    {
        ByMasterClient,
        ByOwner,
    }

    protected static readonly Dictionary<string, string> cachedFunctions = new Dictionary<string, string>();
    public SyncMode syncMode = SyncMode.ByMasterClient;
    public float syncInterval = 1f;

    public abstract void SyncToOther();
    public abstract void SyncToAll();
    public abstract void SyncToTarget(Player target);
}

public abstract class BaseSyncVarRpcComponent<T> : BaseSyncVarRpcComponent
{
    [Serializable]
    public class ValueChangeEvent : UnityEvent<T> { }

    public ValueChangeEvent onValueChange = new ValueChangeEvent();

    protected string rpcFunctionName;
    protected bool syncing = true;
    protected T _value;
    public T Value
    {
        get { return _value; }
        set
        {
            switch (syncMode)
            {
                case SyncMode.ByMasterClient:
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        Debug.LogWarning($"{ToString()} can be synced by master client only");
                        return;
                    }
                    break;
                case SyncMode.ByOwner:
                    if (!photonView.IsMine)
                    {
                        Debug.LogWarning($"{ToString()} can be synced by owner client only");
                        return;
                    }
                    break;
            }
            if (HasChanges(value))
            {
                _value = value;
                syncing = true;
                onValueChange.Invoke(value);
            }
        }
    }

    protected float syncCountdown = 0f;

    protected virtual void Awake()
    {
        Type lookupType = GetType();
        string typeName = lookupType.Name;
        if (!cachedFunctions.ContainsKey(typeName))
        {
            do
            {
                var methods = lookupType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.GetCustomAttributes(typeof(PunRPC), false).Length > 0);
                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(T))
                    {
                        rpcFunctionName = method.Name;
                        cachedFunctions[typeName] = method.Name;
                        return;
                    }
                }
                lookupType = lookupType.BaseType;
            } while (lookupType != typeof(BaseSyncVarRpcComponent));
        }
        else
        {
            rpcFunctionName = cachedFunctions[typeName];
        }
    }

    protected virtual void Start()
    {
        SyncToOther();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        SyncToTarget(newPlayer);
    }

    protected virtual void Update()
    {
        syncCountdown -= Time.unscaledTime;
        if (syncCountdown <= 0 && syncing)
        {
            syncing = false;
            SyncToOther();
            syncCountdown = syncInterval;
        }
    }

    public override void SyncToOther()
    {
        if (syncMode == SyncMode.ByMasterClient && !PhotonNetwork.IsMasterClient)
            return;
        if (syncMode == SyncMode.ByOwner && !photonView.IsMine)
            return;
        photonView.RPC(rpcFunctionName, RpcTarget.Others, _value);
    }

    public override void SyncToAll()
    {
        if (syncMode == SyncMode.ByMasterClient && !PhotonNetwork.IsMasterClient)
            return;
        if (syncMode == SyncMode.ByOwner && !photonView.IsMine)
            return;
        photonView.RPC(rpcFunctionName, RpcTarget.All, _value);
    }

    public override void SyncToTarget(Player target)
    {
        if (syncMode == SyncMode.ByMasterClient && !PhotonNetwork.IsMasterClient)
            return;
        if (syncMode == SyncMode.ByOwner && !photonView.IsMine)
            return;
        photonView.RPC(rpcFunctionName, target, _value);
    }

    public virtual bool HasChanges(T value)
    {
        return !_value.Equals(value);
    }
}
