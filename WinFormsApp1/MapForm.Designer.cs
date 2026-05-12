using TaxiManager.BasicComponent;
using TaxiManager.UI;

namespace TaxiManager
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
        private UIRegionCorrelation1AnalyzingButton _regionCorrelationAnalysis1Button;
        private UIRegionCorrelation2AnalyzingButton _regionCorrelationAnalysis2Button;
        private UIFrequentPathAnalysis1Button _frequentPathAnalysis1Button;
        private UIFrequentPathAnalysis2Button _frequentPathAnalysis2Button;
        private UICommunicationTimeAnalysisButton _communicationTimeAnalysisButton;

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
            _regionCorrelationAnalysis1Button = new UIRegionCorrelation1AnalyzingButton(GMap, this);
            _regionCorrelationAnalysis1Button.Initialize();
            _regionCorrelationAnalysis2Button = new UIRegionCorrelation2AnalyzingButton(GMap, this);
            _regionCorrelationAnalysis2Button.Initialize();
            _frequentPathAnalysis1Button = new UIFrequentPathAnalysis1Button(GMap, this);
            _frequentPathAnalysis1Button.Initialize();
            _frequentPathAnalysis2Button = new UIFrequentPathAnalysis2Button(GMap, this);
            _frequentPathAnalysis2Button.Initialize();
            _communicationTimeAnalysisButton = new UICommunicationTimeAnalysisButton(GMap, this);
            _communicationTimeAnalysisButton.Initialize();
#if DEBUG
            _debugShowGMapButton = new UIDebugShowGMapButton(GMap, this);
            _debugShowGMapButton.Initialize();
#endif
        }

        #endregion
        public void ResetAllButton()
        {
            // 解绑底部按钮的分析函数
            _regionSreachButton.ResetRegionSearchButton();
            _trafficFlowAnalysisButton.ResetTrafficFlowAnalysisButton();
            _regionCorrelationAnalysis1Button.ResetCorrelationAnalysis1Button();
            _regionCorrelationAnalysis2Button.ResetCorrelationAnalysis2Button();
            _frequentPathAnalysis1Button.ResetFrequentPathAnalysis1Button();
            _frequentPathAnalysis2Button.ResetFrequentPathAnalysis2Button();
            _communicationTimeAnalysisButton.ResetCommunicationTimeAnalysisButton();
        }

    }
}
