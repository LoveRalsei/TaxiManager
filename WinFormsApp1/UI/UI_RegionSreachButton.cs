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
        private bool _isRegionSearching = false;
        private bool _isRegionDragging = false;
        private List<PointLatLng> _regionSearchPoints = new List<PointLatLng>();
        private GMapOverlay _regionSearchOverlay = new GMapOverlay("polygons");
        private Point _regionDragStartLocal;
        private Point _regionDragCurrentLocal;

        private void _regionSreachButtonClick(object sender, EventArgs e)
        {
            if (_isRegionSearching == false)
            {
                _resetAllButton();
                _isRegionSearching = true;
                InitializeSingleRegionSelection(
                    _regionSearchOverlay,
                    _mapRegionMouseDown,
                    _mapRegionMouseMove,
                    _mapRegionMouseUp);
                _regionSreachButton.Text = "区域选择中...";
                BindBottomButtonToAnalysis(
                    () => _analyzeRegion(_regionSearchPoints, leftSidebar.StartDateString, leftSidebar.EndDateString),
                    () => _regionSearchPoints.Count >= 1,
                    CleanupRegionSearch);
                sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                _resetRegionSearchButton();
                sidebarController?.Hide();
            }
        }

        private void _resetRegionSearchButton()
        {
            _isRegionSearching = false;
            _isRegionDragging = false;
            _regionSearchPoints.Clear();
            ResetSingleRegionSelection(
                _regionSearchOverlay,
                _mapRegionMouseDown,
                _mapRegionMouseMove,
                _mapRegionMouseUp);
            _regionSreachButton.Text = "区域范围查找";
        }

        private void _mapRegionMouseDown(object sender, MouseEventArgs e)
        {
            HandleSingleRegionMouseDown(
                _isRegionSearching,
                _isRegionDragging,
                out _isRegionDragging,
                ref _regionDragStartLocal,
                ref _regionDragCurrentLocal,
                _regionSearchOverlay,
                _regionSearchPoints,
                e);
        }

        private void _mapRegionMouseMove(object sender, MouseEventArgs e)
        {
            HandleSingleRegionMouseMove(
                _isRegionSearching,
                _isRegionDragging,
                ref _regionDragCurrentLocal,
                _regionDragStartLocal,
                _regionSearchOverlay,
                Color.Blue,
                e);
        }

        private void _mapRegionMouseUp(object sender, MouseEventArgs e)
        {
            if (HandleSingleRegionMouseUp(
                _isRegionSearching,
                _isRegionDragging,
                _regionDragStartLocal,
                _regionDragCurrentLocal,
                out _isRegionDragging,
                out _regionSearchPoints,
                e))
            {
                _regionSreachButton.Text = GetSingleRegionButtonText(_isRegionSearching, "区域范围查找", _regionSearchPoints);
            }
        }

        private void _analyzeRegion(List<PointLatLng> polygonCorners, string startTime, string endTime)
        {
            // TODO: 在这里用 polygonCorners（四个角点经纬度）和时间范围做后续处理或过滤数据
            // 示例：polygonCorners[0].Lat / .Lng 可直接使用
            // 示例时间格式： startTime / endTime 为 "yyyy-MM-dd"
        }
    }
}
