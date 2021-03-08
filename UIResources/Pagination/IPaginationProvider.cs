using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIResources.Pagination
{
    public interface IPaginationProvider<T>: IDisposable
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// 获取指定页的记录
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="rowsPerPage">每页记录数</param>
        /// <returns>当前页的记录</returns>
        Task<IEnumerable<T>> GetPage(int pageIndex, int rowsPerPage);

        /// <summary>
        /// 重置，一般在源数据发生变化后，调用该方法，并重定向到首页
        /// </summary>
        /// <returns>总记录数</returns>
        Task<int> Reset();
    }
}
