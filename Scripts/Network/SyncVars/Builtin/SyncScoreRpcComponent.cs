using Photon.Pun;

public class SyncScoreRpcComponent : BaseSyncVarRpcComponent<int>
{
    [PunRPC]
    protected void RpcUpdateScore(int value)
    {
        _value = value;
    }
}
