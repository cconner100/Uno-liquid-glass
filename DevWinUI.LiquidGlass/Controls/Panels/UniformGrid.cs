// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A panel that arranges its children in a grid of equally sized cells.
/// When Rows/Columns are 0 they are computed automatically from the child count.
/// </summary>
public partial class UniformGrid : Panel
{
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(UniformGrid), new PropertyMetadata(0, OnLayoutPropertyChanged));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(UniformGrid), new PropertyMetadata(0, OnLayoutPropertyChanged));

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(nameof(RowSpacing), typeof(double), typeof(UniformGrid), new PropertyMetadata(0.0, OnLayoutPropertyChanged));

    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register(nameof(ColumnSpacing), typeof(double), typeof(UniformGrid), new PropertyMetadata(0.0, OnLayoutPropertyChanged));

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniformGrid grid)
        {
            grid.InvalidateMeasure();
            grid.InvalidateArrange();
        }
    }

    private void GetDimensions(int childCount, out int rows, out int columns)
    {
        rows = Rows;
        columns = Columns;

        if (childCount <= 0)
        {
            rows = rows > 0 ? rows : 1;
            columns = columns > 0 ? columns : 1;
            return;
        }

        if (rows <= 0 && columns <= 0)
        {
            columns = (int)Math.Ceiling(Math.Sqrt(childCount));
            rows = (int)Math.Ceiling(childCount / (double)columns);
        }
        else if (columns <= 0)
        {
            columns = (int)Math.Ceiling(childCount / (double)rows);
        }
        else if (rows <= 0)
        {
            rows = (int)Math.Ceiling(childCount / (double)columns);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var childCount = Children.Count;
        if (childCount == 0)
        {
            return new Size(0, 0);
        }

        GetDimensions(childCount, out var rows, out var columns);

        var totalColumnSpacing = ColumnSpacing * (columns - 1);
        var totalRowSpacing = RowSpacing * (rows - 1);

        var cellWidth = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : Math.Max(0, (availableSize.Width - totalColumnSpacing) / columns);
        var cellHeight = double.IsInfinity(availableSize.Height)
            ? double.PositiveInfinity
            : Math.Max(0, (availableSize.Height - totalRowSpacing) / rows);

        double maxChildWidth = 0;
        double maxChildHeight = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(cellWidth, cellHeight));
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        var desiredWidth = double.IsInfinity(availableSize.Width)
            ? maxChildWidth * columns + totalColumnSpacing
            : availableSize.Width;
        var desiredHeight = double.IsInfinity(availableSize.Height)
            ? maxChildHeight * rows + totalRowSpacing
            : availableSize.Height;

        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var childCount = Children.Count;
        if (childCount == 0)
        {
            return finalSize;
        }

        GetDimensions(childCount, out var rows, out var columns);

        var cellWidth = Math.Max(0, (finalSize.Width - ColumnSpacing * (columns - 1)) / columns);
        var cellHeight = Math.Max(0, (finalSize.Height - RowSpacing * (rows - 1)) / rows);

        for (var i = 0; i < childCount; i++)
        {
            var row = i / columns;
            var column = i % columns;

            var x = column * (cellWidth + ColumnSpacing);
            var y = row * (cellHeight + RowSpacing);

            Children[i].Arrange(new Rect(x, y, cellWidth, cellHeight));
        }

        return finalSize;
    }
}
