For charting, there's a subset of OxyPlot source incorporated to the project:
OxyPlot.Core (PCL) and OxyPlot.Silverlight_SL4, which seems to mostly work.

It's not fully functional (at least right now), behaves wildly when the
chart area is touched, sometimes crashes when the tooltip shows up on the line chart,
that's why the hit tests on charts are disabled.

Libraries referenced via NuGet:
- Caliburn.Micro
- Caliburn.Micro.BindableAppBar
- Coding4Fun.Toolkit.Controls
- The Windows Phone Toolkit