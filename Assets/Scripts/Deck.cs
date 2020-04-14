using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Deck
{
    private static Card[] cards;

    private static System.Random rng = new System.Random();
    public static int currentCardIndex = 0;
    public static void CreateDeck()
    {
        cards = new Card[52];
        for (int suitVal = 0; suitVal < 4;
             suitVal++)
        {
            for (int rankVal = 2; rankVal < 15;
                 rankVal++)
            {
                cards[suitVal * 13 + rankVal - 2]
                = new Card((Suit)suitVal,
                (Rank)rankVal);
            }
        }
    }
    /// <summary>
    /// Gets next card from deck and increments deck index.
    /// </summary>
    /// <returns></returns>
    public static Card NextCard()
    {
        Card c = GetCard(currentCardIndex);
        currentCardIndex++;
        return c;
    }

    /// <summary>
    /// Burns top card for simulating a deal.
    /// </summary>
    public static void BurnCard()
    {
        currentCardIndex++;
    }
    /// <summary>
    /// Returns a card object given it's index in the deck.
    /// </summary>
    /// <param name="cardNum"></param>
    /// <returns></returns>
    public static Card GetCard(int cardNum)
    {
        if (currentCardIndex > 51)
        {
            currentCardIndex = 0;
        }
        if (cardNum >= 0 && cardNum <= 51)
            return cards[cardNum];
        else
            throw
            (new System.ArgumentOutOfRangeException
            ("cardNum", cardNum,
             "Value must be between 0 and 51."));
    }

    /// <summary>
    /// Shuffle method for lists using Fisher Yates shuffle algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void ShuffleList<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle()
    {
        currentCardIndex = 0;
        Card[] newDeck = new Card[52];
        bool[] assigned = new bool[52];
        Random sourceGen = new Random();

        for (int i = 0; i < 52; i++)
        {
            int destCard = 0;
            bool foundCard = false;
            while (foundCard == false)
            {
                destCard = sourceGen.Next(52);
                if (assigned[destCard] == false)
                    foundCard = true;
            }
            assigned[destCard] = true;
            newDeck[destCard] = cards[i];
        }
        newDeck.CopyTo(cards, 0);
    }
}
