namespace LeaveItThereServer.Models;

public class ModConfig
{
    private int _maxProfileBackupCount = 0;
    public required int MaxProfileBackupCount
    {
        get => _maxProfileBackupCount;
        set => _maxProfileBackupCount = Math.Max(0, value);
    }
    public required bool RemoveInRaidRestrictions { get; set; }
    public required bool EverythingIsDiscardable { get; set; }
    public required bool RemoveBackpackRestrictions { get; set; }
    public required bool GlobalItemDataProfile { get; set; }
}