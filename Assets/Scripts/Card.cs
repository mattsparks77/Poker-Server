using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Rank
{
    TWO = 2, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN,
    JACK, QUEEN, KING, ACE
}

public enum Suit
{
    Club,
    Diamond,
    Heart,
    Spade,
}
public class Card
{
    public Rank rank;
    public Suit suit;
    public string stringRank;
    public string stringSuit;

    public Card(Suit newSuit, Rank newRank)
    {
        suit = newSuit;
        rank = newRank;
        stringSuit = suit.ToString();
        int _rank = (int)rank;
        stringRank = _rank.ToString();
    }


}

