using UnityEngine;

namespace Distance.NitronicHUD
{
    internal static class Utilities
    {
        internal static GameObject FindLocalCar()
        {
            return G.Sys.PlayerManager_?.Current_?.playerData_?.Car_;
        }

        internal static CarLogic FindLocalCarLogic()
        {
            return G.Sys.PlayerManager_?.Current_?.playerData_?.CarLogic_;
        }

        internal static CarScreenLogic FindLocalVehicleScreen()
        {
            var carScreenLogic = G.Sys.PlayerManager_?.Current_?.playerData_?.CarScreenLogic_;
            if (carScreenLogic?.CarLogic_.IsLocalCar_ ?? false)
            {
                return carScreenLogic;
            }
            return null;
        }
    }
}
