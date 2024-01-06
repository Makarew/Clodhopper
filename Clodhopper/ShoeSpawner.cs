using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Clodhopper
{
    // Holds Data For The Player This Is Attached To
    internal class ShoeSpawner : MonoBehaviour
    {
        public List<GameObject> shoesLeft;
        public List<GameObject> shoesRight;

        public string enabledShoe = "Default";

        public bool showShoes = true;

        // Toggle Local Player Shoe Visibility
        public void ToggleLocalPlayerShoes()
        {
            if (showShoes)
            {
                foreach (var sho in shoesLeft)
                {
                    sho.SetActive(false);
                }
                foreach (var sho in shoesRight)
                {
                    sho.SetActive(false);
                }

                showShoes = false;
            } else
            {
                foreach (var sho in shoesLeft)
                {
                    if (sho.GetComponent<ShoeObject>().shoeID == enabledShoe) { sho.SetActive(true); }
                    else { sho.SetActive(false); }
                }
                foreach (var sho in shoesRight)
                {
                    if (sho.GetComponent<ShoeObject>().shoeID == enabledShoe) { sho.SetActive(true); }
                    else { sho.SetActive(false); }
                }

                showShoes = true;
            }
        }
    }
}
