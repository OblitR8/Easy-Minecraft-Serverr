using System;
using System.ComponentModel;

namespace Easy_Minecraft_Serverr
{
    public enum OperationType
    {
        None,
        Starting,
        Stopping,
        Downloading,
        Installing
    }

    public class OperationState : INotifyPropertyChanged
    {
        private OperationType _currentOperation = OperationType.None;
        private string _operationMessage = "";
        private int _operationProgress = 0; // 0-100
        private bool _isOperationInProgress = false;

        public OperationType CurrentOperation
        {
            get => _currentOperation;
            set
            {
                if (_currentOperation != value)
                {
                    _currentOperation = value;
                    OnPropertyChanged(nameof(CurrentOperation));
                    OnPropertyChanged(nameof(IsOperationInProgress));
                }
            }
        }

        public string OperationMessage
        {
            get => _operationMessage;
            set
            {
                if (_operationMessage != value)
                {
                    _operationMessage = value;
                    OnPropertyChanged(nameof(OperationMessage));
                }
            }
        }

        public int OperationProgress
        {
            get => _operationProgress;
            set
            {
                if (_operationProgress != value)
                {
                    _operationProgress = Math.Max(0, Math.Min(100, value));
                    OnPropertyChanged(nameof(OperationProgress));
                }
            }
        }

        public bool IsOperationInProgress => CurrentOperation != OperationType.None;

        public void StartOperation(OperationType type, string message = "")
        {
            CurrentOperation = type;
            OperationMessage = message;
            OperationProgress = 0;
        }

        public void UpdateProgress(int progress, string message = "")
        {
            OperationProgress = progress;
            if (!string.IsNullOrEmpty(message))
                OperationMessage = message;
        }

        public void CompleteOperation()
        {
            CurrentOperation = OperationType.None;
            OperationMessage = "";
            OperationProgress = 0;
        }

        public void CancelOperation()
        {
            CompleteOperation();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
