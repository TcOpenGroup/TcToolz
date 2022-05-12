using HmiPublisher.Model;

namespace HmiPublisher
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

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string TargetUserName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string TargetPass
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string FullSourcePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string SourcePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public string DestinationPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }



        public string ExecutableFilePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool ClearSharedFolder
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public bool Include
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        //public Compression Compression
        //{
        //    get => GetValue<Compression>();
        //    set => SetValue(value);
        //}

        public bool InProgress
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public string IpAddress
        {
            get => GetValue<string>();
            set => SetValue(value);
        }


        public bool IsError
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public bool Compressing
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public bool IsSuccess
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public bool IndeterminateProgressBar
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public string ProgressMessage
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int ProgressPercentage
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
    }
}
