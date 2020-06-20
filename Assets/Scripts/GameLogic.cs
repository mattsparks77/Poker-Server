using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



class GameLogic: MonoBehaviour
{

    private static float roundDelay = 5f;
    public static int countdownTimer = 5;

    public static int startingChips = 2000;

    public static int currentBet = 0;
    public static int totalInPot = 0;
    public static int highestPlayerAmountInPot = 0;

    public static int currentTurnIndex = 0;

    public static int bigBlind = 50;
    public static int smallBlind = 25;
    public static int smallBlindIndex = -1;
    public static int bigBlindIndex = -1;
    public static int dealerIndex = 0;
    public static int firstTurnIndex = 0;

    public static int numPlayersAtTable = 0;
    public static int playersLeft = 0;

    public static List<Player> totalPlayers = new List<Player>();
    public static List<Player> playersInGame = new List<Player>();
    public static List<Card> communityCards = new List<Card>();

    public static int dealCounter = 0;
    public static int deckIndex = 0;


    //change to using an enum for different gamestates.
    public static bool roundStarted = false;
    public static bool isFirstTimePlaying = true;
    public static bool roundOver = false;
    public static bool isPaused = false;
    public static bool sendingBlindIndexes = false;

    public static int numTimesPaused = 0;


    //public static void InitializePlayers()
    //{
    //    for (int i = 0; i < Server.clients.Count - 1; i++) playersInGame.Add(null);
    //}

    /// <summary>
    /// Clears playersInGame list, re-adds in null values for table seats. Gathers players who have a seat index > -1. Then removes the extra null spaces in the playersInGame.
    /// </summary>
    public static void ResetTable()
    {
        roundOver = false;
        isPaused = false;
        numTimesPaused = 0;
        countdownTimer = 5;
        communityCards.Clear();
        RemovePlayersWithoutChips();

        ResetPlayerAmountsInPot();

        ResetPlayerActions();
        dealCounter = 0;
        IncrementBigSmallBlindIndex();
        currentTurnIndex = Increment(bigBlindIndex);
        sendingBlindIndexes = true;

        currentBet = bigBlind;
        totalInPot = bigBlind + smallBlind;
        highestPlayerAmountInPot = bigBlind;
        playersLeft = playersInGame.Count;

    }

    public static void AssignPlayerIndex()
    {
        
    }

    public static void ResetPlayerAmountsInPot()
    {
        foreach (Player p in playersInGame)
        {
            Debug.Log($"[ResetPlayerAmounts] {p.username} now has 0 amount in pot.");
            p.amountInPot = 0;
        }
    }
    /// <summary>
    /// Removes players if they dont have enough for the small blind or if they choose not to play this hand.
    /// </summary>
    public static void RemovePlayersWithoutChips()
    {
        List<Player> toRemove = new List<Player>();
        foreach (Player p in playersInGame)
        {
            if (!p.isPlayingHand && p.chipTotal > smallBlind) // need to add a check if a player has left the table possibly otherwise this just forces everyone who chips to play every hand
            {
                p.isPlayingHand = true;
                continue;
            }
            if (p.chipTotal < smallBlind || !p.isPlayingHand)
            {
                toRemove.Add(p);

            }
        }
        foreach (Player p in toRemove)
        {
            playersInGame.Remove(p);
        }

    }

