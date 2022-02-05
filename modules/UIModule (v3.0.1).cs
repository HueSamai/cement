using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UIModule
{
    public class UIManager : MonoBehaviour
    {
        private static GameObject canvasObject;

        public void Awake()
        {
            canvasObject = new GameObject("Canvas", typeof(RectTransform));
            canvasObject.AddComponent<Canvas>();

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            DontDestroyOnLoad(canvasObject);
            DontDestroyOnLoad(gameObject);
        }

        public static Text AddText(TextSettings textSettings = null)
        {
            if (textSettings == null)
            {
                textSettings = new TextSettings();
            }

            Vector2 position = textSettings.position;
            Vector2 size = textSettings.size;
            Transform parent = textSettings.parent;

            bool useRelativePosition = textSettings.useRelativePosition;

            GameObject textObject = AddElement(position, useRelativePosition, size, parent);
            if (textObject == null) return null;

            Text text = textObject.AddComponent<Text>();
            text.ChangeSettings(textSettings);

            return text;
        }

        public static InputField AddInputField(InputFieldSettings inputFieldSettings = null)
        {
            if (inputFieldSettings == null)
            {
                inputFieldSettings = new InputFieldSettings();
            }

            Vector2 position = inputFieldSettings.position;
            Vector2 size = inputFieldSettings.size;
            Transform parent = inputFieldSettings.parent;

            bool useRelativePosition = inputFieldSettings.useRelativePosition;

            GameObject inputObject = AddElement(position, useRelativePosition, size, parent);
            if (inputObject == null) return null;

            InputField inputField = inputObject.AddComponent<InputField>();
            inputField.ChangeSettings(inputFieldSettings);

            return inputField;
        }

        public static Image AddImage(ImageSettings imageSettings = null)
        {
            if (imageSettings == null)
            {
                imageSettings = new ImageSettings();
            }

            Vector2 position = imageSettings.position;
            Vector2 size = imageSettings.size;

            Transform parent = imageSettings.parent;

            bool useRelativePosition = imageSettings.useRelativePosition;

            GameObject imageObject = AddElement(position, useRelativePosition, size, parent);
            if (imageObject == null) return null;

            Image image = imageObject.AddComponent<Image>();
            image.ChangeSettings(imageSettings);

            return image;
        }

        public static Button AddButton(ButtonSettings buttonSettings = null)
        {
            if (buttonSettings == null)
            {
                buttonSettings = new ButtonSettings();
            }

            Vector2 position = buttonSettings.position;
            Vector2 size = buttonSettings.size;
            Transform parent = buttonSettings.parent;

            bool useRelativePosition = buttonSettings.useRelativePosition;

            GameObject buttonObject = AddElement(position, useRelativePosition, size, parent);
            if (buttonObject == null) return null;

            Button button = buttonObject.AddComponent<Button>();
            button.ChangeSettings(buttonSettings);

            return button;
        }

        private static GameObject AddElement(Vector2 position, bool useRelativePosition, Vector2 size, Transform parent)
        {
            if (canvasObject == null) return null;

            GameObject element = new GameObject("", typeof(RectTransform));
            element.transform.SetParent(canvasObject.transform);

            RectTransform rectTransform = element.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            if (parent != null)
            {
                element.transform.SetParent(parent);

                if (useRelativePosition)
                {
                    rectTransform.anchoredPosition = position;
                }
            }

            return element;
        }
    }

    public static class HandleSettings
    {
        public static void ChangeSettings(this Text text, TextSettings textSettings)
        {
            TextAnchor textAnchor = textSettings.textAnchor;
            FontStyle fontStyle = textSettings.fontStyle;
            string content = textSettings.text;
            Font font = textSettings.font;
            int fontSize = textSettings.fontSize;
            Color color = textSettings.color;

            text.alignment = textAnchor;
            text.text = content;
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = fontStyle;
        }

        public static void ChangeSettings(this Image image, ImageSettings imageSettings)
        {
            Sprite sprite = imageSettings.sprite;
            Color color = imageSettings.color;

            if (sprite != null)
            {
                image.sprite = sprite;
            }
            image.color = color;
        }

        public static void ChangeSettings(this InputField inputField, InputFieldSettings inputFieldSettings)
        {
            Vector2 size = inputFieldSettings.size;
            TextSettings placeholderSettings = inputFieldSettings.placeholderSettings;
            int characterLimit = inputFieldSettings.characterLimit;
            UnityAction<string> valueChangedEvent = inputFieldSettings.valueChangedEvent;
            InputField.InputType inputType = inputFieldSettings.inputType;
            InputField.LineType lineType = inputFieldSettings.lineType;

            ImageSettings backgroundSettings = inputFieldSettings.backgroundSettings;
            TextSettings textSettings = inputFieldSettings.textSettings;

            if (textSettings == null)
            {
                textSettings = new TextSettings();
            }
            if (textSettings.size == Vector2.zero)
            {
                textSettings.size = size;
            }
            textSettings.parent = inputField.transform;
            Text textComponent = UIManager.AddText(textSettings);

            inputField.textComponent = textComponent;

            if (backgroundSettings == null)
            {
                backgroundSettings = new ImageSettings();
            }
            if (backgroundSettings.size == Vector2.zero)
            {
                backgroundSettings.size = size;
            }
            Image background = inputField.gameObject.AddComponent<Image>();
            background.ChangeSettings(backgroundSettings);

            if (placeholderSettings == null)
            {
                placeholderSettings = new TextSettings();
                placeholderSettings.color = Color.grey;
                placeholderSettings.text = "Enter text...";
                placeholderSettings.fontStyle = FontStyle.Italic;
            }
            if (placeholderSettings.size == Vector2.zero)
            {
                placeholderSettings.size = size;
            }
            placeholderSettings.parent = inputField.transform;
            inputField.placeholder = UIManager.AddText(placeholderSettings);

            inputField.inputType = inputType;
            inputField.lineType = lineType;

            if (valueChangedEvent != null)
            {
                inputField.onValueChanged.AddListener(valueChangedEvent);
            }
            inputField.characterLimit = characterLimit;
        }

        public static void ChangeSettings(this Button button, ButtonSettings buttonSettings)
        {
            Vector2 size = buttonSettings.size;
            UnityAction onClickEvent = buttonSettings.onClickEvent;
            Transform parent = buttonSettings.parent;
            ColorBlock colors = buttonSettings.colors;

            ImageSettings backgroundSettings = buttonSettings.backgroundSettings;
            TextSettings textSettings = buttonSettings.textSettings;

            if (textSettings == null)
            {
                textSettings = new TextSettings();
                textSettings.text = "Button";
            }
            if (textSettings.size == Vector2.zero)
            {
                textSettings.size = size;
            }
            textSettings.parent = button.transform;
            Text text = UIManager.AddText(textSettings);

            if (backgroundSettings == null)
            {
                backgroundSettings = new ImageSettings();
            }
            if (backgroundSettings.size == Vector2.zero)
            {
                backgroundSettings.size = size;
            }
            Image background = button.gameObject.AddComponent<Image>();

            button.image = background;
            background.ChangeSettings(backgroundSettings);
            

            if (onClickEvent != null)
            {
                button.onClick.AddListener(onClickEvent);
            }

            button.colors = colors;
        }
    }   

    public class Settings
    {
        public Vector2 position = Vector2.zero;
        public Vector2 size = Vector2.zero;
        public bool useRelativePosition = true;
        public Transform parent = null;
    }

    public class InputFieldSettings : Settings
    {
        public TextSettings textSettings = null;
        public ImageSettings backgroundSettings = null;
        public TextSettings placeholderSettings = null;
        public int characterLimit = 0;
        public UnityAction<string> valueChangedEvent = null;
        public InputField.InputType inputType = InputField.InputType.Standard;
        public InputField.LineType lineType = InputField.LineType.SingleLine;
    }

    public class ButtonSettings : Settings
    {
        public TextSettings textSettings = null;
        public ImageSettings backgroundSettings = null;
        public UnityAction onClickEvent = null;
        public ColorBlock colors = new ColorBlock();
        
        public ButtonSettings()
        {
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.2f;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }

    public class ImageSettings : Settings
    {
        public Color color = Color.white;
        public Sprite sprite = null;
    }

    public class TextSettings : Settings
    {
        public string text = "Text";
        public TextAnchor textAnchor = TextAnchor.MiddleCenter;
        public FontStyle fontStyle = FontStyle.Normal;
        public Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        public int fontSize = 30;
        public Color color = Color.black;
    }

    public class UISpawner : Mod
    {
        public void Init()
        {
            GameObject manager = GameObject.Find("ModuleManager");
            if (manager == null) {
                manager = new GameObject("ModuleManager");
            }
            manager.AddComponent<UIManager>();
        }

        public void Update()
        {
        }

        public void Start()
        {
        }
    }
}
