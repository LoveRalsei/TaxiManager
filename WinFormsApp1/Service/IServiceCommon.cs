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
    }
}
