using Astrum.AstralCore.Hooks;
using MelonLoader;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace Astrum
{
    public partial class AstralESP
    {
        public static class Outlines
        {
            private static bool enabled;
            public static bool Enabled
            {
                get => enabled;
                set
                {
                    if (enabled == value) return;
                    enabled = value;

                    SetAll(value);
                }
            }

            public static void Initialize(HarmonyLib.Harmony harmony)
            {
                harmony.Patch(
                    typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start)),
                    null,
                    typeof(Outlines).GetMethod(nameof(OnAvatarLoaded), Hooks.PrivateStatic).ToNewHarmonyMethod()
                );

                harmony.Patch(
                    typeof(HighlightsFXStandalone).GetMethod(nameof(HighlightsFXStandalone.Awake)),
                    null,
                    typeof(Outlines).GetMethod(nameof(OnHighlightsFXAwake), Hooks.PrivateStatic).ToNewHarmonyMethod()
                );
            }

            private static void OnAvatarLoaded(PipelineManager __instance)
            {
                if (!enabled 
                    || __instance.contentType != PipelineManager.ContentType.avatar 
                    || Networking.GetOwner(__instance.gameObject).isLocal 
                    || __instance.transform.parent.GetComponent<VRC.SimpleAvatarPedestal>() != null) 
                    return;

                Set(__instance.transform, true);
            }

            private static void OnHighlightsFXAwake(HighlightsFXStandalone __instance)
            {
                __instance.highlightColor = new Color32(0x56, 0x00, 0xA5, 0xFF);
                __instance.blurIterations = 1;
                __instance.blurSize = 1;
            }

            private static void Set(Transform transform, bool state)
            {
                foreach (SkinnedMeshRenderer renderer in transform.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    if (renderer.bounds.extents.x <= 1.5 || renderer.bounds.extents.y <= 1.5 || renderer.bounds.extents.z <= 1.5)
                        HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(renderer, state);
            }

            private static void SetAll(bool state)
            {
                foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
                    if (!player.isLocal)
                        Set(player.gameObject.transform.Find("ForwardDirection/Avatar"), state);
            }
        }
    }
}
