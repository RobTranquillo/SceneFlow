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
    
    private bool loadingIsInProcess = false;
    private bool loadingIsFinish = false;

    #region Node Lifecycle
    protected override void OnStart()
    {
        loadingIsInProcess = true;
        loadingIsFinish = false;
        Debug.Log("beginne laden von scene: " + title);

        //SceneLoader.Instance.allScenesUnloaded += LoadScene;
        SceneLoader.Instance.loadingFinished.AddListener(() => loadingIsInProcess = false);
        SceneLoader.Instance.loadingFinished.AddListener(() => loadingIsFinish = true);
        SceneLoader.Instance.LoadScene(scene);
    }

    protected override void OnStop()
    {
        SceneLoader.Instance.loadingFinished.RemoveListener(() => loadingIsInProcess = false);
        SceneLoader.Instance.loadingFinished.RemoveListener(() => loadingIsFinish = true);
        Debug.Log("laden von scene ist beendet");

        children[0].Update();
    }

    protected override State OnUpdate()
    {
        if (loadingIsFinish)
            return State.Success;

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
