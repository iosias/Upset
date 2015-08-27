using RiotSharp.StaticDataEndpoint;
using System.Collections.Generic;

namespace Upset.Models
{
    public class UpsetModel
    {
        public List<ItemStatic> CompleteItems;
        public Dictionary<string, List<ItemStatic>> PotentialUpgrades;
    }
}
