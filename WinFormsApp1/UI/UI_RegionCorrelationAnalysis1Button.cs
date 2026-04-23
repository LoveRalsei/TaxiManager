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
    public partial class UI_RegionCorrelation1AnalyzingButton : UI_Button
    {
        private Button _regionalCorrelationAnalysis1Button;

        private bool _isRegionCorrelation1Analyzing = false;
        private bool _isRegionCorrelation1Dragging = false;
        private List<List<PointLatLng>> _correlation1RegionPoints = new List<List<PointLatLng>>();
        private GMapOverlay _correlation1RegionOverlay = new GMapOverlay("correlationPolygons");
        private Point _correlation1DragStartLocal;
        private Point _correlation1DragCurrentLocal;
        private readonly Color[] _correlation1RegionColors = new Color[] { Color.Green, Color.Orange };

        public UI_RegionCorrelation1AnalyzingButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _regionalCorrelationAnalysis1Button = new Button();

            _regionalCorrelationAnalysis1Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionalCorrelationAnalysis1Button.Location = new Point(593, 464);
            _regionalCorrelationAnalysis1Button.Name = "_regionalCorrelationAnalysis1Button";
            _regionalCorrelationAnalysis1Button.Size = new Size(121, 40);
            _regionalCorrelationAnalysis1Button.TabIndex = 3;
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
            _regionalCorrelationAnalysis1Button.UseVisualStyleBackColor = true;
            _regionalCorrelationAnalysis1Button.Click += _regionalCorrelationAnalysis1ButtonClick;

            _mapForm.Controls.Add(_regionalCorrelationAnalysis1Button);
            _regionalCorrelationAnalysis1Button.BringToFront();
        }

        private void _regionalCorrelationAnalysis1ButtonClick(object sender, EventArgs e)
        {
            if (_isRegionCorrelation1Analyzing == false)
            {
                _mapForm._resetAllButton();
                _isRegionCorrelation1Analyzing = true;
                SelectRegion.InitializeMultiRegionSelection(
                    _correlation1RegionOverlay,
                    _correlation1RegionPoints,
                    _mapCorrelation1RegionMouseDown,
                    _mapCorrelation1RegionMouseMove,
                    _mapCorrelation1RegionMouseUp);
                _regionalCorrelationAnalysis1Button.Text = "选择区域中";
                BindBottomButtonToAnalysis(
                    () => _analyze1RegionCorrelation(_correlation1RegionPoints, _mapForm.leftSidebar.StartDateString, _mapForm.leftSidebar.EndDateString),
                    () => _correlation1RegionPoints.Count >= 2,
                    CleanupRegionCorrelation1);
                _mapForm.sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                _resetCorrelationAnalysis1Button();
                _mapForm.sidebarController?.Hide();
            }
        }

        private void CleanupRegionCorrelation1()
        {
            _isRegionCorrelation1Analyzing = false;
            _isRegionCorrelation1Dragging = false;
            _correlation1RegionPoints.Clear();
            try { _gmap.MouseDown -= _mapCorrelation1RegionMouseDown; } catch { }
            try { _gmap.MouseMove -= _mapCorrelation1RegionMouseMove; } catch { }
            try { _gmap.MouseUp -= _mapCorrelation1RegionMouseUp; } catch { }
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
        }

        public void _resetCorrelationAnalysis1Button()
        {
            _isRegionCorrelation1Analyzing = false;
            _isRegionCorrelation1Dragging = false;
            SelectRegion.ResetMultiRegionSelection(
                _correlation1RegionOverlay,
                _correlation1RegionPoints,
                _mapCorrelation1RegionMouseDown,
                _mapCorrelation1RegionMouseMove,
                _mapCorrelation1RegionMouseUp);
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
        }

        private void _mapCorrelation1RegionMouseDown(object sender, MouseEventArgs e)
        {
            SelectRegion.HandleMultiRegionMouseDown(
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
            SelectRegion.HandleMultiRegionMouseMove(
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
            if (SelectRegion.HandleMultiRegionMouseUp(
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
                    SelectRegion.StopMultiRegionSelection(
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
