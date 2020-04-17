using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HAND_RANK{ highCard, pairs, twoPairs, triples, straight, flush, fullHouse, quads, straightFlush, royalFlush };

public class TieBreakers
{

}
public class PlayerCardRanks
{
    public Dictionary<Suit, List<Card>> SuitTotals;
    public Dictionary<Rank, List<Card>> CardTotals; // dict of card value mapped to a sorted dict of num occurences in hand

    public SortedDictionary<HAND_RANK, List<List<Card>>> MyRank;
    public List<int> cardInHandValues;

    public int highestRank;
    public List<Card> tieBreaker;

    public List<Card> hand;


    public void InitPlayerCardRanks()
    {
        cardInHandValues = new List<int>();

        SuitTotals = new Dictionary<Suit, List<Card>>();
        CardTotals = new Dictionary<Rank, List<Card>>();
        MyRank = new SortedDictionary<HAND_RANK, List<List<Card>>>();

        InitializeDictionaryCardLists();
    }


    public void GetHighestRank()
    {

    }


    public void InitializeDictionaryCardLists() // call once for each player
    {
        foreach (Rank i in Enum.GetValues(typeof(Rank)))
        {
            List<Card> c = new List<Card>();
            CardTotals[i] = c;
        }

        foreach (Suit s in Enum.GetValues(typeof(Suit)))
        {
            List<Card> c = new List<Card>();
            SuitTotals[s] = c;
        }

        foreach (HAND_RANK s in Enum.GetValues(typeof(HAND_RANK)))
        {
            List<List<Card>> c = new List<List<Card>>();
            MyRank[s] = c;
        }

    }


    public void AddCardsToDicts(List<Card> h)
    {

        foreach (Card c in h)
        {
            SuitTotals[c.suit].Add(c);
            cardInHandValues.Add((int)c.rank); // adds in card values for high value comparison

            CardTotals[c.rank].Add(c);

        }
    }

    public void AddSingleCardToDicts(Card c)
    {
        SuitTotals[c.suit].Add(c);
        cardInHandValues.Add((int)c.rank); // adds in card values for high value comparison
        CardTotals[c.rank].Add(c);

    }

    public void ClearDicts()
    {
        foreach (KeyValuePair<Rank, List<Card>> i in CardTotals)
        {
            i.Value.Clear();
        }
        foreach (KeyValuePair<Suit, List<Card>> i in SuitTotals)
        {
            i.Value.Clear();
        }
        foreach (var k in MyRank.Keys)
        {
            MyRank[k].Clear();
        }
    }


    public void InitializeHandWithCards()
    {

    }

    public void PrintRanks()
    {
        string s = "";

        foreach (var v in MyRank)
        {
            s += v.Key + ": ";
            foreach (List<Card> c in v.Value)
            {
                foreach (Card cc in c)
                {
                    s += cc.suit+ ", " + (int)cc.rank;
                }
            }
            s += "\n";



        }
        Debug.Log(s);
        Debug.Log($"Rank List:{MyRank.Count}");
    }

  

}
