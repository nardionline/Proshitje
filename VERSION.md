# ProShitje WPF 2.0.7 - Fatura Popup & Maximized

Base: ProShitje WPF 2.0.6 - Fix Build Errors.

Changes: main window starts maximized; removed redundant quantity keypad button; quantity, price, and line total open touch keypad/editor interactions; keypad is resizable and keeps Cancel/Confirm buttons visible while keys can scroll. Price edit remains permission-gated. Right-clicking price checks cost/margin permission but intentionally does not display fabricated values because the MDB purchase-cost field has not been identified.

Build status: NOT VERIFIED on Windows/MSBuild. Must validate with GitHub Actions before use.
