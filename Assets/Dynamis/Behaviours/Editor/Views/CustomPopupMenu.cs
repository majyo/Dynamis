using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    [UxmlElement]
    public partial class CustomPopupMenu : VisualElement
    {
        private const string ExpandedIcon = "▼";  // U+25BC BLACK DOWN-POINTING TRIANGLE
        private const string CollapsedIcon = "▶"; // U+25B6 BLACK RIGHT-POINTING TRIANGLE

        [UxmlAttribute]
        public string Title { get; set; } = "Menu";

        private VisualElement _container;
        private Label _titleLabel;
        private ScrollView _scrollView;
        private VisualElement _itemsContainer;
        private readonly List<MenuTreeItem> _rootItems;
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
            _rootItems = new List<MenuTreeItem>();
            InitializeMenu();
            SetupStyles();
            Hide();
            
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

            // ScrollView for menu items
            _scrollView = new ScrollView
            {
                name = "popup-scroll",
                mode = ScrollViewMode.Vertical
            };
            _container.Add(_scrollView);

            // Items container inside scroll view
            _itemsContainer = new VisualElement
            {
                name = "popup-items"
            };
            _scrollView.Add(_itemsContainer);
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
            style.maxHeight = 400;
            style.paddingTop = 0;
            style.paddingBottom = 6;
            style.paddingLeft = 4;
            style.paddingRight = 4;

            // Container styles
            _container.style.paddingTop = 0;
            _container.style.paddingBottom = 0;
            _container.style.paddingLeft = 0;
            _container.style.paddingRight = 0;

            // Header styles
            var header = _container.Q("popup-header");
            header.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            header.style.marginBottom = 0;
            header.style.paddingBottom = 4;
            header.style.paddingTop = 4;
            header.style.paddingLeft = 8;
            header.style.paddingRight = 8;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            header.style.borderTopLeftRadius = 2;
            header.style.borderTopRightRadius = 2;

            // Title styles
            _titleLabel.style.fontSize = 13;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // ScrollView styles
            _scrollView.style.flexGrow = 1;
            _scrollView.style.marginTop = 4;

            // Items container styles
            _itemsContainer.style.flexDirection = FlexDirection.Column;
        }

        public void AddMenuItem(string text, Action callback = null, bool enabled = true)
        {
            var menuItem = new MenuTreeItem
            {
                Text = text,
                Callback = callback,
                Enabled = enabled,
                IsGroup = false,
                IsSeparator = false
            };
            _rootItems.Add(menuItem);
            RefreshMenuItems();
        }

        public MenuTreeItem AddGroup(string groupName, bool expanded = false)
        {
            var groupItem = new MenuTreeItem
            {
                Text = groupName,
                IsGroup = true,
                IsSeparator = false,
                Enabled = true,
                IsExpanded = expanded,
                Children = new List<MenuTreeItem>()
            };
            _rootItems.Add(groupItem);
            RefreshMenuItems();
            return groupItem;
        }

        public void AddSeparator()
        {
            var separator = new MenuTreeItem
            {
                Text = "",
                IsSeparator = true,
                IsGroup = false,
                Enabled = false
            };
            _rootItems.Add(separator);
            RefreshMenuItems();
        }

        public void ClearItems()
        {
            _rootItems.Clear();
            RefreshMenuItems();
        }

        private void RefreshMenuItems()
        {
            _itemsContainer.Clear();
            foreach (var item in _rootItems)
            {
                AddItemToContainer(item, 0);
            }
        }

        private void AddItemToContainer(MenuTreeItem item, int indentLevel)
        {
            var itemElement = CreateMenuItemElement(item, indentLevel);
            _itemsContainer.Add(itemElement);

            if (item.IsGroup && item.IsExpanded && item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    AddItemToContainer(child, indentLevel + 1);
                }
            }
        }

        private VisualElement CreateMenuItemElement(MenuTreeItem item, int indentLevel)
        {
            if (item.IsSeparator)
            {
                var separator = new VisualElement
                {
                    style =
                    {
                        height = 1,
                        backgroundColor = Color.gray,
                        marginTop = 3,
                        marginBottom = 3,
                        marginLeft = indentLevel * 16
                    }
                };
                return separator;
            }

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 4 + indentLevel * 16,
                    paddingRight = 4,
                    paddingTop = 2,
                    paddingBottom = 2,
                    minHeight = 22
                }
            };

            if (item.IsGroup)
            {
                // Toggle button for groups
                var toggleButton = new Button(() => {
                    item.IsExpanded = !item.IsExpanded;
                    RefreshMenuItems();
                })
                {
                    text = item.IsExpanded ? ExpandedIcon : CollapsedIcon,
                    style =
                    {
                        width = 16,
                        height = 16,
                        backgroundColor = Color.clear,
                        borderTopWidth = 0,
                        borderBottomWidth = 0,
                        borderLeftWidth = 0,
                        borderRightWidth = 0,
                        color = Color.white,
                        fontSize = 10,
                        marginRight = 4
                    }
                };
                container.Add(toggleButton);

                // Group label
                var label = new Label(item.Text)
                {
                    style =
                    {
                        color = new Color(0.8f, 0.8f, 1f, 1f),
                        unityFontStyleAndWeight = FontStyle.Bold,
                        fontSize = 12
                    }
                };
                container.Add(label);
            }
            else
            {
                // Regular menu item button
                var button = new Button(() => {
                    item.Callback?.Invoke();
                    Hide();
                })
                {
                    text = item.Text
                };
                button.SetEnabled(item.Enabled);
                button.style.flexGrow = 1;
                button.style.backgroundColor = Color.clear;
                button.style.borderTopWidth = 0;
                button.style.borderBottomWidth = 0;
                button.style.borderLeftWidth = 0;
                button.style.borderRightWidth = 0;
                button.style.color = item.Enabled ? Color.white : Color.gray;
                button.style.unityTextAlign = TextAnchor.MiddleLeft;
                button.style.fontSize = 12;

                // Hover effect
                button.RegisterCallback<MouseEnterEvent>(OnMouseEnter);

                button.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

                container.Add(button);
            }

            return container;
        }

        private static void OnMouseLeave(MouseLeaveEvent evt)
        {
            if (evt.target is Button button)
            {
                button.style.backgroundColor = Color.clear;
            }
        }

        private static void OnMouseEnter(MouseEnterEvent evt)
        {
            if (evt.target is Button { enabledSelf: true } button)
            {
                button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            }
        }

        public void Show(Vector2 position)
        {
            _isVisible = true;
            style.display = DisplayStyle.Flex;
            style.left = position.x;
            style.top = position.y;
            
            RefreshMenuItems();
            
            // 调整位置
            this.schedule.Execute(() => {
                this.MarkDirtyRepaint();
                this.schedule.Execute(AdjustPosition).ExecuteLater(2);
            }).ExecuteLater(1);
        }

        public void Hide()
        {
            _isVisible = false;
            style.display = DisplayStyle.None;
            OnClosed?.Invoke();
        }

        private void AdjustPosition()
        {
            if (panel == null || !_isVisible) return;

            if (layout.size == Vector2.zero)
            {
                this.schedule.Execute(AdjustPosition).ExecuteLater(1);
                return;
            }

            var panelSize = panel.visualTree.layout.size;
            var menuSize = layout.size;
            var currentPos = new Vector2(style.left.value.value, style.top.value.value);

            var newLeft = currentPos.x;
            var newTop = currentPos.y;

            // 调整水平位置
            if (currentPos.x + menuSize.x > panelSize.x)
            {
                newLeft = Mathf.Max(0, panelSize.x - menuSize.x - 10);
            }

            // 调整垂直位置
            if (currentPos.y + menuSize.y > panelSize.y)
            {
                newTop = Mathf.Max(0, panelSize.y - menuSize.y - 10);
            }
            
            if (!Mathf.Approximately(newLeft, currentPos.x) || !Mathf.Approximately(newTop, currentPos.y))
            {
                style.left = newLeft;
                style.top = newTop;
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

    public class MenuTreeItem
    {
        public string Text { get; set; }
        public Action Callback { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsGroup { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsExpanded { get; set; } = true;
        public List<MenuTreeItem> Children { get; set; }

        public MenuTreeItem AddChild(string text, Action callback = null, bool enabled = true)
        {
            if (Children == null)
                Children = new List<MenuTreeItem>();

            var childItem = new MenuTreeItem
            {
                Text = text,
                Callback = callback,
                Enabled = enabled,
                IsGroup = false,
                IsSeparator = false
            };
            Children.Add(childItem);
            return childItem;
        }

        public MenuTreeItem AddChildGroup(string groupName)
        {
            Children ??= new List<MenuTreeItem>();

            var childGroup = new MenuTreeItem
            {
                Text = groupName,
                IsGroup = true,
                IsSeparator = false,
                Enabled = true,
                IsExpanded = true,
                Children = new List<MenuTreeItem>()
            };
            Children.Add(childGroup);
            return childGroup;
        }

        public void AddChildSeparator()
        {
            Children ??= new List<MenuTreeItem>();

            var separator = new MenuTreeItem
            {
                Text = "",
                IsSeparator = true,
                IsGroup = false,
                Enabled = false
            };
            Children.Add(separator);
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
