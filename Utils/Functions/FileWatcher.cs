using System; 
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq; 
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Utils.IO;
using Utils.Helps;

namespace Utils.Functions
{
    public class FileWatcher : IDisposable
    {
        private FileSystemWatcher _fileSystemWatcher = null;

        private List<string> _lockFileList = null;
        private readonly object _lockObj = new object();
        private System.Timers.Timer _lockFileTimer = null;

        private bool _isDealDeleteFolderError = false;
        private CancellationTokenSource _cancellationTokenSource = null;

        public object Tag { get; set; }

        public bool IsEnabled
        {
            get { return _fileSystemWatcher.EnableRaisingEvents; }
            set
            {
                if (value != _fileSystemWatcher.EnableRaisingEvents)
                    SetIsEnabled(value);
            }
        }

        public int InternalBufferSize
        {
            get { return _fileSystemWatcher.InternalBufferSize; }
            set { _fileSystemWatcher.InternalBufferSize = value; }
        }

        public bool IncludeSubdirectories
        {
            get { return _fileSystemWatcher.IncludeSubdirectories; }
            set { _fileSystemWatcher.IncludeSubdirectories = value; }
        }

        private bool _isFilterExistingFiles;
        public bool IsFilterExistingFiles
        {
            get { return _isFilterExistingFiles; }
            set
            {
                if (!value && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                _isFilterExistingFiles = value;
            }
        }

        /// <summary>
        /// 默认 NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes
        /// </summary>
        public NotifyFilters NotifyFilter
        {
            get { return _fileSystemWatcher.NotifyFilter; }
            set { _fileSystemWatcher.NotifyFilter = value; }
        }

        /// <summary>
        /// 1.路径中无作业
        ///  （1）删除该路径，触发Error事件；
        ///  （2）重命名该路径，不触发任何事件，还原后，依然可用。
        /// 2.路径中有作业
        ///  （1）删除该路径，只有NotifyFilter中指定了 NotifyFilters.Attributes，方可触发Changed事件；
        ///  （2）重命名该路径，不触发任何事件，还原后，依然可用。
        /// </summary>
        public string Path
        {
            get { return _fileSystemWatcher.Path; }
            set
            {
                CheckPath(value);
                _fileSystemWatcher.Path = value;
            }
        }

        private string[] _filterArray;
        public string Filter
        {
            get
            {
                if (_filterArray == null || _filterArray.Length == 0)
                    return string.Empty;

                return string.Join("|", _filterArray);
            }
            set
            {
                string filter = null;
                if (CheckFilter(value, ref filter))
                    _filterArray = filter.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        #region Constructor

        public FileWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher();
            InitParams();
        }

        public FileWatcher(string path, bool isFilterExistingFiles = false)
            : this(path, "*.*", isFilterExistingFiles)
        { }

        public FileWatcher(string path, string filter = "*.*", bool isFilterExistingFiles = false)
        {
            CheckPath(path);

            Filter = filter;
            _isFilterExistingFiles = isFilterExistingFiles;
            _fileSystemWatcher = new FileSystemWatcher(path, "*.*");

            InitParams();
        }

        #endregion

        #region Method

        private void InitParams()
        {
            _lockFileList = new List<string>();
            _lockFileTimer = new System.Timers.Timer(600);

            NotifyFilter |= NotifyFilters.Attributes;
            SubscribeEvents();
        }

        private void CheckPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0 || !Directory.Exists(path))
                throw new ArgumentException("The path is invalid. Path = " + path);
        }

        private bool CheckFilter(string value, ref string filter)
        {
            if (value == null)
                throw new ArgumentNullException("Filter");

            if (string.IsNullOrEmpty(value))
            {
                filter = "*.*";
                return true;
            }

            if (string.Compare(Filter, value, StringComparison.OrdinalIgnoreCase) != 0)
            {
                filter = value;
                return true;
            }

            return false;
        }

