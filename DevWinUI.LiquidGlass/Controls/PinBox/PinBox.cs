// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Event args for <see cref="PinBox.PasswordChanged"/>.
/// </summary>
public sealed partial class PinBoxPasswordChangedEventArgs : EventArgs
{
    internal PinBoxPasswordChangedEventArgs(string oldPassword, string newPassword)
    {
        OldPassword = oldPassword;
        NewPassword = newPassword;
    }

    public string OldPassword { get; }
    public string NewPassword { get; }
}

/// <summary>
/// PIN entry made of one glass cell per character. Typing advances focus,
/// backspace moves back. IsPasswordVisible toggles between the typed character
/// and a bullet.
/// </summary>
public partial class PinBox : Control
{
    private const string PART_StackPanel = "PART_StackPanel";
    private const char MaskChar = '•';

    private StackPanel? _panel;
    private readonly List<TextBox> _cells = new();
    private char?[] _values = new char?[4];
    private bool _isUpdating;

    public event EventHandler<PinBoxPasswordChangedEventArgs>? PasswordChanged;

    public int PasswordLength
    {
        get => (int)GetValue(PasswordLengthProperty);
        set => SetValue(PasswordLengthProperty, value);
    }

    public static readonly DependencyProperty PasswordLengthProperty =
        DependencyProperty.Register(nameof(PasswordLength), typeof(int), typeof(PinBox), new PropertyMetadata(4, (d, e) => ((PinBox)d).RebuildCells()));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), typeof(string), typeof(PinBox), new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    public static readonly DependencyProperty IsPasswordVisibleProperty =
        DependencyProperty.Register(nameof(IsPasswordVisible), typeof(bool), typeof(PinBox), new PropertyMetadata(false, (d, e) => ((PinBox)d).RefreshCellText()));

    /// <summary>Border brush applied to the focused cell.</summary>
    public Brush? FocusBorderBrush
    {
        get => (Brush?)GetValue(FocusBorderBrushProperty);
        set => SetValue(FocusBorderBrushProperty, value);
    }

    public static readonly DependencyProperty FocusBorderBrushProperty =
        DependencyProperty.Register(nameof(FocusBorderBrush), typeof(Brush), typeof(PinBox), new PropertyMetadata(default(Brush)));

    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(PinBox), new PropertyMetadata(8.0, (d, e) =>
        {
            var pinBox = (PinBox)d;
            if (pinBox._panel is not null)
            {
                pinBox._panel.Spacing = (double)e.NewValue;
            }
        }));

    public PinBox()
    {
        DefaultStyleKey = typeof(PinBox);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _panel = GetTemplateChild(PART_StackPanel) as StackPanel;
        if (_panel is not null)
        {
            _panel.Spacing = ItemSpacing;
        }

        RebuildCells();
    }

    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pinBox = (PinBox)d;
        var newValue = (string)(e.NewValue ?? string.Empty);

        if (!pinBox._isUpdating)
        {
            pinBox.ApplyPasswordToCells(newValue);

            // Once cells exist, Password and the cells must agree: clamp an
            // overlong value to the capacity. (Before the template applies,
            // the full value is preserved so XAML property order doesn't matter.)
            if (pinBox._cells.Count > 0 && newValue.Length > pinBox._values.Length)
            {
                pinBox.Password = newValue[..pinBox._values.Length];
                return; // the re-entrant call raises PasswordChanged with the clamped value
            }
        }

        pinBox.PasswordChanged?.Invoke(pinBox, new PinBoxPasswordChangedEventArgs((string)(e.OldValue ?? string.Empty), newValue));
    }

    private void RebuildCells()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.Children.Clear();
        _cells.Clear();

        var length = Math.Max(1, PasswordLength);
        _values = new char?[length];

        var password = Password ?? string.Empty;
        for (var i = 0; i < length && i < password.Length; i++)
        {
            _values[i] = password[i];
        }

        for (var i = 0; i < length; i++)
        {
            var cell = new TextBox
            {
                MaxLength = 1,
                Width = 44,
                Height = 52,
                FontSize = 20,
                Padding = new Thickness(0),
                TextAlignment = TextAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                Background = Background,
                BorderBrush = BorderBrush,
                Foreground = Foreground,
                IsSpellCheckEnabled = false,
            };

            cell.TextChanged += OnCellTextChanged;
            cell.KeyDown += OnCellKeyDown;
            cell.GotFocus += OnCellGotFocus;
            cell.LostFocus += OnCellLostFocus;

            _cells.Add(cell);
            _panel.Children.Add(cell);
        }

        RefreshCellText();

        // Shrinking PasswordLength truncates the cells; keep Password in sync.
        if ((Password ?? string.Empty).Length > length)
        {
            UpdatePasswordFromCells();
        }
    }

    private string GetDisplayText(int index)
    {
        if (index < 0 || index >= _values.Length || _values[index] is not char c)
        {
            return string.Empty;
        }

        return IsPasswordVisible ? c.ToString() : MaskChar.ToString();
    }

    private void RefreshCellText()
    {
        if (_cells.Count == 0)
        {
            return;
        }

        _isUpdating = true;
        for (var i = 0; i < _cells.Count; i++)
        {
            _cells[i].Text = GetDisplayText(i);
        }
        _isUpdating = false;
    }

    private void ApplyPasswordToCells(string value)
    {
        for (var i = 0; i < _values.Length; i++)
        {
            _values[i] = i < value.Length ? value[i] : null;
        }

        RefreshCellText();
    }

    /// <summary>
    /// Shifts entries left over any cleared cell so the displayed digits and
    /// the Password string always agree positionally (no silent gaps).
    /// </summary>
    private void CompactValues()
    {
        var chars = _values.Where(v => v.HasValue).ToArray();
        for (var i = 0; i < _values.Length; i++)
        {
            _values[i] = i < chars.Length ? chars[i] : null;
        }

        RefreshCellText();
    }

    private void UpdatePasswordFromCells()
    {
        var password = string.Concat(_values.Where(v => v.HasValue).Select(v => v!.Value));

        _isUpdating = true;
        Password = password;
        _isUpdating = false;
    }

    private void OnCellTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || sender is not TextBox cell)
        {
            return;
        }

        var index = _cells.IndexOf(cell);
        if (index < 0)
        {
            return;
        }

        var text = cell.Text;

        // Programmatic echo: on some platforms (WinUI proper) TextChanged is
        // raised asynchronously, after the _isUpdating flag has been cleared —
        // without this the mask character would be consumed as user input.
        if (text == GetDisplayText(index))
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _values[index] = null;
            CompactValues();
        }
        else
        {
            _values[index] = text[text.Length - 1];

            _isUpdating = true;
            cell.Text = GetDisplayText(index);
            cell.SelectionStart = cell.Text.Length;
            _isUpdating = false;

            FocusCell(index + 1);
        }

        UpdatePasswordFromCells();
    }

    private void OnCellKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox cell)
        {
            return;
        }

        var index = _cells.IndexOf(cell);
        if (index < 0)
        {
            return;
        }

        switch (e.Key)
        {
            case VirtualKey.Back:
                // Cell already empty: clear the previous cell and move focus back.
                if (_values[index] is null && index > 0)
                {
                    _values[index - 1] = null;
                    CompactValues();
                    UpdatePasswordFromCells();
                    FocusCell(index - 1);
                    e.Handled = true;
                }
                break;
            case VirtualKey.Left:
                FocusCell(index - 1);
                e.Handled = true;
                break;
            case VirtualKey.Right:
                FocusCell(index + 1);
                e.Handled = true;
                break;
        }
    }

    private void OnCellGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox cell)
        {
            if (FocusBorderBrush is not null)
            {
                cell.BorderBrush = FocusBorderBrush;
            }

            cell.SelectAll();
        }
    }

    private void OnCellLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox cell)
        {
            cell.BorderBrush = BorderBrush;
        }
    }

    private void FocusCell(int index)
    {
        if (index >= 0 && index < _cells.Count)
        {
            _cells[index].Focus(FocusState.Programmatic);
            _cells[index].SelectAll();
        }
    }
}
