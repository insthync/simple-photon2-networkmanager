using Photon.Pun;

public class SyncDieCountRpcComponent : BaseSyncVarRpcComponent<int>
{
    [PunRPC]
    protected void RpcUpdateDieCount(int value)
    {
        _value = value;
    }
}
