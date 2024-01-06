using BepInEx;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using GameNetcodeStuff;
using BepInEx.Configuration;
using UnityEngine.InputSystem;
using System.Numerics;

namespace Clodhopper
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Patcher.Init();
        }

        // Only Have One Instance
        public void Init()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                CheckConfig();
            } else { Destroy(this); }
        }

        private bool addedManager = false;
        private bool addedToRack = true;

        public bool requestedShoes = false;

        public void Update()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                // Load Shoe Manager
                if (!Instance.addedManager)
                {
                    Logger.LogInfo($"Creating Shoe Manager");

                    GameObject shoeMan = GameObject.Instantiate((GameObject)AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "Makarew-Clodhopper", "shoe.manager")).LoadAsset("ShoeManager"));

                    Logger.LogInfo($"Shoe Manager Created");

                    // Load Shoe Asset Bundles
                    Logger.LogInfo($"Loading Shoes");

                    Instance.addedManager = true;

                    ShoeManager.Instance.LoadShoes();
                    Logger.LogInfo($"Shoes Loaded");
                }

                // Reset Variables That Are Needed When Joining Game
                if (Instance.addedToRack)
                {
                    Instance.addedToRack = false;
                    Instance.requestedShoes = false;

                    if (localShoeToggleAction != null) localShoeToggleAction.Disable();
                    if (removeShoeAction != null) removeShoeAction.Disable();
                }
            }

            // Add Shoes To Rack When Loaded Into The Ship
            if (SceneManager.GetActiveScene().name == "SampleSceneRelay" && !Instance.addedToRack)
            {
                Logger.LogInfo($"Adding Shoe Objects To Rack");
                ShoeManager.Instance.AddShoesToRack();
                Instance.addedToRack = true;
                Logger.LogInfo($"Shoes Added To Rack");

                localShoeToggleAction.Enable();
                removeShoeAction.Enable();
            }
        }

        public void SendLog(string message)
        {
            Logger.LogMessage(message);
        }

        private ConfigEntry<string> configToggleKey;
        private ConfigEntry<string> configRemoveKey;

        public static InputAction localShoeToggleAction;
        public static InputAction removeShoeAction;

        private void CheckConfig()
        {
            // Create Input Event To Toggle Local Player Shoe Visibility
            configToggleKey = Config.Bind("General",
                "PlayerViewToggle",
                "<Keyboard>/p",
                "The key used to toggle visibility of the local player's shoes.");

            localShoeToggleAction = new InputAction("ShoeToggle", binding: configToggleKey.Value, interactions: "Press");

            localShoeToggleAction.performed += ToggleEvent;

            // Create Input Event To Remove Local Player Shoes
            configRemoveKey = Config.Bind("General",
                "RemoveShoe",
                "<Keyboard>/o",
                "The key used to remove the local player's shoes.");

            removeShoeAction = new InputAction("ShoeRemove", binding: configRemoveKey.Value, interactions: "Press");

            removeShoeAction.performed += RemoveShoeEvent;
        }

        // Toggle Local Player Shoe Visibility
        public static void ToggleEvent(InputAction.CallbackContext context)
        {
            Instance.Logger.LogMessage("Toggling Local Player Shoe Visibility");
            StartOfRound.Instance.localPlayerController.transform.Find("ScavengerModel/metarig/spine").GetComponent<ShoeSpawner>().ToggleLocalPlayerShoes();
        }

        // Remove Local Player Shoes
        public static void RemoveShoeEvent(InputAction.CallbackContext context)
        {
            HUDManager.Instance.AddTextToChatOnServer("Clodhopper Custom RPC Send Custom Message Shoe Type;Default;" + StartOfRound.Instance.localPlayerController.NetworkObjectId);
        }
    }
}