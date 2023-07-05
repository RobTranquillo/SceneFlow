using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DelayNode waits for a given duration before returning success.
/// All children are started at once after the delay.
/// </summary>
public class DelayNode : CompositeNode
{
    public float duration = 1f;
    public bool debug = false;
    
    float startTime;

    protected override void OnStart()
    {
        startTime = Time.time;
        if (debug)
            Debug.Log("DelayNode started");
    }

    protected override void OnStop()
    {
        if (debug)
            Debug.Log("DelayNode stopped");
    }

    protected override State OnUpdate()
    {
        if (Time.time - startTime < duration)
        {
            if (debug)
                Debug.Log("DelayNode running, wait for end of delay");
            return State.Running;
        }

        State overAllState = State.Success;
        foreach (var child in children)
        {
            State childState = child.Update();

            if (childState == State.Failure)
                return State.Failure;
            
            if (childState != State.Success)
                overAllState = State.Running;
        }

        return overAllState;
    }
}
