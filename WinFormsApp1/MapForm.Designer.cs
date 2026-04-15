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

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            enlargeButton = new Button();
            shrinkButton = new Button();
            regionSreachButton = new Button();
            SuspendLayout();
            // 
            // enlargeButton
            // 
            enlargeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            enlargeButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            enlargeButton.Location = new Point(12, 464);
            enlargeButton.Name = "enlargeButton";
            enlargeButton.Size = new Size(40, 40);
            enlargeButton.TabIndex = 0;
            enlargeButton.Text = "+";
            enlargeButton.UseVisualStyleBackColor = true;
            enlargeButton.Click += enlargeButtonClick;
            // 
            // shrinkButton
            // 
            shrinkButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            shrinkButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            shrinkButton.Location = new Point(58, 464);
            shrinkButton.Name = "shrinkButton";
            shrinkButton.Size = new Size(40, 40);
            shrinkButton.TabIndex = 1;
            shrinkButton.Text = "-";
            shrinkButton.UseVisualStyleBackColor = true;
            shrinkButton.Click += shrinkButtonClick;
            // 
            // button1
            // 
            regionSreachButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            regionSreachButton.Location = new Point(104, 464);
            regionSreachButton.Name = "button1";
            regionSreachButton.Size = new Size(121, 40);
            regionSreachButton.TabIndex = 2;
            regionSreachButton.Text = "区域范围查找";
            regionSreachButton.UseVisualStyleBackColor = true;
            regionSreachButton.Click += regionSreachButtonClick;
            // 
            // MapForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 545);
            Controls.Add(regionSreachButton);
            Controls.Add(shrinkButton);
            Controls.Add(enlargeButton);
            Name = "MapForm";
            Text = "TaxiMap";
            ResumeLayout(false);
        }

        #endregion
        //放大缩小按钮
        private Button enlargeButton;
        private Button shrinkButton;
        private Button regionSreachButton;
    }
}
