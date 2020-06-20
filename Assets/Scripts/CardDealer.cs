using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CardDealer
{
    public static void DealOpeningCards()
    {
        //need to add dealing to specific order at the table and simulating a real deal using player.tableIndex and using current small /big blind and dealer index.
        foreach (Player p in GameLogic.playersInGame) //Server.clients.Values
        {
            if (p != null)
            {
                Card _card = Deck.NextCard();
                Server.clients[p.id].player.cards[0] = _card;

                p.cards[0] = _card; //adds first card to players hand on server
                ServerSend.SendCard(_card, p.id); //sends card to players hand on client
                Debug.Log($"Card {_card.rank.ToString()}- {_card.suit.ToString()}: User: {p.username} , TablePosition: {p.tableIndex}");

            }

        }
        foreach (Player p in GameLogic.playersInGame) //Server.clients.Values
        {
            if (p != null)
            {
                //Debug.Log($"Card 2: User: {p.username} , TablePosition: {p.tableIndex}");

                Card _card = Deck.NextCard();
                Server.clients[p.id].player.cards[1] = _card;
                p.cards[1] = _card;//adds second card to players hand on server
                ServerSend.SendCard(_card, p.id); //sends second card to players hand on client
                Debug.Log($"Card {_card.rank.ToString()}- {_card.suit.ToString()}: User: {p.username} , TablePosition: {p.tableIndex}");

            }

        }
    }


    public static void Flop()
    {
        Deck.BurnCard(); // discards one card from the pile
        Card _card1 = Deck.NextCard();
        Card _card2 = Deck.NextCard();
        Card _card3 = Deck.NextCard();
        Debug.Log($"[Community Card] {_card1.rank.ToString()}- {_card1.suit.ToString()}");
        Debug.Log($"[Community Card] {_card2.rank.ToString()}- {_card2.suit.ToString()}");
        Debug.Log($"[Community Card] {_card3.rank.ToString()}- {_card3.suit.ToString()}");

        //adds cards to servers community card pool
        GameLogic.communityCards.Add(_card1);
        GameLogic.communityCards.Add(_card2);
        GameLogic.communityCards.Add(_card3);

        //sends cards to clients community card pool
        ServerSend.CardToCommunity(_card1);
        ServerSend.CardToCommunity(_card2);
        ServerSend.CardToCommunity(_card3);
    }


    public static void River()
    {
        Deck.BurnCard(); // discards one card from the pile
        Card _card1 = Deck.NextCard();
        Debug.Log($"[River] {_card1.rank.ToString()}- {_card1.suit.ToString()}");

        //adds cards to servers community card pool
        GameLogic.communityCards.Add(_card1);

        //sends cards to clients community card pool
        ServerSend.CardToCommunity(_card1);
    }

    public static void Turn()
    {
        Deck.BurnCard(); // discards one card from the pile
        Card _card1 = Deck.NextCard();
        Debug.Log($"[Turn] {_card1.rank.ToString()}- {_card1.suit.ToString()}");

        //adds cards to servers community card pool
        GameLogic.communityCards.Add(_card1);

        //sends cards to clients community card pool
        ServerSend.CardToCommunity(_card1);
    }
}

