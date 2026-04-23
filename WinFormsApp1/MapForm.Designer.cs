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
        private UI_EnlargeZoomButton _enlargeButton;
        private UI_ShrinkZoomButton _shrinkButton;
        private UI_RegionSreachButton _regionSreachButton;
        private UI_RegionCorrelation1AnalyzingButton _regionalCorrelationAnalysis1Button;
        private UI_RegionCorrelation2AnalyzingButton _regionalCorrelationAnalysis2Button;
        private UI_FrequentPathAnalysis2Button _frequentPathAnalysis2Button;

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            _enlargeButton=new UI_EnlargeZoomButton(gmap, this);
            _enlargeButton.Initialize();
            _shrinkButton=new UI_ShrinkZoomButton(gmap, this);
            _shrinkButton.Initialize();
            _regionSreachButton=new UI_RegionSreachButton(gmap, this);
            _regionSreachButton.Initialize();
            _regionalCorrelationAnalysis1Button=new UI_RegionCorrelation1AnalyzingButton(gmap, this);
            _regionalCorrelationAnalysis1Button.Initialize();
            _regionalCorrelationAnalysis2Button=new UI_RegionCorrelation2AnalyzingButton(gmap, this);
            _regionalCorrelationAnalysis2Button.Initialize();
            _frequentPathAnalysis2Button=new UI_FrequentPathAnalysis2Button(gmap, this);
            _frequentPathAnalysis2Button.Initialize();
            // 
            // MapForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 545);
            
            Name = "MapForm";
            Text = "TaxiMap";
            ResumeLayout(false);

            
        }

        #endregion
        public void _resetAllButton()
        {
            // 解绑底部按钮的分析函数
            _enlargeButton.UnbindBottomButtonAnalysis();

            _regionSreachButton._resetRegionSearchButton();
            _regionalCorrelationAnalysis1Button._resetCorrelationAnalysis1Button();
            _regionalCorrelationAnalysis2Button._resetCorrelationAnalysis2Button();
            _frequentPathAnalysis2Button._resetFrequentPathAnalysis2Button();

            try
            {
                sidebarController?.Hide();
            }
            catch { }
        }

    }
}
