namespace PureToneAudiometer.ViewModels
{
    using System;
    using System.Windows;

    public interface IDialogBuilder<in TView, out TViewModel> where TView : FrameworkElement, new()
                                                              where TViewModel : class
    {
        TViewModel UnderlyingViewModel { get; }
        IDialogBuilder<TView, TViewModel> Title(string title);
        IDialogBuilder<TView, TViewModel> LeftButtonContent(object content);
        IDialogBuilder<TView, TViewModel> RightButtonContent(object content);
        IDialogBuilder<TView, TViewModel> LeftButtonAction(Action action);
        IDialogBuilder<TView, TViewModel> RightButtonAction(Action action);
        void Reset();
        void Show();
    }
}