using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public bool canDropBombs = true;
    public bool canMove = true;
    public PlayerUnit player;

    public GameObject bombPrefab;
    private Rigidbody rigidBody;
    private Transform myTransform;

    private DynamicJoystick joystick;
    private ActionJoystick action;

    private Animator animator;
    private List<Bomb> bombs;
    private network networkInstance;

    private Vector3 lastSentPosition;
    private float distanceThreshold = 0.3f; // 当移动超过0.5米时传送位置

    public void Start()
    {
        player = GetComponent<PlayerUnit>();
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;

        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();
        bombs = FindObjectOfType<LoadMap>().BombList;
        action = FindObjectOfType<ActionJoystick>();
        joystick = FindObjectOfType<DynamicJoystick>();
        networkInstance = network.instance;
        lastSentPosition = transform.position;
    }

    private bool GetKey(Joypad key)
    {
        if (joystick != null) return joystick.GetKey(key);
        else return false;
    }

    private bool GetActionKey()
    {
        if (action != null) return action.GetActionKey();
        else return false;
    }

    public void Update()
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        animator.SetBool("Walking", false);
        if (!canMove) return;
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow) || GetKey(Joypad.UpArrow))
        {
            movement.z = player.moveSpeed;
            myTransform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetKey(KeyCode.LeftArrow) || GetKey(Joypad.LeftArrow))
        {
            movement.x = -player.moveSpeed;
            myTransform.rotation = Quaternion.Euler(0, 270, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetKey(KeyCode.DownArrow) || GetKey(Joypad.DownArrow))
        {
            movement.z = -player.moveSpeed;
            myTransform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetBool("Walking", true);
        }

        if (Input.GetKey(KeyCode.RightArrow) || GetKey(Joypad.RightArrow))
        {
            movement.x = player.moveSpeed;
            myTransform.rotation = Quaternion.Euler(0, 90, 0);
            animator.SetBool("Walking", true);
        }

        if (movement != Vector3.zero)
        {
            rigidBody.velocity = new Vector3(movement.x, rigidBody.velocity.y, movement.z);
            if (Vector3.Distance(rigidBody.velocity, lastSentPosition) > distanceThreshold)
            {
                SendPosition();
                lastSentPosition = rigidBody.velocity;
            }
        }
        

        if (canDropBombs && (Input.GetKeyDown(KeyCode.Space) || GetActionKey()))
            DropBomb();
    }

    private void SendPosition()
    {
        string message = $"{network.playerId}/Position/{myTransform.position.x}/{myTransform.position.z}/{myTransform.rotation.eulerAngles.y}";
        networkInstance.SendData(message);
    }

    public void DropBomb()
    {
        if (player.bombs > 0 && bombPrefab)
        {
            player.bombs--;

            var obj = Instantiate(bombPrefab,
                new Vector3(Mathf.RoundToInt(myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
                bombPrefab.transform.rotation);

            obj.GetComponent<Bomb>().explode_size = player.explosion_power;
            obj.GetComponent<Bomb>().player = player;

            var bomb = obj.GetComponent<Bomb>();
            bomb.PlayerId = player.PlayerId;
            bombs.Add(bomb);

            if (player.canKick) obj.GetComponent<Rigidbody>().isKinematic = false;

            string message = $"{network.playerId}/Bomb/{Mathf.RoundToInt(myTransform.position.x)}/{Mathf.RoundToInt(myTransform.position.z)}/{player.explosion_power}";
            networkInstance.SendData(message);
        }
    }
}