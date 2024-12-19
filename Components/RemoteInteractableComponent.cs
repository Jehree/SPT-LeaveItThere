using Comfort.Common;
using EFT;
using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentCaches.Components
{
    internal class RemoteInteractableComponent : InteractableObject
    {
        public List<CustomInteractionAction> Actions = new List<CustomInteractionAction>();
    }
}
