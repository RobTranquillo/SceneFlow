using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayNode : ActionNode
{
    public float duration = 1f;
    float startTime;

    protected override void OnStart()
    {
        startTime = Time.time;
    }

    protected override void OnStop()
    {
        child.Update();
         
        Stat der ActionNode die keine Kinder besitzt, muss eine neue NNode geschreiben werden,
         die genau ein Kind haben kann.
    }

    protected override State OnUpdate()
    {
        if (Time.time - startTime > duration)
            return State.Success;
        return State.Running;
    }
}
