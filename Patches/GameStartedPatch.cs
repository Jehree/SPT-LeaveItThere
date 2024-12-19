using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using PersistentCaches.Components;
using PersistentCaches.Helpers;
using SPT.Reflection.Patching;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace PersistentCaches.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            PersistentCachesSession.CreateNewSession();

            string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/test.json";
            Item item = ItemHelper.GetItemFromFile(dirName);
            if (item == null) return true;

            ItemHelper.SpawnItem(item, player.Transform.position);

            return true;
        }
    }
}
