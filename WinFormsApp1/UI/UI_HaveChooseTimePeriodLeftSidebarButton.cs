using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class UI_HaveChooseTimePeriodLeftSidebarButton : UI_Button
    {
        protected LeftSidebar_ChooseTimePeriod _leftSidebar_ChooseTimePeriod;
        protected SidebarController _sidebarController;

        public UI_HaveChooseTimePeriodLeftSidebarButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
            _leftSidebar_ChooseTimePeriod=new LeftSidebar_ChooseTimePeriod();
            _leftSidebar_ChooseTimePeriod.Title = "选择日期";

            _sidebarController = new SidebarController(_mapForm, _leftSidebar_ChooseTimePeriod, expandedWidth: 280);
            _mapForm.FormClosed += (s, e) => _sidebarController.Dispose();
        }

        // 绑定底部按钮的分析事件，通常在用户点击某个功能按钮后调用
        public void BindBottomButtonToAnalysis(Action analyzeAction, Func<bool> hasEnoughRegions, Action cleanupAction)
        {
            try { _leftSidebar_ChooseTimePeriod.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }

            _mapForm._bottomButtonAnalyzeHandler = (s, e) =>
            {
                _leftSidebar_ChooseTimePeriod.ErrorMessageVisible = false;

                if (hasEnoughRegions())
                {
                    analyzeAction();
                    cleanupAction();
                    _sidebarController?.Hide();

                    // 分析完成后解绑
                    try { _leftSidebar_ChooseTimePeriod.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
                }
                else
                {
                    _leftSidebar_ChooseTimePeriod.ErrorMessageVisible = true;
                }
            };

            _leftSidebar_ChooseTimePeriod.BottomButton.Click += _mapForm._bottomButtonAnalyzeHandler;
        }
        // 解绑底部按钮的分析事件，通常在分析完成或取消分析时调用
        public void UnbindBottomButtonAnalysis()
        {
            try { _leftSidebar_ChooseTimePeriod.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
            _mapForm._bottomButtonAnalyzeHandler = null;
            if (_leftSidebar_ChooseTimePeriod != null)
                _leftSidebar_ChooseTimePeriod.ErrorMessageVisible = false;
        }

        public void HideSidebar()
        {
            _sidebarController.Hide();
        }
    }
}
