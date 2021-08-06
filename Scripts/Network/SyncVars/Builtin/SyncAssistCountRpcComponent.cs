using Photon.Pun;

public class SyncAssistCountRpcComponent : BaseSyncVarRpcComponent<int>
{
    [PunRPC]
    protected void RpcUpdateAssistCount(int value)
    {
        _value = value;
    }
}
