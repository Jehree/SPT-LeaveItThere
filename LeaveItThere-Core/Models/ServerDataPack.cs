using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LeaveItThere.Models
{
    internal class ServerDataPack
    {
        public string ProfileId { get; set; }
        public string MapId { get; set; }
        public List<PlacedItemData> ItemTemplates { get; set; } = [];

        [JsonIgnore]
        public static ServerDataPack Request => new()
        {
            ProfileId = FikaBridge.GetRaidId(),
            MapId = Singleton<GameWorld>.Instance.LocationId,
        };

        public ServerDataPack() { }

        public ServerDataPack(Dictionary<string, FakeItem> fakeItems)
        {
            List<PlacedItemData> dataList = [];

            foreach (FakeItem fakeItem in fakeItems.Values)
            {
                dataList.Add(new PlacedItemData(fakeItem));
            }

            ItemTemplates = dataList;
            ProfileId = FikaBridge.GetRaidId();
            MapId = Singleton<GameWorld>.Instance.LocationId;
        }
    }
}
