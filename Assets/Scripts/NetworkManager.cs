using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public Text _setText;
    public int _setAmount { get { return int.Parse(_setText.text); } set { _setAmount = int.Parse(_setText.text); } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    /// <summary>
    /// Sets player chips using the text input field
    /// </summary>
    public void SetPlayerChips()
    {
        foreach (Client c in Server.clients.Values)
        {
            Debug.Log($"SET AMOUNT TEXT ${_setAmount}");
            if (c != null && c.player != null)
            {
                ServerSend.SetChips(c.player.id, _setAmount: _setAmount);

            }

        }
 
    }
    /// <summary>
    /// Sets player chips using parameter
    /// </summary>
    public void SetPlayerChips(int _setAmount)
    {
        foreach (Client c in Server.clients.Values)
        {
            Debug.Log($"SET AMOUNT TEXT ${_setAmount}");
            if (c != null && c.player != null)
            {
                ServerSend.SetChips(c.player.id, _setAmount: _setAmount);

            }

        }

    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        Server.Start(9, 26950);
    }

    public void StartNewRoundWithDelay(float delay)
    {
        StartCoroutine(NewRound(delay));
    }

    public IEnumerator NewRound(float delay)
    {
        ServerSend.RoundReset();
        GameLogic.ResetTable();
        GameLogic.dealerIndex++;
        if (GameLogic.dealerIndex > GameLogic.playersInGame.Count)
        {
            GameLogic.dealerIndex = 0;
        }
        yield return new WaitForSeconds(delay);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }
}
