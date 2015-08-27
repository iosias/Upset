using RiotSharp;
using RiotSharp.GameEndpoint;
using RiotSharp.MatchEndpoint;
using RiotSharp.StaticDataEndpoint;
using RiotSharp.SummonerEndpoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upset.Models;
using Upset.Types;

namespace Upset.Functionality
{
    public static class SetBuilder
    {
        private static Dictionary<string, List<ItemStatic>> PotentialUpgrades;
        private static ItemListStatic Items;
        private static ChampionListStatic Champions;
        private static string API_KEY = "ef9fe4c6-c0d3-4d07-87a3-38c50626234f";
        private static int NEW_GROUP_FRAME_GAP = 3;
        private static int MAX_TRIES = 5;

        public static List<BuildSet> GetRecentGameBuilds(string summonerName)
        {
            var sets = new List<BuildSet>();
            var summoner = GetSummoner(summonerName);
            if (summoner == null) { return null; }
            var games = GetRecentGamesEventually(summoner);
            foreach (var match in games.OrderBy(x => x.CreateDate))
            {
                sets.Add(GetGameBuild(match, summoner));
            }
            return sets;
        }


        public static BuildSet GetMatchBuild(long MatchId, long SummonerId, int DamageDealt)
        {
            var match = GetMatchDetailEventually(MatchId);
            var participant = match.Participants.FirstOrDefault(x => x.Stats.TotalDamageDealt == DamageDealt);
            return GetMatchBuild(match, participant);
        }

        public static BuildSet GetMatchBuild(MatchDetail match, Participant participant)
        {
            if (PotentialUpgrades == null) InitializePotentialUpgrades();
            var set = new BuildSet() { HasMatchData = true };
            set.TimeSince = GetTimeSince(match.MatchCreation);

            var timeline = match.Timeline;
            var allPurchasedItems = getAllPurchasedItems(timeline, participant.ParticipantId);

            set.InitialPurchase = getStartingItems(timeline, participant.ParticipantId);
            set.FinalBuild = getFinalBuild(participant);
            set.RushItem = getRushItem(allPurchasedItems.ToList(), set.InitialPurchase.Items.ToList());

            var allConsumables = getConsumables(allPurchasedItems.ToList());
            if(allConsumables.Count > 0) set.Consumables = (getPurchaseSet("Consumables", allConsumables, includePrice: false));

            set.FullBuild = set.FinalBuild.Items.Count == 7 && !(set.FinalBuild.Items.Any(x => PotentialUpgrades.ContainsKey(x.Id.ToString())));
            set.Id = match.MatchId;
            set.MatchDataFetched = true;
            return set;
        }

        public static BuildSet GetGameBuild(Game game, Summoner summoner)
        {
            if (PotentialUpgrades == null) InitializePotentialUpgrades();
            var set = new BuildSet();
            set.TimeSince = GetTimeSince(game.CreateDate);
            set.FinalBuild = getFinalBuild(game.Statistics);
            set.Champion = getChampion(game.ChampionId);
            set.FullBuild = set.FinalBuild.Items.Count == 7 && !(set.FinalBuild.Items.Any(x => PotentialUpgrades.ContainsKey(x.Id.ToString())));
            set.Id = game.GameId;
            set.SummonerId = summoner.Id;
            set.MatchDataFetched = false;
            set.TotalDamageDealt = game.Statistics.TotalDamageDealt;
            return set;
        }

        private static string GetTimeSince(DateTime time)
        {
            return time.ToString("MMMM d, h:mm tt");
        }

        private static List<ItemStatic> getConsumables(List<ItemStatic> list)
        {
            return list.Where(x => x.Consumed).Distinct().ToList();
        }

        private static PurchaseSet getPurchaseSet(string name, List<ItemStatic> items, bool includePrice = true)
        {
            var purchaseSet = new PurchaseSet() { Name = name, RecMath = false };
            foreach (var item in items)
            {
                purchaseSet.Items.Add(item);
                if (includePrice) purchaseSet.TotalCost += item.Gold.TotalPrice;
            }
            return purchaseSet;
        }

