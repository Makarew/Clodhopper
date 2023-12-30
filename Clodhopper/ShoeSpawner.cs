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
    }
}
