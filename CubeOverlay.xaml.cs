using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using CuboAutomatico.Capture;
using CuboAutomatico.Render;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace CuboAutomatico;

public partial class CubeOverlay : Window
{
    private ID3D11Device? _dev;
    private ID3D11DeviceContext? _ctx;
    private IDXGISwapChain1? _swap;
    private ID3D11RenderTargetView? _rtv;
    private ID3D11Texture2D? _depthTex;
    private ID3D11DepthStencilView? _dsv;
    private CubeRenderer? _cube;
    private ScreenCapture? _cap;
    private DispatcherTimer? _tick;
    private readonly Stopwatch _clock = new();
    private readonly TimeSpan _duration;
    private IntPtr _hwnd;
    private int _w, _h;
    private Matrix4x4 _viewProj;

    public CubeOverlay(TimeSpan duration)
    {
        _duration = duration;
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = 0; Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _hwnd = new WindowInteropHelper(this).Handle;
        int ex = GetWindowLong(_hwnd, -20);
        SetWindowLong(_hwnd, -20, ex | 0x80 | 0x8);
        InitD3D();
    }

    public async void Begin()
    {
        try
        {
            _cap ??= new ScreenCapture();
            await _cap.CapturePrimaryAsync(_dev!);
            _cube!.SetScreenTexture(_cap.CapturedTexture!);

            var proj = Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 3f, (float)_w / Math.Max(1, _h), 0.1f, 100f);
            var view = Matrix4x4.CreateLookAt(
                new Vector3(0, 0, 2.7f), Vector3.Zero, Vector3.UnitY);
            _viewProj = view * proj;

            _clock.Restart();
            _tick ??= new DispatcherTimer(TimeSpan.FromMilliseconds(15),
                DispatcherPriority.Render, OnTick, Dispatcher);
            _tick.Start();
        }
        catch (Exception ex)
        {
            try { System.IO.File.AppendAllText("crash.log",
                $"[{DateTime.Now:O}] overlay: {ex}\n"); } catch { }
            Close();
        }
    }

    private void OnTick(object? s, EventArgs e)
    {
        float p = (float)(_clock.Elapsed.TotalSeconds / _duration.TotalSeconds);
        if (p >= 1f) { _tick!.Stop(); Close(); return; }
        Render(p);
    }

    private void Render(float progress)
    {
        _ctx!.OMSetRenderTargets(_rtv!, _dsv!);
        _ctx.ClearRenderTargetView(_rtv!, new Color4(0, 0, 0, 1));
        _ctx.ClearDepthStencilView(_dsv!, DepthStencilClearFlags.Depth, 1f, 0);
        _ctx.RSSetViewport(new Viewport(0, 0, _w, _h, 0, 1));
        _cube!.Draw(_viewProj, progress);
        _swap!.Present(1, PresentFlags.None);
    }

    private void InitD3D()
    {
        GetClientRect(_hwnd, out var rc);
        _w = Math.Max(1, rc.right - rc.left);
        _h = Math.Max(1, rc.bottom - rc.top);

        D3D11.D3D11CreateDevice(null, DriverType.Hardware,
            DeviceCreationFlags.BgraSupport, out _dev, out _ctx).CheckError();

        var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
        var sd = new SwapChainDescription1
        {
            Width = (uint)_w,
            Height = (uint)_h,
            Format = Format.B8G8R8A8_UNorm,
            Stereo = false,
            SampleDescription = new SampleDescription(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 2,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipSequential,
            AlphaMode = Vortice.DXGI.AlphaMode.Ignore,
            Flags = SwapChainFlags.None,
        };
        _swap = factory.CreateSwapChainForHwnd(_dev!, _hwnd, sd);
        factory.Dispose();

        using var bb = _swap.GetBuffer<ID3D11Texture2D>(0);
        _rtv = _dev!.CreateRenderTargetView(bb);
        _depthTex = _dev.CreateTexture2D(new Texture2DDescription
        {
            Width = (uint)_w, Height = (uint)_h, MipLevels = 1, ArraySize = 1,
            Format = Format.D24_UNorm_S8_UInt,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil,
        });
        _dsv = _dev.CreateDepthStencilView(_depthTex);
        _cube = new CubeRenderer(_dev, _ctx!);
    }

    protected override void OnClosed(EventArgs e)
    {
        _tick?.Stop();
        _cube?.Dispose();
        _rtv?.Dispose(); _dsv?.Dispose(); _depthTex?.Dispose();
        _swap?.Dispose(); _ctx?.Dispose(); _dev?.Dispose();
        _cap?.Dispose();
        base.OnClosed(e);
    }

    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr h, out RECT r);
    [StructLayout(LayoutKind.Sequential)] struct RECT { public int left, top, right, bottom; }
}