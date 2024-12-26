using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class TestComponent : MonoBehaviour
    {
        public void Update()
        {
            var lootItem = gameObject.GetComponent<ObservedLootItem>();
            Plugin.LogSource.LogError(lootItem.Item.Id);
        }
    }
}
