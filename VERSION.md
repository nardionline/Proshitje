# ProShitje WPF

Version: 2.0.2
Base: uploaded Proshitje(1).zip
Platform: WPF, .NET Framework 4.8, x86
Status: source fix applied; Windows build/runtime not verified in this environment.

Startup fix: UpdateTotals now waits until both SubtotalText and TotalText controls exist, preventing DiscountBox_TextChanged from dereferencing TotalText during XAML initialization.
