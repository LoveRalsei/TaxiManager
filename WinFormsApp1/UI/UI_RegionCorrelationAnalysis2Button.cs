using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaxiManager
{
    public class UI_RegionCorrelation2AnalyzingButton : UI_HaveChooseTimePeriodLeftSidebarButton
    {
        private Button _regionalCorrelationAnalysis2Button;

        // 区域关联分析2相关的成员变量
        private bool _isRegionCorrelation2Analyzing = false;
        private bool _isRegionCorrelation2Dragging = false;
        private List<PointLatLng> _correlation2RegionPoints = new List<PointLatLng>(); // 存储一个区域的点
        private GMapOverlay _correlation2RegionOverlay = new GMapOverlay("correlation2Polygons");
        private Point _correlation2DragStartLocal;
        private Point _correlation2DragCurrentLocal;

        public UI_RegionCorrelation2AnalyzingButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _regionalCorrelationAnalysis2Button = new Button();

            _regionalCorrelationAnalysis2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionalCorrelationAnalysis2Button.Location = new Point(466, 464);
            _regionalCorrelationAnalysis2Button.Name = "_regionalCorrelationAnalysis2Button";
            _regionalCorrelationAnalysis2Button.Size = new Size(121, 40);
            _regionalCorrelationAnalysis2Button.TabIndex = 4;
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";
            _regionalCorrelationAnalysis2Button.UseVisualStyleBackColor = true;
            _regionalCorrelationAnalysis2Button.Click += _regionalCorrelationAnalysis2ButtonClick;

            _mapForm.Controls.Add(_regionalCorrelationAnalysis2Button);
        }

        // 区域关联分析2按钮点击事件
        private void _regionalCorrelationAnalysis2ButtonClick(object sender, EventArgs e)
        {
            if (_isRegionCorrelation2Analyzing == false)
            {
                _mapForm._resetAllButton();
                _isRegionCorrelation2Analyzing = true;
                SelectRegion.InitializeSingleRegionSelection(
                    _correlation2RegionOverlay,
                    _mapCorrelation2RegionMouseDown,
                    _mapCorrelation2RegionMouseMove,
                    _mapCorrelation2RegionMouseUp);
                _regionalCorrelationAnalysis2Button.Text = "区域选择中...";
                BindBottomButtonToAnalysis(
                    () => _analyze2RegionCorrelation(_correlation2RegionPoints, _leftSidebar_ChooseTimePeriod.StartDateString, _leftSidebar_ChooseTimePeriod.EndDateString),
                    () => _correlation2RegionPoints.Count >= 4,
                    CleanupRegionCorrelation2);
                _sidebarController?.Show();
            }
            else
            {
                UnbindBottomButtonAnalysis();
                ResetCorrelationAnalysis2Button();
                _sidebarController?.Hide();
            }
        }

        private void CleanupRegionCorrelation2()
        {
            _isRegionCorrelation2Analyzing = false;
            _isRegionCorrelation2Dragging = false;
            _correlation2RegionPoints.Clear();
            try { _gmap.MouseDown -= _mapCorrelation2RegionMouseDown; } catch { }
            try { _gmap.MouseMove -= _mapCorrelation2RegionMouseMove; } catch { }
            try { _gmap.MouseUp -= _mapCorrelation2RegionMouseUp; } catch { }
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";
        }

        // 重置区域关联分析2按钮到初始状态（供其他按钮调用）
        public void ResetCorrelationAnalysis2Button()
        {
            _isRegionCorrelation2Analyzing = false;
            _isRegionCorrelation2Dragging = false;
            _correlation2RegionPoints.Clear();
            SelectRegion.ResetSingleRegionSelection(
                _correlation2RegionOverlay,
                _mapCorrelation2RegionMouseDown,
                _mapCorrelation2RegionMouseMove,
                _mapCorrelation2RegionMouseUp);
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";

            HideSidebar();
        }

        // 鼠标按下：开始拖拽
        private void _mapCorrelation2RegionMouseDown(object sender, MouseEventArgs e)
        {
            SelectRegion.HandleSingleRegionMouseDown(
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
            SelectRegion.HandleSingleRegionMouseMove(
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
            if (SelectRegion.HandleSingleRegionMouseUp(
                _isRegionCorrelation2Analyzing,
                _isRegionCorrelation2Dragging,
                _correlation2DragStartLocal,
                _correlation2DragCurrentLocal,
                out _isRegionCorrelation2Dragging,
                out _correlation2RegionPoints,
                e))
            {
                _regionalCorrelationAnalysis2Button.Text = SelectRegion.GetSingleRegionButtonText(_isRegionCorrelation2Analyzing, "区域关联分析2", _correlation2RegionPoints);
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
