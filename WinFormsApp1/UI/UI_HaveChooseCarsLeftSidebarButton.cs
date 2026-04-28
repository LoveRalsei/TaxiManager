using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class UI_HaveChooseCarsLeftSidebarButton : UI_Button
    {
        protected LeftSidebar_ChooseCars _leftSideBar_ChooseCars;
        protected SidebarController _sidebarController;

        public UI_HaveChooseCarsLeftSidebarButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
            _leftSideBar_ChooseCars = new LeftSidebar_ChooseCars();
            _leftSideBar_ChooseCars.Title = "选择汽车ID";

            _sidebarController = new SidebarController(_mapForm, _leftSideBar_ChooseCars, expandedWidth: 280);
            _mapForm.FormClosed += (s, e) => _sidebarController.Dispose();
        }

        // 绑定底部按钮的分析事件，通常在用户点击某个功能按钮后调用
        public void BindBottomButtonToAnalysis(Action analyzeAction, Func<bool> hasEnoughRegions, Action cleanupAction)
        {
            try { _leftSideBar_ChooseCars.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }

            _mapForm._bottomButtonAnalyzeHandler = (s, e) =>
            {
                //_leftSideBar_ChooseCars.ErrorMessageVisible = false;

                if (hasEnoughRegions())
                {
                    analyzeAction();
                    cleanupAction();
                    _sidebarController?.Hide();

                    // 分析完成后解绑
                    try { _leftSideBar_ChooseCars.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
                }
                else
                {
                    //_leftSideBar_ChooseCars.ErrorMessageVisible = true;
                }
            };

            _leftSideBar_ChooseCars.BottomButton.Click += _mapForm._bottomButtonAnalyzeHandler;
        }
        // 解绑底部按钮的分析事件，通常在分析完成或取消分析时调用
        public void UnbindBottomButtonAnalysis()
        {
            try { _leftSideBar_ChooseCars.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
            _mapForm._bottomButtonAnalyzeHandler = null;
            //if (_leftSideBar_ChooseCars != null)
                //_leftSideBar_ChooseCars.ErrorMessageVisible = false;
        }

        public void HideSidebar()
        {
            _sidebarController.Hide();
        }
    }
}
