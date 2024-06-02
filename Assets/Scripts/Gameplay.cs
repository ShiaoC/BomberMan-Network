using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Gameplay : MonoBehaviour
{
    public Text round;
    public GameObject Dashboard;
    public GameObject[] enemy;
    public static Gameplay instance;

    public GameObject bombPrefab;


    void Awake()
    {
        instance = this;
        enemy = new GameObject[12];
    }

    public void GameStart()
    {
        int roundId = MyPlayerPrefs.GetLevel();
        int levels = MyPlayerPrefs.GetLevels();

        round.text = $"{roundId}/{levels}";
        MyPlayerPrefs.SetFollowers();
    }

    public void SetEnemy(GameObject go, int id)
    {
        enemy[id] = go;
        Debug.Log("setting enemy  " + id.ToString());
    }

    public GameObject GetEnemy(int id)
    {
        return enemy[id];
    }

    public void GameEnd()
    {
        int roundId = MyPlayerPrefs.GetLevel();

        int rounds = MyPlayerPrefs.GetLevels();
        var scene = SceneManager.GetActiveScene();
        int id = scene.buildIndex;

        network.instance.EndGame();
        if (roundId < rounds)
        {
            roundId++;
            MyPlayerPrefs.SetLevel(roundId);
            SceneManager.LoadScene(id);
        }
        else
            Dashboard.SetActive(true);
    }

    public void NewGame()
    {
        MyPlayerPrefs.SetLevel(1);
        var scene = SceneManager.GetActiveScene();

        int index = scene.buildIndex;
        SceneManager.LoadScene(index);
    }

    public void KillAgent(int PlayerId)
    {
        MyPlayerPrefs.KillAgent(GameEnd, PlayerId);
    }

    public void SpawnBomb(float x, float z, int playerId, int explodeSize)
    {
        Vector3 position = new Vector3(x, 0.22294f, z);
        GameObject bombInstance = Instantiate(bombPrefab, position, Quaternion.identity);
        Bomb bombScript = bombInstance.GetComponent<Bomb>();
        bombScript.PlayerId = playerId;
        bombScript.explode_size = explodeSize;
    }

}