        private bool MatchPattern(string filter, string relativePath)
        {
            var name = System.IO.Path.GetFileName(relativePath);
            return name != null && PatternMatcher.StrictMatchPattern(filter.ToUpper(CultureInfo.InvariantCulture), name.ToUpper(CultureInfo.InvariantCulture));
        }

        private bool FilterFile(string relativePath)
        {
            if (_filterArray == null || _filterArray.Length == 0)
                return true;

            return Array.Exists(_filterArray, filter => MatchPattern(filter, relativePath));
        }

        private void SetIsEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                CheckPath(Path);

                var fileList = new List<string>();
                GetFilterFiles(Path, fileList);

                _lockFileList.Clear();
                _lockFileTimer.Enabled = true;
                _isDealDeleteFolderError = false;
                _fileSystemWatcher.EnableRaisingEvents = true;

                if (fileList.Count > 0)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    Task.Run(() =>
                    {
                        try
                        {
                            foreach (var file in fileList)
                            {
                                if (_cancellationTokenSource.IsCancellationRequested)
                                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                                if (!FileUtil.CanAccessFile(file))
                                {
                                    lock (_lockObj)
                                        _lockFileList.Add(file);

                                    continue;
                                }

                                OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileName(file)));
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            Trace.TraceWarning("[ FileWatcher ] Cancel the Task.Run(...)");
                        }

                        fileList.Clear();
                        fileList = null;

                    }, _cancellationTokenSource.Token);
                }
            }
            else
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource.Cancel();

                _lockFileTimer.Enabled = false;
                _isDealDeleteFolderError = false;
                _fileSystemWatcher.EnableRaisingEvents = false;
            }
        }

        private void GetFilterFiles(string path, List<string> fileList)
        {
            if (fileList == null)
                fileList = new List<string>();

            fileList.AddRange(Directory.GetFiles(path).Where(file => FilterFile(file)));

            if (IncludeSubdirectories)
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    GetFilterFiles(directory, fileList);
                }
            }
        } 

        #endregion

        #region Event

        // Event handlers 
        private FileSystemEventHandler _onCreatedHandler = null;
        public event FileSystemEventHandler Created
        {
            add { _onCreatedHandler += value; }
            remove { _onCreatedHandler -= value; }
        }

        private FileSystemEventHandler _onDeletedHandler = null;
        public event FileSystemEventHandler Deleted
        {
            add { _onDeletedHandler += value; }
            remove { _onDeletedHandler -= value; }
        }

        private RenamedEventHandler _onRenamedHandler = null;
        public event RenamedEventHandler Renamed
        {
            add { _onRenamedHandler += value; }
            remove { _onRenamedHandler -= value; }
        }

        private ErrorEventHandler _onErrorHandler = null;
        public event ErrorEventHandler Error
        {
            add { _onErrorHandler += value; }
            remove { _onErrorHandler -= value; }
        }

        private void UnSubscribeEvents()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.Renamed -= _fileSystemWatcher_Renamed;
                _fileSystemWatcher.Created -= _fileSystemWatcher_Created;
                _fileSystemWatcher.Deleted -= _fileSystemWatcher_Deleted;
                _fileSystemWatcher.Error -= _fileSystemWatcher_Error;
                _fileSystemWatcher.Changed -= _fileSystemWatcher_Changed;
            }

            if (_lockFileTimer != null)
                _lockFileTimer.Elapsed -= _lockFileTimer_Elapsed;
        }

        private void SubscribeEvents()
        {
            if (_fileSystemWatcher == null || _lockFileTimer == null)
                return;

            UnSubscribeEvents();

            _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            _fileSystemWatcher.Created += _fileSystemWatcher_Created;
            _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
            _fileSystemWatcher.Error += _fileSystemWatcher_Error;
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;

            _lockFileTimer.Elapsed += _lockFileTimer_Elapsed;
        }

        private void _lockFileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<string> nonLockFilesList = new List<string>();
            lock (_lockObj)
            {
                for (int i = 0; i < _lockFileList.Count; i++)
                {
                    var file = _lockFileList[i];
                    if (FileUtil.CanAccessFile(file))
                    {
                        _lockFileList.RemoveAt(i);
                        i--;

                        nonLockFilesList.Add(file);
                    }
                }
            }

            nonLockFilesList.ForEach(file => OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileName(file))));
        }

        private void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (!FilterFile(e.OldName) && !FilterFile(e.Name))
                return;

            var renamedHandler = _onRenamedHandler;
            if (renamedHandler != null)
            {
                if (_fileSystemWatcher.SynchronizingObject != null && _fileSystemWatcher.SynchronizingObject.InvokeRequired)
                    _fileSystemWatcher.SynchronizingObject.BeginInvoke(renamedHandler, new object[] { this, e });
                else
                    renamedHandler(this, e);
            }
        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!FilterFile(e.Name))
                return;

            if (!FileUtil.CanAccessFile(e.FullPath))
            {
                Trace.TraceWarning("[ FileWatcher ] Add lock file, File = " + e.FullPath);
                _lockFileList.Add(e.FullPath);

                return;
            }

            OnCreated(e);
        }

        private void OnCreated(FileSystemEventArgs e)
        {
            // To avoid ---- between remove handler and raising the event
            var createdHandler = _onCreatedHandler;
            if (createdHandler != null)
            {
                if (_fileSystemWatcher.SynchronizingObject != null && _fileSystemWatcher.SynchronizingObject.InvokeRequired)
                    _fileSystemWatcher.SynchronizingObject.BeginInvoke(createdHandler, new object[] { this, e });
                else
                    createdHandler(this, e);
            }
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!FilterFile(e.Name))
                return;

            var deletedHandler = _onDeletedHandler;
            if (deletedHandler != null)
            {
                if (_fileSystemWatcher.SynchronizingObject != null && _fileSystemWatcher.SynchronizingObject.InvokeRequired)
                    _fileSystemWatcher.SynchronizingObject.BeginInvoke(deletedHandler, new object[] { this, e });
                else
                    deletedHandler(this, e);
            }
        }

        private void _fileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            var errorHandler = _onErrorHandler;
            if (errorHandler != null)
            {
                IsEnabled = false;

                var args = !Directory.Exists(Path)
                    ? new ErrorEventArgs(new DirectoryNotFoundException("The path is invalid. Path = " + Path, e.GetException()))
                    : e;

                if (_fileSystemWatcher.SynchronizingObject != null && _fileSystemWatcher.SynchronizingObject.InvokeRequired)
                    _fileSystemWatcher.SynchronizingObject.BeginInvoke(errorHandler, new object[] { this, args });
                else
                    errorHandler(this, args);
            }
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(Path))
            {
                var errorHandler = _onErrorHandler;
                //IsDealDeleteFolderError 为了避免该异常触发多次（若Path中包含多个文件，删除该文件夹后，会多次触发该事件）
                if (!Directory.Exists(Path) && errorHandler != null && _isDealDeleteFolderError == false)
                {
                    _isDealDeleteFolderError = true;
                    IsEnabled = false;
                    var args = new ErrorEventArgs(new DirectoryNotFoundException("The path is invalid. Path = " + Path));

                    if (_fileSystemWatcher.SynchronizingObject != null && _fileSystemWatcher.SynchronizingObject.InvokeRequired)
                        _fileSystemWatcher.SynchronizingObject.BeginInvoke(errorHandler, new object[] { this, args });
                    else
                        errorHandler(this, args);
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            IsEnabled = false;
            UnSubscribeEvents();

            _lockFileTimer.Dispose();
            _lockFileTimer = null;

            _fileSystemWatcher.Dispose();
            _fileSystemWatcher = null;

            _lockFileList.Clear();
            _lockFileList = null;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
