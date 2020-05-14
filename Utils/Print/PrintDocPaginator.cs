using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Utils.Print
{
    public class PrintDocPaginator : DocumentPaginator
    {
        private int _pageCount = 1;
        private Size _pageSize;

        private DocumentPage _prePage = null;
        private Func<int, int, Size, Visual> _getPage = null;

        public PrintDocPaginator(int pageCount, Size pageSize, Func<int, int, Size, Visual> getPage)
        {
            _pageCount = pageCount;
            _pageSize = pageSize;

            _getPage = getPage;
        }

        public void Dispose()
        {
            if (_prePage != null)
                _prePage.Dispose();

            _getPage = null;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            if (_getPage == null)
                return null;

            if (_prePage != null)
                _prePage.Dispose();

            _prePage = new DocumentPage(_getPage(pageNumber, _pageCount, _pageSize), _pageSize, new Rect(_pageSize), new Rect(_pageSize));
            return _prePage;
        }

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public override int PageCount
        {
            get { return _pageCount; }
        }

        public override Size PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }
    }
}

