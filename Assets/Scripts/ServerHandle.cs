using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
        GameLogic.OnGameBeingPlayed();
        //GameLogic.playersInGame.Add(Server.clients[_clientIdCheck].player);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    //public static void PlayerShoot(int _fromClient, Packet _packet)
    //{
    //    Vector3 _shootDirection = _packet.ReadVector3();

    //    Server.clients[_fromClient].player.Shoot(_shootDirection);
    //}

    public static void StartRound(int _fromClient, Packet _packet) // update later for receiving player table seat indexes
    {
        int _clientIdCheck = _packet.ReadInt();
        bool _start = _packet.ReadBool();

        if (_start)
        {
            GameLogic.StartRound();
        }

    }

    public static void PlayerTablePosition(int _fromClient, Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _tableIndex = _packet.ReadInt();
        
        //If clients removes himself from the table then -1 is sent and client is removed from playersInGameList.
        //if (_tableIndex == -1)
        //{
        //    Debug.Log("Removing player?");
        //    Server.clients[_id].player.tableIndex = -1;
        //    GameLogic.playersInGame.Remove(Server.clients[_id].player);
        //    GameLogic.numPlayersAtTable -= 1;

        //    return;

        //}
        // Adds player into the playersInGameList and sets player to have the selected index.
        //if (Server.clients[_id].player == null) { return; }
            
        Server.clients[_id].player.tableIndex = _tableIndex;
        //GameLogic.playersInGame[_tableIndex] = Server.clients[_fromClient].player;
        //GameLogic.playersInGame[_tableIndex].tableIndex = _tableIndex;
        GameLogic.numPlayersAtTable += 1;
        ServerSend.PlayerTablePosition(_id, Server.clients[_id].player.tableIndex);
        
    }


    public static void PlayerAction(int _fromClient, Packet _packet)
    {
        int _id = _packet.ReadInt();
        bool isFold = _packet.ReadBool();
        bool isCheckCall = _packet.ReadBool();
        bool isRaise = _packet.ReadBool();
        int raiseAmount = _packet.ReadInt();

        Server.clients[_fromClient].player.completedActionThisTurn = true;
        Server.clients[_fromClient].player.isFolding = isFold;
        Server.clients[_fromClient].player.isCheckCalling = isCheckCall;
        Server.clients[_fromClient].player.isRaising = isRaise;
        Server.clients[_fromClient].player.raiseAmount = raiseAmount;
        if (isFold)
        {
            Server.clients[_fromClient].player.isPlayingHand = false;
            GameLogic.playersLeft -= 1;
            //GameLogic.playersInGame.RemoveAt(Server.clients[_fromClient].player.tableIndex);
        }
        else if (isCheckCall)
        {
            int amountToCall = 0;
            amountToCall = GameLogic.highestPlayerAmountInPot - Server.clients[_fromClient].player.amountInPot;

            //if (GameLogic.currentBet != 0)
            //{
            //    amountToCall = GameLogic.highestPlayerAmountInPot - Server.clients[_fromClient].player.amountInPot;
            //}
    
            Server.clients[_fromClient].player.SubtractChips(amountToCall);
            Server.clients[_fromClient].player.amountInPot += amountToCall;
            GameLogic.amountInPot += amountToCall;


        }
        else if (isRaise)
        {
            Debug.Log($"Raise Amount: {raiseAmount}");
            Server.clients[_fromClient].player.SubtractChips(raiseAmount);
            Server.clients[_fromClient].player.amountInPot += raiseAmount;
           
            GameLogic.amountInPot += raiseAmount;
            GameLogic.highestPlayerAmountInPot = Server.clients[_fromClient].player.amountInPot;
            GameLogic.currentBet = GameLogic.highestPlayerAmountInPot;
        }
      
        GameLogic.OnPlayerAction(_fromClient, Server.clients[_fromClient].player.tableIndex);
        Debug.Log($"Player {Server.clients[_fromClient].player.username} has: {Server.clients[_fromClient].player.chipTotal}");

        //To Do Increment Game Turn to next player. 
        //
    }
}
