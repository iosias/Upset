using RiotSharp.StaticDataEndpoint;
using System.Collections.Generic;

namespace Upset.Types
{
    public class BuildSet
    {
        public ChampionStatic Champion { get; set; }
        public PurchaseSet InitialPurchase { get; set; }
        public PurchaseSet RushItem { get; set; }
        public PurchaseSet FinalBuild { get; set; }
        public PurchaseSet Consumables { get; set; }
        public bool HasMatchData { get; set; }
        public bool FullBuild { get; set; }
        public bool MatchDataFetched { get; set; }
        public string TimeSince { get; set; }
        public long Id { get; set; }
        public long SummonerId { get; set; }
        public int TotalDamageDealt { get; set; }
    }
}