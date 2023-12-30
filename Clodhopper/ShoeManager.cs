using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using System.IO;
using BepInEx;

namespace Clodhopper
{
    public class ShoeManager : MonoBehaviour
    {
        public List<AssetBundle> shoes;
        public static ShoeManager Instance { get; private set; }

        // Set Instance
        private void Awake()
        {
            if (ShoeManager.Instance == null) ShoeManager.Instance = this;
        }

        // Load All Shoe Asset Bundles
        public void LoadShoes()
        {
            string[] paths = Directory.GetFiles(Path.Combine(Paths.PluginPath, "Makarew-Clodhopper", "ShoeBundles"), "*.shoe", SearchOption.AllDirectories);

            foreach (string path in paths)
            {
                shoes.Add(AssetBundle.LoadFromFile(path));
            }
        }

        // Load Shoe Objects For The Player To Select From In Ship
        public void AddShoesToRack()
        {
            for (int i = 0; i < shoes.Count; i++)
            {
                GameObject obj = GameObject.Instantiate((GameObject)shoes[i].LoadAsset("ShoeObject"));
                ShoeObject shoe = obj.GetComponent<ShoeObject>();
                shoe.SetupAsDisplay();

                AutoParentToShip par = obj.GetComponent<AutoParentToShip>();
                par.overrideOffset = true;
                par.positionOffset = new Vector3(-2.45f + (0.3f * i), 2.9f, -8.41f );
                par.rotationOffset = new Vector3(-90, 130, 0);
            }
        }
    }
}
