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
    public partial class UI_FrequentPathAnalysis2Button: UI_HaveChooseTimePeriodLeftSidebarButton
    {
        private Button _frequentPathAnalysis2Button;

        // 频繁路径分析2相关的成员变量
        private bool _isFrequentPath2Analyzing = false;
        private bool _isFrequentPath2Dragging = false;
        private List<List<PointLatLng>> _frequentPath2RegionPoints = new List<List<PointLatLng>>();
        private GMapOverlay _frequentPath2RegionOverlay = new GMapOverlay("frequentPath2Polygons");
        private Point _frequentPath2DragStartLocal;
        private Point _frequentPath2DragCurrentLocal;
        private readonly Color[] _frequentPath2RegionColors = new Color[] { Color.Green, Color.Orange };

        public UI_FrequentPathAnalysis2Button(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _frequentPathAnalysis2Button = new Button();

            _frequentPathAnalysis2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _frequentPathAnalysis2Button.Location = new Point(339, 464);
            _frequentPathAnalysis2Button.Name = "_frequentPathAnalysis2Button";
            _frequentPathAnalysis2Button.Size = new Size(121, 40);
            _frequentPathAnalysis2Button.TabIndex = 5;
            _frequentPathAnalysis2Button.Text = "频繁路径分析2";
            _frequentPathAnalysis2Button.UseVisualStyleBackColor = true;
            _frequentPathAnalysis2Button.Click += _frequentPathAnalysis2ButtonClick;

            _mapForm.Controls.Add(_frequentPathAnalysis2Button);
        }

        // 频繁路径分析2按钮点击事件
        private void _frequentPathAnalysis2ButtonClick(object sender, EventArgs e)
        {
            if (_isFrequentPath2Analyzing == false)
            {
                _mapForm._resetAllButton();
                _isFrequentPath2Analyzing = true;
                SelectRegion.InitializeMultiRegionSelection(
                    _frequentPath2RegionOverlay,
                    _frequentPath2RegionPoints,
                    _mapFrequentPath2RegionMouseDown,
                    _mapFrequentPath2RegionMouseMove,
                    _mapFrequentPath2RegionMouseUp);
                _frequentPathAnalysis2Button.Text = "选择区域中";
                BindBottomButtonToAnalysis(
                    () => _analyze2FrequentPath(_frequentPath2RegionPoints, _leftSidebar_ChooseTimePeriod.StartDateString, _leftSidebar_ChooseTimePeriod.EndDateString),
                    () => _frequentPath2RegionPoints.Count >= 2,
                    CleanupFrequentPath2);
                _sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                ResetFrequentPathAnalysis2Button();
                _sidebarController?.Hide();
            }
        }

        private void CleanupFrequentPath2()
        {
            _isFrequentPath2Analyzing = false;
            _isFrequentPath2Dragging = false;
            _frequentPath2RegionPoints.Clear();
            try { _gmap.MouseDown -= _mapFrequentPath2RegionMouseDown; } catch { }
            try { _gmap.MouseMove -= _mapFrequentPath2RegionMouseMove; } catch { }
            try { _gmap.MouseUp -= _mapFrequentPath2RegionMouseUp; } catch { }
            _frequentPathAnalysis2Button.Text = "频繁路径分析2";
        }

        // 重置频繁路径分析2按钮到初始状态
        public void ResetFrequentPathAnalysis2Button()
        {
            _isFrequentPath2Analyzing = false;
            _isFrequentPath2Dragging = false;
            _frequentPath2RegionPoints.Clear();
            SelectRegion.ResetMultiRegionSelection(
                _frequentPath2RegionOverlay,
                _frequentPath2RegionPoints,
                _mapFrequentPath2RegionMouseDown,
                _mapFrequentPath2RegionMouseMove,
                _mapFrequentPath2RegionMouseUp);
            _frequentPathAnalysis2Button.Text = "频繁路径分析2";

            HideSidebar();
        }

        // 鼠标按下：开始拖拽
        private void _mapFrequentPath2RegionMouseDown(object sender, MouseEventArgs e)
        {
            SelectRegion.HandleMultiRegionMouseDown(
                _isFrequentPath2Analyzing,
                _frequentPath2RegionPoints.Count,
                2,
                _isFrequentPath2Dragging,
                out _isFrequentPath2Dragging,
                ref _frequentPath2DragStartLocal,
                ref _frequentPath2DragCurrentLocal,
                _frequentPath2RegionOverlay,
                _frequentPath2RegionPoints,
                _frequentPath2RegionColors,
                e);
        }

        // 鼠标移动：更新临时矩形显示
        private void _mapFrequentPath2RegionMouseMove(object sender, MouseEventArgs e)
        {
            SelectRegion.HandleMultiRegionMouseMove(
                _isFrequentPath2Analyzing,
                _isFrequentPath2Dragging,
                ref _frequentPath2DragCurrentLocal,
                _frequentPath2DragStartLocal,
                _frequentPath2RegionOverlay,
                _frequentPath2RegionPoints,
                _frequentPath2RegionColors,
                e);
        }

        // 鼠标抬起：结束拖拽，固定图形
        private void _mapFrequentPath2RegionMouseUp(object sender, MouseEventArgs e)
        {
            if (SelectRegion.HandleMultiRegionMouseUp(
                _isFrequentPath2Analyzing,
                _isFrequentPath2Dragging,
                _frequentPath2DragStartLocal,
                _frequentPath2DragCurrentLocal,
                _frequentPath2RegionOverlay,
                _frequentPath2RegionPoints,
                _frequentPath2RegionColors,
                2,
                out _isFrequentPath2Analyzing,
                out _isFrequentPath2Dragging,
                e))
            {
                if (_isFrequentPath2Analyzing)
                {
                    _frequentPathAnalysis2Button.Text = "选择区域2/2...";
                }
                else
                {
                    _frequentPathAnalysis2Button.Text = "已选2个区域";
                    SelectRegion.StopMultiRegionSelection(
                        _mapFrequentPath2RegionMouseDown,
                        _mapFrequentPath2RegionMouseMove,
                        _mapFrequentPath2RegionMouseUp);
                }
            }
        }

        // 频繁路径分析2占位函数（暂时为空实现）
        private void _analyze2FrequentPath(List<List<PointLatLng>> regions, string startTime, string endTime)
        {
            // TODO: 在这里实现频繁路径分析2的具体逻辑
            // regions[0] 和 regions[1] 分别是两个区域的四个角点经纬度
            // startTime / endTime 为 "yyyy-MM-dd" 格式的时间范围
            // 示例：regions[0][0].Lat / .Lng 可直接使用
        }
    }
}
