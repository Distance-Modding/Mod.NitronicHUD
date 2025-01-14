using HarmonyLib;

namespace Distance.NitronicHUD.Patches
{
    [HarmonyPatch(typeof(GameManager), "Awake")]
    internal static class GameManager__Awake
    {
        [HarmonyPostfix]
        internal static void Postfix()
        {
            Mod.Instance.LateInitialize();
        }
    }
}
