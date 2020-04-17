using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Hand
{
    public List<Card> cards; // includes the cards out on the table
    public PlayerCardRanks handInfo;


    public Hand(List<Card> _cards)
    {
        cards = _cards;
        handInfo = new PlayerCardRanks();
        handInfo.InitPlayerCardRanks();

    }

    public Hand() { }


    public void InitStart(Player p)
    {
        cards = new List<Card>();
        handInfo = new PlayerCardRanks();
        handInfo.InitPlayerCardRanks();

    }


    public void InitStart()
    {
        cards = new List<Card>();
        handInfo = new PlayerCardRanks();
        handInfo.InitPlayerCardRanks();

    }


    public void AddCard(Card c)
    {
        cards.Add(c);
        SortCards();
        handInfo.AddSingleCardToDicts(c);
    }

    public void AddSingleCardToPlayerCardRanks(Card c)
    {
        handInfo.AddSingleCardToDicts(c);
    }

    public void CalculateRanks(List<Card> _cards)
    {
        List<Card> _sortedCards = SortCards(_cards);

        WinnerCalculator.FindPairs(handInfo);
        WinnerCalculator.FindTwoPairs(handInfo);
        WinnerCalculator.FindTriples(handInfo);
        WinnerCalculator.FindQuads(handInfo);
        WinnerCalculator.FindFlush(handInfo);
        WinnerCalculator.FindStraight(this);
        WinnerCalculator.FindStraightFlushOrRoyalFlush(this);
        WinnerCalculator.FindFullHouse(handInfo);

        //handInfo.PrintRanks();
    }

    public void CalculateRanks()
    {
        SortCards();
        Debug.Log($"Find Pairs: { WinnerCalculator.FindPairs(handInfo)}");
        WinnerCalculator.FindPairs(handInfo);
        WinnerCalculator.FindTwoPairs(handInfo);
        WinnerCalculator.FindTriples(handInfo);
        WinnerCalculator.FindQuads(handInfo);
        WinnerCalculator.FindFlush(handInfo);
        WinnerCalculator.FindStraight(this);
        WinnerCalculator.FindStraightFlushOrRoyalFlush(this);
        WinnerCalculator.FindFullHouse(handInfo);

        //handInfo.PrintRanks();
    }



    public void AddHandtoPlayerCardRanks()
    {

        //cards = cards.OrderBy(o => o.value).ToList(); // sorts cards before adding to dict
        handInfo.AddCardsToDicts(cards);

        CalculateRanks();

        //to do straight flush and royal flush

        //foreach (KeyValuePair<string, List<List<Card>>> kv in handInfo.MyRank){
        //    string s = "";
        //    foreach (List<Card> l in kv.Value)
        //    {
        //        s += l[0].ToString()+ ", ";
        //    }
        //    Debug.Log(kv.Key + ": " + s);
        //}
        //Debug.Log(handInfo.MyRank);



    }

    public List<Card> SortCards(List<Card> cards)
    {
        return cards = cards.OrderBy(o => o.rank).ToList(); // sorts cards before adding to dict
    }


    public void SortCards()
    {
        cards = cards.OrderBy(o => o.rank).ToList(); // sorts cards before adding to dict
    }


    public void PrintCards()
    {
        foreach (Card c in cards)
        {
            Debug.Log($"Rank: {c.rank} , Suit: {c.suit}");
        }
    }

    public void PrintCards(List<Card> c)
    {
        string s = "";
        foreach (Card cd in c)
        {
            s += ($"Rank: {cd.rank} , Suit: {cd.suit}\n ");
        }
        Debug.Log(s);
    }
}

