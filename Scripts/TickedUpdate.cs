using System.Collections;
using UnityEngine;

public class TickedUpdateBehavior : MonoBehaviour
{
    [SerializeField] protected float ticksPerSecond = 24f; // Ticks per second
    private float tickTimer = 0f;
    private double tick =0;

    private GameMaster gameMaster;
    private bool initalized = false;
    
    protected virtual void Start()
    {
        tickTimer = 0f;
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (gameMaster == null)
        {
            gameMaster = GameMaster.Instance ?? gameMaster;
            yield return null;
        }

        Debug.Log("TICKED UPDATE | Game Master Instance cached");
        initalized = true;
    }

    private void Update()
    {
        if (!initalized) {return;}
        
        tickTimer += Time.deltaTime;
        
        if (tickTimer >= 1f / ticksPerSecond && tick < 50f)
        {
            TickedUpdate();
            tickTimer = 0f;
            tick++;
        }
    }

    protected virtual void TickedUpdate()
    {
        gameMaster.globalTick.Invoke();
    }
}
