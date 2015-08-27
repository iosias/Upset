using RiotSharp.StaticDataEndpoint;
using System.Collections.Generic;

namespace Upset.Types
{
    public class PurchaseSet
    {
        public string Name { get; set; }
        public int TotalCost { get; set; }
        public List<ItemStatic> Items { get; set; }
        public bool RecMath { get; set; }

        public PurchaseSet()
        {
            Items = new List<ItemStatic>();
        }
    }
}
