using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

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
