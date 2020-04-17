using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



class GameLogic: MonoBehaviour
{


    public static int currentBet = 0;
    public static int amountInPot = 0;
    public static int highestPlayerAmountInPot = 0;

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
    public static bool isFirstTimePlaying = true;


    public static void InitializePlayers()
    {
        for (int i = 0; i < Server.clients.Count - 1; i++) playersInGame.Add(null);
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
        playersLeft = playersInGame.Count;
        

    }

    public static void IncrementBigSmallBlindIndex()
    {

    }
    /// <summary>
    /// Gathers player indexes
    /// </summary>
    public static void GatherPlayers()
    {
        foreach (Client c in Server.clients.Values)
        {
            if (c.player != null && c.player.tableIndex > -1)
            {
                Debug.Log($"Server data: {c.player.username} => Chip Total: {c.player.chipTotal} Table Index: {c.player.tableIndex}");
                playersInGame[c.player.tableIndex] = c.player;
                c.player.isPlayingHand = true;
            }
        }
    }

    public static void RemoveNullPlayers()
    {
        playersInGame.RemoveAll(item => item == null);
        Debug.Log($"Number of players in game: {playersInGame.Count}");
        //sets each players table index to their new index in the shrunken list of players after removing the null players.
        for (int i = 0; i < playersInGame.Count; i++)
        {

            Server.clients[playersInGame[i].id].player.tableIndex = i;
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
        //testing purposes for one player
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
            //ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients.
            ServerSend.PokerState();

            return;
        }
        if (CheckPlayersAllBetEqual())
        {
            if (dealCounter == 3)
            {
                CalculateWinner();
                dealCounter++;
                ResetPlayerActions();
            }
            else if (dealCounter == 2)
            {
                CardDealer.River();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;
                ServerSend.PokerState();



            }
            else if (dealCounter == 1)
            {
                CardDealer.Turn();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;
                ServerSend.PokerState();



            }
            else if (dealCounter == 0)
            {
                CardDealer.Flop();
                dealCounter++;
                ResetPlayerActions();
                currentBet = 0;
                ServerSend.PokerState();
            }
        }

        //incorrect fix
        //if (currentTurnIndex == tableIndex)
        //{
        //    ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients if its their turn.
        //    //ServerSend.PlayerAction(Server.clients[id].player); // this one works but gets player from clients list instead of in game
        //    IncrementPlayerTurn();


        //}
        //ServerSend.PokerState();


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
        if (isFirstTimePlaying)
        {
            NetworkManager.instance.SetPlayerChips(1000);
            isFirstTimePlaying = false;
        }
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

        SubtractBigSmallBlinds();
        highestPlayerAmountInPot = bigBlind;
        Deck.CreateDeck();
        Deck.Shuffle();

        CardDealer.DealOpeningCards();

        ServerSend.PokerState();


    }



    public static void NewRound()
    {
        ServerSend.RoundReset();
        ResetTable();
        dealerIndex++;
        IncrementDealer();
    }


