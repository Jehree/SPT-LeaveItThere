using Comfort.Common;
using EFT;
using PersistentCaches.Components;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            player.gameObject.AddComponent<PersistentCachesSession>();
            return true;
        }
    }
}
