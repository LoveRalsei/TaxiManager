namespace TaxiManager
{
    public partial class MapForm : Form
    {
        private void _resetAllButton()
        {
            // 解绑底部按钮的分析函数
            UnbindBottomButtonAnalysis();

            _resetRegionSearchButton();
            _resetCorrelationAnalysis1Button();
            _resetCorrelationAnalysis2Button();
            _resetFrequentPathAnalysis2Button();

            try
            {
                sidebarController?.Hide();
            }
            catch { }
        }
    }
}
