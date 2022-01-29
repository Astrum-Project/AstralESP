using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(Astrum.AstralESP), "AstralESP", "0.4.0", downloadLink: "github.com/Astrum-Project/AstralESP")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("AstralTags")]

namespace Astrum
{
    public partial class AstralESP : MelonMod
    {
        private List<GameObject> pickups = new List<GameObject>();
        private readonly GUIStyle style = new GUIStyle()
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            normal = new GUIStyleState
            {
                textColor = Color.white,
            }
        };

        public override void OnApplicationStart()
        {
            Outlines.Initialize(HarmonyInstance);

            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "AstralTags"))
                Extern.SetupTags();

            Config.Initialize();
        }

        public override void OnSceneWasLoaded(int index, string name)
        {
            if (index != -1) return;

            MelonCoroutines.Start(FindPickups(name));
        }

        public override void OnSceneWasUnloaded(int index, string _)
        {
            if (index != -1) return;

            pickups.Clear();

            OnGUIAction -= DrawPickups;
        }

        public System.Collections.IEnumerator FindPickups(string name)
        {
            Scene scene = SceneManager.GetSceneByName(name);

            while (Networking.LocalPlayer is null) yield return null;

            pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>().Select(f => f.gameObject).ToList();

            OnGUIAction += DrawPickups;
        }

        public override void OnGUI() => OnGUIAction();
        private Action OnGUIAction = new Action(() => { });

        private void DrawPickups()
        {
            if (!Config.pickupsEnabled.Value) return;
            if (Networking.LocalPlayer is null) return;
            
            foreach (GameObject pickup in pickups)
            {

                if (pickup is null)
                {
                    pickups.Remove(pickup);
                    continue;
                }

                Vector3 p = Camera.main.WorldToScreenPoint(pickup.transform.position);

                if (p.z < 0 || p.z > Config.pickupsMaxDistance.Value) continue;

                StringBuilder builder = new StringBuilder();

                if (Config.pickupsOwner.Value)
                {
                    builder.Append(Networking.GetOwner(pickup).displayName);
                    builder.Append(" | ");
                }

                builder.Append(pickup.name);

                if (Config.pickupsDistance.Value)
                {
                    builder.Append(" | ");
                    builder.Append(p.z.ToString("0.00"));
                }

                GUI.Label(new Rect(p.x, Screen.height - p.y, 0, 0), builder.ToString(), style);
            }
        }

        internal class Extern
        {
            public static void SetupTags()
            {
                MelonCoroutines.Start(RefreshTag(
                    new AstralTags.Tag(_player =>
                    {
                        VRC.Player player = (VRC.Player)_player.Inner;
                        return new AstralTags.TagData()
                        {
                            enabled = true,
                            text = FPS(player._playerNet.field_Private_Byte_0) + " | " + Ping(player._playerNet.field_Private_Int16_0),
                            textColor = Color.white,
                            background = Color.black,
                        };
                    }, 1000000)
                ));
            }

            private static string FPS(byte f)
            {
                if (f == 0) return "\u221E";

                int fps = 1000 / f;

                return (fps >= 60
                ? "<color=#0f0>"
                : fps >= 45
                  ? "<color=#008000>"
                  : fps >= 30
                    ? "<color=#ffff00>"
                    : fps >= 15
                      ? "<color=#ffa500>"
                      : "<color=#ff0000>")
                + fps + "</color>";
            }

            public static string Ping(int ping) =>
                (ping <= 75
                  ? "<color=#00ff00>"
                  : ping <= 125
                    ? "<color=#008000>"
                    : ping <= 175
                      ? "<color=#ffff00>"
                      : ping <= 225
                        ? "<color=#ffa500>"
                        : "<color=#ff0000>") + ping + "</color>ms";

            private static System.Collections.IEnumerator RefreshTag(AstralTags.Tag tag)
            {
                for (;;)
                {
                    tag.CalculateAll();

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
}
