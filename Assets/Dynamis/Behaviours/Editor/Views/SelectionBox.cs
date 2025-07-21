using UnityEngine;
using UnityEngine.UIElements;

namespace Dynamis.Behaviours.Editor.Views
{
    /// <summary>
    /// 用于绘制框选框的工具类
    /// </summary>
    public class SelectionBox : VisualElement
    {
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private bool _isDrawing;
        private VisualElement _boxElement;

        public bool IsDrawing => _isDrawing;
        public Rect SelectionRect => GetSelectionRect();

        public SelectionBox()
        {
            // 设置为覆盖整个父容器
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            
            // 初始状态下隐藏
            style.display = DisplayStyle.None;
            
            // 确保不阻挡鼠标事件传递
            pickingMode = PickingMode.Ignore;
            
            CreateBoxElement();
        }

        private void CreateBoxElement()
        {
            _boxElement = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(0.3f, 0.6f, 1f, 1f),
                    borderRightColor = new Color(0.3f, 0.6f, 1f, 1f),
                    borderTopColor = new Color(0.3f, 0.6f, 1f, 1f),
                    borderBottomColor = new Color(0.3f, 0.6f, 1f, 1f),
                    backgroundColor = new Color(0.3f, 0.6f, 1f, 0.1f)
                },
                pickingMode = PickingMode.Ignore
            };

            Add(_boxElement);
        }

        /// <summary>
        /// 开始绘制框选框
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        public void StartSelection(Vector2 startPosition)
        {
            _startPosition = startPosition;
            _endPosition = startPosition;
            _isDrawing = true;
            
            // 显示选择框
            style.display = DisplayStyle.Flex;
            
            UpdateBoxPosition();
        }

        /// <summary>
        /// 更新框选框的结束位置
        /// </summary>
        /// <param name="currentPosition">当前鼠标位置</param>
        public void UpdateSelection(Vector2 currentPosition)
        {
            if (!_isDrawing) return;
            
            _endPosition = currentPosition;
            UpdateBoxPosition();
        }

        /// <summary>
        /// 结束框选框绘制
        /// </summary>
        public void EndSelection()
        {
            _isDrawing = false;
            style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 取消框选框绘制
        /// </summary>
        public void CancelSelection()
        {
            _isDrawing = false;
            style.display = DisplayStyle.None;
        }

        private void UpdateBoxPosition()
        {
            var rect = GetSelectionRect();
            
            _boxElement.style.left = rect.x;
            _boxElement.style.top = rect.y;
            _boxElement.style.width = rect.width;
            _boxElement.style.height = rect.height;
        }

        private Rect GetSelectionRect()
        {
            var minX = Mathf.Min(_startPosition.x, _endPosition.x);
            var minY = Mathf.Min(_startPosition.y, _endPosition.y);
            var maxX = Mathf.Max(_startPosition.x, _endPosition.x);
            var maxY = Mathf.Max(_startPosition.y, _endPosition.y);
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 设置框选框的样式
        /// </summary>
        /// <param name="borderColor">边框颜色</param>
        /// <param name="fillColor">填充颜色</param>
        /// <param name="borderWidth">边框宽度</param>
        public void SetStyle(Color borderColor, Color fillColor, float borderWidth = 1f)
        {
            _boxElement.style.borderLeftColor = borderColor;
            _boxElement.style.borderRightColor = borderColor;
            _boxElement.style.borderTopColor = borderColor;
            _boxElement.style.borderBottomColor = borderColor;
            _boxElement.style.backgroundColor = fillColor;
            
            _boxElement.style.borderLeftWidth = borderWidth;
            _boxElement.style.borderRightWidth = borderWidth;
            _boxElement.style.borderTopWidth = borderWidth;
            _boxElement.style.borderBottomWidth = borderWidth;
        }
    }
}
