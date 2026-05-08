using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.UI.BasicUI;
using TaxiManager.Service;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI.UI
{
    internal class UIShowTrackButton : UIButton
    {
        public const string KeyChooseCar = "ChooseCar";
        public const string KeyTimePicker = "TimePicker";

        private Button? _showTrackButton;

        public UIShowTrackButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {

            _showTrackButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(847, 370),
                Name = "_showTrackButton",
                Size = new Size(40, 40),
                TabIndex = 7,
                Text = "轨",
                UseVisualStyleBackColor = true
            };
            _showTrackButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_showTrackButton);
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            if (_mapForm.ControlPanel.CurrentComponent != this)
            {
                _mapForm.ControlPanel.SwitchTo(this);
            } else
            {
                _mapForm.ControlPanel.SwitchTo(null);
            }
        }

        private void RenderTrack(ControlPanel controller, int id, DateTime time)
        {

            try
            {
                // 获取 id的司机
                var driver = DataLoader.Drivers.FirstOrDefault(d => d.Id == id);
                if (driver == null)
                    throw new Exception($"It should not happen! There's no such driver with id {id}");
                Stopwatch stopwatch = Stopwatch.StartNew();
                // 获取司机的所有连续轨迹
                //var routes = driver.GetRoutes(TimeTolerance.Minutes(15));
                var routes = ((IServiceF1)ServiceF1.Instance).GetDriverRoutes(id);
                controller.ModifyOverlay((overlay) =>
                {
                    // 遍历所有轨迹并创建 GMapRoute
                    int routeIndex = 0;
                    foreach (var route in routes)
                    {
                        // 创建 GMap.WindowsForms.GMapRoute，需要传入点列表和名称
                        var gmapRoute = new GMap.NET.WindowsForms.GMapRoute(route.Points, $"Driver_Route{routeIndex++}")
                        {
                            // 设置轨迹样式
                            Stroke = new Pen(Color.Blue, 3)
                        };

                        // 添加到 overlay
                        overlay.Routes.Add(gmapRoute);
                        var position = driver.GetPosition(time);
                        if (position != null)
                            overlay.Markers.Add(new GMarkerDot(position.Value, 10, Color.LightBlue));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制轨迹时出错: {ex.Message}", "错误");
            }
        }

        private void RenderAllTrack(ControlPanel controller, DateTime time)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var service = IServiceF1.Instance;
            var renderMode = service.GetRenderMode(_mapForm.GMap.ViewArea, _mapForm.GMap.Size);
            int renderTargetCount = 0;

            controller.ModifyOverlay((overlay) =>
            {
                if (renderMode == F1RenderMode.Points)
                {
                    var points = service.GetPoints(_gmap.ViewArea, _mapForm.GMap.Size, time);
                    renderTargetCount = points.Count;
                    foreach (var point in points)
                    {
                        overlay.Markers.Add(new GMarkerDot(point, 3, Color.Blue));
                    }
                }
                else if (renderMode == F1RenderMode.Tiles)
                {
                    var tilesMap = service.GetTiles(_gmap.ViewArea, _mapForm.GMap.Size, time);
                    renderTargetCount = tilesMap.Count;
                    double avgCount = tilesMap.Count > 0 ? tilesMap.Average(tile => tile.count) : 0;

                    foreach (var tile in tilesMap)
                    {
                        int alpha = (int)((double)127 * (double)tile.count / (double)avgCount);
                        alpha = Math.Clamp(alpha, 0, 255);

                        // 获取瓦片的经纬度范围
                        var range = tile.tile.Range;
                        var minPoint = range.Min.ToGmap();
                        var maxPoint = range.Max.ToGmap();

                        // 创建多边形（矩形）
                        Color tileColor = Color.FromArgb(alpha, Color.Blue);
                        var polygon = new GMapPolygon(
                            [minPoint, new(minPoint.Lat, maxPoint.Lng),
                        maxPoint, new(maxPoint.Lat, minPoint.Lng)]
                            , $"Tile_{tile.tile.X}_{tile.tile.Y}"
                        )
                        {
                            Fill = new SolidBrush(tileColor),
                            Stroke = new Pen(Color.Transparent, 0)
                        };

                        overlay.Polygons.Add(polygon);
                    }
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"以渲染模式{renderMode}，渲染共{renderTargetCount}个对象\n" +
                    $"耗时{stopwatch.ElapsedMilliseconds} ms", "F1出租车轨迹可视化绘制完成");
            
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyChooseCar, new SideBarChooseCar()));
            registry.Add((KeyTimePicker, new SideBarScrollTime()));
        }

        public override void Update(ControlPanel panel)
        {
            int? driverId = (int?)panel.GetItemValue(KeyChooseCar);
            DateTime time = ((DateTime?)panel.GetItemValue(KeyTimePicker))!.Value;
            if (driverId == null)
                RenderAllTrack(panel, time);
            else
                RenderTrack(panel, driverId.Value, time);
        }
    }
}
