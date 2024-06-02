using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public GameObject Menu;

    public static int Players;
    public static int Rounds;

    public void Start()
    {
        Players = 2;
        Rounds = 1;
    }

    public void OnGameStart()
    {
        MyPlayerPrefs.SetPlayers(Players);
        MyPlayerPrefs.SetRounds(Rounds);
        SceneManager.LoadScene(1);
    }

    public void InitGame()
    {
        int playerId = Random.Range(0, Players) + 1;
        MyPlayerPrefs.SetPlayers(Players);
        MyPlayerPrefs.SetPlayerId(playerId);
        MyPlayerPrefs.SetRounds(Rounds);
    }
}
