using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace UIResources.Controls
{
    [TemplatePart(Name = MediaElementTemplateName, Type = typeof(MediaElement))]
    [TemplatePart(Name = SliderTemplateName, Type = typeof(Slider))]
    public class MediaPlayer : Control
    {
        private static readonly Type _typeofSelf = typeof(MediaPlayer);

        private static RoutedCommand _playCommand = null;
        private static RoutedCommand _fastForwardCommand = null;
        private static RoutedCommand _fastBackwardCommand = null;

        private const string MediaElementTemplateName = "PART_MediaElement";
        private const string SliderTemplateName = "PART_Slider";

        private MediaElement _mediaElement = null;
        private Slider _slider = null;

        private DispatcherTimer _timer = null;
        private bool _isDragging = false;

        public MediaPlayer()
        {
            InitTimer();
        }

        static MediaPlayer()
        {
            InitializeCommands();

            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaPlayer), new FrameworkPropertyMetadata(typeof(MediaPlayer)));
        }

        #region Command

        private static void InitializeCommands()
        {
            _playCommand = new RoutedCommand("Play", _typeofSelf);
            _fastForwardCommand = new RoutedCommand("FastForward", _typeofSelf);
            _fastBackwardCommand = new RoutedCommand("FastBackward", _typeofSelf);

            CommandManager.RegisterClassCommandBinding(_typeofSelf, new CommandBinding(_playCommand, OnPalyCommand));
            CommandManager.RegisterClassCommandBinding(_typeofSelf, new CommandBinding(_fastForwardCommand, OnFastForwardCommand));
            CommandManager.RegisterClassCommandBinding(_typeofSelf, new CommandBinding(_fastBackwardCommand, OnFastBackwardCommand));
        }

        public static RoutedCommand PlayCommand
        {
            get { return _playCommand; }
        }

        public static RoutedCommand FastForwardCommand
        {
            get { return _fastForwardCommand; }
        }

        public static RoutedCommand FastBackwardCommand
        {
            get { return _fastBackwardCommand; }
        }

        private static void OnPalyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mediaPlayer = sender as MediaPlayer;
            var isPause = (bool)e.Parameter;

            if (isPause)
            {
                mediaPlayer._timer.Start();
                mediaPlayer._mediaElement.Play();
            }
            else
            {
                mediaPlayer._mediaElement.Pause();
                mediaPlayer._timer.Stop();
            }
        }

        private static void OnFastForwardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mediaPlayer = sender as MediaPlayer;

            mediaPlayer._mediaElement.Position = mediaPlayer._mediaElement.Position + TimeSpan.FromSeconds(5);
            mediaPlayer.UpdateSliderValue();
        }

        private static void OnFastBackwardCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mediaPlayer = sender as MediaPlayer;
            var backwardPos = mediaPlayer._mediaElement.Position - TimeSpan.FromSeconds(5);

            mediaPlayer._mediaElement.Position = backwardPos < TimeSpan.Zero ? TimeSpan.Zero : backwardPos;
            mediaPlayer.UpdateSliderValue();
        }

        #endregion

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

        #endregion

        #region Properties


        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), _typeofSelf);
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UnsubscribeEvents();

            _mediaElement = this.GetTemplateChild(MediaElementTemplateName) as MediaElement;
            _slider = this.GetTemplateChild(SliderTemplateName) as Slider;

            SubscribeEvents();
        }

        #endregion

        #region Private

        private void InitTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            UpdateSliderValue();
        }

        private void UpdateSliderValue()
        {
            if (!_mediaElement.NaturalDuration.HasTimeSpan)
                return;

            var duration = _mediaElement.NaturalDuration.TimeSpan;

            //拖动时，只更新时间，Slider的值，由拖动行为决定
            if (_isDragging)
            {
                var position = _mediaElement.Position;

                SetValue(PlayTimePropertyKey, position);
                SetValue(RemainTimePropertyKey, duration - position);
            }
            else
                _slider.Value = _mediaElement.Position.TotalSeconds / duration.TotalSeconds;
        }

        private void UpdateTimeBySliderValue(double value)
        {
            if (!_mediaElement.NaturalDuration.HasTimeSpan)
                return;

            var duration = _mediaElement.NaturalDuration.TimeSpan;
            var playTime = TimeSpan.FromSeconds(duration.TotalSeconds * value);

            _mediaElement.Position = playTime;

            SetValue(PlayTimePropertyKey, playTime);
            SetValue(RemainTimePropertyKey, duration - playTime);
        }

        private void UnsubscribeEvents()
        {
            if (_mediaElement != null)
            {
                _mediaElement.MediaOpened -= OnMediaOpened;
                _mediaElement.MediaEnded -= OnMediaEnded;
            }

            if (_slider != null)
            {
                _slider.ValueChanged -= OnSliderValueChanged;
                _slider.MouseMove -= OnSliderMouseMove;
                _slider.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSliderMouseLeftButtonUp));
            }
        }

        private void SubscribeEvents()
        {
            if (_mediaElement != null)
            {
                _mediaElement.MediaOpened += OnMediaOpened;
                _mediaElement.MediaEnded += OnMediaEnded;
                _mediaElement.BufferingStarted += OnBufferingStarted;
                _mediaElement.BufferingEnded += OnBufferingEnded;
            }

            if (_slider != null)
            {
                _slider.ValueChanged += OnSliderValueChanged;
                _slider.MouseMove += OnSliderMouseMove;
                _slider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnSliderMouseLeftButtonUp), true);
            }
        }

        #endregion

        #region Event

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            _slider.Value = 0;
            SetValue(RemainTimePropertyKey, _mediaElement.NaturalDuration.TimeSpan);
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void OnBufferingStarted(object sender, RoutedEventArgs e)
        {
            SetValue(IsBufferingPropertyKey, true);
        }

        private void OnBufferingEnded(object sender, RoutedEventArgs e)
        {
            SetValue(IsBufferingPropertyKey, false);
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
}