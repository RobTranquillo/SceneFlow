using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using vrbits;

public class SceneLoadNode : CompositeNode
{
    public string title = "";
    public string description = "";
    public AssetReferenceScene scene;
    public bool debug = false;
    
    private bool loadingIsInProcess = false;
    private bool loadingIsFinish = false;

    #region Node Lifecycle
    protected override void OnStart()
    {
        loadingIsInProcess = true;
        loadingIsFinish = false;
        Debug.Log($"Loading scene: <color=lightblue>{title}</color>");

        //SceneLoader.Instance.allScenesUnloaded += LoadScene;
        SceneLoader.Instance.loadingFinished.AddListener(() => loadingIsInProcess = false);
        SceneLoader.Instance.loadingFinished.AddListener(() => loadingIsFinish = true);
        SceneLoader.Instance.LoadScene(scene);
    }

    protected override void OnStop()
    {
        SceneLoader.Instance.loadingFinished.RemoveListener(() => loadingIsInProcess = false);
        SceneLoader.Instance.loadingFinished.RemoveListener(() => loadingIsFinish = true);
        if (debug)
            Debug.Log("laden von scene ist beendet");
    }

    protected override State OnUpdate()
    {
        if (debug)
            Debug.Log("scene loading Update");
        if (loadingIsFinish)
        {
            State overAllState = State.Success;
            foreach (var child in children)
                if (child.Update() != State.Success)
                    overAllState = child.state;
            return overAllState;
        }

        if (loadingIsInProcess)
            return State.Running;
        
        return State.Running;
    }
    #endregion Node Lifecycle

    private void StartSceneLoading(Node node)
    {
        if (node is not SceneLoadNode)
            return;

        var sceneLoadNode = node as SceneLoadNode;
        sceneLoadNode.loadingIsFinish = false;
        SceneLoader.Instance.loadingFinished.AddListener(() => sceneLoadNode.loadingIsFinish = true);
        SceneLoader.Instance.LoadScene(sceneLoadNode.scene);
    }
}
