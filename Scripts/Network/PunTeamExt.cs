using UnityEngine;
using System.Collections;
using Photon.Realtime;

namespace Photon.Pun.UtilityScripts
{
    public static class PunTeamExt
    {
        public static bool JoinOrSwitchTeam(this Player player, byte teamCode)
        {
            if (player.GetPhotonTeam() == null)
                return player.JoinTeam(teamCode);
            return player.SwitchTeam(teamCode);
        }
    }
}