        private static PurchaseSet getFinalBuild(RawStat stats)
        {
            var finalBuild = new List<ItemStatic>();
            if (stats.Item0 != 0)
            {
                finalBuild.Add(getItem(stats.Item0));
            }
            if (stats.Item1 != 0)
            {
                finalBuild.Add(getItem(stats.Item1));
            }
            if (stats.Item2 != 0)
            {
                finalBuild.Add(getItem(stats.Item2));
            }
            if (stats.Item3 != 0)
            {
                finalBuild.Add(getItem(stats.Item3));
            }
            if (stats.Item4 != 0)
            {
                finalBuild.Add(getItem(stats.Item4));
            }
            if (stats.Item5 != 0)
            {
                finalBuild.Add(getItem(stats.Item5));
            }
            if (stats.Item6 != 0)
            {
                finalBuild.Add(getItem(stats.Item6));
            }
            return new PurchaseSet() { Items = finalBuild, RecMath = false, Name = "Final Build" };
        }

        private static PurchaseSet getFinalBuild(Participant participant)
        {
            var finalBuild = new List<ItemStatic>();
            var stats = participant.Stats;
            if (stats.Item0 != 0)
            {
                finalBuild.Add(getItem(stats.Item0));
            }
            if (stats.Item1 != 0)
            {
                finalBuild.Add(getItem(stats.Item1));
            }
            if (stats.Item2 != 0)
            {
                finalBuild.Add(getItem(stats.Item2));
            }
            if (stats.Item3 != 0)
            {
                finalBuild.Add(getItem(stats.Item3));
            }
            if (stats.Item4 != 0)
            {
                finalBuild.Add(getItem(stats.Item4));
            }
            if (stats.Item5 != 0)
            {
                finalBuild.Add(getItem(stats.Item5));
            }
            if (stats.Item6 != 0)
            {
                finalBuild.Add(getItem(stats.Item6));
            }
            return new PurchaseSet() { Items = finalBuild, RecMath = false, Name = "Final Build" };
        }

        private static List<ItemStatic> getAllPurchasedItems(Timeline timeline, int participantId)
        {
            List<ItemStatic> items = new List<ItemStatic>();

            foreach (var frame in timeline.Frames)
            {
                var purchasesThisFrame = new List<Event>();
                var sellbacksThisFrame = new List<Event>();
                if (frame.Events != null)
                {
                    purchasesThisFrame = frame.Events.Where(x => x.ParticipantId == participantId && x.EventType == EventType.ItemPurchased).ToList();
                    sellbacksThisFrame = frame.Events.Where(x => x.ParticipantId == participantId && x.EventType == EventType.ItemUndo).ToList();
                }
                foreach (var purchase in purchasesThisFrame)
                {
                    items.Add(getItem(purchase.ItemId));
                }
                foreach (var sellback in sellbacksThisFrame)
                {
                    if (items.Any(x => x.Id == sellback.ItemId)) items.Remove(items.First(x => x.Id == sellback.ItemId));
                }
            }
            return items;
        }

        private static PurchaseSet getRushItem(List<ItemStatic> items, List<ItemStatic> alreadyOwnedItems)
        {
            ItemStatic RushItem = items.FirstOrDefault(x => x.From != null && (x.Into == null || x.Into.Count == 1 && getItem(x.Into.First()).SpecialRecipe != 0) && x.Gold.TotalPrice > 300);
            if (RushItem == null) return null;
            var possibleComponents = items.GetRange(0, items.IndexOf(RushItem));
            var buildOrder = possibleComponents.Where(x => RushItem.From.Contains(x.Id.ToString())).ToList();
            PurchaseSet set = new PurchaseSet() { Name = "Rush " + RushItem.Name, RecMath = true };
            foreach (var item in buildOrder)
            {
                set.Items.Add(item);
                set.TotalCost += item.Gold.TotalPrice;
            }
            foreach (var from in RushItem.From.ToList())
            {
                if (!set.Items.Any(x => x.Id.ToString() == from))
                {
                    var item = getItem(from);
                    set.Items.Add(item);
                    set.TotalCost += item.Gold.TotalPrice;

                }
            }
            var componentTree = RushItem.From.ToList();
            while (componentTree.Any())
            {
                var item = getItem(componentTree[0]);
                componentTree.RemoveAt(0);
                var dupe = alreadyOwnedItems.FirstOrDefault(x => x.Id == item.Id);
                if (dupe is ItemStatic)
                {
                    alreadyOwnedItems.Remove(alreadyOwnedItems.First(x => x.Id == dupe.Id));
                    if (set.Items.Any(x => x.Id == dupe.Id))
                    {
                        set.Items.Remove(set.Items.First(x => x.Id == dupe.Id));
                    }
                    set.TotalCost -= dupe.Gold.TotalPrice;
                }
                else
                {
                    if (item.From != null)
                    {
                        foreach (var key in item.From)
                        {
                            componentTree.Add(key);
                        }
                    }
                }
            }


            set.Items.Add(RushItem);
            if (RushItem.Into != null && RushItem.Into.Count == 1 && getItem(RushItem.Into.First()).SpecialRecipe != 0)
            {
                set.Items.Add(getItem(RushItem.Into.First()));
            }
            set.TotalCost += RushItem.Gold.BasePrice;

            set.Name = set.Name + " (" + set.TotalCost + " gold)";
            return set;
        }


