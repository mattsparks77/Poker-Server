using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GameLogic
{
    public static int currentBet = 0;
    public static int amountInPot = 0;

    public static int currentTurnIndex = 0;

    public static int bigBlind = 10;
    public static int smallBlind = 5;
    public static int smallBlindIndex = 0;
    public static int bigBlindIndex = 0;
    public static int dealerIndex = 0;
    public static int firstTurnIndex = 0;

    public static int numPlayersAtTable = 0;
    public static int playersLeft = 0;

    public static List<Player> playersInGame = new List<Player>();
    public static List<Card> communityCards = new List<Card>();

    public static int dealCounter = 0;
    public static int deckIndex = 0;

    public static bool roundStarted = false;


    public static void InitializePlayers()
    {
        for (int i = 0; i < Server.clients.Count; i++) playersInGame.Add(null);
    }

    /// <summary>
    /// Clears playersInGame list, re-adds in null values for table seats. Gathers players who have a seat index > -1. Then removes the extra null spaces in the playersInGame.
    /// </summary>
    public static void ResetTable()
    {
        roundStarted = false;
        playersInGame.Clear();
        dealCounter = 0;
        InitializePlayers();
        GatherPlayers();
        RemoveNullPlayers();
        

    }

    public static void IncrementBigSmallBlindIndex()
    {

    }


    public static void RemoveNullPlayers()
    {
        playersInGame.RemoveAll(item => item == null);
        //sets each players table index to their new index in the shrunken list of players after removing the null players.
        for (int i = 0; i < playersInGame.Count - 1; i++)
        {
            playersInGame[i].tableIndex = i;
            ServerSend.PlayerTablePosition(playersInGame[i].id, i);

        }

    }
    /// <summary>
    /// Sends cards and hand to player if they disconnect. Need to also implement sending their chip amount back to the player.
    /// </summary>
    public static void OnGameBeingPlayed()
    {
        if (!roundStarted) { return; }

        foreach(Card c in communityCards)
        {
            ServerSend.CardToCommunity(c);
        }
        //does not work, seems to be because on disconnect in server it sets player to null so it does keep track of the players in game
        foreach (Player p in playersInGame)
        {
            if (p != null)
            {
                ServerSend.SendCard(p.cards[0], p.id);
                ServerSend.SendCard(p.cards[1], p.id);
            }
        }

        
    }

    public static void WaitForPlayerAction()
    {

    }


    public static bool CheckPlayersAllBetEqual()
    {
        int prev = playersInGame[0].amountInPot;
        foreach (Player p in playersInGame)
        {
            if (p!= null && !p.completedActionThisTurn) { return false; }
            if (p == null || p.isFolding || !p.isPlayingHand)
            {
                // do nothing since player is out
            }
            else
            {
                if (p.amountInPot != prev)
                {
                    //checks if player is all in but with less cash in middle. will need to make a side pot.
                    if (p.chipTotal != 0) { return false; }
                }
            }
            
        }
        return true;
    }

    public static void OnPlayerAction(int id, int tableIndex)
    {
        //testing purposes
        //if (true)
        //{
        //    ServerSend.PlayerAction(playersInGame[tableIndex]);
        //    IncrementPlayerTurn();
        //    ServerSend.PokerState();
        //    return;
        //}
        if (currentTurnIndex == tableIndex)
        {
            ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients if its their turn.
            //ServerSend.PlayerAction(Server.clients[id].player); // this one works but gets player from clients list instead of in game
            IncrementPlayerTurn();


        }
        // checks if theres only one player left
        if (playersLeft <= 1)
        {
            CalculateWinner();
            ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients.
            ServerSend.PokerState();

            return;
        }
        if (CheckPlayersAllBetEqual())
        {
            if (dealCounter == 3)
            {
                CalculateWinner();
                ResetPlayerActions();
            }
            else if (dealCounter == 2)
            {
                CardDealer.River();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;


            }
            else if (dealCounter == 1)
            {
                CardDealer.Turn();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;


            }
            else if (dealCounter == 0)
            {
                CardDealer.Flop();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;
            }
        }

    }


    public static void ResetPlayerActions()
    {
        foreach (Client c in Server.clients.Values)
        {
            if (c.player != null)
            {
                c.player.completedActionThisTurn = false;
                
            }
        }
    }

    public static void IncrementPlayerTurn()
    {
        currentTurnIndex++;
        if (currentTurnIndex > playersInGame.Count - 1)
        {
            currentTurnIndex = firstTurnIndex;
        }
     
        ServerSend.PokerState();
    }


    public static void StartRound()
    {
        currentBet = 0;
        amountInPot = 0;
        //also need to add a check here for 2 or more players in game/ numplayersInGame. 
        if (roundStarted)
        {
            return;
        }
        roundStarted = true;
        ResetTable();
        int maxPlayers = playersInGame.Count;
        playersLeft = maxPlayers;
        if (maxPlayers > 2)
        {
            firstTurnIndex = dealerIndex + 2;
            currentTurnIndex = firstTurnIndex;// adds two so it skips the small and big blind for first round of betting. 
        }
        else
        {
            currentTurnIndex = 0;
        }

        //SubtractBigSmallBlinds();

        Deck.CreateDeck();
        Deck.Shuffle();

        CardDealer.DealOpeningCards();

        ServerSend.PokerState();


    }

    public static void GatherPlayers()
    {
        foreach (Client c in Server.clients.Values)
        {
            if (c.player != null && c.player.tableIndex > -1)
            {
                Debug.Log($"Table Index: {c.player.tableIndex}");
                playersInGame[c.player.tableIndex] = c.player;
                c.player.isPlayingHand = true;
            }
        }
    }

    public static void CalculateWinner()
    {
        //Do winner calculations for chips and adding chips. will have to make an add chips packet
        ServerSend.RoundReset();
        ResetTable();
        dealerIndex++;
        if (dealerIndex > playersInGame.Count)
        {
            dealerIndex = 0;
        }
    }

    public void AdvanceNextRound()
    {

        roundStarted = false;
    }

    public void ReceivePlayerAction()
    {

    }

    public void SendUpdatedGameState()
    {

    }
    /// <summary>
    /// Subtracts the big and small blinds from the appropriate indexes. 
    /// </summary>
    public static void SubtractBigSmallBlinds()
    {
        int smallIndex = dealerIndex + 1;
        int bigIndex = dealerIndex + 2;
        if (bigIndex > playersInGame.Count)
        {
            bigIndex = dealerIndex;
        }
        // need to add a check for single player
        ServerSend.SetChips(playersInGame[smallIndex].id, _subtractAmount: smallBlind);
        ServerSend.SetChips(playersInGame[bigIndex].id, _subtractAmount: bigBlind);
    }

    public static void RemovePlayer(Player p)
    {
        playersInGame.Remove(p);
        //playersInGame.RemoveAt(tableIndex);
    }

    //public static void Update()
    //{
    //    foreach (Client _client in Server.clients.Values)
    //    {
    //        if (_client.player != null)
    //        {
    //            // may need to change to adjust for not updating every player every tick time. Only on their turn.
    //            _client.player.Update();
    //        }
    //    }
    //    ThreadManager.UpdateMain();
    //}
}

