using System.Collections.Generic;

namespace Fireball.Game.Client.Models
{
    public enum DialogButtonAction
    {
        OK = 0,
        Reload = 1,
        Retry = 2,
        Redirect = 3,
    }

    public class ErrorDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public List<ErrorDialogButton> Buttons { get; set; }

        public ErrorDialog() { }

        public ErrorDialog(string title, string message, List<ErrorDialogButton> buttons)
        {
            Title = title;
            Message = message;
            Buttons = buttons;
        }

        public static ErrorDialog Information(string title, string message, string buttonLabel = "OK")
        {
            return new ErrorDialog(title, message, new List<ErrorDialogButton>()
            {
                new ErrorDialogButton(buttonLabel, DialogButtonAction.OK)
            });
        }
    }

    public class ErrorDialogButton
    {
        public string Text { get; set; }
        public DialogButtonAction Action { get; set; }
        public string Link { get; set; } // Require for Action type Redirect/3

        public ErrorDialogButton() { }

        public ErrorDialogButton(string text, DialogButtonAction action)
        {
            Text = text;
            Action = action;
            Link = null;
        }

        public ErrorDialogButton(string text, string link)
        {
            Text = text;
            Action = DialogButtonAction.Redirect;
            Link = link;
        }
    }
}
