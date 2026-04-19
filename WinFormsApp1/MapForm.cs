using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;

namespace TaxiManager
{
    public partial class MapForm : Form
    {

        private GMapControl gmap;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        // sidebar fields
        private LeftSidebar_ChooseTimePeriod? leftSidebar;
        private SidebarController? sidebarController;
        private EventHandler? _bottomButtonAnalyzeHandler;

        // 用于侧边栏底部按钮绑定分析函数的辅助方法
        private void BindBottomButtonToAnalysis(Action analyzeAction, Func<bool> hasEnoughRegions, Action cleanupAction)
        {
            // 先解绑之前的处理函数
            try { leftSidebar.BottomButton.Click -= _bottomButtonAnalyzeHandler; } catch { }

            _bottomButtonAnalyzeHandler = (s, e) =>
            {
                leftSidebar.ErrorMessageVisible = false;

                if (hasEnoughRegions())
                {
                    analyzeAction();
                    cleanupAction();
                    sidebarController?.Hide();

                    // 分析完成后解绑
                    try { leftSidebar.BottomButton.Click -= _bottomButtonAnalyzeHandler; } catch { }
                }
                else
                {
                    leftSidebar.ErrorMessageVisible = true;
                }
            };

            leftSidebar.BottomButton.Click += _bottomButtonAnalyzeHandler;
        }

        // 解绑底部按钮的分析函数
        private void UnbindBottomButtonAnalysis()
        {
            try { leftSidebar.BottomButton.Click -= _bottomButtonAnalyzeHandler; } catch { }
            _bottomButtonAnalyzeHandler = null;
            if (leftSidebar != null)
                leftSidebar.ErrorMessageVisible = false;
        }

        // 区域范围查找的清理方法
        private void CleanupRegionSearch()
        {
            _isRegionSearching = false;
            _isRegionDragging = false;
            _regionSearchPoints.Clear();
            try { gmap.MouseDown -= _mapRegionMouseDown; } catch { }
            try { gmap.MouseMove -= _mapRegionMouseMove; } catch { }
            try { gmap.MouseUp -= _mapRegionMouseUp; } catch { }
            _regionSreachButton.Text = "区域范围查找";
        }

        // 区域关联分析1的清理方法
        private void CleanupRegionCorrelation1()
        {
            _isRegionCorrelation1Analyzing = false;
            _isRegionCorrelation1Dragging = false;
            _correlation1RegionPoints.Clear();
            try { gmap.MouseDown -= _mapCorrelation1RegionMouseDown; } catch { }
            try { gmap.MouseMove -= _mapCorrelation1RegionMouseMove; } catch { }
            try { gmap.MouseUp -= _mapCorrelation1RegionMouseUp; } catch { }
            _regionalCorrelationAnalysis1Button.Text = "区域关联分析1";
        }

        // 区域关联分析2的清理方法
        private void CleanupRegionCorrelation2()
        {
            _isRegionCorrelation2Analyzing = false;
            _isRegionCorrelation2Dragging = false;
            _correlation2RegionPoints.Clear();
            try { gmap.MouseDown -= _mapCorrelation2RegionMouseDown; } catch { }
            try { gmap.MouseMove -= _mapCorrelation2RegionMouseMove; } catch { }
            try { gmap.MouseUp -= _mapCorrelation2RegionMouseUp; } catch { }
            _regionalCorrelationAnalysis2Button.Text = "区域关联分析2";
        }

        // 频繁路径分析2的清理方法
        private void CleanupFrequentPath2()
        {
            _isFrequentPath2Analyzing = false;
            _isFrequentPath2Dragging = false;
            _frequentPath2RegionPoints.Clear();
            try { gmap.MouseDown -= _mapFrequentPath2RegionMouseDown; } catch { }
            try { gmap.MouseMove -= _mapFrequentPath2RegionMouseMove; } catch { }
            try { gmap.MouseUp -= _mapFrequentPath2RegionMouseUp; } catch { }
            _frequentPathAnalysis2Button.Text = "频繁路径分析2";
        }

        public MapForm()
        {
            InitializeComponent();

            gmap = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = AMapProvider.Instance,
                Position = new PointLatLng(39.9042, 116.4074), // 北京坐标
                MinZoom = _oringinMinZoom,
                MaxZoom = _oringinMaxZoom,
                Zoom = 12,     // 初始缩放级别
                ShowCenter = false,
                MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter,
                CanDragMap = true
            };
            this.Controls.Add(gmap);

            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(statusStrip);
            statusStrip.BringToFront();
            // 将 statusStrip 设置为最后一个（最上层显示）
            this.Controls.SetChildIndex(statusStrip, this.Controls.Count - 1);

            statusLabel.Text = "正在加载数据...";
            var statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 100;
            statusTimer.Tick += (s, e) =>
            {
                if (DataLoader.Loaded && !DataLoader.IsError)
                {
                    int nodeCount = 0;
                    foreach (var driver in DataLoader.Drivers)
                    {
                        nodeCount += driver.Nodes.Count;
                    }
                    statusLabel.Text = $"数据加载完成！({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} Valid {DataLoader.DriversCount}D {nodeCount}N Load{DataLoader.LoadTotalMs}ms/Disk{DataLoader.LoadDiskMs}ms)";
                    statusTimer.Stop();
                }
                else if (DataLoader.IsError)
                {
                    statusLabel.Text = $"数据加载出错！！！({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} Valid {DataLoader.DriversCount}D)";
                    statusTimer.Stop();
                }
                else
                {
                    statusLabel.Text = $"正在加载数据...({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} Valid {DataLoader.DriversCount}D)";
                }
            };
            statusTimer.Start();

            // 初始化左侧侧边栏并由 SidebarController 管理（不修改现有控件）
            try
            {
                leftSidebar = new LeftSidebar_ChooseTimePeriod();
                leftSidebar.Title = "筛选";

                sidebarController = new SidebarController(this, leftSidebar, expandedWidth: 280);
                //sidebarController.Show();
                // 窗体关闭时释放 controller
                this.FormClosed += (s, e) => sidebarController?.Dispose();
            }
            catch
            {
                // 若初始化失败则忽略，保持程序可用
            }
        }
    }
}
