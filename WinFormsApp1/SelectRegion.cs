using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using TaxiManager.Structure;

namespace TaxiManager
{
    public class SelectRegion
    {
        public static readonly SelectRegion Instance = new();

        private enum SelectionMode { None, Raw, Tile }

        #region Fields

        private static GMapControl _gmap;
        private GMapOverlay _selectionOverlay;

        private bool _isSelecting = false;
        private bool _isDragging = false;
        private bool? _savedCanDragMap;
        private SelectionMode _mode = SelectionMode.None;

        // Raw mode
        private int _selectRegionNum = 1;
        private readonly List<PositionRange> _ranges = new();
        private PointLatLng? _startLatLng;
        private PointLatLng? _currentLatLng;

        // Tile mode
        private byte _tileSize = 1;
        private int _tileSelectRegionNum = 1;
        private readonly List<PositionRange> _tileRanges = new();
        private Tile? _tileStart;
        private Tile? _tileCurrent;

        #endregion

        #region Public API

        public void SetGMapControl(GMapControl gmap)
        {
            _gmap = gmap;
        }

        public void StartSelectRegion(int n = 1)
        {
            if (_gmap == null) throw new InvalidOperationException("GMapControl not set. Call SetGMapControl first.");

            EndSelectRegion();

            _selectRegionNum = Math.Max(1, n);
            _mode = SelectionMode.Raw;

            _selectionOverlay = new GMapOverlay("select_region") { Id = "select_region" };
            _selectionOverlay.Polygons.Clear();
            _selectionOverlay.Markers.Clear();

            if (!_gmap.Overlays.Contains(_selectionOverlay))
                _gmap.Overlays.Add(_selectionOverlay);

            //try { _savedCanDragMap = _gmap.CanDragMap; _gmap.CanDragMap = false; } catch { }

            SubscribeEvents();

            _isSelecting = true;
            _isDragging = false;
            _startLatLng = null;
            _currentLatLng = null;
        }

        public void StartSelectTileRegion(byte tileSize = 1, int n = 1)
        {
            if (_gmap == null) throw new InvalidOperationException("GMapControl not set. Call SetGMapControl first.");
            if (tileSize < 1) throw new ArgumentException("Tile size must be >= 1.", nameof(tileSize));

            EndSelectRegion();

            _tileSize = tileSize;
            _tileSelectRegionNum = Math.Max(1, n);
            _mode = SelectionMode.Tile;

            _selectionOverlay = new GMapOverlay("select_region") { Id = "select_region" };
            _selectionOverlay.Polygons.Clear();
            _selectionOverlay.Markers.Clear();

            if (!_gmap.Overlays.Contains(_selectionOverlay))
                _gmap.Overlays.Add(_selectionOverlay);

            //try { _savedCanDragMap = _gmap.CanDragMap; _gmap.CanDragMap = false; } catch { }

            SubscribeEvents();

            _isSelecting = true;
            _isDragging = false;
            _tileStart = null;
            _tileCurrent = null;
        }

        public void EndSelectRegion()
        {
            UnsubscribeEvents();
            _gmap.CanDragMap = true;
            

            _isSelecting = false;
            _isDragging = false;
            _mode = SelectionMode.None;

            _ranges.Clear();
            _tileRanges.Clear();

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

        // Raw mode results
        public PositionRange? GetOneRegion()
        {
            if (_ranges.Count == 0) return null;
            return _ranges[0];
        }

        public List<PositionRange> GetRegions()
        {
            return new List<PositionRange>(_ranges);
        }

        // Tile mode results
        public PositionRange? GetOneTileRegion()
        {
            if (_tileRanges.Count == 0) return null;
            return _tileRanges[0];
        }

        public List<PositionRange> GetTileRegions()
        {
            return new List<PositionRange>(_tileRanges);
        }

        #endregion

        #region Event Dispatch

        private void OnMapMouseDown(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;
            if (e.Button != MouseButtons.Left) return;
            _gmap.CanDragMap = false;
            switch (_mode)
            {
                case SelectionMode.Raw: HandleRawMouseDown(e); break;
                case SelectionMode.Tile: HandleTileMouseDown(e); break;
            }
        }

        private void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || !_isDragging) return;

            switch (_mode)
            {
                case SelectionMode.Raw: HandleRawMouseMove(e); break;
                case SelectionMode.Tile: HandleTileMouseMove(e); break;
            }
        }

