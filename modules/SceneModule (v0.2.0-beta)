using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using GB.Config;
using GB.UI;
using System.Reflection;
using GB.Core.Loading;
using GB.Core;
using System.Collections.Generic;
using GB.Networking.Utils.Spawn;
using System.Linq;
using Random = UnityEngine.Random;
using GB.Gamemodes;

namespace SceneModule
{
    public enum GameMode
    {
        Melee
    }

    public class CustomScene
    {
        public string name { get; private set; }
        public List<GameObject> sceneObjects = new List<GameObject>();
        public List<Action> actions = new List<Action>();

        public CustomScene(string name)
        {
            this.name = name;
            CustomSceneManager.CreateScene(this);
        }

        public CustomScene InvokeOnLoad(Action action)
        {
            actions.Add(action);
            return this;
        }

        public CustomScene InvokeOnLoad(Action[] action)
        {
            actions.AddRange(action);
            return this;
        }

        public CustomScene InvokeOnLoad(List<Action> action)
        {
            actions.AddRange(action);
            return this;
        }

        public CustomScene AddObject(GameObject gameObject)
        {
            sceneObjects.Add(gameObject);
            return this;
        }

        public CustomScene AddObjects(GameObject[] gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                AddObject(gameObject);
            }
            return this;
        }

        public CustomScene AddObjects(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                AddObject(gameObject);
            }
            return this;
        }

        public void Load()
        {
            foreach (GameObject gameObject in sceneObjects)
            {
                GameObject.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
            }
            foreach (Action action in actions)
            {
                try
                {
                    action.Invoke();
                }
                catch
                {
                    Debug.Log($"Action '{action.Method.Name}' of custom scene '{name}' caused an error.");
                }
            }
        }

        public void RemoveAllObjects()
        {
            sceneObjects.Clear();
        }
    }

    public class CustomRotationConfig
    {
        public bool random { get; private set; }
        public int time { get; private set; }
        public GameMode gameMode { get; private set; }
        private string[] sceneNames;

        private int levelIndex = 0;

        private string[] ScrambleNames(string[] levelNames)
        {
            List<int> notPicked = new List<int>();
            for (int i = 0; i < levelNames.Length; i++) {
                notPicked.Add(i);
            }

            string[] scrambledNames = new string[levelNames.Length];

            int j = 0;
            while (notPicked.Count > 0)
            {
                int randomIndex = notPicked[Random.Range(0, notPicked.Count)];
                notPicked.Remove(randomIndex);
                scrambledNames[j] = levelNames[randomIndex];
                j++;
            }

            return scrambledNames;
        }

        public CustomRotationConfig(string[] levelNames, GameMode gameMode, bool random, int time)
        {
            if (random)
            {
                sceneNames = ScrambleNames(levelNames);
            }
            else
            {
                sceneNames = levelNames;
            }
            this.random = random;
            this.gameMode = gameMode;
            this.time = time;
        }

        public string GetNextLevel()
        {
            return sceneNames[levelIndex];
        }

        public RotationConfig GetRotationConfig()
        {
            GameModeEnum mode = GameModeEnum.Melee; ;
            if (gameMode == GameMode.Melee)
            {
                mode = GameModeEnum.Melee;
            }

            string[] normalLevels = new string[sceneNames.Length];

            int i = 0;
            foreach(string level in sceneNames)
            {
                Debug.Log($"{level} is custom: {IsCustom(level)}");
                if (IsCustom(level))
                {
                    normalLevels[i] = "Grind";
                }
                else
                {
                    normalLevels[i] = level;
                }
                i++;
            }

            return GBConfigLoader.CreateRotationConfig(
                normalLevels, mode, normalLevels.Length, false, time
            );
        }

        private bool IsCustom(string name)
        {
            return CustomSceneManager.SceneExists(name);
        }
    }

    public class CustomSceneManager : MonoBehaviour
    {
        private static Dictionary<string, CustomScene> customScenes = new Dictionary<string, CustomScene>();

        private static CustomRotationConfig rotationConfig;

        private static string cachedSceneName;
        private static bool loadingCustomScene;
        private static bool cachedDeleteSpawns;

        private static FieldInfo selectedConfig;
        private static TextReplacer subTitle;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            // Gets private fields that are used
            selectedConfig =
                typeof(MenuHandlerGamemodes).GetField("selectedConfig", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo subTitleInfo =
                typeof(LoadScreenDisplayHandler).GetField("_subTitle", BindingFlags.NonPublic | BindingFlags.Instance);

            subTitle = subTitleInfo.GetValue(MonoSingleton<Global>.Instance.LevelLoadSystem.LoadingScreen) as TextReplacer;
        }

        public void Update()
        {
            // Forces the sub title of the loading screen to be set
            if (loadingCustomScene)
            {
                subTitle.SetToCode(cachedSceneName);
            }
        }

        public static bool SceneExists(string name)
        {
            return customScenes.ContainsKey(name);
        }

        public static void CreateScene(CustomScene scene)
        {
            string name = scene.name;
            if (customScenes.ContainsKey(name))
            {
                Debug.Log($"A custom scene with the name '{name}' has already been created.");
            }
            customScenes[name] = scene;
        }

        public static CustomScene GetScene(string name)
        {
            if (!customScenes.ContainsKey(name))
            {
                Debug.Log($"No custom scene with the name '{name}' exists.");
                return null;
            }
            return customScenes[name];
        }

        public static void DeleteScene(string name)
        {
            if (!customScenes.ContainsKey(name))
            {
                Debug.Log($"No custom scene with the name '{name}' exists.");
                return;
            }
            customScenes[name] = null;
            customScenes.Remove(name);
        }

        private static void HandleCustomRotationConfig(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "Menu")
            {
                SceneManager.sceneLoaded -= HandleCustomRotationConfig;
                return;
            }

            string nextLevel = rotationConfig.GetNextLevel();
            if (SceneExists(nextLevel))
            {
                LoadCustomScene(nextLevel, false);
            }
        }

        public static void StartCustomGame(CustomRotationConfig customRotationConfig)
        {
            if (customRotationConfig == null)
            {
                Debug.Log($"Custom rotation config can't be null.");
                return;
            }

            rotationConfig = customRotationConfig;
            SceneManager.sceneLoaded += HandleCustomRotationConfig;

            MenuHandlerGamemodes handler = GameObject.FindObjectOfType<MenuHandlerGamemodes>();
            if (handler == null)
            {
                Debug.Log($"A custom scene can't be loaded while in game.");
            }
            selectedConfig.SetValue(handler, rotationConfig.GetRotationConfig());
            handler.OnCountdownComplete();
        }

        public static void LoadScene(string name, bool deleteSpawns = true, GameMode gameMode = GameMode.Melee)
        {
            // Check if custom scene exists
            if (!customScenes.ContainsKey(name))
            {
                Debug.Log($"No custom scene with the name '{name}' exists.");
                return;
            }


            // Loads Grind
            MenuHandlerGamemodes handler = GameObject.FindObjectOfType<MenuHandlerGamemodes>();
            if (handler == null)
            {
                Debug.Log($"A custom scene can't be loaded while in game.");
            }

            string mode = "Melee";
            if (gameMode == GameMode.Melee)
            {
                mode = "Melee";
            }

            selectedConfig.SetValue(handler, GBConfigLoader.CreateRotationConfig(
                "Grind", mode, 1
            ));
            handler.OnCountdownComplete();


            // Caches scene name so that it knows which scene to load when Grind loads
            cachedSceneName = name;

            // Caches delete spawns so that when Grind loads, it knows if the player spawns should be deleted
            cachedDeleteSpawns = deleteSpawns;

            loadingCustomScene = true;
            SceneManager.sceneLoaded += OnCustomSceneLoaded;
        }

        private static void OnCustomSceneLoaded(Scene scene, LoadSceneMode _)
        {
            LoadCustomScene(cachedSceneName, cachedDeleteSpawns);
            SceneManager.sceneLoaded -= OnCustomSceneLoaded;
        }

        // Removes objects from Grind and adds custom objects
        private static void LoadCustomScene(string name, bool deleteSpawns)
        {
            // Sets sub title of loading screen
            subTitle.SetToCode(name);

            // Gets rid of objects from grind
            List<string> objectsToDestroy = new List<string>()
            {
                "World", "Void", "KillVolumes", "AI", "AchievementTrackers", "Plane", "Plane (1)", "Sounds",
                "Sphere", "RoomLeavers", "CCTV Camera"
            };

            // Checks if should delete spawns
            if (deleteSpawns)
            {
                objectsToDestroy.Add("PlayerSpawns");
            }

            foreach (string gameObjectName in objectsToDestroy)
            {
                GameObject objectToDestroy = GameObject.Find(gameObjectName);
                if (objectsToDestroy != null)
                {
                    GameObject.Destroy(objectToDestroy);
                }
            }
            foreach (Transform child in GameObject.Find("Lighting & Effects").GetComponentInChildren<Transform>())
            {
                if (child.name != "Postprocessing Global Volume")
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            // Loads custom scene
            customScenes[name].Load();


            // Resets values
            loadingCustomScene = false;
            cachedSceneName = "";
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
            manager.AddComponent<CustomSceneManager>();
        }

        public void Start()
        {
        }

        public void Update()
        {
        }
    }
}
