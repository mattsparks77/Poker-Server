﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }


    public static void RoundReset()
    {
        using (Packet _packet = new Packet((int)ServerPackets.roundReset))
        {
            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerAction(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerAction))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.isFolding);
            _packet.Write(_player.isCheckCalling);
            _packet.Write(_player.isRaising);

            _packet.Write(_player.raiseAmount);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SetChips(int _id, int _setAmount = 0, int _subtractAmount = 0, int _addAmount = 0, bool _isBlinds = false)
    {
        using (Packet _packet = new Packet((int)ServerPackets.setChips))
        {
            _packet.Write(_id);
            _packet.Write(_setAmount);
            _packet.Write(_subtractAmount);
            _packet.Write(_addAmount);
            _packet.Write(_isBlinds);


            SendTCPDataToAll(_packet);
        }
    }


    public static void SendCard(Card _card, int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.sendCard))
        {
           // Debug.Log($"Card: Suit: {_card.suit} Rank: {_card.rank}");
            _packet.Write(_playerId);


            _packet.Write((int)_card.suit);
            _packet.Write((int)_card.rank);

            SendTCPDataToAll(_packet); // may want to change this to only send it to one client
                                       //SendTCPData(_player.id, _packet); // may want to change this to only send it to one client
        }
    }

    public static void CardToCommunity(Card _card)
    {
        using (Packet _packet = new Packet((int)ServerPackets.cardToCommunity))
        {
            _packet.Write((int)_card.suit);
            _packet.Write((int)_card.rank);

            SendTCPDataToAll(_packet); // may want to change this to only send it to one client
                                       //SendTCPData(_player.id, _packet); // may want to change this to only send it to one client
        }
    }

    public static void PlayerTablePosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerTablePosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.tableIndex);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerTablePosition(int _id, int _tableIndex)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerTablePosition))
        {
            _packet.Write(_id);
            _packet.Write(_tableIndex);
            
            SendTCPDataToAll(_packet);
        }
    }

    public static void PokerState()
    {
        using (Packet _packet = new Packet((int)ServerPackets.pokerState))
        {
            _packet.Write(GameLogic.currentBet);
            _packet.Write(GameLogic.amountInPot);
            _packet.Write(GameLogic.currentTurnIndex);
            _packet.Write(GameLogic.playersInGame[GameLogic.currentTurnIndex].id);

            _packet.Write(GameLogic.highestPlayerAmountInPot);



            //_packet.Write(_player.isFolding);
            //_packet.Write(_player.isCheckCalling);
            //_packet.Write(_player.isRaising);

            //_packet.Write(_player.raiseAmount);

            SendTCPDataToAll(_packet);
        }
    }


    //public static void PlayerHealth(Player _player)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
    //    {
    //        _packet.Write(_player.id);
    //        _packet.Write(_player.health);

    //        SendTCPDataToAll(_packet);
    //    }
    //}

    //public static void PlayerRespawned(Player _player)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
    //    {
    //        _packet.Write(_player.id);

    //        SendTCPDataToAll(_packet);
    //    }
    //}

    //public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _spawnerPosition, bool _hasItem)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
    //    {
    //        _packet.Write(_spawnerId);
    //        _packet.Write(_spawnerPosition);
    //        _packet.Write(_hasItem);

    //        SendTCPData(_toClient, _packet);
    //    }
    //}

    //public static void ItemSpawned(int _spawnerId)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
    //    {
    //        _packet.Write(_spawnerId);

    //        SendTCPDataToAll(_packet);
    //    }
    //}

    //public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    //{
    //    using (Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
    //    {
    //        _packet.Write(_spawnerId);
    //        _packet.Write(_byPlayer);

    //        SendTCPDataToAll(_packet);
    //    }
    //}
    #endregion
}
