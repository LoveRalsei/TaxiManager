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

        private int oringinMinZoom = 3;
        private int oringinMaxZoom = 18;
        private bool lockZoom = false;
        private bool isRegionSearching = false;
        private List<PointLatLng> regionSearchPoints = new List<PointLatLng>();
        private GMapOverlay regionSearchOverlay = new GMapOverlay("polygons");
        

        public MapForm()
        {
            InitializeComponent();

            gmap = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = AMapProvider.Instance,
                Position = new PointLatLng(39.9042, 116.4074), // 北京坐标
                MinZoom = oringinMinZoom,
                MaxZoom = oringinMaxZoom,
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
        }

        private void enlargeButtonClick(object sender, EventArgs e)
        {
            if (lockZoom) return;
            gmap.Zoom++;
        }

        private void shrinkButtonClick(object sender, EventArgs e)
        {
            if (lockZoom) return;
            gmap.Zoom--;
        }

        private void regionSreachButtonClick(object sender, EventArgs e)
        {
            regionSreachButton.Text="区域选择中...";
            if (isRegionSearching==false)
            {
                isRegionSearching = true;
                gmap.MouseClick += regionSreachClick;
                regionSearchPoints.Clear();
            }
            else
            { 
                isRegionSearching = false;
                gmap.MouseClick -= regionSreachClick;
            }
        }

        private void regionSreachClick(object sender, MouseEventArgs e)
        {
            // 只响应左键
            if (e.Button != MouseButtons.Left) return;

            // 把点击位置转为经纬度并加入点列表
            var latlng = gmap.FromLocalToLatLng(e.X, e.Y);
            regionSearchPoints.Add(latlng);

            // 每次重绘前先清空 overlay 中的图形和标记，避免重复叠加
            regionSearchOverlay.Polygons.Clear();
            regionSearchOverlay.Markers.Clear();

            // 为每个顶点添加一个标记，便于用户识别
            for (int i = 0; i < regionSearchPoints.Count; i++)
            {
                var m = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(regionSearchPoints[i], GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue_dot);
                regionSearchOverlay.Markers.Add(m);
            }

            // 当点数 >= 3 时绘制多边形；点数不足时只显示标记
            if (regionSearchPoints.Count >= 3)
            {
                var polyPoints = new List<PointLatLng>(regionSearchPoints);
                var regionSearchPolygon = new GMapPolygon(polyPoints, "区域选择多边形");
                regionSearchPolygon.Fill = new SolidBrush(Color.FromArgb(80, Color.Red));
                regionSearchPolygon.Stroke = new Pen(Color.Red, 2);
                regionSearchOverlay.Polygons.Add(regionSearchPolygon);
            }

            // 确保 overlay 只被加入一次
            if (!gmap.Overlays.Contains(regionSearchOverlay))
            {
                gmap.Overlays.Add(regionSearchOverlay);
            }

            // 更新按钮文本显示当前点数
            regionSreachButton.Text = $"区域选择中...({regionSearchPoints.Count})";
        }
    }
}
