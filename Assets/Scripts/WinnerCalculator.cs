using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WinnerCalculator
{
    bool hasRoyalFlush = false;
    bool hasStraightFlush = false;
    bool hasQuads = false;
    bool hasFullHouse = false;
    bool hasFlush = false;
    bool hasStraight = false;
    bool hasTrips = false;
    bool hasTwoPair = false;
    bool hasPair = false;
    bool hasOnlyHighCard = false;

    int highCardrank = 0;
    int highPair1rank = 0;
    int highPair2rank = 0;
    int highPair3rank = 0;
    int highTriprank = 0;
    int highTrip1rank = 0;
    int highQuadrank = 0;


    public List<Card> SortCards(List<Card> cards)
    {
        return cards = cards.OrderBy(o => o.rank).ToList(); // sorts cards before adding to dict
    }

    /// <summary>
    /// Only use this to dynamically add cards to players hands as you go
    /// </summary>
    /// <param name="h"></param>
    public void CheckHand(Hand h)
    {

        int numCards = h.cards.Count;

        switch (numCards)
        {
            case 2:
                if (h.handInfo.CardTotals[h.cards[0].rank].Count == 2) // checks for pocket hand
                {
                    hasPair = true;
                    highPair1rank = (int)h.cards[0].rank;
                }
                else
                {
                    highCardrank = Mathf.Max((int)h.cards[0].rank, (int)h.cards[1].rank);
                    hasOnlyHighCard = true;
                }
                break;


            case 3:
                foreach (Card c in h.cards)
                {
                    if (h.handInfo.CardTotals[c.rank].Count == 2)
                    {

                    }
                    else if (h.handInfo.CardTotals[c.rank].Count == 3)
                    {
                        hasTrips = true;
                    }
                }
                break;
        }
        foreach (KeyValuePair<Rank, List<Card>> item in h.handInfo.CardTotals)
        {
            if (item.Value.Count >= 2) // checks how many cards are in the dictionary for each card rank 1-13
            {
                hasPair = true;
                highPair1rank = (int)item.Key;
            }
        }

        foreach (KeyValuePair<Suit, List<Card>> item in h.handInfo.SuitTotals)
        {
            if (item.Value.Count >= 5) // checks how many cards are in the dictionary for each card rank 1-13
            {
                hasFlush = true;


            }
        }
    }


    public static bool FindPairs(PlayerCardRanks p) // returns true if any pairs (including more than one) are found and adds into hand info.
    {
        bool anyPairs = false;
        foreach (Rank key in p.CardTotals.Keys)
        {
            if (p.CardTotals[key].Count == 2 && !p.MyRank[HAND_RANK.pairs].Contains(p.CardTotals[key]))
            {
                p.MyRank[HAND_RANK.pairs].Add(p.CardTotals[key]);
                anyPairs = true;
            }
        }
        return anyPairs;
    }

    public static bool FindTwoPairs(PlayerCardRanks p)
    {
        if (p.MyRank[HAND_RANK.pairs].Count == 2)
        {
            p.MyRank[HAND_RANK.twoPairs].Add(p.MyRank[HAND_RANK.pairs][0]);
            p.MyRank[HAND_RANK.twoPairs].Add(p.MyRank[HAND_RANK.pairs][1]);
            return true;

        }
        else if (p.MyRank[HAND_RANK.pairs].Count > 2)
        {
            p.MyRank[HAND_RANK.twoPairs] = FindMaxTwoPairInCards(p.MyRank[HAND_RANK.pairs]);
            return true;
        }
        return false;
    }


    public static bool FindTriples(PlayerCardRanks p) // returns true if any pairs (including more than one) are found and adds into hand info.
    {
        bool anyTrips = false;
        foreach (Rank key in p.CardTotals.Keys)
        {
            if (p.CardTotals[key].Count == 3 && !p.MyRank[HAND_RANK.triples].Contains(p.CardTotals[key]))
            {
                p.MyRank[HAND_RANK.triples].Add(p.CardTotals[key]);
                anyTrips = true;
            }
        }
        return anyTrips;
    }

    public static bool FindQuads(PlayerCardRanks p) // returns true if any pairs (including more than one) are found and adds into hand info.
    {
        bool any = false;
        foreach (Rank key in p.CardTotals.Keys)
        {
            if (p.CardTotals[key].Count == 4 && !p.MyRank[HAND_RANK.quads].Contains(p.CardTotals[key]))
            {
                p.MyRank[HAND_RANK.quads].Add(p.CardTotals[key]);
                any = true;
            }
        }
        return any;
    }


    public static bool FindUnusedHighCards(PlayerCardRanks p) // to be written
    {
        return false;
    }

    public static List<Card> FindMaxRankInCards(List<List<Card>> cl) // takes in list of list of cards which returns the highest ranking list of cards with that list
    {
        int maxrankPair = 0;
        List<Card> maxPair = new List<Card>();
        foreach (List<Card> cards in cl)
        {

            if ((int)cards[0].rank > maxrankPair)
            {
                maxrankPair = (int)cards[0].rank;
                maxPair = cards;
            }
        }
        return maxPair;
    }


    public static List<List<Card>> FindMaxTwoPairInCards(List<List<Card>> cl) // takes in list of list of cards which returns the highest ranking list of cards with that list
    {
        int maxrankPair = 0;
        int nextMax = 0;
        List<Card> tempMax = new List<Card>();
        List<Card> tempNextMax = new List<Card>();

        List<List<Card>> maxPair = new List<List<Card>>();
        foreach (List<Card> cards in cl)
        {

            if ((int)cards[0].rank > maxrankPair)
            {
                tempNextMax = tempMax;
                tempMax = cards;
                nextMax = maxrankPair;
                maxrankPair = (int)cards[0].rank;
            }
        }
        maxPair.Add(tempMax);
        maxPair.Add(tempNextMax);

        return maxPair;
    }

    /// <summary>
    /// takes in list of cards and returns highest ranking card
    /// </summary>
    /// <param name="cl"></param>
    /// <returns></returns>
    public static Card FindMaxRankInHand(List<Card> cl) 
    {
        int maxrank = 0;
        Card maxCard = null;
        foreach (Card cards in cl)
        {

            if ((int)cards.rank > maxrank)
            {
                maxrank = (int)cards.rank;
                maxCard = cards;
            }
        }
        return maxCard;
    }


    public static bool FindFullHouse(PlayerCardRanks p) // returns true if any pairs (including more than one) are found and adds into hand info must be called after triples and pairs.
    {
        bool anyPairs = false;
        if (p.MyRank[HAND_RANK.pairs].Count > 0 && p.MyRank[HAND_RANK.triples].Count > 0)
        {
            anyPairs = true;
            if (p.MyRank[HAND_RANK.pairs].Count > 1)
            {
                p.MyRank[HAND_RANK.fullHouse].Add(FindMaxRankInCards(p.MyRank[HAND_RANK.pairs]));
            }
            else if (p.MyRank[HAND_RANK.triples].Count > 1)
            {
                p.MyRank[HAND_RANK.fullHouse].Add(FindMaxRankInCards(p.MyRank[HAND_RANK.triples]));
            }
            else
            {
                p.MyRank[HAND_RANK.fullHouse].Add(p.MyRank[HAND_RANK.pairs][0]);
                p.MyRank[HAND_RANK.fullHouse].Add(p.MyRank[HAND_RANK.triples][0]);

            }


        }
        return anyPairs;

    }


    public static bool FindFlush(PlayerCardRanks p) // need to handle checking for highest card in event of tie
    {
        bool hasFlushy = false;
        foreach (List<Card> c in p.SuitTotals.Values)
        {
            if (c.Count >= 5) // checksfor flush if more than 5 cards are in same suit then returns true
            {
                //List<Card> sc;
                //sc = c.OrderBy(o => o.rank).ToList();
                // sc.Reverse();// sorts cards before adding to dict

                //p.MyRank[HAND_RANK.flush].Add(sc.GetRange(0,5));
                p.MyRank[HAND_RANK.flush].Add(c);

                hasFlushy = true;
            }
        }
        return hasFlushy;
    }

    public static bool FindStraightFlushOrRoyalFlush(Hand h) // not very scalable since it checks based on hand sizes being 5, 6, or 7
    {

        if (h.handInfo.MyRank[HAND_RANK.flush].Count > 0)
        {
            foreach (List<Card> lc in h.handInfo.MyRank[HAND_RANK.flush])
            {
                List<Card> sc;
                sc = lc.OrderBy(o => o.rank).ToList();
                sc.Reverse();
                //need to check  A K 5 4 3 2

                if ((int)sc[0].rank == 14 && (int)sc[4].rank == 10) // checks for royal flush
                {
                    h.handInfo.MyRank[HAND_RANK.royalFlush].Add(sc.GetRange(0, 5));
                    return true;
                }
                else if (sc[0].rank == sc[4].rank + 4)
                {
                    h.handInfo.MyRank[HAND_RANK.straightFlush].Add(sc.GetRange(0, 5));
                    return true;

                }
                else if (sc.Count > 5 && sc[1].rank == sc[5].rank + 4)
                {
                    h.handInfo.MyRank[HAND_RANK.straightFlush].Add(sc.GetRange(1, 5));
                    return true;

                }
                else if (sc.Count > 6 && sc[2].rank == sc[6].rank + 4)
                {
                    h.handInfo.MyRank[HAND_RANK.straightFlush].Add(sc.GetRange(2, 5));
                    return true;

                }
                else if ((int)sc[0].rank == 14 && sc[sc.Count - 1].rank == sc[sc.Count - 4].rank - 3) // checks for A k 5 4 3 2
                {
                    List<Card> c = new List<Card>
                    {
                        sc[0],
                        sc[sc.Count - 1],
                        sc[sc.Count - 2],
                        sc[sc.Count - 3],
                        sc[sc.Count - 4]
                    };

                    h.handInfo.MyRank[HAND_RANK.straightFlush].Add(c);
                    return true;

                }

            }
        }
        return false;

    }

    public static bool FindRoyalFlush(Hand h)
    {
        if (h.handInfo.MyRank[HAND_RANK.straightFlush].Count > 0)
        {
            if ((int)h.handInfo.MyRank[HAND_RANK.straightFlush][0][0].rank == 10 && (int)h.handInfo.MyRank[HAND_RANK.straightFlush][0][4].rank == 14) // checks first and last rank of straight to see if its royal
            {
                h.handInfo.MyRank[HAND_RANK.royalFlush].Add(h.handInfo.MyRank[HAND_RANK.straightFlush][0]);
                return true;
            }
        }
        return false;
    }


    public static bool FindStraight(Hand h)
    {

        List<Card> sc;
        sc = h.cards.OrderBy(o => o.rank).ToList();
        sc.Reverse();

        sc = sc.GroupBy(x => x.rank).Select(x => x.First()).ToList();
        //DEBUGGING
        //h.PrintCards(sc);
        //if (sc.Count >= 5)
        //{
        //    Debug.Log(sc[0].rank);
        //    Debug.Log(sc[4].rank);

        //    Debug.Log(sc[sc.Count - 1]);
        //    Debug.Log(sc[sc.Count - 4]);
        //}


        if (sc.Count >= 5 && sc[0].rank == sc[4].rank + 4) // if count is >=5
        {
            h.handInfo.MyRank[HAND_RANK.straight].Add(sc.GetRange(0, 5));
            return true;

        }
        else if (sc.Count > 5 && sc[1].rank == sc[5].rank + 4) //if count is 6
        {
            h.handInfo.MyRank[HAND_RANK.straight].Add(sc.GetRange(1, 5));
            return true;

        }
        else if (sc.Count > 6 && sc[2].rank == sc[6].rank + 4) // if count is 7
        {
            h.handInfo.MyRank[HAND_RANK.straight].Add(sc.GetRange(2, 5));
            return true;

        }
        else if (sc.Count >= 5 && (int)sc[0].rank == 14 && sc[sc.Count - 1].rank == sc[sc.Count - 4].rank - 3) // checks for A k 5 4 3 2
        {
            List<Card> c = new List<Card>
            {
                sc[0],
                sc[sc.Count - 1],
                sc[sc.Count - 2],
                sc[sc.Count - 3],
                sc[sc.Count - 4]
            };
            h.handInfo.MyRank[HAND_RANK.straight].Add(c);
            return true;

        }
        return false;
    }
}
