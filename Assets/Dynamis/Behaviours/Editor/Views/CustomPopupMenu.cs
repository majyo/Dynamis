using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    [UxmlElement]
    public partial class CustomPopupMenu : VisualElement
    {
        [UxmlAttribute]
        public string Title { get; set; } = "Menu";

        private VisualElement _container;
        private Label _titleLabel;
        private VisualElement _itemsContainer;
        private Button _closeButton;
        private readonly List<PopupMenuItem> _menuItems;
        private bool _isVisible;

        public string TitleContent
        {
            get => _titleLabel?.text ?? "";
            set
            {
                if (_titleLabel != null)
                {
                    _titleLabel.text = value;
                }
            }
        }

        public bool IsVisible => _isVisible;

        public event Action OnClosed;

        public CustomPopupMenu()
        {
            _menuItems = new List<PopupMenuItem>();
            InitializeMenu();
            SetupStyles();
            Hide();
            
            // Set the title from the UXML attribute
            TitleContent = Title;
        }

        private void InitializeMenu()
        {
            // Main container
            _container = new VisualElement();
            _container.name = "popup-container";
            Add(_container);

            // Header with title and close button
            var header = new VisualElement();
            header.name = "popup-header";
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            _container.Add(header);

            _titleLabel = new Label("Menu");
            _titleLabel.name = "popup-title";
            header.Add(_titleLabel);

            _closeButton = new Button(() => Hide()) { text = "✕" };
            _closeButton.name = "popup-close-button";
            header.Add(_closeButton);

            // Items container
            _itemsContainer = new VisualElement();
            _itemsContainer.name = "popup-items";
            _container.Add(_itemsContainer);
        }

        private void SetupStyles()
        {
            // Main popup styles
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopColor = Color.gray;
            style.borderBottomColor = Color.gray;
            style.borderLeftColor = Color.gray;
            style.borderRightColor = Color.gray;
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            style.minWidth = 200;
            style.maxWidth = 400;

            // Container styles
            // m_Container.style.padding = new StyleLength(10);

            // Header styles
            var header = _container.Q("popup-header");
            header.style.marginBottom = 10;
            header.style.paddingBottom = 5;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = Color.gray;

            // Title styles
            _titleLabel.style.fontSize = 16;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Close button styles
            _closeButton.style.width = 20;
            _closeButton.style.height = 20;
            _closeButton.style.backgroundColor = Color.clear;
            _closeButton.style.borderTopWidth = 0;
            _closeButton.style.borderBottomWidth = 0;
            _closeButton.style.borderLeftWidth = 0;
            _closeButton.style.borderRightWidth = 0;
            _closeButton.style.color = Color.white;

            // Items container styles
            _itemsContainer.style.flexDirection = FlexDirection.Column;
        }

        public void AddMenuItem(string text, Action callback = null, bool enabled = true)
        {
            var menuItem = new PopupMenuItem(text, callback, enabled);
            _menuItems.Add(menuItem);
            _itemsContainer.Add(menuItem);
        }

        public void AddSeparator()
        {
            var separator = new VisualElement();
            separator.name = "popup-separator";
            separator.style.height = 1;
            separator.style.backgroundColor = Color.gray;
            separator.style.marginTop = 5;
            separator.style.marginBottom = 5;
            _itemsContainer.Add(separator);
        }

        public void ClearItems()
        {
            _menuItems.Clear();
            _itemsContainer.Clear();
        }

        public void Show(Vector2 position)
        {
            _isVisible = true;
            style.display = DisplayStyle.Flex;
            style.left = position.x;
            style.top = position.y;
            
            // Ensure the menu stays within screen bounds
            this.schedule.Execute(() => AdjustPosition()).ExecuteLater(1);
        }

        public void Hide()
        {
            _isVisible = false;
            style.display = DisplayStyle.None;
            OnClosed?.Invoke();
        }

        private void AdjustPosition()
        {
            if (panel == null) return;

            var panelSize = panel.visualTree.layout.size;
            var menuSize = layout.size;
            var currentPos = new Vector2(style.left.value.value, style.top.value.value);

            // Adjust horizontal position
            if (currentPos.x + menuSize.x > panelSize.x)
            {
                style.left = panelSize.x - menuSize.x - 10;
            }

            // Adjust vertical position
            if (currentPos.y + menuSize.y > panelSize.y)
            {
                style.top = panelSize.y - menuSize.y - 10;
            }
        }

        public void ShowAt(VisualElement target, MenuPosition position = MenuPosition.Bottom)
        {
            if (target == null) return;

            var targetBounds = target.worldBound;
            Vector2 showPosition;

            switch (position)
            {
                case MenuPosition.Top:
                    showPosition = new Vector2(targetBounds.x, targetBounds.y - 5);
                    break;
                case MenuPosition.Bottom:
                    showPosition = new Vector2(targetBounds.x, targetBounds.yMax + 5);
                    break;
                case MenuPosition.Left:
                    showPosition = new Vector2(targetBounds.x - 5, targetBounds.y);
                    break;
                case MenuPosition.Right:
                    showPosition = new Vector2(targetBounds.xMax + 5, targetBounds.y);
                    break;
                default:
                    showPosition = new Vector2(targetBounds.x, targetBounds.yMax + 5);
                    break;
            }

            Show(showPosition);
        }
    }

    public class PopupMenuItem : Button
    {
        public PopupMenuItem(string text, Action callback = null, bool enabled = true)
        {
            this.text = text;
            this.SetEnabled(enabled);
            
            if (callback != null)
            {
                clicked += callback;
            }

            SetupStyles();
        }

        private void SetupStyles()
        {
            style.backgroundColor = Color.clear;
            style.borderTopWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            style.borderRightWidth = 0;
            style.paddingTop = 8;
            style.paddingBottom = 8;
            style.paddingLeft = 12;
            style.paddingRight = 12;
            style.marginTop = 1;
            style.marginBottom = 1;
            style.color = Color.white;
            style.unityTextAlign = TextAnchor.MiddleLeft;

            // Hover effect
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (enabledSelf)
                    style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            });

            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                style.backgroundColor = Color.clear;
            });
        }
    }

    public enum MenuPosition
    {
        Top,
        Bottom,
        Left,
        Right
    }
}
