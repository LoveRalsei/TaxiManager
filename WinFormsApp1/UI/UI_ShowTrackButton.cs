using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.UI
{
    internal class UI_ShowTrackButton : UI_HaveChooseCarsLeftSidebarButton
    {
        private Button _showTrackButton;

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
            // 清除轨迹 overlay
            var oldOverlay = _mapForm.gmap.Overlays.FirstOrDefault(o => o.Id == $"DriverTrack");
            if (oldOverlay != null)
            {
                oldOverlay.Routes.Clear();
                _mapForm.gmap.Overlays.Remove(oldOverlay);
                _mapForm.gmap.Refresh();
            }
            _isSidebarShowing= false;

            HideSidebar();
        }

        public void ShowTrack()
        {
            if (_leftSideBar_ChooseCars._result == null)
                return;
            int id = (int)_leftSideBar_ChooseCars._result;

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

                // 获取司机的所有连续轨迹
                var routes = driver.GetRoutes(TimeTolerance.Minutes(15));
                //MessageBox.Show($"routes第一项的内容：\n名称：{routes[0].Name}\n：{routes[0].Points[0].Lat} {routes[0].Points[0].Lng}");

                // 创建一个新的 overlay 用于显示轨迹
                var trackOverlay = new GMapOverlay($"Driver{id}Track");

                // 清除旧的轨迹 overlay（如果存在）
                //var oldOverlay = _mapForm.gmap.Overlays.FirstOrDefault(o => o.Id == $"DriverTrack");
                //if (oldOverlay != null)
                //{
                //    oldOverlay.Routes.Clear();
                //    _mapForm.gmap.Overlays.Remove(oldOverlay);
                //}
                // 遍历所有轨迹并创建 GMapRoute
                int routeIndex = 0;
                foreach (var route in routes)
                {
                    // 创建 GMap.WindowsForms.GMapRoute，需要传入点列表和名称
                    //var points = route.Points.Cast<PointLatLng>().ToList();
                    var gmapRoute = new GMap.NET.WindowsForms.GMapRoute(route.Points, $"Driver_Route{routeIndex}");
                    //if(routeIndex==0)
                    //    MessageBox.Show($"第一条轨迹的点数: {route.Points.Count}, \n" +
                    //        $"第一条轨迹第一点: Lat={route.Points[0].Lat}, Lng={route.Points[0].Lng}\n" +
                    //        $"第一条轨迹第二点: Lat={route.Points[1].Lat}, Lng={route.Points[1].Lng}");
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
                _mapForm.gmap.Zoom+=0.001; // 目前发现绘制轨迹不会立即显示，
                                           // 但修改缩放比例会显示，所以利用这个触发地图重绘
                

                MessageBox.Show($"已绘制司机 {driver.Id} 的 {routes.Count} 条轨迹", "绘制完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制轨迹时出错: {ex.Message}", "错误");
            }
        }
    }
}
