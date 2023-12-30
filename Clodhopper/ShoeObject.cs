using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Clodhopper
{
    public class ShoeObject : MonoBehaviour
    {
        public string shoeID;

        // Setup Interactable Trigger And Parent To Ship
        public void SetupAsDisplay()
        {
            gameObject.AddComponent<AutoParentToShip>();
            InteractTrigger trigger = gameObject.AddComponent<InteractTrigger>();
            trigger.animationWaitTime = 2;
            trigger.cooldownTime = 1;
            trigger.disableTriggerMesh = true;
            trigger.holdInteraction = true;
            trigger.hoverTip = "Change Shoes: " + shoeID;
            trigger.interactable = true;
            trigger.interactCooldown = true;
            trigger.oneHandedItemAllowed = true;
            trigger.stopAnimationString = "SA_stopAnimation";
            trigger.timeToHold = 0.5f;
            
            trigger.holdingInteractEvent = new InteractEventFloat();
            trigger.onCancelAnimation = new InteractEvent();
            trigger.onInteract = new InteractEvent();
            trigger.onInteract.AddListener(SwitchShoesToThis);
            trigger.onInteractEarly = new InteractEvent();
            trigger.onStopInteract = new InteractEvent();
        }

        // When Interacting With Shoe Object
        // Send Message To Tell Client's What Shoe To Enable For This Player
        public void SwitchShoesToThis(PlayerControllerB player = null)
        {
            if (player == null) player = GameNetworkManager.Instance.localPlayerController;

            HUDManager.Instance.AddTextToChatOnServer("Clodhopper Custom RPC Send Custom Message Shoe Type;" + shoeID + ";" + player.NetworkObjectId);
        }
    }
}
