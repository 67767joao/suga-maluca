using System;
using System.Windows;
using System.Windows.Threading;

namespace CuboAutomatico;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandled;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            try { System.IO.File.AppendAllText("crash.log",
                $"[{DateTime.Now:O}] {args.ExceptionObject}\n"); } catch { }
        };
    }
    private void OnUnhandled(object? s, DispatcherUnhandledExceptionEventArgs e)
    {
        try { System.IO.File.AppendAllText("crash.log",
            $"[{DateTime.Now:O}] {e.Exception}\n"); } catch { }
        e.Handled = true;
    }
}