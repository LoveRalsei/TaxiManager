using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TaxiManager
{
    public static class SelectRegion
    {
        private static GMapControl _gmap;

        public static void SetGMapControl(GMapControl gmap)
        {
            _gmap = gmap;
        }
        #region 单区域选择辅助方法

        /// <summary>
        /// 初始化单区域选择状态
        /// </summary>
        /// <param name="overlay">用于绘制的图层</param>
        /// <param name="onMouseDown">鼠标按下事件处理</param>
        /// <param name="onMouseMove">鼠标移动事件处理</param>
        /// <param name="onMouseUp">鼠标抬起事件处理</param>
        public static void InitializeSingleRegionSelection(
            GMapOverlay overlay,
            MouseEventHandler onMouseDown,
            MouseEventHandler onMouseMove,
            MouseEventHandler onMouseUp)
        {
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            _gmap.MouseDown += onMouseDown;
            _gmap.MouseMove += onMouseMove;
            _gmap.MouseUp += onMouseUp;

            if (!_gmap.Overlays.Contains(overlay))
                _gmap.Overlays.Add(overlay);
        }

        /// <summary>
        /// 重置单区域选择状态
        /// </summary>
        public static void ResetSingleRegionSelection(
            GMapOverlay overlay,
            MouseEventHandler onMouseDown,
            MouseEventHandler onMouseMove,
            MouseEventHandler onMouseUp)
        {
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            try { _gmap.MouseDown -= onMouseDown; } catch { }
            try { _gmap.MouseMove -= onMouseMove; } catch { }
            try { _gmap.MouseUp -= onMouseUp; } catch { }
        }

        /// <summary>
        /// 处理单区域选择的鼠标按下事件
        /// </summary>
        public static bool HandleSingleRegionMouseDown(
            bool isAnalyzing,
            bool isDragging,
            out bool newIsDragging,
            ref Point dragStartLocal,
            ref Point dragCurrentLocal,
            GMapOverlay overlay,
            List<PointLatLng> regionPoints,
            MouseEventArgs e)
        {
            if (!isAnalyzing) { newIsDragging = false; return false; }
            if (e.Button != MouseButtons.Left) { newIsDragging = false; return false; }

            // 如果已经有区域被选择过，清除已有选择并重新开始
            if (regionPoints.Count >= 4)
            {
                regionPoints.Clear();
                overlay.Polygons.Clear();
                overlay.Markers.Clear();
            }

            newIsDragging = true;
            dragStartLocal = new Point(e.X, e.Y);
            dragCurrentLocal = dragStartLocal;

            // 清理上一次绘制
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            return true;
        }

        /// <summary>
        /// 处理单区域选择的鼠标移动事件
        /// </summary>
        public static bool HandleSingleRegionMouseMove(
            bool isAnalyzing,
            bool isDragging,
            ref Point dragCurrentLocal,
            Point dragStartLocal,
            GMapOverlay overlay,
            Color regionColor,
            MouseEventArgs e)
        {
            if (!isAnalyzing || !isDragging) return false;

            dragCurrentLocal = new Point(e.X, e.Y);
            return UpdateTemporaryRectangle(dragStartLocal, dragCurrentLocal, overlay, regionColor, "选择矩形");
        }

        /// <summary>
        /// 处理单区域选择的鼠标抬起事件
        /// </summary>
        public static bool HandleSingleRegionMouseUp(
            bool isAnalyzing,
            bool isDragging,
            Point dragStartLocal,
            Point dragCurrentLocal,
            out bool newIsDragging,
            out List<PointLatLng> regionPoints,
            MouseEventArgs e)
        {
            newIsDragging = false;
            regionPoints = new List<PointLatLng>();

            if (!isAnalyzing || !isDragging) return false;
            if (e.Button != MouseButtons.Left) return false;

            // 计算并保存最终四角经纬度
            regionPoints = CalculateRegionCorners(dragStartLocal, dragCurrentLocal);
            return true;
        }

        #endregion

        #region 多区域选择辅助方法

        /// <summary>
        /// 初始化多区域选择状态
        /// </summary>
        public static void InitializeMultiRegionSelection(
            GMapOverlay overlay,
            List<List<PointLatLng>> regionPointsList,
            MouseEventHandler onMouseDown,
            MouseEventHandler onMouseMove,
            MouseEventHandler onMouseUp)
        {
            regionPointsList.Clear();
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            _gmap.MouseDown += onMouseDown;
            _gmap.MouseMove += onMouseMove;
            _gmap.MouseUp += onMouseUp;

            if (!_gmap.Overlays.Contains(overlay))
                _gmap.Overlays.Add(overlay);
        }

        /// <summary>
        /// 重置多区域选择状态
        /// </summary>
        public static void ResetMultiRegionSelection(
            GMapOverlay overlay,
            List<List<PointLatLng>> regionPointsList,
            MouseEventHandler onMouseDown,
            MouseEventHandler onMouseMove,
            MouseEventHandler onMouseUp)
        {
            regionPointsList.Clear();
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            try { _gmap.MouseDown -= onMouseDown; } catch { }
            try { _gmap.MouseMove -= onMouseMove; } catch { }
            try { _gmap.MouseUp -= onMouseUp; } catch { }
        }

        /// <summary>
        /// 停止多区域选择（不清理数据和overlay，只取消鼠标事件订阅）
        /// </summary>
        public static void StopMultiRegionSelection(
            MouseEventHandler onMouseDown,
            MouseEventHandler onMouseMove,
            MouseEventHandler onMouseUp)
        {
            try { _gmap.MouseDown -= onMouseDown; } catch { }
            try { _gmap.MouseMove -= onMouseMove; } catch { }
            try { _gmap.MouseUp -= onMouseUp; } catch { }
        }

        /// <summary>
        /// 处理多区域选择的鼠标按下事件
        /// </summary>
        public static bool HandleMultiRegionMouseDown(
            bool isAnalyzing,
            int currentRegionCount,
            int maxRegions,
            bool isDragging,
            out bool newIsDragging,
            ref Point dragStartLocal,
            ref Point dragCurrentLocal,
            GMapOverlay overlay,
            List<List<PointLatLng>> regionPointsList,
            Color[] regionColors,
            MouseEventArgs e)
        {
            if (!isAnalyzing) { newIsDragging = false; return false; }
            if (e.Button != MouseButtons.Left) { newIsDragging = false; return false; }

            // 如果已经选择了最大数量，清除已有选择并重新开始
            if (currentRegionCount >= maxRegions)
            {
                // 清除已有区域
                regionPointsList.Clear();
                overlay.Polygons.Clear();
                overlay.Markers.Clear();
            }

            newIsDragging = true;
            dragStartLocal = new Point(e.X, e.Y);
            dragCurrentLocal = dragStartLocal;

            // 清理当前正在拖拽区域的临时绘制（不清除已选定的区域）
            return true;
        }

        /// <summary>
        /// 处理多区域选择的鼠标移动事件
        /// </summary>
        public static bool HandleMultiRegionMouseMove(
            bool isAnalyzing,
            bool isDragging,
            ref Point dragCurrentLocal,
            Point dragStartLocal,
            GMapOverlay overlay,
            List<List<PointLatLng>> regionPointsList,
            Color[] regionColors,
            MouseEventArgs e)
        {
            if (!isAnalyzing || !isDragging) return false;

            dragCurrentLocal = new Point(e.X, e.Y);
            int regionIndex = regionPointsList.Count;
            Color regionColor = regionColors[regionIndex % regionColors.Length];

            // 计算矩形四个角（控件坐标）
            int left = Math.Min(dragStartLocal.X, dragCurrentLocal.X);
            int right = Math.Max(dragStartLocal.X, dragCurrentLocal.X);
            int top = Math.Min(dragStartLocal.Y, dragCurrentLocal.Y);
            int bottom = Math.Max(dragStartLocal.Y, dragCurrentLocal.Y);

            var cornersLocal = new Point[]
            {
                new Point(left, top),
                new Point(right, top),
                new Point(right, bottom),
                new Point(left, bottom)
            };

            // 把控件坐标转为经纬度
            var polyPoints = new List<PointLatLng>();
            foreach (var p in cornersLocal)
                polyPoints.Add(_gmap.FromLocalToLatLng(p.X, p.Y));

            // 先清除临时的多边形（保留已选定的区域）
            RemoveTempPolygonsAndMarkers(overlay);

            // 多边形（半透明填充）
            var polygon = new GMapPolygon(polyPoints, $"temp_{regionIndex}");
            polygon.Fill = new SolidBrush(Color.FromArgb(20, regionColor));
            polygon.Stroke = new Pen(regionColor, 2);
            overlay.Polygons.Add(polygon);

            // 在四个角添加标记
            for (int i = 0; i < polyPoints.Count; i++)
            {
                var marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(polyPoints[i],
                    GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_small);
                marker.Tag = $"temp_{regionIndex}_{i}";
                overlay.Markers.Add(marker);
            }

            if (!_gmap.Overlays.Contains(overlay))
                _gmap.Overlays.Add(overlay);

            return true;
        }

        /// <summary>
        /// 处理多区域选择的鼠标抬起事件
        /// </summary>
        public static bool HandleMultiRegionMouseUp(
            bool isAnalyzing,
            bool isDragging,
            Point dragStartLocal,
            Point dragCurrentLocal,
            GMapOverlay overlay,
            List<List<PointLatLng>> regionPointsList,
            Color[] regionColors,
            int maxRegions,
            out bool newIsAnalyzing,
            out bool newIsDragging,
            MouseEventArgs e)
        {
            newIsAnalyzing = isAnalyzing;
            newIsDragging = false;

            if (!isAnalyzing || !isDragging) return false;
            if (e.Button != MouseButtons.Left) return false;

            // 如果已经选择了最大数量，检查是否需要重新开始
            // （在MouseDown中可能已清除旧数据，此时允许添加新区域）
            if (regionPointsList.Count >= maxRegions)
            {
                newIsAnalyzing = false;
                return false;
            }

            // 计算并保存最终四角经纬度
            var currentRegionPoints = CalculateRegionCorners(dragStartLocal, dragCurrentLocal);
            regionPointsList.Add(currentRegionPoints);

            // 清除临时多边形
            RemoveTempPolygonsAndMarkers(overlay);

            // 绘制正式的多边形
            int regionIndex = regionPointsList.Count - 1;
            Color regionColor = regionColors[regionIndex % regionColors.Length];
            var polygon = new GMapPolygon(currentRegionPoints, $"region_{regionIndex}");
            polygon.Fill = new SolidBrush(Color.FromArgb(30, regionColor));
            polygon.Stroke = new Pen(regionColor, 2);
            overlay.Polygons.Add(polygon);

            // 在四个角添加标记
            for (int i = 0; i < currentRegionPoints.Count; i++)
            {
                var marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(currentRegionPoints[i],
                    GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_small);
                marker.Tag = $"region_{regionIndex}_{i}";
                overlay.Markers.Add(marker);
            }

            // 检查是否已完成所有区域选择
            if (regionPointsList.Count == maxRegions)
            {
                newIsAnalyzing = false;
            }

            return true;
        }

        #endregion

        #region 通用辅助方法

        /// <summary>
        /// 更新临时矩形显示
        /// </summary>
        public static bool UpdateTemporaryRectangle(
            Point dragStartLocal,
            Point dragCurrentLocal,
            GMapOverlay overlay,
            Color regionColor,
            string polygonName = "选择矩形")
        {
            // 计算矩形四个角（控件坐标）
            int left = Math.Min(dragStartLocal.X, dragCurrentLocal.X);
            int right = Math.Max(dragStartLocal.X, dragCurrentLocal.X);
            int top = Math.Min(dragStartLocal.Y, dragCurrentLocal.Y);
            int bottom = Math.Max(dragStartLocal.Y, dragCurrentLocal.Y);

            var cornersLocal = new Point[]
            {
                new Point(left, top),
                new Point(right, top),
                new Point(right, bottom),
                new Point(left, bottom)
            };

            // 把控件坐标转为经纬度
            var polyPoints = new List<PointLatLng>();
            foreach (var p in cornersLocal)
                polyPoints.Add(_gmap.FromLocalToLatLng(p.X, p.Y));

            // 重建 overlay（清除旧的临时图形）
            overlay.Polygons.Clear();
            overlay.Markers.Clear();

            // 多边形（半透明填充）
            var polygon = new GMapPolygon(polyPoints, polygonName);
            polygon.Fill = new SolidBrush(Color.FromArgb(20, regionColor));
            polygon.Stroke = new Pen(regionColor, 2);
            overlay.Polygons.Add(polygon);

            // 在四个角添加标记
            for (int i = 0; i < polyPoints.Count; i++)
            {
                var marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(polyPoints[i],
                    GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_small);
                overlay.Markers.Add(marker);
            }

            // 确保 overlay 已经在 gmap 上
            if (!_gmap.Overlays.Contains(overlay))
                _gmap.Overlays.Add(overlay);

            return true;
        }

        /// <summary>
        /// 计算区域四个角的经纬度
        /// </summary>
        public static List<PointLatLng> CalculateRegionCorners(Point dragStartLocal, Point dragCurrentLocal)
        {
            int left = Math.Min(dragStartLocal.X, dragCurrentLocal.X);
            int right = Math.Max(dragStartLocal.X, dragCurrentLocal.X);
            int top = Math.Min(dragStartLocal.Y, dragCurrentLocal.Y);
            int bottom = Math.Max(dragStartLocal.Y, dragCurrentLocal.Y);

            var cornersLocal = new Point[]
            {
                new Point(left, top),
                new Point(right, top),
                new Point(right, bottom),
                new Point(left, bottom)
            };

            var regionPoints = new List<PointLatLng>();
            foreach (var p in cornersLocal)
                regionPoints.Add(_gmap.FromLocalToLatLng(p.X, p.Y));

            return regionPoints;
        }

        /// <summary>
        /// 清除临时多边形和标记
        /// </summary>
        public static void RemoveTempPolygonsAndMarkers(GMapOverlay overlay)
        {
            var tempPolygons = overlay.Polygons
                .Where(p => p.Name.StartsWith("temp_"))
                .ToList();
            foreach (var p in tempPolygons)
                overlay.Polygons.Remove(p);

            var tempMarkers = overlay.Markers
                .Where(m => m.Tag != null && m.Tag.ToString().StartsWith("temp_"))
                .ToList();
            foreach (var m in tempMarkers)
                overlay.Markers.Remove(m);
        }

        /// <summary>
        /// 获取单区域选择的按钮文本
        /// </summary>
        public static string GetSingleRegionButtonText(bool isAnalyzing, string baseText, List<PointLatLng> regionPoints)
        {
            if (!isAnalyzing) return baseText;
            if (regionPoints.Count == 0) return "区域选择中...";
            return $"已选区域({regionPoints.Count})";
        }

        /// <summary>
        /// 获取多区域选择的按钮文本
        /// </summary>
        public static string GetMultiRegionButtonText(bool isAnalyzing, string baseText, List<List<PointLatLng>> regionPointsList, int maxRegions)
        {
            if (!isAnalyzing) return baseText;
            if (regionPointsList.Count == 0) return "选择区域中";
            if (regionPointsList.Count < maxRegions) return $"选择区域{regionPointsList.Count + 1}/{maxRegions}...";
            return $"已选{maxRegions}个区域";
        }

        #endregion
    }
}