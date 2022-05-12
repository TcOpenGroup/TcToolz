using HmiPublisher.Model;

namespace HmiPublisher.UI.Wrapper
{
    public class RemoteWrapper : ModelWrapper<Remote>
    {
        public RemoteWrapper(Remote model) : base(model)
        {
        }

        public int Id
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
        public int IdOriginalValue => GetOriginalValue<int>(nameof(Id));
        public bool IdIsChanged => GetIsChanged(nameof(Id));


        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string NameOriginalValue => GetOriginalValue<string>(nameof(Name));
        public bool NameIsChanged => GetIsChanged(nameof(Name));

        public string TargetUserName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string TargetUserNameOriginalValue => GetOriginalValue<string>(nameof(TargetUserName));
        public bool TargetUserNameIsChanged => GetIsChanged(nameof(TargetUserName));

        public string TargetPass
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string TargetPassOriginalValue => GetOriginalValue<string>(nameof(TargetPass));
        public bool TargetPassIsChanged => GetIsChanged(nameof(TargetPass));

        public string SourcePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SourcePathOriginalValue => GetOriginalValue<string>(nameof(SourcePath));
        public bool SourcePathIsChanged => GetIsChanged(nameof(SourcePath));

        public string DestinationPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string DestinationPathOriginalValue => GetOriginalValue<string>(nameof(DestinationPath));
        public bool DestinationPathIsChanged => GetIsChanged(nameof(DestinationPath));


        public string ExecutableFilePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string ExecutableFilePathOriginalValue => GetOriginalValue<string>(nameof(ExecutableFilePath));
        public bool ExecutableFilePathIsChanged => GetIsChanged(nameof(ExecutableFilePath));

        public bool ClearSharedFolder
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public bool ClearSharedFolderOriginalValue => GetOriginalValue<bool>(nameof(ClearSharedFolder));
        public bool ClearSharedFolderIsChanged => GetIsChanged(nameof(ClearSharedFolder));

        public bool Exclude
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public bool ExcludeOriginalValue => GetOriginalValue<bool>(nameof(Exclude));
        public bool ExcludeIsChanged => GetIsChanged(nameof(Exclude));

        public Compression Compression
        {
            get => GetValue<Compression>();
            set => SetValue(value);
        }
        public Compression CompresionOriginalValue => GetOriginalValue<Compression>(nameof(Compression));
        public bool CompresionIsChanged => GetIsChanged(nameof(Compression));

        private bool _inProgress;
        public bool InProgress
        {
            get => _inProgress;
            set
            {
                _inProgress = value;
                OnPropertyChanged();
            }
        }
        private bool _isError;
        public bool IsError
        {
            get => _isError;
            set
            {
                _isError = value;
                OnPropertyChanged();
            }
        }
        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                OnPropertyChanged();
            }
        }
        private bool _indeterminateProgressBar;
        public bool IndeterminateProgressBar
        {
            get => _indeterminateProgressBar;
            set
            {
                _indeterminateProgressBar = value;
                OnPropertyChanged();
            }
        }
        private string _progressMessage;
        public string ProgressMessage
        {
            get => _progressMessage;
            set
            {
                _progressMessage = value;
                OnPropertyChanged();
            }
        }
        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                _progressPercentage = value;
                OnPropertyChanged();
            }
        }
    }
}
