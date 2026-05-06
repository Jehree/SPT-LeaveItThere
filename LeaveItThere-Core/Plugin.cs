using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using Helpers.CursorHelper;
using LeaveItThere.ConsoleCommands;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.ModSettings;
using LeaveItThere.Patches;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LeaveItThere-FikaModule")]

namespace LeaveItThere;

[BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "3.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const string DataToServerURL = "/jehree/leaveitthere/data_to_server";
    public const string DataToClientURL = "/jehree/leaveitthere/data_to_client";

    public static ManualLogSource LogSource;

    internal void Awake()
    {
        LogSource = Logger;

        Settings.Init(Config);

        if (LeaveItThereHelper.FikaInstalled)
        {
            FikaBridge.LoadFikaModuleAssembly();
            new EarlyGameStartedPatchFika().Enable();
        }
        else
        {
            new EarlyGameStartedPatch().Enable();
        }

        new GameEndedPatch().Enable();
        new GetAvailableActionsPatch().Enable();
        new InteractionsChangedHandlerPatch().Enable();
        new LootExperiencePatch().Enable();

        new InteractionHelper.InteractionsChangedHandlerPatch().Enable();
        new CursorHelper.InputManagerUpdatePatch().Enable();

        ConsoleScreen.Processor.RegisterCommandGroup<CommandGroup>();
    }

    internal void OnEnable()
    {
        FikaBridge.PluginEnable();
    }
}
