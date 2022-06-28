using HarmonyLib;
using UnityEngine;

namespace TownOfSuper.Patches {
    [HarmonyPatch]
    public static class LogoMenuPatch {

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(MainMenuManager __instance) {
                DestroyableSingleton<ModManager>.Instance.ShowModStamp();

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null) {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.down;
                }

                var tosLogo = new GameObject("bannerLogo_TOR");
                tosLogo.transform.position = Vector3.up;
                var renderer = tosLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TownOfSuper.Resources.Logo.png", 100f);
            }
        }
    }
}