        private static PurchaseSet getStartingItems(Timeline timeline, int participantId)
        {
            PurchaseSet set = new PurchaseSet() { Name = "Starting Items", RecMath = false };
            bool foundAny = false;

            foreach (var frame in timeline.Frames)
            {
                var purchasesThisFrame = new List<Event>();
                var sellbacksThisFrame = new List<Event>();
                if (frame.Events != null)
                {
                    purchasesThisFrame = frame.Events.Where(x => x.ParticipantId == participantId && x.EventType == EventType.ItemPurchased).ToList();
                    sellbacksThisFrame = frame.Events.Where(x => x.ParticipantId == participantId && (x.EventType == EventType.ItemSold || x.EventType == EventType.ItemUndo)).ToList();
                }
                foreach (var purchase in purchasesThisFrame)
                {
                    set.Items.Add(getItem(purchase.ItemId));
                }
                foreach (var sellback in sellbacksThisFrame)
                {
                    if (set.Items.Any(x => x.Id == sellback.ItemId)) set.Items.Remove(set.Items.First(x => x.Id == sellback.ItemId));
                }
                if (foundAny) { break; }
                if (purchasesThisFrame.Any()) { foundAny = true; }
            }
            foreach (var item in set.Items)
            {
                set.TotalCost += item.Gold.TotalPrice;
            }
            set.Name = set.Name + " (" + set.TotalCost + " gold)";
            return set;
        }

        private static List<List<Event>> getPurchaseGroups(Timeline timeline, int participantId)
        {
            var purchaseGroups = new List<List<Event>>();
            var emptyFrameCount = 0;
            var currentGroup = new List<Event>();
            foreach (var frame in timeline.Frames)
            {
                if (emptyFrameCount > NEW_GROUP_FRAME_GAP)
                {
                    if (currentGroup.Any()) purchaseGroups.Add(currentGroup);
                    currentGroup = new List<Event>();
                }
                var purchasesThisFrame = new List<Event>();
                var sellbacksThisFrame = new List<Event>();
                if (frame.Events != null)
                {
                    purchasesThisFrame = frame.Events.Where(x => x.ParticipantId == participantId && x.EventType == EventType.ItemPurchased).ToList();
                    sellbacksThisFrame = frame.Events.Where(x => x.EventType == EventType.ItemSold || x.EventType == EventType.ItemUndo).ToList();
                }

                foreach (var purchase in purchasesThisFrame)
                {
                    currentGroup.Add(purchase);
                }
                foreach (var sellback in sellbacksThisFrame)
                {
                    if (currentGroup.Any(x => x.ItemId == sellback.ItemId)) currentGroup.Remove(currentGroup.First(x => x.ItemId == sellback.ItemId));
                    if (purchasesThisFrame.Any(x => x.ItemId == sellback.ItemId)) purchasesThisFrame.Remove(purchasesThisFrame.First(x => x.ItemId == sellback.ItemId));
                }
                var meaningfulPurchases = purchasesThisFrame.Any(x => getItem(x.ItemId).Gold.TotalPrice > 150);

                var isFirstPurchase = meaningfulPurchases && !purchaseGroups.Any();
                if (isFirstPurchase && emptyFrameCount > 0)
                {
                    purchaseGroups.Add(currentGroup);
                    currentGroup = new List<Event>();
                }
                emptyFrameCount = (meaningfulPurchases) ? 0 : emptyFrameCount + 1;
            }
            if (currentGroup.Any()) purchaseGroups.Add(currentGroup);
            return purchaseGroups;
        }


        private static ItemStatic getItem(string itemId)
        {
            return getItem(Int32.Parse(itemId));
        }

        private static ItemStatic getItem(long itemId)
        {
            return getItem(itemId.ToString());
        }


        private static ItemStatic getItem(int itemId)
        {
            if (Items == null)
            {
                Items = StaticRiotApi.GetInstance(API_KEY).GetItems(Region.na, ItemData.all);
            }
            if (Items.Items.ContainsKey(itemId))
            {
                var item = Items.Items[itemId];
                return item;
            }
            return Items.Items.Values.First();
        }


