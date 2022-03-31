using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BuildModule
{
    public class BuildInputSettings
    {
        public InputDevice device;
        public InputControl build;
        public InputControl cancel;
        public InputControl rotateLeft;
        public InputControl rotateRight;

        public bool isGamepad;

        public BuildInputSettings(
        InputDevice device,
        InputControl build,
        InputControl cancel,
        InputControl rotateLeft,
        InputControl rotateRight,
        bool isGamepad)
        {
            this.device = device;
            this.build = build;
            this.cancel = cancel;
            this.rotateLeft = rotateLeft;
            this.rotateRight = rotateRight;
            this.isGamepad = isGamepad;
        }
    }

    public class BuildManager : MonoBehaviour
    {

        private static GameObject activePrefab; // caches the prefab
        private static GameObject activeBlueprint; // stores the active blueprint
        private static bool building; // bool for if currently building

        private static bool inputSettingsSet;
        private static BuildInputSettings cachedBuildInputSettings;
        private static float cachedHeightOffset;
        private static Action<GameObject> cachedOnSpawnedBlueprint = null;
        private static Action<GameObject> cachedOnBuild = null;
        private static Action cachedOnCanceled = null;
        private static Action<GameObject> cachedOnRotated = null;
        private static Func<GameObject, bool> cachedIsValidPosition = null;

        public static void StartBuilding(GameObject prefab,
        float heightOffset = 0f,
        Action<GameObject> onSpawnedBlueprint = null,
        Action<GameObject> onBuild = null,
        Action onCanceled = null,
        Action<GameObject> onRotated = null,
        Func<GameObject, bool> isValidPosition = null,
        BuildInputSettings buildInputSettings = null)
        {
            if (building)
            {
                CancelBuild();
            }
            activePrefab = prefab;
            activeBlueprint = SpawnBluePrint();

            cachedOnSpawnedBlueprint = onSpawnedBlueprint;
            cachedOnBuild = onBuild;
            cachedHeightOffset = heightOffset;
            cachedIsValidPosition = isValidPosition;
            cachedOnRotated = onRotated;
            cachedOnCanceled = onCanceled;
            cachedBuildInputSettings = buildInputSettings;
            inputSettingsSet = buildInputSettings != null;

            building = true;
        }

        // cancels the build
        public static void CancelBuild()
        {
            if (!building) return;

            Destroy(activeBlueprint);
            building = false;

            if (cachedOnCanceled != null)
            {
                cachedOnCanceled();
            }
        }

        // places the prefab at the blueprints position
        public static void Build()
        {
            if (!building) return;

            building = false;
            GameObject placedObject =
                Instantiate(activePrefab, activeBlueprint.transform.position, activeBlueprint.transform.rotation);

            Destroy(activeBlueprint);

            if (cachedOnBuild != null)
            {
                cachedOnBuild(placedObject);
            }
        }

        // Destroys the collider of the given transform
        private static void DestroyColliderAndRigidbody(Transform transform)
        {
            Collider collider = transform.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.useGravity = false;
            }
        }

        // Destroys all colliders of children of the given transform
        private static void DestroyCollidersAndRigidbodies(Transform @object)
        {
            Destroy(@object.GetComponent<Collider>());
            foreach (Transform child in @object)
            {
                DestroyColliderAndRigidbody(child);
                DestroyCollidersAndRigidbodies(child);
            }
        }

        // Spawns the blueprint from the cached prefab and removes all colliders from children
        private static GameObject SpawnBluePrint()
        {
            GameObject blueprint = Instantiate(activePrefab);
            DestroyCollidersAndRigidbodies(blueprint.transform);
            return blueprint;
        }

        private Vector3 GetMousePositionInWorld()
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 2f;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit)) 
            {
                return hit.point;
            }

            return new Vector3(0f, -1000f, 0f);
        }

        void HandleRightStickAsMouse()
        {
            Gamepad gamepad;
            try 
            {
                gamepad = cachedBuildInputSettings.device as Gamepad;
            }
            catch
            {
                Debug.Log("'useRightStickAsMouse' only works when the given device is not null and is a Gamepad.");
                return;
            }

            Vector2 gamepadOffset = Vector2Int.zero;
            if (gamepad.rightStick.up.isPressed)
            {
                gamepadOffset.y = 1;
            }
            else if (gamepad.rightStick.down.isPressed)
            {
                gamepadOffset.y = -1;
            }

            if (gamepad.rightStick.left.isPressed)
            {
                gamepadOffset.x = -1;
            }
            else if (gamepad.rightStick.right.isPressed)
            {
                gamepadOffset.x = 1;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 newPos = mousePos + gamepadOffset * 150f * Time.deltaTime;

            Mouse.current.WarpCursorPosition(newPos);
            InputState.Change(Mouse.current.position, newPos);
        }

        void HandleBlueprintPosition()
        {
            activeBlueprint.transform.position = GetMousePositionInWorld() + (Vector3.up * cachedHeightOffset);
        }

        void HandleBlueprintRotation()
        {
            if (inputSettingsSet)
            {
                if (cachedBuildInputSettings.rotateLeft.IsPressed())
                {
                    activeBlueprint.transform.eulerAngles = activeBlueprint.transform.eulerAngles - Vector3.up * 100f * Time.deltaTime;
                }
                else if (cachedBuildInputSettings.rotateRight.IsPressed())
                {
                    activeBlueprint.transform.eulerAngles = activeBlueprint.transform.eulerAngles + Vector3.up * 100f * Time.deltaTime;
                }
            }
            else
            {
                if (Keyboard.current.rKey.isPressed)
                {
                    activeBlueprint.transform.eulerAngles = activeBlueprint.transform.eulerAngles + Vector3.up * 100f * Time.deltaTime;
                    if (cachedOnRotated != null)
                    {
                        cachedOnRotated(activeBlueprint);
                    }
                }
                else
                {
                    foreach (Gamepad gamepad in Gamepad.all)
                    {
                        if (gamepad.rightShoulder.isPressed)
                        {
                            activeBlueprint.transform.eulerAngles = activeBlueprint.transform.eulerAngles + Vector3.up * 100f * Time.deltaTime;
                            if (cachedOnRotated != null)
                            {
                                cachedOnRotated(activeBlueprint);
                            }
                            break;
                        }
                        else if (gamepad.leftShoulder.isPressed)
                        {
                            activeBlueprint.transform.eulerAngles = activeBlueprint.transform.eulerAngles - Vector3.up * 100f * Time.deltaTime;
                            if (cachedOnRotated != null)
                            {
                                cachedOnRotated(activeBlueprint);
                            }
                            break;
                        }
                    }
                }
            }
        }

        void HandleBuilding()
        {
            if (inputSettingsSet)
            {
                if (cachedBuildInputSettings.build.IsPressed())
                {
                    if (cachedIsValidPosition != null)
                    {
                        if (cachedIsValidPosition(activeBlueprint)) 
                        {
                            Build();
                        }
                    }
                    else
                    {
                        Build();
                    }
                }

                if (cachedBuildInputSettings.cancel.IsPressed())
                {
                    CancelBuild();
                }
            }
            else
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (cachedIsValidPosition != null)
                    {
                        if (cachedIsValidPosition(activeBlueprint)) {
                            Build();
                        }
                    }
                    else
                    {
                        Build();
                    }
                }
                else
                {
                    foreach (Gamepad gamepad in Gamepad.all)
                    {
                        if (gamepad.aButton.wasPressedThisFrame)
                        {
                            if (cachedIsValidPosition != null)
                            {
                                if (cachedIsValidPosition(activeBlueprint)) {
                                    Build();
                                    break;
                                }
                            }
                            Build();
                            break;
                        }
                    }
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    CancelBuild();
                }
                else
                {
                    foreach (Gamepad gamepad in Gamepad.all)
                    {
                        if (gamepad.rightTrigger.wasPressedThisFrame)
                        {
                            CancelBuild();
                            break;
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (!building) return;

            if (cachedBuildInputSettings.isGamepad)
            {
                Debug.Log("IS A GAMEPAD");
                HandleRightStickAsMouse();
            }
            HandleBlueprintPosition();
            HandleBlueprintRotation();
            HandleBuilding();
        }
    }

    // Used to create an instance of the BuildManager class
    public class Spawner : Mod
    {
        public void Init()
        {
            GameObject gameObject = GameObject.Find("ModuleManager");
            bool flag = gameObject == null;
            if (flag)
            {
                gameObject = new GameObject("ModuleManager");
            }
            gameObject.AddComponent<BuildManager>();
        }

        public void Start()
        {
        }

        public void Update()
        {
        }
    }
}