    public static void CalculateWinner()
    {
        if (playersLeft <= 1)
        {
            foreach (Player p in playersInGame)
            {
                if (p.isPlayingHand)
                {
                    Server.clients[p.id].player.chipTotal += amountInPot;
                    ServerSend.SetChips(p.id, _addAmount: amountInPot);

                    NewRound();
                    return;

                }
            }
        }
        Debug.Log($"[COMMUNITY] Rank: {communityCards[0].rank.ToString()} , Suit: {communityCards[0].suit.ToString()}\n ");
        List<ApplicationUser> users = new List<ApplicationUser>();
        Room room = new Room
        {
            numUsers = playersLeft,
            RoomName = "Calculating Winner..",
            CardsOnTable = new List<string[]>
            {
                new string[]{communityCards[0].stringRank, communityCards[0].stringSuit },
                new string[]{communityCards[1].stringRank, communityCards[1].stringSuit },
                new string[]{communityCards[2].stringRank, communityCards[2].stringSuit },
                new string[]{communityCards[3].stringRank, communityCards[3].stringSuit },
                new string[]{communityCards[4].stringRank, communityCards[4].stringSuit }
            }
        };

        for (int i = 0; i < playersInGame.Count; i++)
        {
            if (playersInGame[i].isPlayingHand)
            {
                ApplicationUser user = new ApplicationUser
                {
                    player = playersInGame[i],
                    Name = playersInGame[i].username,
                    PlayerCards = new List<string[]>
                    {
                        new string[]{playersInGame[i].cards[0].stringRank,playersInGame[i].cards[0].stringSuit },
                        new string[]{playersInGame[i].cards[1].stringRank,playersInGame[i].cards[1].stringSuit },
                    },
                    Chips = 0
                };
                users.Add(user);
                
            }
        }

        room.Chair0 = users[0];
        room.Chair1 = users[1];
        //room.Chair2 = users[2];
        //room.Chair3 = users[3];
        //room.Chair4 = users[4];
        //room.Chair5 = users[3];
        //room.Chair6 = users[3];
        //room.Chair7 = users[7];
        //room.Chair8 = users[8];
        //room.Chair9 = users[9];

        room.PotOfChair0 = users[0].player.amountInPot;
        room.PotOfChair1 = users[1].player.amountInPot;
        //room.PotOfChair2 = users[2].player.amountInPot;
        //room.PotOfChair3 = users[3].player.amountInPot;
        //room.PotOfChair4 = users[4].player.amountInPot;
        //room.PotOfChair5 = users[5].player.amountInPot;
        //room.PotOfChair6 = users[6].player.amountInPot;
        //room.PotOfChair7 = users[7].player.amountInPot;
        //room.PotOfChair8 = users[8].player.amountInPot;
        //room.PotOfChair9 = users[9].player.amountInPot;
        var result = room.SpreadMoneyToWinners();
        Debug.Log($"WINNER is {result[0].Winners[0].Name} with {result[0].RankName} for ${result[0].PotAmount}");

        foreach (var user in users)
        {
           // user.player.chipTotal += user.Chips;
            Server.clients[user.player.id].player.chipTotal += user.Chips;
            ServerSend.SetChips(user.player.id, _addAmount: user.Chips);
        }




        ////Do winner calculations for chips and adding chips. SetChips packet made.
        //foreach (Player p in playersInGame)
        //{
        //    List<Card> allCards = new List<Card>();
        //    allCards.AddRange(p.cards);
        //    allCards.AddRange(communityCards);
        //    p.hand = new Hand(allCards);
        //    p.hand.CalculateRanks();
        
        //    p.hand.handInfo.PrintRanks();
        //    p.hand.PrintCards(allCards);
        //    allCards.Clear();
        //}
        NewRound();
    }

    public static void IncrementDealer()
    {
        dealerIndex++;
        if (dealerIndex > playersInGame.Count - 1)
        {
            dealerIndex = 0;
        }
    }


    /// <summary>
    /// Subtracts the big and small blinds from the appropriate indexes. Needs to be fixed.
    /// </summary>
    public static void SubtractBigSmallBlinds()
    {
        int smallIndex = dealerIndex;
        int bigIndex = dealerIndex + 1;
        if (bigIndex > playersInGame.Count - 1)
        {
            bigIndex = 0;
        }
        // need to add a check for single player
        if (playersInGame[smallIndex].chipTotal < bigBlind)
        {
            playersInGame.RemoveAt(smallIndex);
        }
        if (playersInGame[bigIndex].chipTotal < bigBlind)
        {
            playersInGame.RemoveAt(bigIndex);
            currentBet = 0;
        }
        else
        {
            ServerSend.SetChips(playersInGame[smallIndex].id, _subtractAmount: smallBlind, _isBlinds: true);
            ServerSend.SetChips(playersInGame[bigIndex].id, _subtractAmount: bigBlind, _isBlinds: true);

            playersInGame[smallIndex].amountInPot = smallBlind;
            playersInGame[smallIndex].chipTotal -= smallBlind;
            playersInGame[bigIndex].amountInPot = bigBlind;
            playersInGame[bigIndex].chipTotal -= bigBlind;

            currentBet = bigBlind;
            amountInPot += bigBlind + smallBlind;
        }

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

