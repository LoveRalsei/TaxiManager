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

        public void UnbindBottomButtonAnalysis()
        {
            try { _mapForm.leftSidebar.BottomButton.Click -= _mapForm._bottomButtonAnalyzeHandler; } catch { }
            _mapForm._bottomButtonAnalyzeHandler = null;
            if (_mapForm.leftSidebar != null)
                _mapForm.leftSidebar.ErrorMessageVisible = false;
        }
    }
}
