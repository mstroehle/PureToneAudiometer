namespace PureToneAudiometer.ViewModels.Presets
{
    using System;
    using Microsoft.Phone.Controls;
    using Views.Presets;

    public class AddItemDialogBuilder
    {
        private readonly AddItemView dialogView;

        private Action leftButtonAction;
        private Action rightButtonAction;

        private readonly CustomMessageBox messageBox;

        public AddItemDialogBuilder()
        {
            dialogView = new AddItemView();
            
            messageBox = new CustomMessageBox {Content = dialogView};
        }

        public AddItemViewModel UnderlyingViewModel
        {
            get { return dialogView.DataContext as AddItemViewModel; }
        }

        public AddItemDialogBuilder Title(string title)
        {
            messageBox.Caption = title;
            return this;
        }

        public AddItemDialogBuilder LeftButtonContent(object content)
        {
            messageBox.LeftButtonContent = content;
            return this;
        }

        public AddItemDialogBuilder RightButtonContent(object content)
        {
            messageBox.RightButtonContent = content;
            return this;
        }

        public AddItemDialogBuilder LeftButtonAction(Action action)
        {
            leftButtonAction = action;
            return this;
        }

        public AddItemDialogBuilder RightButtonAction(Action action)
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
