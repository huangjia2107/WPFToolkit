using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UIResources.Controls
{
    /// <summary>
    /// <PackageReference Include="LibVLCSharp.WPF" Version="3.4.9" />
    /// <PackageReference Include = "VideoLAN.LibVLC.Windows" Version="3.0.12" />
    /// </summary>
    /*
    [TemplatePart(Name = MediaElementTemplateName, Type = typeof(VideoView))]
    [TemplatePart(Name = SliderTemplateName, Type = typeof(Slider))]
    [TemplatePart(Name = PlayButtonTemplateName, Type = typeof(StatusToggle))]
    [TemplatePart(Name = FastBackwardButtonTemplateName, Type = typeof(Button))]
    [TemplatePart(Name = FastForwardButtonTemplateName, Type = typeof(Button))]
    public class VlcMediaPlayer : Control
    {
        private static readonly Type _typeofSelf = typeof(VlcMediaPlayer);

        private const string MediaElementTemplateName = "PART_MediaElement";
        private const string SliderTemplateName = "PART_Slider";
        private const string PlayButtonTemplateName = "PART_PlayButton";

        private const string FastBackwardButtonTemplateName = "PART_FastBackwardButton";
        private const string FastForwardButtonTemplateName = "PART_FastForwardButton";

        private VideoView _mediaElement = null;
        private Slider _slider = null;
        private StatusToggle _statusToggle = null;
        private Button _fastBackwardButton = null;
        private Button _fastForwardButton = null;

        private bool _isDragging = false;
        private bool _isAutoPlay = false;

        //VLC
        private LibVLC _libVLC = null;
        private LibVLCSharp.Shared.MediaPlayer _innerPlayer = null;

        static MediaPlayer()
        {
            Core.Initialize();

            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaPlayer), new FrameworkPropertyMetadata(typeof(MediaPlayer)));
        }

        #region Readonly Properties

        private static readonly DependencyPropertyKey PlayTimePropertyKey =
           DependencyProperty.RegisterReadOnly("PlayTime", typeof(TimeSpan), _typeofSelf, new PropertyMetadata(TimeSpan.Zero));
        public static readonly DependencyProperty PlayTimeProperty = PlayTimePropertyKey.DependencyProperty;
        public TimeSpan PlayTime
        {
            get { return (TimeSpan)GetValue(PlayTimeProperty); }
        }

        private static readonly DependencyPropertyKey RemainTimePropertyKey =
           DependencyProperty.RegisterReadOnly("RemainTime", typeof(TimeSpan), _typeofSelf, new PropertyMetadata(TimeSpan.Zero));
        public static readonly DependencyProperty RamainTimeProperty = RemainTimePropertyKey.DependencyProperty;
        public TimeSpan RamainTime
        {
            get { return (TimeSpan)GetValue(RamainTimeProperty); }
        }

        private static readonly DependencyPropertyKey IsBufferingPropertyKey =
           DependencyProperty.RegisterReadOnly("IsBuffering", typeof(bool), _typeofSelf, new PropertyMetadata(false));
        public static readonly DependencyProperty IsBufferingProperty = IsBufferingPropertyKey.DependencyProperty;
        public bool IsBuffering
        {
            get { return (bool)GetValue(IsBufferingProperty); }
        }

        private static readonly DependencyPropertyKey BufferingProgressPropertyKey =
           DependencyProperty.RegisterReadOnly("BufferingProgress", typeof(float), _typeofSelf, new PropertyMetadata(0f));
        public static readonly DependencyProperty BufferingProgressProperty = BufferingProgressPropertyKey.DependencyProperty;
        public float BufferingProgress
        {
            get { return (float)GetValue(BufferingProgressProperty); }
        }

        private static readonly DependencyPropertyKey IsVideoViewVisiblePropertyKey =
           DependencyProperty.RegisterReadOnly("IsVideoViewVisible", typeof(bool), _typeofSelf, new PropertyMetadata(false));
        public static readonly DependencyProperty IsVideoViewVisibleProperty = IsVideoViewVisiblePropertyKey.DependencyProperty;
        public bool IsVideoViewVisible
        {
            get { return (bool)GetValue(IsVideoViewVisibleProperty); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), _typeofSelf, new PropertyMetadata(null, OnSourcePropertyChanged));
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as MediaPlayer;
            ctrl.SourceChanged();
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnsubscribeEvents();

            _mediaElement = this.GetTemplateChild(MediaElementTemplateName) as VideoView;
            _slider = this.GetTemplateChild(SliderTemplateName) as Slider;
            _statusToggle = this.GetTemplateChild(PlayButtonTemplateName) as StatusToggle;

            _fastForwardButton = this.GetTemplateChild(FastForwardButtonTemplateName) as Button;
            _fastBackwardButton = this.GetTemplateChild(FastBackwardButtonTemplateName) as Button;

            Reset();
            InitVLC();

            SubscribeEvents();
        }

        #endregion

        #region Private

        /// <summary>
        /// 重置
        /// </summary>
        private void Reset()
        {
            _isDragging = false;
            _isAutoPlay = false;

            InvokeUI(() =>
            {
                if (_slider != null)
                    _slider.Value = 0;

                SetValue(PlayTimePropertyKey, new TimeSpan());
            });
        }

        /// <summary>
        /// 切换源后的行为
        /// </summary>
        private void SourceChanged()
        {
            //源为空，则切换到 “非 VLC” 模板
            if (Source == null)
            {
                Dispose();

                SetValue(IsVideoViewVisiblePropertyKey, false);
                return;
            }

            //VLC 未初始化，则切换到 VLC 模板
            if (_mediaElement == null || _innerPlayer == null)
            {
                SetValue(IsVideoViewVisiblePropertyKey, true);
                return;
            }

            //先停止，后播放
            var media = new Media(_libVLC, Source);
            Task.Run(() =>
            {
                if (_innerPlayer.IsPlaying)
                    _innerPlayer.Stop();

                _innerPlayer.Media?.Dispose();
                _innerPlayer.Media = null;

                Reset();
                _innerPlayer.Play(media);
            });
        }

        /// <summary>
        /// 初始化 VLC
        /// </summary>
        private void InitVLC()
        {
            if (!IsVideoViewVisible)
                return;

            if (_libVLC == null)
                _libVLC = new LibVLC();

            if (_innerPlayer == null && _libVLC != null)
                _innerPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC) { EnableHardwareDecoding = true };

            if (_mediaElement != null)
                _mediaElement.MediaPlayer = _innerPlayer;
        }

        private void UpdateSliderValue()
        {
            //ms
            var duration = _innerPlayer.Length;
            if (duration < 0)
                return;

            //0-1 percent
            var position = _innerPlayer.Position;

            //拖动时，只更新时间，Slider的值，由拖动行为决定
            if (_isDragging)
            {
                BeginInvokeUI(() =>
                {
                    SetValue(PlayTimePropertyKey, TimeSpan.FromMilliseconds(duration * position));
                    SetValue(RemainTimePropertyKey, TimeSpan.FromMilliseconds(duration * (1 - position)));
                });
            }
            else
                BeginInvokeUI(() =>
                {
                    _isAutoPlay = true;

                    if (_slider != null)
                        _slider.Value = position;

                    _isAutoPlay = false;
                });
        }

        private void UpdateTimeBySliderValue(double value)
        {
            //ms
            var duration = _innerPlayer.Length;
            if (duration < 0)
                return;

            if (!_isAutoPlay)
                _innerPlayer.Position = (float)value;

            SetValue(PlayTimePropertyKey, TimeSpan.FromMilliseconds(duration * value));
            SetValue(RemainTimePropertyKey, TimeSpan.FromMilliseconds(duration * (1 - value)));
        }

        private void UnsubscribeEvents()
        {
            if (_mediaElement != null)
                _mediaElement.Loaded -= MediaElement_Loaded;

            if (_innerPlayer != null)
            {
                _innerPlayer.PositionChanged -= MediaPlayer_PositionChanged;
                _innerPlayer.Opening -= MediaPlayer_Opening;
                _innerPlayer.Buffering -= MediaPlayer_Buffering;
                _innerPlayer.Stopped -= MediaPlayer_Stopped;
                _innerPlayer.Playing -= MediaPlayer_Playing;
                _innerPlayer.Paused -= MediaPlayer_Paused;
            }

            if (_slider != null)
            {
                _slider.ValueChanged -= OnSliderValueChanged;
                _slider.MouseMove -= OnSliderMouseMove;
                _slider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSliderMouseLeftButtonUp));
            }

            if (_statusToggle != null)
                _statusToggle.Click -= PlayButton_Click;

            if (_fastBackwardButton != null)
                _fastBackwardButton.Click -= FastBackwardButton_Click;

            if (_fastForwardButton != null)
                _fastForwardButton.Click -= FastForwardButton_Click;
        }

        private void SubscribeEvents()
        {
            if (_mediaElement != null)
                _mediaElement.Loaded += MediaElement_Loaded;

            if (_innerPlayer != null)
            {
                _innerPlayer.PositionChanged += MediaPlayer_PositionChanged;
                _innerPlayer.Opening += MediaPlayer_Opening;
                _innerPlayer.Buffering += MediaPlayer_Buffering;
                _innerPlayer.Stopped += MediaPlayer_Stopped;
                _innerPlayer.Playing += MediaPlayer_Playing;
                _innerPlayer.Paused += MediaPlayer_Paused;
            }

            if (_slider != null)
            {
                _slider.ValueChanged += OnSliderValueChanged;
                _slider.MouseMove += OnSliderMouseMove;
                _slider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSliderMouseLeftButtonUp), true);
            }

            if (_statusToggle != null)
                _statusToggle.Click += PlayButton_Click;

            if (_fastBackwardButton != null)
                _fastBackwardButton.Click += FastBackwardButton_Click;

            if (_fastForwardButton != null)
                _fastForwardButton.Click += FastForwardButton_Click;
        }

        private void BeginInvokeUI(Action action)
        {
            if (action == null)
                return;

            if (this.CheckAccess())
                action.Invoke();
            else
                this.Dispatcher.BeginInvoke(action);
        }

        private void InvokeUI(Action action)
        {
            if (action == null)
                return;

            if (this.CheckAccess())
                action.Invoke();
            else
                this.Dispatcher.Invoke(action);
        }

        #endregion

        #region Public

        public void Dispose()
        {
            UnsubscribeEvents();

            Task.Run(() =>
            {
                _innerPlayer?.Stop();
                _innerPlayer?.Media?.Dispose();

                if (_innerPlayer != null)
                    _innerPlayer.Media = null;

                _innerPlayer?.Dispose();
                _innerPlayer = null;

                _libVLC?.Dispose();
                _libVLC = null;
            });
        }

        #endregion

        #region Event

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (Source == null || _libVLC == null || _innerPlayer == null)
                return;

            var media = new Media(_libVLC, Source);
            Task.Run(() =>
            {
                if (_innerPlayer.IsPlaying)
                    _innerPlayer.Stop();

                _innerPlayer.Media?.Dispose();
                _innerPlayer.Media = null;

                _innerPlayer.Play(media);
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_innerPlayer == null || Source == null)
                return;

            if (_innerPlayer.Media != null)
            {
                if (_innerPlayer.Media.State == VLCState.Playing)
                    Task.Run(() => _innerPlayer.SetPause(true));

                if (_innerPlayer.Media.State == VLCState.Paused)
                    Task.Run(() => _innerPlayer.SetPause(false));

                if (_innerPlayer.Media.State == VLCState.Ended)
                    _innerPlayer.Play(_innerPlayer.Media);

                return;
            }

            _innerPlayer.Play(new Media(_libVLC, Source));
        }

        private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            _innerPlayer.Position = Math.Min(1, _innerPlayer.Position + 5000f / _innerPlayer.Length);
            UpdateSliderValue();
        }

        private void FastBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            var backwardPos = _innerPlayer.Position - 5000f / _innerPlayer.Length;

            _innerPlayer.Position = backwardPos < 0 ? 0 : backwardPos;
            UpdateSliderValue();
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            BeginInvokeUI(() =>
            {
                if (_statusToggle != null)
                    _statusToggle.IsChecked = false;
            });
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            BeginInvokeUI(() =>
            {
                if (_statusToggle != null)
                    _statusToggle.IsChecked = true;
            });
        }

        private void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            //防止访问 _innerPlayer 的时候，已经被注销
            var playerLength = _innerPlayer?.Length;

            BeginInvokeUI(() =>
            {
                if (_statusToggle != null)
                    _statusToggle.IsChecked = false;

                if (_slider != null)
                    _slider.Value = 0;

                if (playerLength.HasValue)
                    SetValue(RemainTimePropertyKey, TimeSpan.FromMilliseconds(playerLength.Value));
            });
        }

        private void MediaPlayer_Buffering(object sender, MediaPlayerBufferingEventArgs e)
        {
            BeginInvokeUI(() =>
            {
                SetValue(BufferingProgressPropertyKey, e.Cache);
                SetValue(IsBufferingPropertyKey, DoubleUtil.LessThan(e.Cache, 100));
            });
        }

        private void MediaPlayer_Opening(object sender, EventArgs e)
        {
            //防止访问 _innerPlayer 的时候，已经被注销
            var playerLength = _innerPlayer?.Length;

            BeginInvokeUI(() =>
            {
                if (_slider != null)
                    _slider.Value = 0;

                if (playerLength.HasValue)
                    SetValue(RemainTimePropertyKey, TimeSpan.FromMilliseconds(playerLength.Value));
            });
        }

        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            UpdateSliderValue();
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isDragging)
                UpdateTimeBySliderValue(e.NewValue);
        }

        private void OnSliderMouseMove(object sender, MouseEventArgs e)
        {
            //当鼠标点击非滑块处，直接拖动
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;

                if (!(e.OriginalSource is Thumb))
                {
                    _slider.CaptureMouse();
                    _slider.Value = _slider.Minimum + e.GetPosition(_slider).X / _slider.ActualWidth * (_slider.Maximum - _slider.Minimum);
                }
            }

            e.Handled = true;
        }

        private void OnSliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _slider.ReleaseMouseCapture();

                UpdateTimeBySliderValue(_slider.Value);

                e.Handled = true;
            }
        }

        #endregion
    }
    */
}
