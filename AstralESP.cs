using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(Astrum.AstralESP), "AstralESP", "0.1.0", downloadLink: "github.com/Astrum-Project/AstralESP")]
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
        public bool playersDistance = true;
        public float playersMaxDistance = 10000f;

        private List<GameObject> pickups = new List<GameObject>();
        private readonly GUIStyle style = new GUIStyle()
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
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
        }

        public System.Collections.IEnumerator FindPickups(string name)
        {
            Scene scene = SceneManager.GetSceneByName(name);

            while (scene.GetRootGameObjects().FirstOrDefault(go => go.name.StartsWith("VRCPlayer[Local]")) is null) yield return null;

            pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>().Select(f => f.gameObject).ToList();
        }

        public override void OnGUI()
        {
            if (pickupsEnabled) DrawPickups();
            if (playersEnabled) DrawPlayers();
        }

        private void DrawPickups()
        {
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
                    builder.Append(p.z);
                }

                GUI.Label(new Rect(p.x, Screen.height - p.y, 0, 0), builder.ToString(), style);
            }
        }

        private void DrawPlayers()
        {
            foreach (VRC.Player player in VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                Vector3 p = Camera.main.WorldToScreenPoint(player.transform.position);

                if (p.z < 0 || p.z > playersMaxDistance) continue;

                StringBuilder builder = new StringBuilder();

                builder.Append(player.field_Private_APIUser_0.displayName);

                if (playersDistance)
                {
                    builder.Append(" | ");
                    builder.Append(p.z);
                }

                GUI.Label(new Rect(p.x, Screen.height - p.y, 0, 0), builder.ToString(), style);
            }
        }
    }
}
