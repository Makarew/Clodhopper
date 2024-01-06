using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using System.Numerics;

namespace Clodhopper
{
    internal static class Patcher
    {
        public static void Init()
        {
            // Load Shoe Models When A New Player Joins By Any Means Necessary
            Harmony harmony = new Harmony("ShoePatcher");
            MethodInfo sorClientCon = AccessTools.Method(typeof(PlayerControllerB), "ConnectClientToPlayerObject");
            MethodInfo shoeClientPatch = AccessTools.Method(typeof(Patcher), nameof(Patcher.AttachAllShoes));
            MethodInfo playerJoined = AccessTools.Method(typeof(StartOfRound), "OnPlayerConnectedClientRpc");

            harmony.Patch(sorClientCon,null, new HarmonyMethod(shoeClientPatch));
            harmony.Patch(playerJoined, null, new HarmonyMethod(shoeClientPatch));

            // Check If The Chat Message Is For Clodhopper
            MethodInfo chatPatch = AccessTools.Method(typeof(Patcher), nameof(Patcher.SendCustomChatClientRPC));
            MethodInfo chatOrig = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

            harmony.Patch(chatOrig, new HarmonyMethod(chatPatch));

            // Request Shoes From Other Clients When Joining Server
            MethodInfo serverJoinPatch = AccessTools.Method(typeof(Patcher), nameof(Patcher.RequestShoesOnJoinServer));
            MethodInfo camSwitchEvent = AccessTools.Method(typeof(PlayerControllerB), "SpawnPlayerAnimation");

            harmony.Patch(camSwitchEvent, new HarmonyMethod(serverJoinPatch));

            // Make Sure Plugin Stays Loaded
            MethodInfo initPatch = AccessTools.Method(typeof(Patcher), nameof(Patcher.LoadPlugin));
            MethodInfo init = AccessTools.Method(typeof(PreInitSceneScript), "Awake");

            harmony.Patch(init, new HarmonyMethod(initPatch));
        }

        public static void AddShoes(Transform parentObj)
        {
            // Load A Shoe Spawner Object For The Player
            ShoeSpawner ss = parentObj.gameObject.AddComponent<ShoeSpawner>();
            ss.shoesLeft = new List<GameObject>();
            ss.shoesRight = new List<GameObject>();

            // Load Each Shoe For The Player
            foreach (AssetBundle shoe in ShoeManager.Instance.shoes)
            {
                GameObject right = GameObject.Instantiate((GameObject)shoe.LoadAsset("ShoeObjectR"), parentObj.transform.Find("thigh.R/shin.R/foot.R/heel.02.R"));
                GameObject left = GameObject.Instantiate((GameObject)shoe.LoadAsset("ShoeObject"), parentObj.transform.Find("thigh.L/shin.L/foot.L/heel.02.L"));

                ss.shoesRight.Add(right);
                ss.shoesLeft.Add(left);

                right.tag = "Untagged";
                right.layer = 0;

                left.tag = "Untagged";
                left.layer = 0;

                right.SetActive(false);
                left.SetActive(false);
            }
        }

        public static void AttachAllShoes()
        {
            // Load Shoes For All Players
            foreach (PlayerControllerB player in StartOfRound.Instance.OtherClients)
            {
                if (player.transform.Find("ScavengerModel/metarig/spine/thigh.L/shin.L/foot.L/heel.02.L").childCount == 1)
                {
                        Patcher.AddShoes(player.transform.Find("ScavengerModel/metarig/spine"));
                }
            }

            // Load Shoes For Local Player
            if (StartOfRound.Instance.localPlayerController.transform.Find("ScavengerModel/metarig/spine/thigh.L/shin.L/foot.L/heel.02.L").childCount == 1)
            {
                Patcher.AddShoes(StartOfRound.Instance.localPlayerController.transform.Find("ScavengerModel/metarig/spine"));
            }
        }

