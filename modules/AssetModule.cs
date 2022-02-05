using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using GB.Config;
using GB.UI;
using System.Reflection;
using GB.Core.Loading;
using GB.Core;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace AssetModule
{
    public class AssetManager : MonoBehaviour
    {
        private static List<AssetBundle> assetBundles = new List<AssetBundle>();

        public void OnApplicationQuit()
        {
            UnloadAllBundles();
        }

        public static void UnloadAllBundles()
        {
            foreach (AssetBundle assetBundle in assetBundles)
            {
                // Checks if already unloaded
                if (assetBundle != null)
                {
                    assetBundle.Unload(false);
                }
            }
            assetBundles.Clear();
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static AssetBundle LoadBundle(string path)
        {
            // Makes sure user only loads asset bundles in assets folder
            if (path.Contains("\\..") || path.Contains("/.."))
            {
                Debug.Log($"AssetBundles can only be loaded from the 'Assets' directory or one of its sub directories.");
                return null;
            }

            string finalPath = Path.Combine(Application.dataPath, "../Assets", path);

            try
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(finalPath);
                assetBundles.Add(assetBundle);
                return assetBundle;
            }
            catch
            {
                Debug.Log($"Couldn't find an asset bundle with the path of {path}.");
                return null;
            }
        }

        public static T LoadAssetFromBundle<T>(string assetName, string bundleName) where T : Object
        {
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle == null) return default(T);

            T asset = bundle.LoadAsset<T>(assetName);
            bundle.Unload(false);
            return asset;
        }

        public static Object LoadAssetFromBundle(string assetName, string bundleName)
        {
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle == null) return null;

            Object asset = bundle.LoadAsset(assetName);
            bundle.Unload(false);
            return asset;
        }

        public static Object[] LoadAllFromBundle(string bundleName)
        {
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle == null) return null;

            Object[] assets = bundle.LoadAllAssets();
            bundle.Unload(false);
            return assets;
        }

        public static T[] LoadAllFromBundle<T>(string bundleName) where T : Object
        {
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle == null) return default(T[]);

            T[] assets = bundle.LoadAllAssets<T>();
            bundle.Unload(false);
            return assets;
        }
    }

    public class Spawner : Mod
    {
        public void Init()
        {
            GameObject manager = GameObject.Find("ModuleManager");
            if (manager == null)
            {
                manager = new GameObject("ModuleManager");
            }
            manager.AddComponent<AssetManager>();
        }

        public void Start()
        {
        }

        public void Update()
        {
        }
    }
}
