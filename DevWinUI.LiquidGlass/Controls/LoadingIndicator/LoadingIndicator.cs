// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license; itself a
// port of LoadingIndicators.WPF), restyled for Liquid Glass. Upstream targets
// Windows App SDK only; this port runs on Uno Platform. Simplified to four modes
// with code-built visuals and storyboards (no Composition APIs).
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace DevWinUI.LiquidGlass;

/// <summary>Animation style of a <see cref="LoadingIndicator"/>.</summary>
public enum LoadingIndicatorMode
{
    Arcs,
    Ring,
    ThreeDots,
    Pulse,
}

/// <summary>
/// An animated loading indicator with several looping animation styles, driven
/// entirely by XAML storyboards (Uno-safe). The indicator color comes from
/// <see cref="Control.Foreground"/>.
/// </summary>
public partial class LoadingIndicator : Control
{
    private const string PART_Root = "PART_Root";
    private const double CanvasSize = 36;

    private Grid? _root;
    private readonly List<Storyboard> _storyboards = new();

    public LoadingIndicatorMode Mode
    {
        get => (LoadingIndicatorMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(LoadingIndicatorMode), typeof(LoadingIndicator), new PropertyMetadata(LoadingIndicatorMode.Arcs, (d, _) => ((LoadingIndicator)d).Rebuild()));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingIndicator), new PropertyMetadata(true, (d, _) => ((LoadingIndicator)d).Rebuild()));

    public LoadingIndicator()
    {
        DefaultStyleKey = typeof(LoadingIndicator);
        Loaded += (_, _) => StartAnimations();
        Unloaded += (_, _) => StopAnimations();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild(PART_Root) as Grid;
        Rebuild();
    }

    private void StartAnimations()
    {
        if (!IsActive)
        {
            return;
        }

        foreach (var storyboard in _storyboards)
        {
            storyboard.Begin();
        }
    }

    private void StopAnimations()
    {
        foreach (var storyboard in _storyboards)
        {
            storyboard.Stop();
        }
    }

    private void Rebuild()
    {
        if (_root is null)
        {
            return;
        }

        StopAnimations();
        _storyboards.Clear();
        _root.Children.Clear();

        if (!IsActive)
        {
            return;
        }

        var canvas = new Grid
        {
            Width = CanvasSize,
            Height = CanvasSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _root.Children.Add(canvas);

        switch (Mode)
        {
            case LoadingIndicatorMode.Arcs:
                BuildArcs(canvas);
                break;
            case LoadingIndicatorMode.Ring:
                BuildRing(canvas);
                break;
            case LoadingIndicatorMode.ThreeDots:
                BuildThreeDots(canvas);
                break;
            case LoadingIndicatorMode.Pulse:
                BuildPulse(canvas);
                break;
        }

        if (IsLoaded)
        {
            StartAnimations();
        }
    }

    private void BuildArcs(Grid canvas)
    {
        const double radius = 14;
        var holder = new Grid
        {
            Width = CanvasSize,
            Height = CanvasSize,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        holder.Children.Add(CreateArc(radius, 0, 120, 3));
        holder.Children.Add(CreateArc(radius, 180, 300, 3));
        canvas.Children.Add(holder);

        AddRotationStoryboard(holder, TimeSpan.FromSeconds(1.1));
    }

    private void BuildRing(Grid canvas)
    {
        const double radius = 14;

        // Faint full track.
        var track = new Ellipse
        {
            Width = radius * 2 + 3,
            Height = radius * 2 + 3,
            Stroke = Foreground,
            StrokeThickness = 3,
            Opacity = 0.25,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        canvas.Children.Add(track);

        var holder = new Grid
        {
            Width = CanvasSize,
            Height = CanvasSize,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        holder.Children.Add(CreateArc(radius, -90, 10, 3));
        canvas.Children.Add(holder);

        AddRotationStoryboard(holder, TimeSpan.FromSeconds(0.9));
    }

    private void BuildThreeDots(Grid canvas)
    {
        const double dotSize = 8;
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        canvas.Children.Add(panel);

        var storyboard = new Storyboard();

        for (var i = 0; i < 3; i++)
        {
            var dot = new Ellipse
            {
                Width = dotSize,
                Height = dotSize,
                Fill = Foreground,
                Opacity = 0.25,
            };
            panel.Children.Add(dot);

            var animation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = TimeSpan.FromMilliseconds(i * 160),
                RepeatBehavior = RepeatBehavior.Forever,
            };
            animation.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0.25 });
            animation.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)), Value = 1 });
            animation.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(600)), Value = 0.25 });
            animation.KeyFrames.Add(new LinearDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(960)), Value = 0.25 });
            Storyboard.SetTarget(animation, dot);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
        }

        _storyboards.Add(storyboard);
    }

    private void BuildPulse(Grid canvas)
    {
        const double size = 28;
        var scale = new ScaleTransform();
        var circle = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = Foreground,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = scale,
        };
        canvas.Children.Add(circle);

        var duration = new Duration(TimeSpan.FromSeconds(1.2));
        var easing = new SineEase { EasingMode = EasingMode.EaseOut };
        var storyboard = new Storyboard();
        var forever = RepeatBehavior.Forever;

        var scaleX = new DoubleAnimation { From = 0.2, To = 1, Duration = duration, EasingFunction = easing, RepeatBehavior = forever };
        Storyboard.SetTarget(scaleX, scale);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");
        storyboard.Children.Add(scaleX);

        var scaleY = new DoubleAnimation { From = 0.2, To = 1, Duration = duration, EasingFunction = easing, RepeatBehavior = forever };
        Storyboard.SetTarget(scaleY, scale);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");
        storyboard.Children.Add(scaleY);

        var fade = new DoubleAnimation { From = 1, To = 0, Duration = duration, EasingFunction = easing, RepeatBehavior = forever };
        Storyboard.SetTarget(fade, circle);
        Storyboard.SetTargetProperty(fade, "Opacity");
        storyboard.Children.Add(fade);

        _storyboards.Add(storyboard);
    }

    private void AddRotationStoryboard(UIElement target, TimeSpan period)
    {
        var rotate = new RotateTransform();
        target.RenderTransform = rotate;

        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(period),
            RepeatBehavior = RepeatBehavior.Forever,
        };
        Storyboard.SetTarget(animation, rotate);
        Storyboard.SetTargetProperty(animation, "Angle");
        storyboard.Children.Add(animation);
        _storyboards.Add(storyboard);
    }

    private Path CreateArc(double radius, double startDegrees, double endDegrees, double thickness)
    {
        var center = CanvasSize / 2;
        var figure = new PathFigure
        {
            StartPoint = PointOnCircle(center, radius, startDegrees),
            IsClosed = false,
            IsFilled = false,
        };
        figure.Segments.Add(new ArcSegment
        {
            Point = PointOnCircle(center, radius, endDegrees),
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = endDegrees - startDegrees > 180,
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        return new Path
        {
            Data = geometry,
            Stroke = Foreground,
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
        };
    }

    private static Point PointOnCircle(double center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center + radius * Math.Cos(radians), center + radius * Math.Sin(radians));
    }
}
