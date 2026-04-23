using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;

namespace TaxiManager
{
    public partial class MapForm : Form
    {

        public GMapControl gmap { get;private set; }
        public StatusStrip statusStrip { get; private set; }
        public ToolStripStatusLabel statusLabel { get; private set; }

        // sidebar fields
        public LeftSidebar_ChooseTimePeriod? leftSidebar { get; private set; }
        public SidebarController? sidebarController { get; private set; }
        public EventHandler? _bottomButtonAnalyzeHandler;//公开以便 UI_Button 可以访问
        private int _oringinMinZoom = 3;
        public int _oringinMaxZoom = 18;

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

            SelectRegion.SetGMapControl(gmap);
        }
    }
}
