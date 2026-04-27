using GMap.NET.WindowsForms;

namespace TaxiManager
{
    public class UI_Button
    {
        protected GMapControl _gmap;
        protected MapForm _mapForm;

        public UI_Button(GMapControl gmap, MapForm mapForm)
        {
            _gmap = gmap;
            _mapForm = mapForm;
        }

        public UI_Button(GMapControl gmap)
        {
            _gmap = gmap;
        }

        public virtual void Initialize()
        {
        }
        // 绑定底部按钮的分析事件，通常在用户点击某个功能按钮后调用
        public void BindBottomButtonToAnalysis(Action analyzeAction, Func<bool> hasEnoughRegions, Action cleanupAction)
        {
            try { _mapForm.leftSidebar.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }

            _mapForm._bottomButtonAnalyzeHandler = (s, e) =>
            {
                _mapForm.leftSidebar.ErrorMessageVisible = false;

                if (hasEnoughRegions())
                {
                    analyzeAction();
                    cleanupAction();
                    _mapForm.sidebarController?.Hide();

                    // 分析完成后解绑
                    try { _mapForm.leftSidebar.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
                }
                else
                {
                    _mapForm.leftSidebar.ErrorMessageVisible = true;
                }
            };

            _mapForm.leftSidebar.BottomButton.Click += _mapForm._bottomButtonAnalyzeHandler;
        }
        // 解绑底部按钮的分析事件，通常在分析完成或取消分析时调用
        public void UnbindBottomButtonAnalysis()
        {
            try { _mapForm.leftSidebar.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
            _mapForm._bottomButtonAnalyzeHandler = null;
            if (_mapForm.leftSidebar != null)
                _mapForm.leftSidebar.ErrorMessageVisible = false;
        }
    }
}
