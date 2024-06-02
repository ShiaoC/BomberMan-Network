using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    [Range(1, 9)]
    public int PlayerId;
    public float moveSpeed = 5f;
    public int bombs = 2;

    public bool canKick = false;
    public int explosion_power = 2;
    public Material[] Materials;

    public bool dead = false;
    public bool respawning = false;
    public Gameplay gameplay;

    public void Start()
    {
        gameplay = FindObjectOfType<Gameplay>();
        var mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        mesh.material = Materials[PlayerId - 1];
    }

    private int Manhattan(Vector3 left, Vector3 right)
    {
        int lx = Mathf.RoundToInt(left.x);
        int ly = Mathf.RoundToInt(left.z);

        int rx = Mathf.RoundToInt(right.x);
        int ry = Mathf.RoundToInt(right.z);
        return Math.Abs(lx - rx) + Math.Abs(ly - ry);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!dead && other.CompareTag("Explosion"))
        {
            dead = true;
            gameplay.KillAgent(PlayerId);
            MyPlayerPrefs.state -= 1;

            int playerId = MyPlayerPrefs.GetPlayerId();
            var self = other.gameObject.GetComponent<DestroySelf>();

            string message = $"{PlayerId}/Dead";
            network.instance.SendData(message);

            if (self.EnemyId != playerId && self.EnemyId > 0)
                MyPlayerPrefs.SetEnemyId(self.EnemyId);
            else
            {
                int enemyId = MyPlayerPrefs.GetEnemyId();
            }

            Destroy(gameObject);
        }
    }
}
