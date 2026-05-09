using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaxiManager.Structure;

namespace TaxiManager.BasicComponent
{
    public class SelectRegion
    {
        public static readonly SelectRegion Instance = new();

        private int _selectRegionNum = 1;
        private readonly List<PositionRange> _ranges = new();

        private static GMapControl _gmap;
        private GMapOverlay _selectionOverlay;

        private bool _isSelecting = false;
        private bool _isDragging = false;
        private PointLatLng? _startLatLng;
        private PointLatLng? _currentLatLng;
        private bool? _savedCanDragMap;

        public void SetGMapControl(GMapControl gmap)
        {
            _gmap = gmap;
        }

        // Start selection mode. n = number of regions to select (default 1)
        public void StartSelectRegion(int n = 1)
        {
            if (_gmap == null) throw new InvalidOperationException("GMapControl not set. Call SetGMapControl first.");

            EndSelectRegion(); // ensure clean state (idempotent)

            _selectRegionNum = Math.Max(1, n);

            // prepare overlay
            _selectionOverlay = new GMapOverlay("select_region") { Id = "select_region" };
            _selectionOverlay.Polygons.Clear();
            _selectionOverlay.Markers.Clear();

            if (!_gmap.Overlays.Contains(_selectionOverlay))
                _gmap.Overlays.Add(_selectionOverlay);

            // save and disable map dragging to avoid interference
            try { _savedCanDragMap = _gmap.CanDragMap; _gmap.CanDragMap = false; } catch { }

            // subscribe events
            try { _gmap.MouseDown -= OnMapMouseDown; } catch { }
            try { _gmap.MouseMove -= OnMapMouseMove; } catch { }
            try { _gmap.MouseUp -= OnMapMouseUp; } catch { }
            try { _gmap.MouseLeave -= OnMapMouseLeave; } catch { }

            _gmap.MouseDown += OnMapMouseDown;
            _gmap.MouseMove += OnMapMouseMove;
            _gmap.MouseUp += OnMapMouseUp;
            _gmap.MouseLeave += OnMapMouseLeave;

            _isSelecting = true;
            _isDragging = false;
            _startLatLng = null;
            _currentLatLng = null;
        }

        // End selection mode and clear stored ranges
        public void EndSelectRegion()
        {
            if (_gmap != null)
            {
                try { _gmap.MouseDown -= OnMapMouseDown; } catch { }
                try { _gmap.MouseMove -= OnMapMouseMove; } catch { }
                try { _gmap.MouseUp -= OnMapMouseUp; } catch { }
                try { _gmap.MouseLeave -= OnMapMouseLeave; } catch { }

                try
                {
                    if (_savedCanDragMap.HasValue)
                        _gmap.CanDragMap = _savedCanDragMap.Value;
                }
                catch { }
            }

            _isSelecting = false;
            _isDragging = false;

            _ranges.Clear();

            // clear visuals
            if (_selectionOverlay != null && _gmap != null)
            {
                EnsureOnUi(() =>
                {
                    _selectionOverlay.Polygons.Clear();
                    _selectionOverlay.Markers.Clear();
                    try { _gmap.Overlays.Remove(_selectionOverlay); } catch { }
                });
                _selectionOverlay = null;
            }
        }

        // Return first region or null
        public PositionRange? GetOneRegion()
        {
            if (_ranges.Count == 0) return null;
            return _ranges[0];
        }

        // Return a copy of ranges
        public List<PositionRange> GetRegions()
        {
            return new List<PositionRange>(_ranges);
        }

        #region Event handlers

        private void OnMapMouseDown(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;
            if (e.Button != MouseButtons.Left) return;

            // if already have enough regions, clear and start new
            if (_ranges.Count >= _selectRegionNum)
            {
                _ranges.Clear();
                EnsureOnUi(() =>
                {
                    if (_selectionOverlay != null)
                    {
                        // remove previously drawn polygons/markers
                        var oldPolys = _selectionOverlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("select_")).ToList();
                        foreach (var p in oldPolys) _selectionOverlay.Polygons.Remove(p);
                        var oldMarks = _selectionOverlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("select_")).ToList();
                        foreach (var m in oldMarks) _selectionOverlay.Markers.Remove(m);
                    }
                });
            }

            _isDragging = true;

            // snap pixel to meter grid and get latlng
            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _startLatLng = latlng;
            _currentLatLng = latlng;

