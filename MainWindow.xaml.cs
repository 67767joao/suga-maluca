using System;
using System.Windows;
using System.Windows.Threading;

namespace CuboAutomatico;

public partial class MainWindow : Window
{
    private DispatcherTimer? _schedule;
    private DispatcherTimer _statusTimer;
    private DateTime _nextRun;
    private bool _running;

    public MainWindow()
    {
        InitializeComponent();
        BtnStartPause.Click += (_, _) => Toggle();
        BtnTest.Click      += (_, _) => PlayNow();
        BtnExit.Click      += (_, _) => Application.Current.Shutdown();
        _statusTimer = new DispatcherTimer(TimeSpan.FromSeconds(1),
            DispatcherPriority.Background, (_, _) => UpdateStatus(), Dispatcher);
        _statusTimer.Start();
    }

    private void Toggle() { if (_running) Stop(); else Start(); }

    private void Start()
    {
        if (!int.TryParse(TxtInterval.Text, out int minutes) || minutes <= 0) minutes = 1;
        _running = true;
        BtnStartPause.Content = "Pausar";
        _schedule = new DispatcherTimer(TimeSpan.FromMinutes(minutes),
            DispatcherPriority.Normal, (_, _) => PlayNow(), Dispatcher);
        _schedule.Start();
        _nextRun = DateTime.Now.AddMinutes(minutes);
        UpdateStatus();
    }

    private void Stop()
    {
        _running = false;
        _schedule?.Stop(); _schedule = null;
        BtnStartPause.Content = "Iniciar";
        LblStatus.Text = "Pausado.";
    }

    private void PlayNow()
    {
        if (!double.TryParse(TxtDuration.Text, out double secs) || secs < 0.3) secs = 2.0;
        var overlay = new CubeOverlay(TimeSpan.FromSeconds(secs));
        overlay.Show();
        overlay.Begin();
        if (_running && _schedule is not null)
            _nextRun = DateTime.Now.Add(_schedule.Interval);
    }

    private void UpdateStatus()
    {
        if (!_running) return;
        var left = _nextRun - DateTime.Now;
        if (left < TimeSpan.Zero) left = TimeSpan.Zero;
        LblStatus.Text = $"Ativo. Proxima animacao em {left:mm\\:ss}.";
    }
}