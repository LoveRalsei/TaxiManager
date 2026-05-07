using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.UI
{
    internal class UI_ShowTrackButton : UI_HaveChooseCarsLeftSidebarButton
    {
        private Button _showTrackButton;

        private GMapOverlay trackOverlay;

        private bool _isSidebarShowing = false;

        public UI_ShowTrackButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _showTrackButton = new Button();

            _showTrackButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _showTrackButton.Location = new Point(847, 370);
            _showTrackButton.Name = "_showTrackButton";
            _showTrackButton.Size = new Size(40, 40);
            _showTrackButton.TabIndex = 7;
            _showTrackButton.Text = "轨";
            _showTrackButton.UseVisualStyleBackColor = true;
            _showTrackButton.Click += _showTrackButtonClick;

            _mapForm.Controls.Add(_showTrackButton);
        }

        private void _showTrackButtonClick(object sender, EventArgs e)
        {
            if (_isSidebarShowing == false)
            {
                _mapForm._resetAllButton();
                _isSidebarShowing = true;

                _sidebarController?.Show();
                BindBottomButtonToAnalysis(
                    () => ShowTrack(),
                    () => true,
                    ResetShowTrackButton);
            }
            else
            {
                ResetShowTrackButton();
                _isSidebarShowing = false;

                UnbindBottomButtonAnalysis();
            }
        }

        public void ResetShowTrackButton()
        {
            _isSidebarShowing = false;

            HideSidebar();
        }

        public void ClearTrackOverlay()
        {
            // 清除轨迹 overlay
            //var oldOverlay = _mapForm.gmap.Overlays.FirstOrDefault(o => o.Id == $"DriverTrack");
            if (trackOverlay != null)
            {
                trackOverlay.Routes.Clear();
                _mapForm.gmap.Overlays.Remove(trackOverlay);
                _mapForm.gmap.Refresh();
                trackOverlay = null;
            }

        }

        public void ShowTrack()
        {
            ClearTrackOverlay();
            int id;
            if (_leftSideBar_ChooseCars._result.HasValue)
            {
                id = _leftSideBar_ChooseCars._result.Value;
                if (id != 0)
                    ShowOneTrack(id);
                else
                    ShowAllTracks();
            }
        }

        private void ShowOneTrack(int id)
        {
            if (_leftSideBar_ChooseCars._result == null)
                return;

            try
            {
                // 等待数据加载完成
                if (!DataLoader.Loaded)
                {
                    MessageBox.Show("数据尚未加载完成，请稍候...", "提示");
                    return;
                }

                if (DataLoader.IsError)
                {
                    MessageBox.Show($"数据加载出错: {DataLoader.Error?.Message}", "错误");
                    return;
                }

                // 获取 id的司机
                var driver = DataLoader.Drivers.FirstOrDefault(d => d.Id == id);
                if (driver == null)
                {
                    MessageBox.Show($"未找到 id={id} 的司机", "提示");
                    return;
                }
                Stopwatch stopwatch = Stopwatch.StartNew();
                // 获取司机的所有连续轨迹
                //var routes = driver.GetRoutes(TimeTolerance.Minutes(15));
                var routes = ((IServiceF1)ServiceF1.Instance).GetDriverRoutes(id);
                trackOverlay = new GMapOverlay($"DriverTrack");
                // 遍历所有轨迹并创建 GMapRoute
                int routeIndex = 0;
                foreach (var route in routes)
                {
                    // 创建 GMap.WindowsForms.GMapRoute，需要传入点列表和名称
                    //var points = route.Points.Cast<PointLatLng>().ToList();
                    var gmapRoute = new GMap.NET.WindowsForms.GMapRoute(route.Points, $"Driver_Route{routeIndex}");
                    //var gmapRoute = ((IServiceF1)ServiceF1.Instance).GetDriverRoutes(driver.Id).Select(r => new GMap.NET.WindowsForms.GMapRoute(r.Points, $"Driver_Route{routeIndex}")).FirstOrDefault();

                    // 设置轨迹样式
                    gmapRoute.Stroke = new Pen(Color.Blue, 3);

                    // 添加到 overlay
                    trackOverlay.Routes.Add(gmapRoute);
                    routeIndex++;
                }

                //将 overlay 添加到地图
                _mapForm.gmap.Overlays.Add(trackOverlay);

                //刷新地图显示
                _mapForm.gmap.Refresh();
                _mapForm.gmap.Zoom += 0.001; // 目前发现绘制轨迹不会立即显示，
                                             // 但修改缩放比例会显示，所以利用这个触发地图重绘

                stopwatch.Stop();
                MessageBox.Show($"已绘制司机 {driver.Id} 的 {routes.Count} 条轨迹\n" +
                    $"耗时{stopwatch.ElapsedMilliseconds} ms", "F1出租车轨迹可视化绘制完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制轨迹时出错: {ex.Message}", "错误");
            }
        }

        private void ShowAllTracks()
        {
            ClearTrackOverlay();
            Stopwatch stopwatch = Stopwatch.StartNew();

            var tileData = ((IServiceF1)ServiceF1.Instance).GetTiles(_mapForm.gmap.ViewArea,_leftSideBar_ChooseCars._dateTimePicker.Value );
            // 获取最大count值用于计算透明度
            int maxCount = tileData.Max(t => (int)t.count);
            
            // 创建overlay
            trackOverlay = new GMapOverlay("TileOverlay");
            
            foreach (var tile in tileData)
            {
                // 计算透明度：count为0时alpha=255（完全不透明），count为最大值时alpha=0（完全透明）
                // 注意：这里假设你希望count越大越透明，如果相反请调整计算逻辑
                int alpha = (int)((double)255 * (double)tile.count / (double)maxCount);
                alpha = Math.Clamp(alpha, 0, 255);
            
                // 获取瓦片的经纬度范围
                var range = tile.tile.Range;
                var minPoint = range.Min.ToGmap();
                var maxPoint = range.Max.ToGmap();
            
                // 创建矩形区域的四个顶点
                var points = new List<PointLatLng>
                {
                    minPoint,
                    new PointLatLng(minPoint.Lat, maxPoint.Lng),
                    maxPoint,
                    new PointLatLng(maxPoint.Lat, minPoint.Lng)
                };
            
                // 创建多边形（矩形）
                Color tileColor = Color.FromArgb(alpha, Color.Blue);
                var polygon = new GMapPolygon(points, $"Tile_{tile.tile.X}_{tile.tile.Y}")
                {
                    Fill = new SolidBrush(tileColor),
                    Stroke = new Pen(Color.Transparent, 0)
                };
            
                trackOverlay.Polygons.Add(polygon);
            }
            
            // 添加到地图
            _mapForm.gmap.Overlays.Add(trackOverlay);
            _mapForm.gmap.Refresh();
            _mapForm.gmap.Zoom += 0.001;

            stopwatch.Stop();
            MessageBox.Show($"已绘制{DataLoader.Drivers.Count()}个司机的轨迹瓦片共{tileData.Count}个\n" +
                    $"耗时{stopwatch.ElapsedMilliseconds} ms", "F1出租车轨迹可视化绘制完成");
            
        }

        
    }
}