        private void OnMapMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isSelecting || !_isDragging) return;
            if (e.Button != MouseButtons.Left) return;
            _gmap.CanDragMap = true;
            
            switch (_mode)
            {
                case SelectionMode.Raw: HandleRawMouseUp(e); break;
                case SelectionMode.Tile: HandleTileMouseUp(e); break;
            }
        }

        private void OnMapMouseLeave(object sender, EventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
            }
        }

        #endregion

        #region Raw Mode Handlers

        private void HandleRawMouseDown(MouseEventArgs e)
        {
            if (_ranges.Count >= _selectRegionNum)
            {
                _ranges.Clear();
                EnsureOnUi(() =>
                {
                    if (_selectionOverlay != null)
                    {
                        var oldPolys = _selectionOverlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("select_")).ToList();
                        foreach (var p in oldPolys) _selectionOverlay.Polygons.Remove(p);
                        var oldMarks = _selectionOverlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("select_")).ToList();
                        foreach (var m in oldMarks) _selectionOverlay.Markers.Remove(m);
                    }
                });
            }

            _isDragging = true;

            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _startLatLng = latlng;
            _currentLatLng = latlng;

            EnsureOnUi(() =>
            {
                if (_selectionOverlay != null)
                {
                    RemoveSelectTemp(_selectionOverlay);
                }
            });
        }

        private void HandleRawMouseMove(MouseEventArgs e)
        {
            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _currentLatLng = latlng;

            if (_startLatLng == null) return;

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

                RemoveSelectTemp(_selectionOverlay);

                var poly = new GMapPolygon(corners, "select_temp") { Stroke = new Pen(Color.Blue, 2), Fill = new SolidBrush(Color.FromArgb(30, Color.Blue)) };
                _selectionOverlay.Polygons.Add(poly);

                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], GMarkerGoogleType.red_small) { Tag = $"select_temp_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });
        }

        private void HandleRawMouseUp(MouseEventArgs e)
        {
            _isDragging = false;

            var snapped = SnapToMeterGrid(_gmap, e.X, e.Y);
            var latlng = _gmap.FromLocalToLatLng(snapped.X, snapped.Y);
            _currentLatLng = latlng;

            if (_startLatLng == null) return;

            var a = _startLatLng.Value;
            var b = _currentLatLng.Value;

            double aLat = Math.Round(a.Lat * 1e5) / 1e5;
            double aLng = Math.Round(a.Lng * 1e5) / 1e5;
            double bLat = Math.Round(b.Lat * 1e5) / 1e5;
            double bLng = Math.Round(b.Lng * 1e5) / 1e5;

            var pA = Position.FromRaw(aLng, aLat);
            var pB = Position.FromRaw(bLng, bLat);

            var range = PositionRange.FromUnsort(pA, pB);

            _ranges.Add(range);

            EnsureOnUi(() =>
            {
                if (_selectionOverlay == null) return;

                RemoveSelectTemp(_selectionOverlay);

                int index = _ranges.Count - 1;
                var isSecond = index == 1;
                var rectColor = isSecond ? Color.Blue : Color.Red;
                var markerType = isSecond ? GMarkerGoogleType.blue_small : GMarkerGoogleType.red_small;

                var corners = new List<PointLatLng>
                {
                    new PointLatLng(Math.Min(aLat,bLat), Math.Min(aLng,bLng)),
                    new PointLatLng(Math.Max(aLat,bLat), Math.Min(aLng,bLng)),
                    new PointLatLng(Math.Max(aLat,bLat), Math.Max(aLng,bLng)),
                    new PointLatLng(Math.Min(aLat,bLat), Math.Max(aLng,bLng))
                };

                var poly = new GMapPolygon(corners, $"select_final_{index}") { Stroke = new Pen(rectColor, 2), Fill = new SolidBrush(Color.FromArgb(40, rectColor)) };
                _selectionOverlay.Polygons.Add(poly);

                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], markerType) { Tag = $"select_final_{index}_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });
        }

        #endregion

        #region Tile Mode Handlers

        private void HandleTileMouseDown(MouseEventArgs e)
        {
            if (_tileRanges.Count >= _tileSelectRegionNum)
            {
                _tileRanges.Clear();
                EnsureOnUi(() =>
                {
                    if (_selectionOverlay != null)
                    {
                        var oldPolys = _selectionOverlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("tile_")).ToList();
                        foreach (var p in oldPolys) _selectionOverlay.Polygons.Remove(p);
                        var oldMarks = _selectionOverlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("tile_")).ToList();
                        foreach (var m in oldMarks) _selectionOverlay.Markers.Remove(m);
                    }
                });
            }

            _isDragging = true;

            var latlng = _gmap.FromLocalToLatLng(e.X, e.Y);
            var position = Position.FromGmap(latlng);
            _tileStart = Tile.From(_tileSize, position);
            _tileCurrent = _tileStart;

            EnsureOnUi(() =>
            {
                if (_selectionOverlay != null)
                {
                    RemoveTileTemp(_selectionOverlay);
                }
            });
        }

        private void HandleTileMouseMove(MouseEventArgs e)
        {
            var latlng = _gmap.FromLocalToLatLng(e.X, e.Y);
            var position = Position.FromGmap(latlng);
            _tileCurrent = Tile.From(_tileSize, position);

            if (_tileStart == null) return;

            var st = _tileStart.Value;
            var ct = _tileCurrent.Value;

            uint minX = Math.Min(st.X, ct.X);
            uint maxX = Math.Max(st.X, ct.X);
            uint minY = Math.Min(st.Y, ct.Y);
            uint maxY = Math.Max(st.Y, ct.Y);

            var minPos = Tile.From(_tileSize, minX, minY).Range.Min;
            var maxPos = Tile.From(_tileSize, maxX, maxY).Range.Max;

            var corners = new List<PointLatLng>
            {
                minPos.ToGmap(),
                Position.From(maxPos.X, minPos.Y).ToGmap(),
                maxPos.ToGmap(),
                Position.From(minPos.X, maxPos.Y).ToGmap()
            };

            EnsureOnUi(() =>
            {
                if (_selectionOverlay == null) return;

                RemoveTileTemp(_selectionOverlay);

                var poly = new GMapPolygon(corners, "tile_temp") { Stroke = new Pen(Color.Green, 2), Fill = new SolidBrush(Color.FromArgb(30, Color.Green)) };
                _selectionOverlay.Polygons.Add(poly);

                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], GMarkerGoogleType.green_small) { Tag = $"tile_temp_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });
        }

        private void HandleTileMouseUp(MouseEventArgs e)
        {
            _isDragging = false;

            var latlng = _gmap.FromLocalToLatLng(e.X, e.Y);
            var position = Position.FromGmap(latlng);
            _tileCurrent = Tile.From(_tileSize, position);

            if (_tileStart == null) return;

            var st = _tileStart.Value;
            var ct = _tileCurrent.Value;

            uint minX = Math.Min(st.X, ct.X);
            uint maxX = Math.Max(st.X, ct.X);
            uint minY = Math.Min(st.Y, ct.Y);
            uint maxY = Math.Max(st.Y, ct.Y);

            var minTile = Tile.From(_tileSize, minX, minY);
            var maxTile = Tile.From(_tileSize, maxX, maxY);

            var range = PositionRange.FromUnsort(minTile.Range.Min, maxTile.Range.Max);

            _tileRanges.Add(range);

            var minPos = minTile.Range.Min;
            var maxPos = maxTile.Range.Max;

            var corners = new List<PointLatLng>
            {
                minPos.ToGmap(),
                Position.From(maxPos.X, minPos.Y).ToGmap(),
                maxPos.ToGmap(),
                Position.From(minPos.X, maxPos.Y).ToGmap()
            };

            EnsureOnUi(() =>
            {
                if (_selectionOverlay == null) return;

                RemoveTileTemp(_selectionOverlay);

                int index = _tileRanges.Count - 1;
                var isSecond = index == 1;
                var rectColor = isSecond ? Color.Blue : Color.Green;
                var markerType = isSecond ? GMarkerGoogleType.blue_small : GMarkerGoogleType.green_small;

                var poly = new GMapPolygon(corners, $"tile_final_{index}") { Stroke = new Pen(rectColor, 2), Fill = new SolidBrush(Color.FromArgb(40, rectColor)) };
                _selectionOverlay.Polygons.Add(poly);

                for (int i = 0; i < corners.Count; i++)
                {
                    var m = new GMarkerGoogle(corners[i], markerType) { Tag = $"tile_final_{index}_{i}" };
                    _selectionOverlay.Markers.Add(m);
                }

                if (!_gmap.Overlays.Contains(_selectionOverlay)) _gmap.Overlays.Add(_selectionOverlay);
            });
        }

        #endregion

        #region Helpers

        private void SubscribeEvents()
        {
            try { _gmap.MouseDown -= OnMapMouseDown; } catch { }
            try { _gmap.MouseMove -= OnMapMouseMove; } catch { }
            try { _gmap.MouseUp -= OnMapMouseUp; } catch { }
            try { _gmap.MouseLeave -= OnMapMouseLeave; } catch { }

            _gmap.MouseDown += OnMapMouseDown;
            _gmap.MouseMove += OnMapMouseMove;
            _gmap.MouseUp += OnMapMouseUp;
            _gmap.MouseLeave += OnMapMouseLeave;
        }

        private void UnsubscribeEvents()
        {
            try { _gmap.MouseDown -= OnMapMouseDown; } catch { }
            try { _gmap.MouseMove -= OnMapMouseMove; } catch { }
            try { _gmap.MouseUp -= OnMapMouseUp; } catch { }
            try { _gmap.MouseLeave -= OnMapMouseLeave; } catch { }
        }

        private void RemoveSelectTemp(GMapOverlay overlay)
        {
            var tempPolys = overlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("select_temp")).ToList();
            foreach (var p in tempPolys) overlay.Polygons.Remove(p);

            var tempMarks = overlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("select_temp")).ToList();
            foreach (var m in tempMarks) overlay.Markers.Remove(m);
        }

        private void RemoveTileTemp(GMapOverlay overlay)
        {
            var tempPolys = overlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("tile_temp")).ToList();
            foreach (var p in tempPolys) overlay.Polygons.Remove(p);

            var tempMarks = overlay.Markers.Where(m => m.Tag != null && m.Tag.ToString().StartsWith("tile_temp")).ToList();
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
            var latLng = gmap.FromLocalToLatLng(x, y);

            double snappedLat = Math.Floor(latLng.Lat * 1e5) / 1e5;
            double snappedLng = Math.Floor(latLng.Lng * 1e5) / 1e5;

            var snappedPoint = gmap.FromLatLngToLocal(new PointLatLng(snappedLat, snappedLng));

            return new Point((int)snappedPoint.X, (int)snappedPoint.Y);
        }

        #endregion
    }
}
