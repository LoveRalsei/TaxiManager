using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaxiManager
{
    public partial class UI_RegionSreachButton: UI_HaveChooseTimePeriodLeftSidebarButton
    {
        private Button _regionSreachButton;

        private bool _isRegionSearching = false;
        private bool _isRegionDragging = false;
        private List<PointLatLng> _regionSearchPoints = new List<PointLatLng>();
        private GMapOverlay _regionSearchOverlay = new GMapOverlay("polygons");
        private Point _regionDragStartLocal;
        private Point _regionDragCurrentLocal;

        public UI_RegionSreachButton(GMapControl gmap, MapForm mapForm) : base(gmap,mapForm)
        {
            _mapForm = mapForm;
        }

        public override void Initialize()
        {
            _regionSreachButton = new Button();

            _regionSreachButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionSreachButton.Location = new Point(720, 464);
            _regionSreachButton.Name = "_regionSreachButton";
            _regionSreachButton.Size = new Size(121, 40);
            _regionSreachButton.TabIndex = 2;
            _regionSreachButton.Text = "区域范围查找";
            _regionSreachButton.UseVisualStyleBackColor = true;
            _regionSreachButton.Click += _regionSreachButtonClick;

            _mapForm.Controls.Add(_regionSreachButton);
        }

        private void _regionSreachButtonClick(object sender, EventArgs e)
        {
            if (_isRegionSearching == false)
            {
                _mapForm._resetAllButton();
                _isRegionSearching = true;
                SelectRegion.InitializeSingleRegionSelection(
                    _regionSearchOverlay,
                    _mapRegionMouseDown,
                    _mapRegionMouseMove,
                    _mapRegionMouseUp);
                _regionSreachButton.Text = "区域选择中...";
                BindBottomButtonToAnalysis(
                    () => _analyzeRegion(_regionSearchPoints),
                    () => _regionSearchPoints.Count >= 1,
                    CleanupRegionSearch);
                _sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                ResetRegionSearchButton();
                _sidebarController?.Hide();
            }
        }

        private void CleanupRegionSearch()
        {
            _isRegionSearching = false;
            _isRegionDragging = false;
            _regionSearchPoints.Clear();
            try { _gmap.MouseDown -= _mapRegionMouseDown; } catch { }
            try { _gmap.MouseMove -= _mapRegionMouseMove; } catch { }
            try { _gmap.MouseUp -= _mapRegionMouseUp; } catch { }
            _regionSreachButton.Text = "区域范围查找";
        }

        public void ResetRegionSearchButton()
        {
            _isRegionSearching = false;
            _isRegionDragging = false;
            _regionSearchPoints.Clear();
            SelectRegion.ResetSingleRegionSelection(
                _regionSearchOverlay,
                _mapRegionMouseDown,
                _mapRegionMouseMove,
                _mapRegionMouseUp);
            _regionSreachButton.Text = "区域范围查找";

            HideSidebar();
        }

        private void _mapRegionMouseDown(object sender, MouseEventArgs e)
        {
            SelectRegion.HandleSingleRegionMouseDown(
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
            SelectRegion.HandleSingleRegionMouseMove(
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
            if (SelectRegion.HandleSingleRegionMouseUp(
                _isRegionSearching,
                _isRegionDragging,
                _regionDragStartLocal,
                _regionDragCurrentLocal,
                out _isRegionDragging,
                out _regionSearchPoints,
                e))
            {
                _regionSreachButton.Text = SelectRegion.GetSingleRegionButtonText(_isRegionSearching, "区域范围查找", _regionSearchPoints);
            }
        }

        private void _analyzeRegion(List<PointLatLng> polygonCorners)
        {
            // TODO: 在这里用 polygonCorners（四个角点经纬度）和时间范围做后续处理或过滤数据
            // 示例：polygonCorners[0].Lat / .Lng 可直接使用
            // 示例时间格式： startTime / endTime 为 "yyyy-MM-dd"
            // 示例
            // 从4个角点中提取最小和最大经纬度
            double minLng = polygonCorners.Min(p => p.Lng);
            double maxLng = polygonCorners.Max(p => p.Lng);
            double minLat = polygonCorners.Min(p => p.Lat);
            double maxLat = polygonCorners.Max(p => p.Lat);

            // 转换为 Position（内部单位：经纬度 × 1e5）
            Position minPos = Position.FromGmap(new PointLatLng(minLat, minLng));
            Position maxPos = Position.FromGmap(new PointLatLng(maxLat, maxLng));

            // 构建 PositionRange
            PositionRange range = PositionRange.FromUnsort(minPos, maxPos);

            // 调用 CountDrivers
            uint count = ((IServiceF3)ServiceF3.Instance).CountDrivers(
                range,
                _leftSidebar_ChooseTimePeriod._datePicker2.Value,
                _leftSidebar_ChooseTimePeriod._datePicker1.Value
            );

            MessageBox.Show($"矩形区域内出租车数量：{count}","F3区域范围查找结果");
        }
    }
}
