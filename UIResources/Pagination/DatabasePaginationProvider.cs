using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;
using Utils.IO;

namespace UIResources.Pagination
{
    public class DatabasePaginationProvider<T> : IPaginationProvider<T> where T : class, new()
    {
        private string _database = null;
        private string _table = null;
        private string _condition = null;
        private DynamicParameters _param = null;

        private Func<(string condition, DynamicParameters param)> _filterFunc = null;

        private const string RowCountParamString = "rowCount";
        private const string RowIndexParamString = "rowIndex";

        public DatabasePaginationProvider(string database, string table, Func<(string condition, DynamicParameters param)> filterFunc = null)
        {
            _database = database;
            _table = table;
            _filterFunc = filterFunc;

            Init();
        }

        public int TotalCount { get; private set; }

        public async Task<IEnumerable<T>> GetPage(int pageIndex, int rowsPerPage)
        {
            if (TotalCount == 0 || pageIndex == 0 || rowsPerPage == 0)
                return null;

            return await DapperHelper.Instance.GetByCondition<T>(_database, _table, _condition, GetParam(pageIndex, rowsPerPage));
        }

        public async Task<int> Reset()
        {
            await Init();
            return TotalCount;
        }

        public void Dispose()
        {
            _param = null;
            _filterFunc = null;
        }

        private async Task Init()
        {
            if (_filterFunc != null)
            {
                var filter = _filterFunc();

                _condition = filter.condition;
                _param = string.IsNullOrWhiteSpace(filter.condition) ? null : filter.param;
            }

            //先根据查询条件获取总个数
            TotalCount = await DapperHelper.Instance.GetTotalCount(_database, _table, _condition, _param);

            //再将分页语句与查询语句拼接，为后续的提取分页数据做准备
            var pageSql = $"limit @{RowCountParamString} offset @{RowIndexParamString}";

            if (string.IsNullOrWhiteSpace(_condition))
                _condition = pageSql;
            else
                _condition = _condition.Trim() + " " + pageSql;
        }

        private object GetParam(int pageIndex, int rowsPerPage)
        {
            var rowCount = rowsPerPage;
            var rowIndex = rowsPerPage * (pageIndex - 1);

            if (_param == null)
                _param = new DynamicParameters();

            _param.Add(RowCountParamString, rowCount);
            _param.Add(RowIndexParamString, rowIndex);

            return _param;
        }
    }
}
