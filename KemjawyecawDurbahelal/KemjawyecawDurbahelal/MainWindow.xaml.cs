﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KemjawyecawDurbahelal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StylusMove += MainWindow_StylusMove;
            StylusUp += MainWindow_StylusUp;

            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var windowInteropHelper = new WindowInteropHelper(this);
            var hwnd = windowInteropHelper.Handle;

            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(Hook);
        }

        private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            const int WM_TABLET_DEFBASE =0x02C0;
            const int WM_TABLET_QUERYSYSTEMGESTURESTATUS = WM_TABLET_DEFBASE + 12;
            const int WM_TABLET_FLICK = WM_TABLET_DEFBASE + 11;

            if (msg == WM_TABLET_QUERYSYSTEMGESTURESTATUS)
            {
                uint flags = 0;

                flags |= TABLET_PRESSANDHOLD_DISABLED;
                flags |= TABLET_TAPFEEDBACK_DISABLED;
                flags |= TABLET_TOUCHUI_FORCEON;
                flags |= TABLET_TOUCHUI_FORCEOFF;
                flags |= TABLET_FLICKS_DISABLED;

                handled = true;
                return new IntPtr(flags);
            }
            else if (msg == WM_TABLET_FLICK)
            {
                handled = true;
                return new IntPtr(1);
            }

            return IntPtr.Zero;
        }

        private const uint TABLET_PRESSANDHOLD_DISABLED = 0x00000001;
        private const uint TABLET_TAPFEEDBACK_DISABLED = 0x00000008;
        private const uint TABLET_TOUCHUI_FORCEON = 0x00000100;
        private const uint TABLET_TOUCHUI_FORCEOFF = 0x00000200;
        private const uint TABLET_FLICKS_DISABLED = 0x00010000;

        private void MainWindow_StylusUp(object sender, StylusEventArgs e)
        {
            StrokeVisualList.Remove(e.StylusDevice.Id);
        }

        private void MainWindow_StylusMove(object sender, StylusEventArgs e)
        {
            var strokeVisual = GetStrokeVisual(e.StylusDevice.Id);
            var stylusPointCollection = e.GetStylusPoints(this);
            foreach (var stylusPoint in stylusPointCollection)
            {
                strokeVisual.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y));
            }

            strokeVisual.Redraw();
        }

        private StrokeVisual GetStrokeVisual(int id)
        {
            if (StrokeVisualList.TryGetValue(id, out var visual))
            {
                return visual;
            }

            var strokeVisual = new StrokeVisual();
            StrokeVisualList[id] = strokeVisual;
            var visualCanvas = new VisualCanvas(strokeVisual);
            Grid.Children.Add(visualCanvas);

            return strokeVisual;
        }

        private Dictionary<int, StrokeVisual> StrokeVisualList { get; } = new Dictionary<int, StrokeVisual>();
    }

    public class VisualCanvas : FrameworkElement
    {
        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }

        protected override int VisualChildrenCount => 1;

        public VisualCanvas(DrawingVisual visual)
        {
            Visual = visual;
            AddVisualChild(visual);
        }

        public DrawingVisual Visual { get; }
    }

    /// <summary>
    ///     用于显示笔迹的类
    /// </summary>
    public class StrokeVisual : DrawingVisual
    {
        /// <summary>
        ///     创建显示笔迹的类
        /// </summary>
        public StrokeVisual() : this(new DrawingAttributes()
        {
            Color = Colors.Red,
            FitToCurve = true,
            Width = 5
        })
        {
        }

        /// <summary>
        ///     创建显示笔迹的类
        /// </summary>
        /// <param name="drawingAttributes"></param>
        public StrokeVisual(DrawingAttributes drawingAttributes)
        {
            _drawingAttributes = drawingAttributes;
        }

        /// <summary>
        ///     设置或获取显示的笔迹
        /// </summary>
        public Stroke Stroke { set; get; }

        /// <summary>
        ///     在笔迹中添加点
        /// </summary>
        /// <param name="point"></param>
        public void Add(StylusPoint point)
        {
            if (Stroke == null)
            {
                var collection = new StylusPointCollection {point};
                Stroke = new Stroke(collection) {DrawingAttributes = _drawingAttributes};
            }
            else
            {
                Stroke.StylusPoints.Add(point);
            }
        }

        /// <summary>
        ///     重新画出笔迹
        /// </summary>
        public void Redraw()
        {
            using var dc = RenderOpen();
            Stroke.Draw(dc);
        }

        private readonly DrawingAttributes _drawingAttributes;
    }
}