        private static ChampionStatic getChampion(int championId)
        {
            if (Champions == null)
            {
                Champions = StaticRiotApi.GetInstance(API_KEY).GetChampions(Region.na, ChampionData.all);
            }
            var champion = Champions.Champions.First(x => x.Value.Id == championId);
            return champion.Value;
        }

        public static MatchDetail GetMatchDetailEventually(MatchSummary summary)
        {
            return GetMatchDetailEventually(summary.MatchId);
        }

        public static MatchDetail GetMatchDetailEventually(long id)
        {
            MatchDetail detail = null;
            var tries = 0;
            while (detail == null)
            {
                if (tries > MAX_TRIES) { throw new Exception(); }
                tries++;
                detail = GetMatchDetail(id);
            }
            return detail;
        }

        private static MatchDetail GetMatchDetail(long id)
        {
            var api = RiotApi.GetInstance(API_KEY);
            var detail = api.GetMatch(Region.na, id, includeTimeline: true);
            return detail;
        }

        public static List<MatchSummary> GetRecentMatchesEventually(Summoner summoner)
        {
            List<MatchSummary> summaries = null;
            var tries = 0;
            while (summaries == null)
            {
                if (tries > MAX_TRIES) { throw new Exception(); }
                tries++;
                summaries = GetRecentMatches(summoner);
            }
            return summaries;
        }

        public static List<MatchSummary> GetRecentMatches(Summoner summoner)
        {

            try
            {
                var api = RiotApi.GetInstance(API_KEY);
                var matches = api.GetMatchHistory(Region.na, summoner.Id);
                return matches;
            }
            catch
            {
                return null;
            }
        }

        public static List<Game> GetRecentGamesEventually(Summoner summoner)
        {
            var tries = 0;
            List<Game> summaries = null;
            while (summaries == null)
            {
                if (tries > MAX_TRIES) { throw new Exception(); }
                tries++;
                summaries = GetRecentGames(summoner);
            }
            return summaries;
        }

        public static List<Game> GetRecentGames(Summoner summoner)
        {

            try
            {
                var api = RiotApi.GetInstance(API_KEY);
                var games = api.GetRecentGames(Region.na, summoner.Id);
                return games;
            }
            catch
            {
                return null;
            }
        }

        public static Summoner GetSummonerEventually(string name)
        {
            var tries = 0;
            Summoner summoner = null;
            while (summoner == null)
            {
                if (tries > MAX_TRIES) { throw new Exception(); }
                tries++;
                summoner = GetSummoner(name);
            }
            return summoner;
        }

        public static Summoner GetSummoner(string name)
        {
            try
            {
                var api = RiotApi.GetInstance(API_KEY);
                var summoner = api.GetSummoner(Region.na, name);
                return summoner;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        internal static UpsetModel GetUpsetModel()
        {
            UpsetModel model = new UpsetModel();
            if (Items == null) Items = StaticRiotApi.GetInstance(API_KEY).GetItems(Region.na, ItemData.all);
            if (PotentialUpgrades == null) InitializePotentialUpgrades();
            model.CompleteItems = Items.Items.Where(x => x.Value.Into == null && x.Value.From != null).Select(x => x.Value).ToList();
            model.PotentialUpgrades = PotentialUpgrades;
            return model;
        }

        private static void InitializePotentialUpgrades()
        {
            PotentialUpgrades = new Dictionary<string, List<ItemStatic>>();
            foreach (var item in Items.Items)
            {
                var key = item.Key;
                if (item.Value.Into == null) continue;

                var openList = new Stack<int>(item.Value.Into);
                var potentialUpgrades = new List<ItemStatic>();
                while (openList.Any())
                {
                    var id = openList.Pop();
                    var potentialUpgrade = getItem(id);
                    if (potentialUpgrade.Into != null)
                    {
                        foreach (var nextId in potentialUpgrade.Into) openList.Push(nextId);
                    }
                    else
                    {
                        if (!potentialUpgrades.Contains(potentialUpgrade) &&
                            (potentialUpgrade.Maps == null || potentialUpgrade.Maps["11"]) &&
                            (potentialUpgrade.Tags == null || !potentialUpgrade.Tags.Contains("Bilgewater")))
                        {
                            potentialUpgrades.Add(potentialUpgrade);
                        }
                    }
                }
                PotentialUpgrades[key.ToString()] = potentialUpgrades;
            }
        }
    }
}
