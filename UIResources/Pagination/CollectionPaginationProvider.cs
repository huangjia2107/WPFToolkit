using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIResources.Pagination
{
    public class CollectionPaginationProvider<T> : IPaginationProvider<T>
    {
        private IEnumerable<T> _source = null;

        private Func<IEnumerable<T>> _getSource = null;
        private Predicate<T> _predicate = null;

        public CollectionPaginationProvider(Func<IEnumerable<T>> getSource, Predicate<T> predicate = null)
        {
            _getSource = getSource;
            _predicate = predicate;

            Init();
        }

        public int TotalCount { get; private set; }

        public async Task<IEnumerable<T>> GetPage(int pageIndex, int rowsPerPage)
        {
            if (TotalCount == 0 || pageIndex == 0 || rowsPerPage == 0)
                return null;

            if (_predicate == null)
                return _source.Skip((pageIndex - 1) * rowsPerPage).Take(rowsPerPage);

            return _source.Where(t => _predicate(t)).Skip((pageIndex - 1) * rowsPerPage).Take(rowsPerPage);
        }

        public async Task<int> Reset()
        {
            Init();
            return TotalCount;
        }

        public void Dispose()
        {
            _source = null;
            _predicate = null;
        }

        private void Init()
        {
            if(_getSource != null)
                _source = _getSource();

            if (_source == null)
            {
                TotalCount = 0;
                return;
            }

            if (_predicate == null)
            {
                TotalCount = _source.Count();
                return;
            }

            TotalCount = _source.Count(t => _predicate(t));
        }
    }
}