    public static void IncrementBigSmallBlindIndex()
    {
        dealerIndex = Increment(dealerIndex);
        smallBlindIndex = Increment(smallBlindIndex);
        bigBlindIndex = Increment(bigBlindIndex);
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


    public static bool CheckPlayersAllBetEqual()
    {
        Debug.Log($"Outside Loop [Highest Player Amount in Pot] {highestPlayerAmountInPot}");

        foreach (Player p in playersInGame)
        {
        
            //if player is all in (p.amountInPot > 0 && p.chipTotal <= 0) ||
   
            if (p == null || p.isFolding || !p.isPlayingHand)
            {
                continue;
                // do nothing since player is out
            }
            if (p != null && !p.completedActionThisTurn) { return false; }
            else
            {
                if (p.amountInPot != highestPlayerAmountInPot)
                {
                    if (highestPlayerAmountInPot == 0)
                    {
                        continue;
                    }
                    Debug.Log($"Inside Loop [Highest Player Amount in Pot] {highestPlayerAmountInPot}");
                    //checks if player is all in but with less cash in middle. will need to make a side pot.
                    if (p.chipTotal != 0) { return false; }
                }
            }
            
        }
        return true;
    }

    public static void OnPlayerAction(int id, int tableIndex)
    {
        // if its not players turn dont accept input or if they have already completed an action. Action gets set to false for all other players if someone raises.
        if (currentTurnIndex != tableIndex)//|| playersInGame[currentTurnIndex].completedActionThisTurn)
        {
            Debug.Log($"Not player's turn or move has already been registered.");
            return;
        }       
        
        ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients if its their turn.

        IncrementPlayerTurn();
        //currentTurnIndex = Increment(currentTurnIndex);


        //ServerSend.PlayerAction(Server.clients[id].player); // this one works but gets player from clients list instead of in game
        ServerSend.PokerState();
        // checks if theres only one player left
        if (playersLeft <= 1)
        {
            CalculateWinner();
            //ServerSend.PlayerAction(playersInGame[tableIndex]); // sends player action to all other clients.
            return;
        }
        if (CheckPlayersAllBetEqual())
        {
            if (dealCounter == 3)
            {
                CalculateWinner();
                dealCounter++;
                highestPlayerAmountInPot = 0;
                currentBet = 0;
                ResetRoundPlayerActions();


            }
            else if (dealCounter == 2)
            {
                CardDealer.River();
                dealCounter++;
                ResetRoundPlayerActions();
                currentBet = 0;
                highestPlayerAmountInPot = 0;
                currentTurnIndex = smallBlindIndex;
                ServerSend.RoundOver();
            }
            else if (dealCounter == 1)
            {
                CardDealer.Turn();
                dealCounter++;
                ResetRoundPlayerActions();
                currentBet = 0;
                highestPlayerAmountInPot = 0;
                currentTurnIndex = smallBlindIndex;
                ServerSend.RoundOver();

            }
            else if (dealCounter == 0)
            {
                CardDealer.Flop();
                dealCounter++;
                ResetRoundPlayerActions();
                currentBet = 0;
                highestPlayerAmountInPot = 0;
                currentTurnIndex = smallBlindIndex;
                ServerSend.RoundOver();
            }
        }

        ServerSend.PokerState();


    }

    public static void ResetRoundPlayerActions()
    {
        foreach (Player p in totalPlayers)
        {
         
            if (p != null)
            {
                p.completedActionThisTurn = false;
                p.isCheckCalling = false;
                p.isRaising = false;

            }
        }
    }

    public static void ResetPlayerActions()
    {
        foreach (Player p in totalPlayers)
        {

            if (p != null)
            {
                p.completedActionThisTurn = false;
                p.isFolding = false;
                p.isCheckCalling = false;
                p.isRaising = false;

            }
        }
    }

    public static void IncrementPlayerTurn()
    {
        currentTurnIndex++;
        if (currentTurnIndex > playersInGame.Count - 1)
        {
            currentTurnIndex = 0;
        }
        while (!playersInGame[currentTurnIndex].isPlayingHand)
        {
            currentTurnIndex++;
            if (currentTurnIndex > playersInGame.Count - 1)
            {
                currentTurnIndex = 0;
            }
        }
  
        // Auto skipping player if theyre all in If player is all in then skip that players turn and go to the next.
        // Bug is when everyone is all in it repeatedly calls incrementplayerturn
        //if (playersInGame[currentTurnIndex].chipTotal == 0 && playersInGame[currentTurnIndex].amountInPot > 0)
        //{
        //    IncrementPlayerTurn();
        //}
    }

    public static int Increment(int i)
    {
        int j = i + 1;
        if ( j > playersInGame.Count-1 )
        {
            j = 0;
        }
        return j;
    }


    public static void StartFirstGamePlayed()
    {
        InitializeGame();
        StartRound();
    }

    /// <summary>
    /// Run this for the very first game played afterwards no longer needs to be ran.
    /// </summary>
    public static void InitializeGame()
    {
        InitializePlayersInGame();
        NetworkManager.instance.SetPlayerChips(startingChips);
        dealerIndex = 0;
        smallBlindIndex = dealerIndex;
        bigBlindIndex = 1;

        currentTurnIndex = Increment(bigBlindIndex);
        sendingBlindIndexes = true;
 

        currentBet = bigBlind;
        totalInPot += bigBlind + smallBlind;
        highestPlayerAmountInPot = bigBlind;


    }

    public static void InitializePlayersInGame()
    {
        playersInGame.Clear();
        foreach (Player p in totalPlayers)
        {
            if (p != null)
            {
                Debug.Log($"Adding {p.username} into the game!");
                playersInGame.Add(p);
                p.tableIndex = playersInGame.IndexOf(p);
                p.isPlayingHand = true;
                ServerSend.PlayerTablePosition(p.id, p.tableIndex);
            }
        }
        numPlayersAtTable = playersInGame.Count;
        playersLeft = playersInGame.Count;
    }

    public static void AddPlayerToGame(int id)
    {
        if (!playersInGame.Contains(Server.clients[id].player))
        {
            playersInGame.Add(Server.clients[id].player);
            Server.clients[id].player.tableIndex = playersInGame.IndexOf(Server.clients[id].player);
            ServerSend.PlayerTablePosition(id, Server.clients[id].player.tableIndex);

        }
    }

    public static void StartRound()
    {
        if (roundStarted)
        {
            return;
        }
        roundStarted = true;
        if (playersInGame.Count > 1)
        {
            SubtractBigSmallBlinds();

        }
        highestPlayerAmountInPot = bigBlind;

        Deck.CreateDeck();
        Deck.Shuffle();
        CardDealer.DealOpeningCards();
        ServerSend.PokerState();
        sendingBlindIndexes = false;

    }

    
    public static void NewRound()
    {

        ServerSend.RoundReset();
        ResetTable();
        SubtractBigSmallBlinds();

        Deck.CreateDeck();
        Deck.Shuffle();
        CardDealer.DealOpeningCards();
        ServerSend.PokerState();
        sendingBlindIndexes = false;
    }


    public static void CalculateWinner()
    {
        roundOver = true;
        ServerSend.PokerState();
        if (playersLeft <= 1)
        {
            foreach (Player p in playersInGame)
            {
                if (p.isPlayingHand)
                {
                    Server.clients[p.id].player.chipTotal += totalInPot;
                    ServerSend.SetChips(p.id, _addAmount: totalInPot, _isWinner: true);
                    ServerSend.RoundOver(isGameOver: true);
                    roundOver = true;
                    NetworkManager.instance.StartNewRoundWithDelay();
                    return;

                }
            }
        }
        Debug.Log($"[COMMUNITY]1 Rank: {communityCards[0].rank.ToString()} , Suit: {communityCards[0].suit.ToString()}\n ");
        Debug.Log($"[COMMUNITY]2 Rank: {communityCards[1].rank.ToString()} , Suit: {communityCards[1].suit.ToString()}\n ");
        Debug.Log($"[COMMUNITY]3 Rank: {communityCards[2].rank.ToString()} , Suit: {communityCards[2].suit.ToString()}\n ");
        Debug.Log($"[COMMUNITY]4 Rank: {communityCards[3].rank.ToString()} , Suit: {communityCards[3].suit.ToString()}\n ");
        Debug.Log($"[COMMUNITY]5 Rank: {communityCards[4].rank.ToString()} , Suit: {communityCards[4].suit.ToString()}\n ");
        List<ApplicationUser> users = new List<ApplicationUser>() { null, null, null, null, null, null, null, null, null, null};
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
        string ts = $"Card 1: {communityCards[0].stringRank}, {communityCards[0].stringSuit}\nCard 2: {communityCards[1].stringRank}, {communityCards[1].stringSuit}\n"+ 
            $"Card 3: { communityCards[2].stringRank}, { communityCards[2].stringSuit}\nCard 4: {communityCards[3].stringRank}, {communityCards[3].stringSuit}\nCard 5: {communityCards[4].stringRank}, {communityCards[4].stringSuit}\n";
        Debug.Log(ts);
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
                Debug.Log($"[Calculate Winner] Index of {playersInGame[i].username} is {i} at table position {playersInGame[i].tableIndex}");
                users.Insert(i, user);
                
            }
        }
        users.RemoveAll(item => item == null);
        room.Chair0 = users[0];
        room.PotOfChair0 = users[0].player.amountInPot;

