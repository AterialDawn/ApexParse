using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse.ViewModel
{
    class RequestUserInputViewModel : ViewModelBase
    {
        string _requestText;
        public string RequestText
        {
            get { return _requestText; }
            set { CallerSetProperty(ref _requestText, value); }
        }

        string _userInput;
        public string UserInput
        {
            get { return _userInput; }
            set { CallerSetProperty(ref _userInput, value); }
        }

        public RelayCommand<object> AcceptPressedCommand { get; private set; }
        public RelayCommand<object> CancelPressedCommand { get; private set; }

        public event EventHandler<ValidateInputEventArgs> ValidateUserInput;
        public event EventHandler UserInputAccepted;
        public event EventHandler UserInputRejected;
        public event EventHandler OnCancelled;

        public RequestUserInputViewModel(string defaultRequestText)
        {
            RequestText = defaultRequestText;
            AcceptPressedCommand = new RelayCommand<object>((_) => onAcceptPressed());
            CancelPressedCommand = new RelayCommand<object>((_) => onCancelPressed());
        }

        void onAcceptPressed()
        {
            ValidateInputEventArgs args = new ValidateInputEventArgs(UserInput);
            ValidateUserInput?.Invoke(this, args);
            if (args.ValidationSucceeded)
            {
                UserInputAccepted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                UserInputRejected?.Invoke(this, EventArgs.Empty);
            }
        }

        void onCancelPressed()
        {
            UserInput = "";
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    class ValidateInputEventArgs : EventArgs
    {
        public string TextToValidate { get; private set; }
        public bool ValidationSucceeded { get; set; } = false;

        public ValidateInputEventArgs(string text)
        {
            TextToValidate = text;
        }
    }
}
