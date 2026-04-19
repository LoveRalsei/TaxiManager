using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public partial class MapForm : Form
    {
        // 区域关联分析2相关的成员变量
        private bool _isRegionCorrelation2Analyzing = false;
        private bool _isRegionCorrelation2Dragging = false;
        private List<PointLatLng> _correlation2RegionPoints = new List<PointLatLng>(); // 存储一个区域的点
        private GMapOverlay _correlation2RegionOverlay = new GMapOverlay("correlation2Polygons");
        private Point _correlation2DragStartLocal;
        private Point _correlation2DragCurrentLocal;

        // 区域关联分析2按钮点击事件
        private void _regionalCorrelationAnalysis2ButtonClick(object sender, EventArgs e)
        {
            if (_isRegionCorrelation2Analyzing == false)
            {
                _resetAllButton();
                _isRegionCorrelation2Analyzing = true;
                InitializeSingleRegionSelection(
                    _correlation2RegionOverlay,
                    _mapCorrelation2RegionMouseDown,
                    _mapCorrelation2RegionMouseMove,
                    _mapCorrelation2RegionMouseUp);
                _regionalCorrelationAnalysis2Button.Text = "区域选择中...";
                BindBottomButtonToAnalysis(
                    () => _analyze2RegionCorrelation(_correlation2RegionPoints, leftSidebar.StartDateString, leftSidebar.EndDateString),
                    () => _correlation2RegionPoints.Count >= 4,
                    CleanupRegionCorrelation2);
                sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                _resetCorrelationAnalysis2Button();
                sidebarController?.Hide();
            }
        }
        // 重置区域关联分析2按钮到初始状态（供其他按钮调用）
        private void _resetCorrelationAnalysis2Button()
        {
            _isRegionCorrelation2Analyzing = false;
            _isRegionCorrelation2Dragging = false;
            _correlation2RegionPoints.Clear();
            ResetSingleRegionSelection(
                _correlation2RegionOverlay,
                _mapCorrelation2RegionMouseDown,
                _mapCorrelation2RegionMouseMove,
                _mapCorrelation2RegionMouseUp);
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";
        }

        // 鼠标按下：开始拖拽
        private void _mapCorrelation2RegionMouseDown(object sender, MouseEventArgs e)
        {
            HandleSingleRegionMouseDown(
                _isRegionCorrelation2Analyzing,
                _isRegionCorrelation2Dragging,
                out _isRegionCorrelation2Dragging,
                ref _correlation2DragStartLocal,
                ref _correlation2DragCurrentLocal,
                _correlation2RegionOverlay,
                _correlation2RegionPoints,
                e);
        }

        // 鼠标移动：更新临时矩形显示
        private void _mapCorrelation2RegionMouseMove(object sender, MouseEventArgs e)
        {
            HandleSingleRegionMouseMove(
                _isRegionCorrelation2Analyzing,
                _isRegionCorrelation2Dragging,
                ref _correlation2DragCurrentLocal,
                _correlation2DragStartLocal,
                _correlation2RegionOverlay,
                Color.Purple,
                e);
        }

        // 鼠标抬起：结束拖拽，固定图形
        private void _mapCorrelation2RegionMouseUp(object sender, MouseEventArgs e)
        {
            if (HandleSingleRegionMouseUp(
                _isRegionCorrelation2Analyzing,
                _isRegionCorrelation2Dragging,
                _correlation2DragStartLocal,
                _correlation2DragCurrentLocal,
                out _isRegionCorrelation2Dragging,
                out _correlation2RegionPoints,
                e))
            {
                _regionalCorrelationAnalysis2Button.Text = GetSingleRegionButtonText(_isRegionCorrelation2Analyzing, "区域关联分析2", _correlation2RegionPoints);
            }
        }

        // 区域关联分析2占位函数（暂时为空实现）
        private void _analyze2RegionCorrelation(List<PointLatLng> region, string startTime, string endTime)
        {
            // TODO: 在这里实现区域关联分析2的具体逻辑（分析一个区域与其他区域的关联）
            // region 是一个区域的四个角点经纬度
            // startTime / endTime 为 "yyyy-MM-dd" 格式的时间范围
            // 示例：region[0].Lat / .Lng 可直接使用
        }
    }
}
