namespace PureToneAudiometer.ViewModels
{
    using System;
    using System.Windows;
    using Microsoft.Phone.Controls;

    public class DialogBuilder<TView, TViewModel> : IDialogBuilder<TView, TViewModel> where TView : FrameworkElement, new() 
                                                                                      where TViewModel : class
    {
        private Action leftButtonAction;
        private Action rightButtonAction;

        private CustomMessageBox messageBox;
        
        private TView dialogView;

        public DialogBuilder()
        {
            Reset();
        }

        public TViewModel UnderlyingViewModel
        {
            get
            {
                return (TViewModel)dialogView.DataContext;
            }
        }

        public void Reset()
        {
            dialogView = new TView();
            messageBox = new CustomMessageBox { Content = dialogView };
            leftButtonAction = null;
            rightButtonAction = null;
        }

        public IDialogBuilder<TView, TViewModel> Title(string title)
        {
            messageBox.Caption = title;
            return this;
        }

        public IDialogBuilder<TView, TViewModel> LeftButtonContent(object content)
        {
            messageBox.LeftButtonContent = content;
            return this;
        }

        public IDialogBuilder<TView, TViewModel> RightButtonContent(object content)
        {
            messageBox.RightButtonContent = content;
            return this;
        }

        public IDialogBuilder<TView, TViewModel> LeftButtonAction(Action action)
        {
            leftButtonAction = action;
            return this;
        }

        public IDialogBuilder<TView, TViewModel> RightButtonAction(Action action)
        {
            rightButtonAction = action;
            return this;
        }

        public void Show()
        {
            messageBox.Dismissed += (sender, args) =>
            {
                switch (args.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        var okAction = leftButtonAction ?? (() => { });
                        okAction();
                        break;
                    case CustomMessageBoxResult.RightButton:
                        var cancelAction = rightButtonAction ?? (() => { });
                        cancelAction();
                        break;
                    case CustomMessageBoxResult.None:
                        break;
                }
            };
            messageBox.Show();
        }
    }
}
