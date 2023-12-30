using BepInEx;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using GameNetcodeStuff;

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
            } else { Destroy(this); }
        }

        private bool addedManager = false;
        private ShoeManager manager;
        private bool addedToRack = false;

        public bool requestedShoes = false;

        public void Update()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                // Load Shoe Manager
                if (!addedManager)
                {
                    Logger.LogInfo($"Creating Shoe Manager");

                    GameObject shoeMan = GameObject.Instantiate((GameObject)AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "Makarew-Clodhopper", "shoe.manager")).LoadAsset("ShoeManager"));
                    manager = shoeMan.GetComponent<ShoeManager>();
                    DontDestroyOnLoad(shoeMan);

                    Logger.LogInfo($"Shoe Manager Created");

                    // Load Shoe Asset Bundles
                    Logger.LogInfo($"Loading Shoes");

                    addedManager = true;

                    manager.LoadShoes();
                    Logger.LogInfo($"Shoes Loaded");
                }

                // Reset Variables That Are Needed When Joining Game
                if (addedToRack)
                {
                    addedToRack = false;
                    requestedShoes = false;
                }
            }

            // Add Shoes To Rack When Loaded Into The Ship
            if (SceneManager.GetActiveScene().name == "SampleSceneRelay" && !addedToRack)
            {
                Logger.LogInfo($"Adding Shoe Objects To Rack");
                manager.AddShoesToRack();
                addedToRack = true;
                Logger.LogInfo($"Shoes Added To Rack");
            }
        }

        public void SendLog(string message)
        {
            Logger.LogMessage(message);
        }
    }
}