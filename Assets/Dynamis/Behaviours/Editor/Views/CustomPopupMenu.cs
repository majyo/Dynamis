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
            _container = new VisualElement
            {
                name = "popup-container"
            };
            Add(_container);

            // Header with title
            var header = new VisualElement
            {
                name = "popup-header",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center
                }
            };
            _container.Add(header);

            _titleLabel = new Label("Menu")
            {
                name = "popup-title"
            };
            header.Add(_titleLabel);

            // Items container
            _itemsContainer = new VisualElement
            {
                name = "popup-items"
            };
            _container.Add(_itemsContainer);
        }

        private void SetupStyles()
        {
            // Main popup styles
            style.position = Position.Absolute;
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopColor = Color.gray;
            style.borderBottomColor = Color.gray;
            style.borderLeftColor = Color.gray;
            style.borderRightColor = Color.gray;
            style.borderTopLeftRadius = 3;
            style.borderTopRightRadius = 3;
            style.borderBottomLeftRadius = 3;
            style.borderBottomRightRadius = 3;
            style.minWidth = 150;
            style.maxWidth = 300;
            style.paddingTop = 0;
            style.paddingBottom = 6;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            // Container styles - 减少内边距
            _container.style.paddingTop = 0;
            _container.style.paddingBottom = 0;
            _container.style.paddingLeft = 0;
            _container.style.paddingRight = 0;

            // Header styles - 加深背景色，增强区分
            var header = _container.Q("popup-header");
            header.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f); // 更深的背景色
            header.style.marginBottom = 0;
            header.style.paddingBottom = 4;
            header.style.paddingTop = 4;
            header.style.paddingLeft = 8;
            header.style.paddingRight = 8;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            header.style.borderTopLeftRadius = 2;
            header.style.borderTopRightRadius = 2;

            // Title styles - 减少字体大小
            _titleLabel.style.fontSize = 13;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Items container styles - 减少间距
            _itemsContainer.style.flexDirection = FlexDirection.Column;
            _itemsContainer.style.paddingTop = 4;
            _itemsContainer.style.paddingBottom = 0;
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
            this.schedule.Execute(AdjustPosition).ExecuteLater(1);
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
            Initialize(text, callback, enabled);
        }

        private void Initialize(string itemText, Action callback, bool enabled)
        {
            this.text = itemText;
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
            style.paddingTop = 4;
            style.paddingBottom = 4;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.color = Color.white;
            style.unityTextAlign = TextAnchor.MiddleLeft;
            style.fontSize = 12;
            style.height = 22;

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