        room.Chair1 = users[1];
        room.PotOfChair1 = users[1].player.amountInPot;

        if (playersLeft >= 3)
        {
            room.Chair2 = users[2];
            room.PotOfChair2 = users[2].player.amountInPot;
        }
        else if (playersLeft >= 4)
        {
            room.Chair3 = users[3];
            room.PotOfChair3 = users[3].player.amountInPot;
        }
        else if (playersLeft >= 5)
        {
            room.Chair4 = users[4];
            room.PotOfChair4 = users[4].player.amountInPot;
        }
        else if (playersLeft >= 6)
        {
            room.Chair5 = users[5];
            room.PotOfChair5 = users[5].player.amountInPot;
        }
        else if (playersLeft >= 7)
        {
            room.Chair6 = users[6];
            room.PotOfChair6 = users[6].player.amountInPot;
        }
        else if (playersLeft >= 8)
        {
            room.Chair7 = users[7];
            room.PotOfChair7 = users[7].player.amountInPot;
        }
        else if (playersLeft >= 9)
        {
            room.Chair8 = users[8];
            room.PotOfChair8 = users[8].player.amountInPot;
        }

        else if (playersLeft >= 10)
        {
            room.Chair9 = users[9];
            room.PotOfChair9 = users[9].player.amountInPot;
        }



