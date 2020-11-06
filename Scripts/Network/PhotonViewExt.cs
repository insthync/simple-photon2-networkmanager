using Photon.Realtime;

namespace Photon.Pun
{
    public static class PhotonViewExt
    {
        public delegate void RPCDelegate();
        public delegate void RPCDelegate<T1>(T1 param1);
        public delegate void RPCDelegate<T1, T2>(T1 param1, T2 param2);
        public delegate void RPCDelegate<T1, T2, T3>(T1 param1, T2 param2, T3 param3);
        public delegate void RPCDelegate<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5, T6, T7>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9);
        public delegate void RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10);

        public static void TargetRPC(this PhotonView photonView, RPCDelegate function, Player target)
        {
            photonView.RPC(function.Method.Name, target);
        }

        public static void TargetRPC<T1>(this PhotonView photonView, RPCDelegate<T1> function, Player target, T1 param1)
        {
            photonView.RPC(function.Method.Name, target, param1);
        }

        public static void TargetRPC<T1, T2>(this PhotonView photonView, RPCDelegate<T1, T2> function, Player target, T1 param1, T2 param2)
        {
            photonView.RPC(function.Method.Name, target, param1, param2);
        }

        public static void TargetRPC<T1, T2, T3>(this PhotonView photonView, RPCDelegate<T1, T2, T3> function, Player target, T1 param1, T2 param2, T3 param3)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3);
        }

        public static void TargetRPC<T1, T2, T3, T4>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5, T6>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5, param6);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5, T6, T7>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5, param6, param7);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5, T6, T7, T8>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5, param6, param7, param8);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }

        public static void TargetRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> function, Player target, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10)
        {
            photonView.RPC(function.Method.Name, target, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }

        public static void AllRPC(this PhotonView photonView, RPCDelegate function)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All);
        }

        public static void AllRPC<T1>(this PhotonView photonView, RPCDelegate<T1> function, T1 param1)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1);
        }

        public static void AllRPC<T1, T2>(this PhotonView photonView, RPCDelegate<T1, T2> function, T1 param1, T2 param2)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2);
        }

        public static void AllRPC<T1, T2, T3>(this PhotonView photonView, RPCDelegate<T1, T2, T3> function, T1 param1, T2 param2, T3 param3)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3);
        }

        public static void AllRPC<T1, T2, T3, T4>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4);
        }

        public static void AllRPC<T1, T2, T3, T4, T5>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5);
        }

        public static void AllRPC<T1, T2, T3, T4, T5, T6>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5, param6);
        }

        public static void AllRPC<T1, T2, T3, T4, T5, T6, T7>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5, param6, param7);
        }

        public static void AllRPC<T1, T2, T3, T4, T5, T6, T7, T8>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5, param6, param7, param8);
        }

        public static void AllRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }

        public static void AllRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10)
        {
            photonView.RPC(function.Method.Name, RpcTarget.All, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }

        public static void OthersRPC(this PhotonView photonView, RPCDelegate function)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others);
        }

        public static void OthersRPC<T1>(this PhotonView photonView, RPCDelegate<T1> function, T1 param1)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1);
        }

        public static void OthersRPC<T1, T2>(this PhotonView photonView, RPCDelegate<T1, T2> function, T1 param1, T2 param2)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2);
        }

        public static void OthersRPC<T1, T2, T3>(this PhotonView photonView, RPCDelegate<T1, T2, T3> function, T1 param1, T2 param2, T3 param3)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3);
        }

        public static void OthersRPC<T1, T2, T3, T4>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5, T6>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5, param6);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5, T6, T7>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5, param6, param7);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5, T6, T7, T8>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5, param6, param7, param8);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }

        public static void OthersRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10)
        {
            photonView.RPC(function.Method.Name, RpcTarget.Others, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }

        public static void MasterRPC(this PhotonView photonView, RPCDelegate function)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient);
        }

        public static void MasterRPC<T1>(this PhotonView photonView, RPCDelegate<T1> function, T1 param1)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1);
        }

        public static void MasterRPC<T1, T2>(this PhotonView photonView, RPCDelegate<T1, T2> function, T1 param1, T2 param2)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2);
        }

        public static void MasterRPC<T1, T2, T3>(this PhotonView photonView, RPCDelegate<T1, T2, T3> function, T1 param1, T2 param2, T3 param3)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3);
        }

        public static void MasterRPC<T1, T2, T3, T4>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4> function, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5, T6>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5, param6);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5, T6, T7>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5, param6, param7);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5, T6, T7, T8>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5, param6, param7, param8);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5, param6, param7, param8, param9);
        }

        public static void MasterRPC<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this PhotonView photonView, RPCDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> function, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, T10 param10)
        {
            photonView.RPC(function.Method.Name, RpcTarget.MasterClient, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
        }
    }
}
