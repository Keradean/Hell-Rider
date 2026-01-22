using UnityEngine;
using FishNet.Object;

public class BossOne : NetworkBehaviour
{
    private float speedX;
    private float speedY;
    private bool charging;

    private float switchInterval;
    private float switchTimer;

    void Start()
    {
        EnterPatrolState();
    }

    void Update()
    {
        if (!IsServerInitialized) return;

        if(switchTimer < 0)   
        {
            switchTimer -= Time.deltaTime;
        }
        else
        {
            if (charging)
            {
                EnterPatrolState();
            }
            else
            {
                EnterChargeState();
            }
        }

        if (transform.position.y > 3 || transform.position.y < -3)
        {
            speedY *= -1;
        }

        float moveX = speedX * Time.deltaTime;
        float moveY = speedY * Time.deltaTime;

        transform.position += new Vector3(moveX, moveY, 0);

        if (transform.position.x <= -11 && IsSpawned)
        {
            ServerManager.Despawn(gameObject);
        }
    }

    void EnterPatrolState()
    {
        speedX = 0;
        speedY = Random.Range(-2f, 2f);
        switchInterval = 1f; 
        switchTimer = switchInterval;
        charging = false;
    }

    void EnterChargeState()
    {
        speedX = -5f;
        speedY = 0;
        switchInterval = 1f;
        switchTimer = switchInterval;
        charging = true;
    }
}