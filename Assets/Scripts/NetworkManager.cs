using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject[] playerPrefabs = new GameObject[4];
    public Text _setText;
    public Text _setBigBlind;
    public Text _setSmallBlind;
    public int _setAmount { get { return int.Parse(_setText.text); } set { _setAmount = int.Parse(_setText.text); } }
    public Text _setId;

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
    public void SetBigSmallBlinds()
    {
        GameLogic.smallBlind = int.Parse(_setSmallBlind.text);
        GameLogic.bigBlind = int.Parse(_setBigBlind.text);
       
    }

    public void StartFirstGame()
    {
        GameLogic.StartFirstGamePlayed();
    }


    /// <summary>
    /// Sets player chips using the text input field
    /// </summary>
    public void SetSpecificPlayerChips()
    {
        if (_setId.text == null) { return; }
        int id = int.Parse(_setId.text);
        if (GameLogic.totalPlayers[id] != null)
        {
            ServerSend.SetChips(id, _setAmount: _setAmount);

        }
      
    }

    public void SetPlayerChips()
    {
        foreach (Player p in GameLogic.totalPlayers)
        {
            if (p != null)
            {
                ServerSend.SetChips(p.id, _setAmount: _setAmount);
                p.chipTotal = _setAmount;
            }
        }

    }
    /// <summary>
    /// Sets player chips using parameter
    /// </summary>
    public void SetPlayerChips(int _setAmount)
    {
        foreach (Player p in GameLogic.totalPlayers)
        {
            if (p != null)
            {
                ServerSend.SetChips(p.id, _setAmount: _setAmount);
                p.chipTotal = _setAmount;
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
        yield return new WaitForSeconds(delay);
        GameLogic.NewRound();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer(int _prefabId)
    {

        return Instantiate(playerPrefabs[_prefabId], new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Player>();
    }
}
