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
        public MapForm()
        {
            InitializeComponent();

            gmap = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = AMapProvider.Instance,
                Position = new PointLatLng(39.9042, 116.4074), // 北京坐标
                MinZoom = 3,
                MaxZoom = 18,
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
                if (DataLoader.Loaded && ! DataLoader.IsError)
                {
                    statusLabel.Text = $"数据加载完成！({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} valid {DataLoader.DriversCount})";
                    statusTimer.Stop();
                }
                else if (DataLoader.IsError)
                {
                    statusLabel.Text = $"数据加载出错！！！({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} valid {DataLoader.DriversCount})";
                    statusTimer.Stop();
                }
                else
                {
                    statusLabel.Text = $"正在加载数据...({DataLoader.LoadedCount}/{DataLoader.RawDriversCount} valid {DataLoader.DriversCount})";
                }
            };
            statusTimer.Start();
        }
    }
}
