using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace TaxiManager
{
    public partial class MapForm : Form
    {

        private GMapControl gmap;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        // sidebar fields
        private LeftSidebar? leftSidebar;
        private SidebarController? sidebarController;

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
                leftSidebar = new LeftSidebar();
                leftSidebar.Title = "筛选";
                // 示例：处理侧边栏底部按钮点击 -> 调用区域分析并收回侧边栏
                leftSidebar.BottomButton.Click += (s, e) =>
                {
                    try
                    {
                        // _regionSearchPoints 在同一 partial 类中定义（UI_RegionSreachButton.cs）
                        _analyzeRegion(_regionSearchPoints, leftSidebar.StartDateString, leftSidebar.EndDateString);
                    }
                    catch
                    {
                        // 忽略调用时的异常，保证侧边栏能收回
                    }

                    // 将区域选择按钮恢复到初始状态：取消选择状态、清理 overlay、取消订阅鼠标事件并复位文本
                    try
                    {
                        _isRegionSearching = false;
                        _isRegionDragging = false;
                        _regionSearchPoints.Clear();
                        //_regionSearchOverlay.Polygons.Clear();
                        //_regionSearchOverlay.Markers.Clear();

                        // 取消订阅，以防尚未取消
                        try { gmap.MouseDown -= _mapRegionMouseDown; } catch { }
                        try { gmap.MouseMove -= _mapRegionMouseMove; } catch { }
                        try { gmap.MouseUp -= _mapRegionMouseUp; } catch { }

                        // 恢复按钮文本为设计时初始文本
                        _regionSreachButton.Text = "区域范围查找";
                    }
                    catch { }

                    sidebarController?.Hide();
                };

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
