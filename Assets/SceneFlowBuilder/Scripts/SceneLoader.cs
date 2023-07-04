using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace vrbits
{
    /// <summary>
    /// Configuration for linking events with scenes to be loaded.
    /// Loading and unloading of scenes will also be handled.
    /// Is the scene allready open in the editor instantiating is canceled after download.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        public UnityEvent loadingStarted, loadingFinished;
        public UnityEvent<string, float> loadingUpdated;
        const bool SceneActivateOnLoad = false;

        //Highlanders are scenes of which only one can be active at a time.
        //Every other Highlander is unloaded beforehand.
        private SceneInstance highlanderScene;
        public List<SceneInstance> loadedScenes;

        [Tooltip("For unloading purposes, the MainMap scene addressable is needed")]
        public AssetReference mapScene;

        private AssetReference _mapSceneRuntimeReference = null;
        internal LoadSceneMode loadSceneMode = LoadSceneMode.Additive;

        public override void Awake()
        {
            base.Awake();
            loadedScenes = new List<SceneInstance>();
        }

        public void LoadScene(string sceneName)
        {
            throw new NotImplementedException();
        }

        public void LoadScene(Scene scene)
        {
            throw new NotImplementedException();
        }

        public void LoadScene(AssetReference reference)
        {
            Debug.Log($"Start loading of Addressable: {reference}");
            StartCoroutine(LoadSceneAsync(reference));
            loadingStarted?.Invoke();
        }


        /// <summary>
        /// Loading a AssetReference to a scene as Highlander.
        /// Takes care of finish unloading the other scene before loading the new one.
        /// </summary>
        /// <param name="reference"></param>
        public void LoadSceneAsHighlander(AssetReference reference)
        {
            StartCoroutine(LoadSceneAsync(reference, true));
        }


        private IEnumerator LoadSceneAsync(AssetReference sceneReference, bool highlander = false)
        {
            //Check if Scene is already Loaded
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                //Check if Addressable Scene is already loaded
                if (loadedScenes[i].Scene.name == sceneReference.RuntimeKey.ToString())
                {
                    if (loadedScenes[i].Scene.isLoaded)
                    {
                        Debug.LogError("Scene " + sceneReference.SubObjectName + " is already loaded and activated");
                        yield break;
                    }

                    Debug.Log($"Scene {sceneReference.RuntimeKey} is already loaded");
                    StartCoroutine(ActivateSceneAsync(loadedScenes[i], highlander, sceneReference));
                    if (loadingUpdated != null)
                        loadingUpdated.Invoke(sceneReference.SubObjectName + " Activating", 100);
                    yield break;
                }
            }

            AsyncOperationHandle handle;
            //Init Addressable System
            handle = Addressables.InitializeAsync();

            while (!handle.IsDone)
            {
                if (loadingUpdated != null)
                    loadingUpdated.Invoke("Activating System", handle.PercentComplete * 100);
                yield return null;
            }

            while (!sceneReference.IsDone)
            {
                handle = Addressables.DownloadDependenciesAsync(sceneReference);
                while (!handle.IsDone)
                {
                    Debug.Log("Addressable Download " + handle.PercentComplete);
                    if (loadingUpdated != null)
                        loadingUpdated.Invoke(sceneReference.SubObjectName + " downloading", handle.PercentComplete);
                    yield return null;
                }
            }

            handle = Addressables.LoadResourceLocationsAsync(sceneReference);

            while (!handle.IsDone)
            {
                var downloadStatus = handle.GetDownloadStatus();
                if (loadingUpdated != null)
                    loadingUpdated.Invoke(sceneReference.SubObjectName + " Loading Ressources", downloadStatus.Percent);
                yield return null;
            }

            if (!sceneReference.OperationHandle.IsValid())
            {
                handle = sceneReference.LoadSceneAsync(loadSceneMode, SceneActivateOnLoad);
                while (!handle.IsDone)
                {
                    var downloadStatus = handle.GetDownloadStatus();
                    if (loadingUpdated != null)
                        loadingUpdated.Invoke(sceneReference.SubObjectName + " loading", downloadStatus.Percent);
                    yield return null;
                }

                StartCoroutine(ActivateSceneAsync((SceneInstance)handle.Result, highlander, sceneReference));
            }
        }

        private IEnumerator ActivateSceneAsync(SceneInstance loadedSceneHandle, bool highlander, AssetReference sceneReference)
        {
            if (loadedSceneHandle.Scene.isLoaded)
            {
                Debug.Log("Scene " + loadedSceneHandle.Scene.name + " is already loaded");
                yield break;
            }


            var async = loadedSceneHandle.ActivateAsync();
            while (!async.isDone)
            {
                if (loadingUpdated != null)
                    loadingUpdated.Invoke(sceneReference.SubObjectName + " activating", async.progress);
                yield return null;
            }

            if (loadingUpdated != null)
                loadingUpdated.Invoke(sceneReference.SubObjectName + " unloading", async.progress);

            if (highlanderScene.Scene.isLoaded)
            {
                //Remove Other Highlander
                var asyncHandle = Addressables.UnloadSceneAsync(highlanderScene);
                while (!asyncHandle.IsDone)
                {
                    yield return null;
                    if (loadingUpdated != null && asyncHandle.IsValid())
                        loadingUpdated.Invoke(sceneReference.SubObjectName + " unloading", asyncHandle.PercentComplete);
                }
            }

            if (highlander)
                highlanderScene = loadedSceneHandle;
            else
                loadedScenes.Add(loadedSceneHandle);
            StoreMapSceneReference(sceneReference);
            if (loadingFinished != null)
                loadingFinished.Invoke();
        }

        private void StoreMapSceneReference(AssetReference sceneReference)
        {
            if (mapScene == null)
                return;
            if (sceneReference.RuntimeKey.ToString() == mapScene.RuntimeKey.ToString())
                _mapSceneRuntimeReference = sceneReference;
        }

        internal void UnloadMapScene()
        {
            if (_mapSceneRuntimeReference != null)
                _mapSceneRuntimeReference.UnLoadScene();
        }

        #region Scene Unloading

        public Action allScenesUnloaded;
        private int _scenesToUnload = 0;

        public void UnloadScene(AssetReference scene)
        {
            _scenesToUnload++;
            StartCoroutine(UnloadAsync(scene));
        }

        private IEnumerator UnloadAsync(AssetReference scene)
        {
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i].Scene.name == scene.SubObjectName)
                {
                    Debug.Log($"<color=green>Start unlading<color> of scene \"{scene.SubObjectName}\"");
                    loadedScenes.RemoveAt(i);
                    yield return Addressables.UnloadSceneAsync(loadedScenes[i]);
                    Debug.Log($"<color=green>Done unlading<color> of scene \"{scene.SubObjectName}\"");
                    break;
                }
            }

            _scenesToUnload--;
            if (_scenesToUnload <= 0)
                allScenesUnloaded?.Invoke();
        }

        #endregion
    }
}