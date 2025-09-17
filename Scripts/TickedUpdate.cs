using UnityEngine;
using GlobalEvents;

public class TickedUpdateBehavior : MonoBehaviour
{
    [SerializeField] protected float ticksPerSecond = 24f; // Ticks per second
    private float tickTimer = 0f;
    private double tick =0;

    [SerializeField] private GameMaster gameMaster;
    
    protected virtual void Start()
    {
        tickTimer = 0f;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        
        if (tickTimer >= 1f / ticksPerSecond)
        {
            TickedUpdate();
            tickTimer = 0f;
            tick++;
        }
    }

    protected virtual void TickedUpdate()
    {
        // gameMaster.globalTick.Invoke();
        Events.RaiseTick();
    }
}
