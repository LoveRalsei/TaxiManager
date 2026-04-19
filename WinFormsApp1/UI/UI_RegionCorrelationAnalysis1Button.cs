using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public partial class MapForm : Form
    {
        private bool _isRegionCorrelation1Analyzing = false;
        private bool _isRegionCorrelation1Dragging = false;
        private List<List<PointLatLng>> _correlation1RegionPoints = new List<List<PointLatLng>>();
        private GMapOverlay _correlation1RegionOverlay = new GMapOverlay("correlationPolygons");
        private Point _correlation1DragStartLocal;
        private Point _correlation1DragCurrentLocal;
        private readonly Color[] _correlation1RegionColors = new Color[] { Color.Green, Color.Orange };

        private void _regionalCorrelationAnalysis1ButtonClick(object sender, EventArgs e)
        {
            if (_isRegionCorrelation1Analyzing == false)
            {
                _resetAllButton();
                _isRegionCorrelation1Analyzing = true;
                InitializeMultiRegionSelection(
                    _correlation1RegionOverlay,
                    _correlation1RegionPoints,
                    _mapCorrelation1RegionMouseDown,
                    _mapCorrelation1RegionMouseMove,
                    _mapCorrelation1RegionMouseUp);
                _regionalCorrelationAnalysis1Button.Text = "选择区域中";
                BindBottomButtonToAnalysis(
                    () => _analyze1RegionCorrelation(_correlation1RegionPoints, leftSidebar.StartDateString, leftSidebar.EndDateString),
                    () => _correlation1RegionPoints.Count >= 2,
                    CleanupRegionCorrelation1);
                sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                _resetCorrelationAnalysis1Button();
                sidebarController?.Hide();
            }
        }

        private void _resetCorrelationAnalysis1Button()
        {
            _isRegionCorrelation1Analyzing = false;
            _isRegionCorrelation1Dragging = false;
            ResetMultiRegionSelection(
                _correlation1RegionOverlay,
                _correlation1RegionPoints,
                _mapCorrelation1RegionMouseDown,
                _mapCorrelation1RegionMouseMove,
                _mapCorrelation1RegionMouseUp);
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
        }

        private void _mapCorrelation1RegionMouseDown(object sender, MouseEventArgs e)
        {
            HandleMultiRegionMouseDown(
                _isRegionCorrelation1Analyzing,
                _correlation1RegionPoints.Count,
                2,
                _isRegionCorrelation1Dragging,
                out _isRegionCorrelation1Dragging,
                ref _correlation1DragStartLocal,
                ref _correlation1DragCurrentLocal,
                _correlation1RegionOverlay,
                _correlation1RegionPoints,
                _correlation1RegionColors,
                e);
        }

        private void _mapCorrelation1RegionMouseMove(object sender, MouseEventArgs e)
        {
            HandleMultiRegionMouseMove(
                _isRegionCorrelation1Analyzing,
                _isRegionCorrelation1Dragging,
                ref _correlation1DragCurrentLocal,
                _correlation1DragStartLocal,
                _correlation1RegionOverlay,
                _correlation1RegionPoints,
                _correlation1RegionColors,
                e);
        }

        private void _mapCorrelation1RegionMouseUp(object sender, MouseEventArgs e)
        {
            if (HandleMultiRegionMouseUp(
                _isRegionCorrelation1Analyzing,
                _isRegionCorrelation1Dragging,
                _correlation1DragStartLocal,
                _correlation1DragCurrentLocal,
                _correlation1RegionOverlay,
                _correlation1RegionPoints,
                _correlation1RegionColors,
                2,
                out _isRegionCorrelation1Analyzing,
                out _isRegionCorrelation1Dragging,
                e))
            {
                if (_isRegionCorrelation1Analyzing)
                {
                    _regionalCorrelationAnalysis1Button.Text = "选择区域2/2...";
                }
                else
                {
                    _regionalCorrelationAnalysis1Button.Text = "已选2个区域";
                    StopMultiRegionSelection(
                        _mapCorrelation1RegionMouseDown,
                        _mapCorrelation1RegionMouseMove,
                        _mapCorrelation1RegionMouseUp);
                }
            }
        }

        private void _analyze1RegionCorrelation(List<List<PointLatLng>> regions, string startTime, string endTime)
        {
            // TODO: 在这里实现区域关联分析的具体逻辑
            // regions[0] 和 regions[1] 分别是两个区域的四个角点经纬度
            // startTime / endTime 为 "yyyy-MM-dd" 格式的时间范围
            // 示例：regions[0][0].Lat / .Lng 可直接使用
        }
    }
}
