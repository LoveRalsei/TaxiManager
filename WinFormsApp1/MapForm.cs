using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;

namespace TaxiManager.BasicComponent
{
    public partial class MapForm : Form
    {

        public GMapControl GMap { get;private set; }
        public ControlPanel ControlPanel { get; private set; }
        public StatusStrip BottomStrip { get; private set; }
        public ToolStripStatusLabel statusLabel { get; private set; }

        // sidebar fields
        //public LeftSidebar_ChooseTimePeriod? LeftSidebar_ChooseTimePeriod { get; private set; }
        //public LeftSideBar_ChooseCars LeftSideBar_ChooseCars { get; private set; }
        //public SidebarController? sidebarController { get; private set; }
        public EventHandler? _bottomButtonAnalyzeHandler;//公开以便 UI_Button 可以访问
        private int _oringinMinZoom = 3;
        public int _oringinMaxZoom = 18;

        public MapForm()
        {
            GMap = new GMapControl
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
            InitializeComponent();

            
            this.Controls.Add(GMap);


            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            BottomStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            BottomStrip.Items.Add(statusLabel);
            BottomStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(BottomStrip);
            BottomStrip.BringToFront();
            // 将 statusStrip 设置为最后一个（最上层显示）
            this.Controls.SetChildIndex(BottomStrip, this.Controls.Count - 1);

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

            ControlPanel = new(this, GMap);
            this.Controls.Add(ControlPanel);

            SelectRegion.SetGMapControl(GMap);
        }
    }
}
