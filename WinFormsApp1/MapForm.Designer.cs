using TaxiManager.BasicComponent.UI;
using TaxiManager.UI;

namespace TaxiManager.BasicComponent
{
    partial class MapForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        //控件声明
        private UIEnlargeZoomButton _enlargeButton;
        private UIShrinkZoomButton _shrinkButton;
        private UIShowTrackButton _showTrackButton;
        private UIRegionSreachButton _regionSreachButton;
        private UITrafficFlowAnalysisButton _trafficFlowAnalysisButton;
        private UI_RegionCorrelation1AnalyzingButton _regionalCorrelationAnalysis1Button;
        private UI_RegionCorrelation2AnalyzingButton _regionalCorrelationAnalysis2Button;
        private UIFrequentPathAnalysis2Button _frequentPathAnalysis2Button;

        private UIDebugShowGMapButton _debugShowGMapButton;


        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            InitialiazeCustomButtons();          

            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 545);

            Name = "MapForm";
            Text = "TaxiMap";
            ResumeLayout(false);


        }

        private void InitialiazeCustomButtons()
        {
            //初始化按钮
            _enlargeButton = new UIEnlargeZoomButton(GMap, this);
            _enlargeButton.Initialize();
            _shrinkButton = new UIShrinkZoomButton(GMap, this);
            _shrinkButton.Initialize();
            _showTrackButton = new UIShowTrackButton(GMap, this);
            _showTrackButton.Initialize();
            _regionSreachButton = new UIRegionSreachButton(GMap, this);
            _regionSreachButton.Initialize();
            _trafficFlowAnalysisButton=new UITrafficFlowAnalysisButton(GMap, this);
            _trafficFlowAnalysisButton.Initialize();
            //_regionalCorrelationAnalysis1Button = new UI_RegionCorrelation1AnalyzingButton(GMap, this);
            //_regionalCorrelationAnalysis1Button.Initialize();
            //_regionalCorrelationAnalysis2Button = new UI_RegionCorrelation2AnalyzingButton(GMap, this);
            //_regionalCorrelationAnalysis2Button.Initialize();
            //_frequentPathAnalysis2Button = new UIFrequentPathAnalysis2Button(GMap, this);
            //_frequentPathAnalysis2Button.Initialize();
#if DEBUG
            _debugShowGMapButton = new UIDebugShowGMapButton(GMap, this);
            _debugShowGMapButton.Initialize();
#endif
        }

        #endregion
        public void _resetAllButton()
        {
            // 解绑底部按钮的分析函数
            _regionSreachButton.ResetRegionSearchButton();
            //_regionalCorrelationAnalysis1Button.ResetCorrelationAnalysis1Button();
            //_regionalCorrelationAnalysis2Button.ResetCorrelationAnalysis2Button();
            //_frequentPathAnalysis2Button.ResetFrequentPathAnalysis2Button();
        }

    }
}
