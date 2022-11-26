using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using PicoController.Gui.Converters;
using PicoController.Gui.Themes;
using Splat;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;

namespace PicoController.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.PropertyChanged += MainWindow_PropertyChanged;

        _themeManager = Locator.Current.GetService<Themes.ThemeManager>();
        if (OperatingSystem.IsWindows())
        {
            var build = Environment.OSVersion.Version.Build;
            TransparencyLevelHint = build switch
            {
                > 20000 => WindowTransparencyLevel.Mica,
                > 10000 => WindowTransparencyLevel.AcrylicBlur,
                _ => WindowTransparencyLevel.None,
            };


            Classes.Add(build switch
            {
                > 20000 => "mica",
                > 10000 => "mica", //"acrylic",
                _ => "",
            });

            
        }
    }

    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if(e.Property.Name == nameof(FrameSize))
        {
            MainGrid.Classes.Clear();
            MainGrid.Classes.Add(((e.NewValue as Size?)?.Width ?? 0) switch
            {
                <= 650 => "small",
                _      => "normal",
                });
        }
    }

    internal void RestoreWindowState()
    {
        Show();
        ShowInTaskbar = true;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        //_timer.Start();
    }
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        ShowInTaskbar = false;
        Hide();
        base.OnClosing(e);
    }

    public static readonly BooleanToGridLengthConverterParameter OutputHeight = new()
    {
        ForTrue = new GridLength(150),
        ForFalse = new GridLength(0)
    };
    //private int index = 0;
    //private readonly DispatcherTimer? _timer;
    private ThemeManager? _themeManager;

    private string[] keys =
    {
        "TextFillColorPrimaryBrush",
"TextFillColorSecondaryBrush",
"TextFillColorTertiaryBrush",
"TextFillColorDisabledBrush",
"TextFillColorInverseBrush",
"AccentTextFillColorPrimaryBrush",
"AccentTextFillColorSecondaryBrush",
"AccentTextFillColorTertiaryBrush",
"AccentTextFillColorDisabledBrush",
"TextOnAccentFillColorSelectedTextBrush",
"TextOnAccentFillColorPrimaryBrush",
"TextOnAccentFillColorSecondaryBrush",
"TextOnAccentFillColorDisabledBrush",
"ControlFillColorDefaultBrush",
"ControlFillColorSecondaryBrush",
"ControlFillColorTertiaryBrush",
"ControlFillColorDisabledBrush",
"ControlFillColorTransparentBrush",
"ControlFillColorInputActiveBrush",
"ControlStrongFillColorDefaultBrush",
"ControlStrongFillColorDisabledBrush",
"ControlSolidFillColorDefaultBrush",
"SubtleFillColorTransparentBrush",
"SubtleFillColorSecondaryBrush",
"SubtleFillColorTertiaryBrush",
"SubtleFillColorDisabledBrush",
"ControlAltFillColorTransparentBrush",
"ControlAltFillColorSecondaryBrush",
"ControlAltFillColorTertiaryBrush",
"ControlAltFillColorQuarternaryBrush",
"ControlAltFillColorDisabledBrush",
"ControlOnImageFillColorDefaultBrush",
"ControlOnImageFillColorSecondaryBrush",
"ControlOnImageFillColorTertiaryBrush",
"ControlOnImageFillColorDisabledBrush",
"AccentFillColorSelectedTextBackgroundBrush",
"AccentFillColorDefaultBrush",
"AccentFillColorSecondaryBrush",
"AccentFillColorTertiaryBrush",
"AccentFillColorDisabledBrush",
"ControlStrokeColorDefaultBrush",
"ControlStrokeColorSecondaryBrush",
"ControlStrokeColorOnAccentDefaultBrush",
"ControlStrokeColorOnAccentSecondaryBrush",
"ControlStrokeColorOnAccentTertiaryBrush",
"ControlStrokeColorOnAccentDisabledBrush",
"ControlStrokeColorForStrongFillWhenOnImageBrush",
"CardStrokeColorDefaultBrush",
"CardStrokeColorDefaultSolidBrush",
"ControlStrongStrokeColorDefaultBrush",
"ControlStrongStrokeColorDisabledBrush",
"SurfaceStrokeColorDefaultBrush",
"SurfaceStrokeColorFlyoutBrush",
"SurfaceStrokeColorInverseBrush",
"DividerStrokeColorDefaultBrush",
"FocusStrokeColorOuterBrush",
"FocusStrokeColorInnerBrush",
"CardBackgroundFillColorDefaultBrush",
"CardBackgroundFillColorSecondaryBrush",
"SmokeFillColorDefaultBrush",
"LayerFillColorDefaultBrush",
"LayerFillColorAltBrush",
"LayerOnAcrylicFillColorDefaultBrush",
"LayerOnAccentAcrylicFillColorDefaultBrush",
"LayerOnMicaBaseAltFillColorDefaultBrush",
"LayerOnMicaBaseAltFillColorSecondaryBrush",
"LayerOnMicaBaseAltFillColorTertiaryBrush",
"LayerOnMicaBaseAltFillColorTransparentBrush",
"SolidBackgroundFillColorBaseBrush",
"SolidBackgroundFillColorSecondaryBrush",
"SolidBackgroundFillColorTertiaryBrush",
"SolidBackgroundFillColorQuarternaryBrush",
"SystemFillColorAttentionBrush",
"SystemFillColorSuccessBrush",
"SystemFillColorCautionBrush",
"SystemFillColorCriticalBrush",
"SystemFillColorNeutralBrush",
"SystemFillColorSolidNeutralBrush",
"SystemFillColorAttentionBackgroundBrush",
"SystemFillColorSuccessBackgroundBrush",
"SystemFillColorCautionBackgroundBrush",
"SystemFillColorCriticalBackgroundBrush",
"SystemFillColorNeutralBackgroundBrush",
"SystemFillColorSolidAttentionBackgroundBrush",
"SystemFillColorSolidNeutralBackgroundBrush",
"SystemColorWindowTextColorBrush",
"SystemColorWindowColorBrush",
"SystemColorButtonFaceColorBrush",
"SystemColorButtonTextColorBrush",
"SystemColorHighlightColorBrush",
"SystemColorHighlightTextColorBrush",
"SystemColorHotlightColorBrush",
"SystemColorGrayTextColorBrush",
"SystemControlTransparentBrush",
"SystemControlHighlightListAccentVeryHighBrush",
"SystemControlHighlightListAccentMediumLowBrush",
"ApplicationPageBackgroundThemeBrush",
"AcrylicBackgroundFillColorDefaultBrush",
"AcrylicInAppFillColorDefaultBrush",
"AcrylicBackgroundFillColorDefaultInverseBrush",
"AcrylicInAppFillColorDefaultInverseBrush",
"AcrylicBackgroundFillColorBaseBrush",
"AcrylicInAppFillColorBaseBrush",
"AccentAcrylicBackgroundFillColorDefaultBrush",
"AccentAcrylicInAppFillColorDefaultBrush",
"AccentAcrylicBackgroundFillColorBaseBrush",
"AccentAcrylicInAppFillColorBaseBrush",
"DatePickerHeaderForegroundThemeBrush",
"DatePickerForegroundThemeBrush",
"ScrollBarButtonForegroundThemeBrush",
"ScrollBarButtonPointerOverBackgroundThemeBrush",
"ScrollBarButtonPointerOverBorderThemeBrush",
"ScrollBarButtonPointerOverForegroundThemeBrush",
"ScrollBarButtonPressedBackgroundThemeBrush",
"ScrollBarButtonPressedBorderThemeBrush",
"ScrollBarButtonPressedForegroundThemeBrush",
"ScrollBarPanningBackgroundThemeBrush",
"ScrollBarPanningBorderThemeBrush",
"ScrollBarThumbBackgroundThemeBrush",
"ScrollBarThumbBorderThemeBrush",
"ScrollBarThumbPointerOverBackgroundThemeBrush",
"ScrollBarThumbPointerOverBorderThemeBrush",
"ScrollBarThumbPressedBackgroundThemeBrush",
"ScrollBarThumbPressedBorderThemeBrush",
"ScrollBarTrackBackgroundThemeBrush",
"ScrollBarTrackBorderThemeBrush",
"ColorControlLightSelectorBrush",
"ColorControlDarkSelectorBrush",
"ColorViewContentBackgroundBrush",
"ColorViewContentBorderBrush",
"ColorViewTabBorderBrush",
"DataGridDropLocationIndicatorBackground",
"DataGridDisabledVisualElementBackground",
"TeachingTipTopHighlightBrush",
    };
}
