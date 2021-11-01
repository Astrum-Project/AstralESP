using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(Astrum.AstralESP), "AstralESP", "0.2.1", downloadLink: "github.com/Astrum-Project/AstralESP")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public class AstralESP : MelonMod
    {
        public bool pickupsEnabled = true;
        public bool pickupsOwner = true;
        public bool pickupsDistance = true;
        public float pickupsMaxDistance = 10000f;

        public bool playersEnabled = true;
        public bool playersFPS = true;
        public bool playersPing = true;
        public bool playersDistance = true;
        public float playersMaxDistance = 10000f;

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
            MelonPreferences_Category category = MelonPreferences.CreateCategory("Astrum-AstralESP", "Astral ESP");
            category.CreateEntry("Pickups-Enabled", true, "Pickups - Enabled");
            category.CreateEntry("Pickups-Owner", true, "Pickups - Show Owner");
            category.CreateEntry("Pickups-Distance", true, "Pickups - Show Distance");
            category.CreateEntry("Pickups-MaxDistance", 10000f, "Pickups - Max Distance");

            category.CreateEntry("Players-Enabled", true, "Pickups - Enabled");
            category.CreateEntry("Players-FPS", true, "Show - FPS");
            category.CreateEntry("Players-Ping", true, "Show - Ping");
            category.CreateEntry("Players-Distance", true, "Pickups - Show Distance");
            category.CreateEntry("Players-MaxDistance", 10000f, "Pickups - MaxDistance");

            category.CreateEntry("DisableInVR", true, "Disable In VR");

            OnPreferencesLoaded();
        }

        public override void OnPreferencesLoaded()
        {
            MelonPreferences_Category category = MelonPreferences.GetCategory("Astrum-AstralESP");

            pickupsEnabled = category.GetEntry<bool>("Pickups-Enabled").Value;
            pickupsOwner = category.GetEntry<bool>("Pickups-Owner").Value;
            pickupsDistance = category.GetEntry<bool>("Pickups-Distance").Value;
            pickupsMaxDistance = category.GetEntry<float>("Pickups-MaxDistance").Value;

            playersEnabled = category.GetEntry<bool>("Players-Enabled").Value;
            playersFPS = category.GetEntry<bool>("Players-FPS").Value;
            playersPing = category.GetEntry<bool>("Players-Ping").Value;
            playersDistance = category.GetEntry<bool>("Players-Distance").Value;
            playersMaxDistance = category.GetEntry<float>("Players-MaxDistance").Value;

            if (XRDevice.isPresent && category.GetEntry<bool>("DisableInVR").Value)
                pickupsEnabled = playersEnabled = false;
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
            OnGUIAction -= DrawPlayers;
        }

        public System.Collections.IEnumerator FindPickups(string name)
        {
            Scene scene = SceneManager.GetSceneByName(name);

            while (scene.GetRootGameObjects().FirstOrDefault(go => go.name.StartsWith("VRCPlayer[Local]")) is null) yield return null;

            pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>().Select(f => f.gameObject).ToList();

            OnGUIAction += DrawPickups;
            OnGUIAction += DrawPlayers;
        }

        public override void OnGUI() => OnGUIAction();
        private Action OnGUIAction = new Action(() => { });

        private void DrawPickups()
        {
            if (!pickupsEnabled) return;
            if (Networking.LocalPlayer is null) return;
            
            foreach (GameObject pickup in pickups)
            {

                if (pickup is null)
                {
                    pickups.Remove(pickup);
                    continue;
                }

                Vector3 p = Camera.main.WorldToScreenPoint(pickup.transform.position);

                if (p.z < 0 || p.z > pickupsMaxDistance) continue;

                StringBuilder builder = new StringBuilder();

                if (pickupsOwner)
                {
                    builder.Append(Networking.GetOwner(pickup).displayName);
                    builder.Append(" | ");
                }

                builder.Append(pickup.name);

                if (pickupsDistance)
                {
                    builder.Append(" | ");
                    builder.Append(p.z.ToString("0.00"));
                }

                GUI.Label(new Rect(p.x, Screen.height - p.y, 0, 0), builder.ToString(), style);
            }
        }

        private void DrawPlayers()
        {
            if (!playersEnabled) return;
            if (VRC.PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 is null) return;

            foreach (VRC.Player player in VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player is null || player == VRC.Player.prop_Player_0 || player.field_Private_APIUser_0 is null) continue;
                
                Vector3 p = Camera.main.WorldToScreenPoint(player.transform.position);
                if (p.z < 0 || p.z > playersMaxDistance) continue;

                StringBuilder builder = new StringBuilder();

                if (player.field_Private_VRCPlayerApi_0.isMaster)
                    builder.Append("<color=orange>");

                if (player.field_Private_APIUser_0.isFriend)
                    builder.Append("<b>");

                builder.Append(player.field_Private_APIUser_0.displayName);

                if (playersFPS)
                {
                    builder.Append(" | ");
                    byte f = player._playerNet.field_Private_Byte_0;
                    if (f == 0) builder.Append('\u221E');
                    else builder.Append(1000 / f); 
                }

                if (playersPing)
                {
                    builder.Append(" | ");
                    builder.Append(player.prop_Int32_0);
                }

                if (playersDistance)
                {
                    builder.Append(" | ");
                    builder.Append(p.z.ToString("0.00"));
                }

                if (player.field_Private_VRCPlayerApi_0.isMaster)
                    builder.Append("</color>");

                if (player.field_Private_APIUser_0.isFriend)
                    builder.Append("</b>");

                GUI.Label(new Rect(p.x, Screen.height - p.y, 0, 0), builder.ToString(), style);
            }
        }
    }
}