        public static bool SendCustomChatClientRPC(string chatMessage, string nameOfUserWhoTyped)
        {
            // Check If A Message Has Been Sent To Load Custom Shoes
            if (chatMessage.StartsWith("Clodhopper Custom RPC Send Custom Message Shoe Type"))
            {
                // Get The Shoe ID And Player ID From The Message
                string shoeID = chatMessage.Split(';')[1];
                ulong playerID = ulong.Parse(chatMessage.Split(';')[2]);

                Transform player = null;
                bool localPlayer = false;

                // Check If The Local Player Sent The Message
                if (StartOfRound.Instance.localPlayerController.NetworkObjectId == playerID)
                {
                    player = StartOfRound.Instance.localPlayerController.transform;
                    localPlayer = true;
                }
                else
                {
                    // Find The Player That Sent The Message
                    foreach (PlayerControllerB playerS in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (playerS.NetworkObjectId == playerID) { player = playerS.transform; break; }
                    }
                }

                // Do Nothing If The Player Has The Current Shoe Enabled Already
                if (player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().enabledShoe == shoeID || (localPlayer && !player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().showShoes))
                {
                    player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().enabledShoe = shoeID;
                    return false;
                }

                    // Disable All Shoes On The Left Foot, And Enable The Requested Shoe
                    foreach (GameObject shoes in player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().shoesLeft)
                {
                    if (shoes.GetComponent<ShoeObject>().shoeID == shoeID) { shoes.SetActive(true); }
                    else { shoes.SetActive(false); }
                }
                // Disable All Shoes On The Right Foot, And Enable The Requested Shoe
                foreach (GameObject shoes in player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().shoesRight)
                {
                    if (shoes.GetComponent<ShoeObject>().shoeID == shoeID) { shoes.SetActive(true); }
                    else { shoes.SetActive(false); }
                }

                // Set The Enabled Shoe ID
                player.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().enabledShoe = shoeID;

                Plugin.Instance.SendLog("Message: " + chatMessage + " - User: " + playerID + " - Shoe ID: " + shoeID);

                return false;
            } 
            // Check If A Player Is Requesting Shoe IDs Be Sent
            else if (chatMessage.StartsWith("Clodhopper Custom RPC Request Custom Message Shoe Type"))
            {
                ulong playerID = ulong.Parse(chatMessage.Split(';')[1]);

                // If The Local Player Sent The Message, Do Nothing
                if (playerID == StartOfRound.Instance.localPlayerController.NetworkObjectId) return false;

                // Otherwise, Send The Local Player's Current Shoes
                SetShoesOnClientJoin();

                return false;
            }

            // If Not A Clodhopper Message, Display Message As Normal
            return true;
        }

        // Make Sure The Plugin Is Loaded
        public static void LoadPlugin()
        {
            if (Plugin.Instance == null)
            {
                Plugin plugin = GameObject.Instantiate(new GameObject()).AddComponent<Plugin>();
                plugin.Init();
            }
        }

        public static void RequestShoesOnJoinServer()
        {
            // Don't Request Shoes If This Is The Host Or Server
            // Don't Request Shoes If Client Has Already Requested Shoes
            if (Plugin.Instance.requestedShoes || StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer) return;

            Plugin.Instance.SendLog("Connected To Server - Requesting Shoes");

            // Send Shoe Request Message To All Clients
            PlayerControllerB player = StartOfRound.Instance.localPlayerController;
            HUDManager.Instance.AddTextToChatOnServer("Clodhopper Custom RPC Request Custom Message Shoe Type;" + player.NetworkObjectId);

            // Don't Request Shoes Again Unless Player Returns To Main Menu
            Plugin.Instance.requestedShoes = true;
        }

        public static void SetShoesOnClientJoin()
        {
            Plugin.Instance.SendLog("Shoes Requested - Attempting To Send Shoe Message");

            // Get The Local Player's Enabled Shoe ID
            PlayerControllerB player = StartOfRound.Instance.localPlayerController;
            string shoeID = player.transform.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().enabledShoe;

            // Send Player's Custom Shoe Message
            HUDManager.Instance.AddTextToChatOnServer("Clodhopper Custom RPC Send Custom Message Shoe Type;" + shoeID + ";" + player.NetworkObjectId);
        }
    }
}
