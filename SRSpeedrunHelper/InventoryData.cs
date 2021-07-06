using System.Collections.Generic;

namespace SRSpeedrunHelper
{
    public class InventoryData
    {
        public List<IdentifiableCountPair> AmmoList { get; set; }

        public PlayerState.AmmoMode AmmoMode { get; set; }

        // Need parameterless constructor for serialization
        public InventoryData()
        {
            AmmoList = new List<IdentifiableCountPair>();
            AmmoMode = PlayerState.AmmoMode.DEFAULT;
        }

        public InventoryData(PlayerState.AmmoMode ammoMode)
        {
            AmmoList = new List<IdentifiableCountPair>();
            AmmoMode = ammoMode;
        }

        public void AddSlot(Identifiable.Id id, int count)
        {
            AmmoList.Add(new IdentifiableCountPair(id, count));
        }

        public class IdentifiableCountPair
        {
            public Identifiable.Id Id { get; set; }

            public int Count { get; set; }

            // Need parameterless constructor for serialization
            public IdentifiableCountPair()
            {
                Id = Identifiable.Id.NONE;
                Count = -1;
            }

            public IdentifiableCountPair(Identifiable.Id id, int count)
            {
                Id = id;
                Count = count;
            }
        }
    }
}
