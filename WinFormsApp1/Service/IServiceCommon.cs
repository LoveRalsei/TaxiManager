using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public interface IServiceCommon
    {
        public static IServiceCommon Instance => ServiceCommon.Instance;
        public abstract DateTime GetMinTime();
        public abstract DateTime GetMaxTime();
        
        /// <summary>
        /// 判断从Position a到Position b的有向线段是否经过两个PositionRange范围，以及经过的顺序
        /// </summary>
        /// <param name="a">线段起点</param>
        /// <param name="b">线段终点</param>
        /// <param name="range1">第一个位置范围</param>
        /// <param name="range2">第二个位置范围</param>
        /// <returns>
        /// 返回值说明：
        /// - 0: 线段不经过任何一个范围
        /// - 1: 只经过range1
        /// - 2: 只经过range2
        /// - 12: 先经过range1，再经过range2
        /// - 21: 先经过range2，再经过range1
        /// </returns>
        public abstract int CheckLinePassThroughTwoRanges(Position a, Position b, PositionRange range1, PositionRange range2);

        public abstract Color GetHotColor(float percent, float yellowPercent = 0.5f);

        /// <summary>
        /// 获取从起点到终点直线经过的所有大小为1的瓦片
        /// </summary>
        /// <param name="ignoreArguments">0: 无忽略, 1: 忽略头, 2: 忽略尾, 3: 忽略头尾</param>
        public abstract List<Tile> GetTilesOnLine(byte tileSize, Position from, Position to, int ignoreArguments = 0);
    }
}
