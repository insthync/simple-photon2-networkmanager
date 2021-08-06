using Photon.Pun;

public class SyncKillCountRpcComponent : BaseSyncVarRpcComponent<int>
{
    [PunRPC]
    protected void RpcUpdateKillCount(int value)
    {
        _value = value;
    }
}