            // clear current temp visuals
            EnsureOnUi(() =>
            {
                if (_selectionOverlay != null)
                {
                    RemoveSelectTemp(_selectionOverlay);
                }
            });
        }

        private void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || !_isDragging) return;

            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _currentLatLng = latlng;

            if (_startLatLng == null) return;

            // compute corner points (min/max lat/lng)
            var a = _startLatLng.Value;
            var b = _currentLatLng.Value;

            double minLat = Math.Min(a.Lat, b.Lat);
            double maxLat = Math.Max(a.Lat, b.Lat);
            double minLng = Math.Min(a.Lng, b.Lng);
            double maxLng = Math.Max(a.Lng, b.Lng);

            var corners = new List<PointLatLng>
            {
                new PointLatLng(minLat, minLng),
                new PointLatLng(maxLat, minLng),
                new PointLatLng(maxLat, maxLng),
                new PointLatLng(minLat, maxLng)
            };

            EnsureOnUi(() =>
            {
                if (_selectionOverlay == null) return;

                // clear previous temp
                RemoveSelectTemp(_selectionOverlay);

                // temp polygon
                var poly = new GMapPolygon(corners, "select_temp") { Stroke = new Pen(Color.Blue, 2), Fill = new SolidBrush(Color.FromArgb(30, Color.Blue)) };
                _selectionOverlay.Polygons.Add(poly);

                // markers
                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], GMarkerGoogleType.red_small) { Tag = $"select_temp_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });
        }

        private void OnMapMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || !_isDragging) return;
            if (e.Button != MouseButtons.Left) return;

            _isDragging = false;

            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _currentLatLng = latlng;

            if (_startLatLng == null) return;

            var a = _startLatLng.Value;
            var b = _currentLatLng.Value;

            // snap lat/lng values to 1e-5 using rounding to be consistent
            double aLat = Math.Round(a.Lat * 1e5) / 1e5;
            double aLng = Math.Round(a.Lng * 1e5) / 1e5;
            double bLat = Math.Round(b.Lat * 1e5) / 1e5;
            double bLng = Math.Round(b.Lng * 1e5) / 1e5;

            var pA = Position.FromRaw(aLng, aLat);
            var pB = Position.FromRaw(bLng, bLat);

            var range = PositionRange.FromUnsort(pA, pB);

            _ranges.Add(range);

            // create final polygon and markers
            EnsureOnUi(() =>
            {
                if (_selectionOverlay == null) return;

                // clear temp
                RemoveSelectTemp(_selectionOverlay);

                int index = _ranges.Count - 1;
                var corners = new List<PointLatLng>
                {
                    new PointLatLng(Math.Min(aLat,bLat), Math.Min(aLng,bLng)),
                    new PointLatLng(Math.Max(aLat,bLat), Math.Min(aLng,bLng)),
                    new PointLatLng(Math.Max(aLat,bLat), Math.Max(aLng,bLng)),
                    new PointLatLng(Math.Min(aLat,bLat), Math.Max(aLng,bLng))
                };

                var poly = new GMapPolygon(corners, $"select_final_{index}") { Stroke = new Pen(Color.Red, 2), Fill = new SolidBrush(Color.FromArgb(40, Color.Red)) };
                _selectionOverlay.Polygons.Add(poly);

                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], GMarkerGoogleType.red_small) { Tag = $"select_final_{index}_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });

            // if reached target count, stop selecting (but keep visuals)
            //if (_ranges.Count >= _selectRegionNum)
            //{
            //    // do not automatically clear visuals; simply stop accepting more unless StartSelectRegion is called
            //    _isSelecting = false;
            //    try { _gmap.MouseDown -= OnMapMouseDown; } catch { }
            //    try { _gmap.MouseMove -= OnMapMouseMove; } catch { }
            //    try { _gmap.MouseUp -= OnMapMouseUp; } catch { }
            //    try { _gmap.MouseLeave -= OnMapMouseLeave; } catch { }

            //    try
            //    {
            //        if (_savedCanDragMap.HasValue)
            //            _gmap.CanDragMap = _savedCanDragMap.Value;
            //    }
            //    catch { }
            //}
        }

        private void OnMapMouseLeave(object sender, EventArgs e)
        {
            // if mouse leaves map while dragging, cancel dragging
            if (_isDragging)
            {
                _isDragging = false;
            }
        }

        #endregion

        #region Helpers

        private void RemoveSelectTemp(GMapOverlay overlay)
        {
            var tempPolys = overlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("select_temp")).ToList();
            foreach (var p in tempPolys) overlay.Polygons.Remove(p);

            var tempMarks = overlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("select_temp")).ToList();
            foreach (var m in tempMarks) overlay.Markers.Remove(m);
        }

        private void EnsureOnUi(Action action)
        {
            if (_gmap == null) { action(); return; }
            if (_gmap.InvokeRequired)
            {
                try { _gmap.BeginInvoke(action); } catch { }
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// 将像素坐标取整到1e-5经纬度的整数倍（对应约1米）
        /// </summary>
        public static Point SnapToMeterGrid(GMapControl gmap, int x, int y)
        {
            // 像素坐标转经纬度
            var latLng = gmap.FromLocalToLatLng(x, y);

            // 经纬度取整到1e-5的整数倍
            double snappedLat = Math.Floor(latLng.Lat * 1e5) / 1e5;
            double snappedLng = Math.Floor(latLng.Lng * 1e5) / 1e5;

            // 经纬度转回像素坐标
            var snappedPoint = gmap.FromLatLngToLocal(new PointLatLng(snappedLat, snappedLng));

            return new Point((int)snappedPoint.X, (int)snappedPoint.Y);
        }

        #endregion
    }
}