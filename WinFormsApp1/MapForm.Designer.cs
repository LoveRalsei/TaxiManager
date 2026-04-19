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
        private Button _enlargeButton;
        private Button _shrinkButton;
        private Button _regionSreachButton;
        private Button _regionalCorrelationAnalysis1Button;
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _enlargeButton = new Button();
            _shrinkButton = new Button();
            _regionSreachButton = new Button();
            _regionalCorrelationAnalysis1Button = new Button();
            _regionalCorrelationAnalysis2Button = new Button();
            _frequentPathAnalysis2Button = new Button();
            SuspendLayout();
            // 
            // _enlargeButton
            // 
            _enlargeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _enlargeButton.BackColor = SystemColors.ButtonHighlight;
            _enlargeButton.FlatAppearance.BorderColor = Color.White;
            _enlargeButton.FlatAppearance.BorderSize = 0;
            _enlargeButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            _enlargeButton.Location = new Point(847, 418);
            _enlargeButton.Name = "_enlargeButton";
            _enlargeButton.Size = new Size(40, 40);
            _enlargeButton.TabIndex = 0;
            _enlargeButton.Text = "+";
            _enlargeButton.UseVisualStyleBackColor = false;
            _enlargeButton.Click += _enlargeButtonClick;
            // 
            // _shrinkButton
            // 
            _shrinkButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _shrinkButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            _shrinkButton.Location = new Point(847, 464);
            _shrinkButton.Name = "_shrinkButton";
            _shrinkButton.Size = new Size(40, 40);
            _shrinkButton.TabIndex = 1;
            _shrinkButton.Text = "-";
            _shrinkButton.UseVisualStyleBackColor = true;
            _shrinkButton.Click += _shrinkButtonClick;
            // 
            // _regionSreachButton
            // 
            _regionSreachButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionSreachButton.Location = new Point(720, 464);
            _regionSreachButton.Name = "_regionSreachButton";
            _regionSreachButton.Size = new Size(121, 40);
            _regionSreachButton.TabIndex = 2;
            _regionSreachButton.Text = "区域范围查找";
            _regionSreachButton.UseVisualStyleBackColor = true;
            _regionSreachButton.Click += _regionSreachButtonClick;
            // 
            // _regionalCorrelationAnalysis1Button
            // 
            _regionalCorrelationAnalysis1Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionalCorrelationAnalysis1Button.Location = new Point(593, 464);
            _regionalCorrelationAnalysis1Button.Name = "_regionalCorrelationAnalysis1Button";
            _regionalCorrelationAnalysis1Button.Size = new Size(121, 40);
            _regionalCorrelationAnalysis1Button.TabIndex = 3;
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
            _regionalCorrelationAnalysis1Button.UseVisualStyleBackColor = true;
            _regionalCorrelationAnalysis1Button.Click += _regionalCorrelationAnalysis1ButtonClick;
            // 
            // _regionalCorrelationAnalysis2Button
            // 
            _regionalCorrelationAnalysis2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _regionalCorrelationAnalysis2Button.Location = new Point(466, 464);
            _regionalCorrelationAnalysis2Button.Name = "_regionalCorrelationAnalysis2Button";
            _regionalCorrelationAnalysis2Button.Size = new Size(121, 40);
            _regionalCorrelationAnalysis2Button.TabIndex = 4;
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";
            _regionalCorrelationAnalysis2Button.UseVisualStyleBackColor = true;
            _regionalCorrelationAnalysis2Button.Click += _regionalCorrelationAnalysis2ButtonClick;
            // 
            // _frequentPathAnalysis2Button
            // 
            _frequentPathAnalysis2Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _frequentPathAnalysis2Button.Location = new Point(339, 464);
            _frequentPathAnalysis2Button.Name = "_frequentPathAnalysis2Button";
            _frequentPathAnalysis2Button.Size = new Size(121, 40);
            _frequentPathAnalysis2Button.TabIndex = 5;
            _frequentPathAnalysis2Button.Text = "频繁路径分析2";
            _frequentPathAnalysis2Button.UseVisualStyleBackColor = true;
            _frequentPathAnalysis2Button.Click += _frequentPathAnalysis2ButtonClick;
            // 
            // MapForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 545);
            Controls.Add(_frequentPathAnalysis2Button);
            Controls.Add(_regionalCorrelationAnalysis2Button);
            Controls.Add(_enlargeButton);
            Controls.Add(_shrinkButton);
            Controls.Add(_regionSreachButton);
            Controls.Add(_regionalCorrelationAnalysis1Button);
            Name = "MapForm";
            Text = "TaxiMap";
            ResumeLayout(false);
        }

        #endregion

        private Button _regionalCorrelationAnalysis2Button;
        private Button _frequentPathAnalysis2Button;
    }
}
