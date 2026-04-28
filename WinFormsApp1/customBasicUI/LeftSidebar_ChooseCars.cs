using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager
{
    public class LeftSidebar_ChooseCars : LeftSidebar
    {
        private TextBox _inputBox;
        public int? _result {  get; set; }
        private string _originalText;
        private System.Windows.Forms.Timer _restoreTextTimer;

        public LeftSidebar_ChooseCars()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 查找内容面板
            Panel contentPanel = null;
            foreach (Control c in Controls)
            {
                if (c is Panel panel && c != _titleLabel)
                {
                    contentPanel = panel;
                    break;
                }
            }

            if (contentPanel == null) return;

            // 保存原始文本
            _text.Text = "";
            _originalText = _text.Text;

            // 创建输入框
            _inputBox = new TextBox
            {
                Width = 70,
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle
            };
            _inputBox.TextChanged += OnInputBoxTextChanged;

            // 初始化恢复文本的定时器
            _restoreTextTimer = new System.Windows.Forms.Timer();
            _restoreTextTimer.Interval = 2000; // 2秒
            _restoreTextTimer.Tick += (s, e) =>
            {
                _text.Text = _originalText;
                _restoreTextTimer.Stop();
            };

            // 设置_bottomButton的点击事件
            _bottomButton.Click += OnBottomButtonClick;

            // 将输入框插入到contentPanel中（在_text下方）
            int textIndex = contentPanel.Controls.IndexOf(_text);
            if (textIndex >= 0)
            {
                contentPanel.Controls.Add(_inputBox);
                contentPanel.Controls.SetChildIndex(_inputBox, textIndex + 1);
            }
        }

        private void OnInputBoxTextChanged(object sender, EventArgs e)
        {
            string text = _inputBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                _result = null;
                return;
            }

            // 尝试解析输入框中的整数
            if (!int.TryParse(text, out int num))
            {
                _result = null;
                return;
            }

            // 验证条件：大于0，小于10358
            if (num <= 0 || num >= 10358)
            {
                _result = null;
                return;
            }

            // 存储结果
            _result = num;
        }

        private void ShowError(string message)
        {
            _text.Text = message;
            _restoreTextTimer.Stop();
            _restoreTextTimer.Start();
        }

        private void OnBottomButtonClick(object sender, EventArgs e)
        {
            if (_result == null)
            {
                ShowError("请输入有效的整数");
                return;
            }
            PerformAnalysis(_result.Value);
        }

        /// <summary>
        /// 分析函数，传入存储的整数
        /// </summary>
        /// <param name="results">存储的整数</param>
        protected virtual void PerformAnalysis(int results)
        {
            // 暂时为空实现，子类可重写
        }

        [Category("Behavior")]
        public int? Results => _result;

        /// <summary>
        /// 清空已设置的整数
        /// </summary>
        public void ClearResults()
        {
            _result = null;
            _inputBox.Text = "";
        }
    }
}