        //room.Chair3 = users[3];
        //room.Chair4 = users[4];
        //room.Chair5 = users[3];
        //room.Chair6 = users[3];
        //room.Chair7 = users[7];
        //room.Chair8 = users[8];
        //room.Chair9 = users[9];



        //room.PotOfChair3 = users[3].player.amountInPot;
        //room.PotOfChair4 = users[4].player.amountInPot;
        //room.PotOfChair5 = users[5].player.amountInPot;
        //room.PotOfChair6 = users[6].player.amountInPot;
        //room.PotOfChair7 = users[7].player.amountInPot;
        //room.PotOfChair8 = users[8].player.amountInPot;
        //room.PotOfChair9 = users[9].player.amountInPot;


        var result = room.SpreadMoneyToWinners();
        // send this info to client! Need to change result[0] to check for other winners and then send to clients so they know who else won.
        //if (result)
        for (int i = 0; i < result.Count; i++)
        {
            string dataString = "";
            string cards = "";
            foreach (string[] card in result[i].WinningCards)
            {
                foreach (string s in card)
                {
                    cards += s;
                }
  
                //Debug.Log($"Result[{i}] {dataString} \nRank Name {result[i].RankName}\nWinning Cards: {card[0].ToString()}");

            }
            Debug.Log($"Result[{i}] {dataString} \nRank Name {result[i].RankName}\nWinning Cards: {cards}");

            // Debug.Log($"Result[{i}] {dataString} \nRank Name {result[i].RankName}\nWinning Cards: {result[i].WinningCards[0].ToString()}");

        }

        //Debug.Log($"WINNER is {result[0].Winners[0].Name} with {result[0].RankName} for ${result[0].WinningCards[0][1]}");

        foreach (var user in users)
        {
            // user.player.chipTotal += user.Chips;
            Debug.Log($"{user.Name} is adding ${user.Chips} to their pot.");
            if (user.Chips <= 0)
            {
                continue;
            }
            Server.clients[user.player.id].player.chipTotal += user.Chips;
       
            ServerSend.SetChips(user.player.id, _addAmount: user.Chips, _isWinner: true, winningCards: result[0].RankName);
            if (result.Count >= 2)
            {
                Debug.Log($"Result[1]. RankName = {result[1].RankName}");
            }
            if (result.Count >= 3)
            {
                Debug.Log($"Result[2]. RankName = {result[2].RankName}");
            }

        }
        roundOver = true;
        ServerSend.RoundOver(isGameOver: true);
        
        NetworkManager.instance.StartNewRoundWithDelay();

    }


    /// <summary>
    /// Subtracts the big and small blinds from the appropriate indexes. Needs to be fixed.
    /// </summary>
    public static void SubtractBigSmallBlinds()
    {
        Debug.Log($"Smallblind {smallBlindIndex} \n BigBlind {bigBlindIndex}\n DealerIndex {dealerIndex}\n CurrentTurnIndex: {currentTurnIndex}");
        ServerSend.SetChips(playersInGame[smallBlindIndex].id, _subtractAmount: smallBlind, _isBlinds: true);
        ServerSend.SetChips(playersInGame[bigBlindIndex].id, _subtractAmount: bigBlind, _isBlinds: true);

        playersInGame[smallBlindIndex].amountInPot = smallBlind;
        playersInGame[smallBlindIndex].chipTotal -= smallBlind;
        playersInGame[bigBlindIndex].amountInPot = bigBlind;
        playersInGame[bigBlindIndex].chipTotal -= bigBlind;


    }

    public static void RemovePlayer(Player p)
    {
        playersInGame.Remove(p);
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

