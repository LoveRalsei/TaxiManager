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
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _enlargeButton = new Button();
            _shrinkButton = new Button();
            _regionSreachButton = new Button();
            label1 = new Label();
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
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(407, 270);
            label1.Name = "label1";
            label1.Size = new Size(53, 20);
            label1.TabIndex = 3;
            label1.Text = "label1";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MapForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 545);
            Controls.Add(label1);
            Controls.Add(_regionSreachButton);
            Controls.Add(_shrinkButton);
            Controls.Add(_enlargeButton);
            Name = "MapForm";
            Text = "TaxiMap";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
    }
}
