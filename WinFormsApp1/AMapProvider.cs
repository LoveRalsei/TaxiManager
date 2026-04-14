using GMap.NET.MapProviders;
using GMap.NET.Projections;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public abstract class AMapProviderBase : GMapProvider
    {
public AMapProviderBase()
        {
            MaxZoom = null;
            MinZoom = 3;  // 限制最小缩放级别，避免地图太小
            RefererUrl = "http://www.amap.com/";
        }

        public override PureProjection Projection
        {
            get { return MercatorProjection.Instance; }
        }

        private GMapProvider[] _overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                _overlays ??= [this];
                return _overlays;
            }
        }
    }

    public class AMapProvider : AMapProviderBase
    {
        public static readonly AMapProvider Instance;

        readonly Guid id = new("1B784A77-2F13-9BDC-EBF6-89CD21CC52D7");
        public override Guid Id
        {
            get { return id; }
        }

        readonly string name = "AMap";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        static AMapProvider()
        {
            Instance = new AMapProvider();
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            try
            {
                string url = MakeTileImageUrl(pos, zoom, LanguageStr);
                return GetTileImageUsingHttp(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // zoom较大时额外放大
            int scale =  zoom > 15? 2 : 1;
            string url = string.Format(UrlFormat, pos.X, pos.Y, zoom, scale);
            return url;
        }

        // 高德地图瓦片URL模板
        static readonly string UrlFormat = "http://webrd01.is.autonavi.com/appmaptile?lang=zh_cn&size=1&scale={3}&style=7&x={0}&y={1}&z={2}";
    }
}
