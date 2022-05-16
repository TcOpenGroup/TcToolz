using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace TcOpenVSTools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("TcOpen Visual Studio Tools", "", "1.0")]
    [Guid(GuidList.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class MyPackage : AsyncPackage
    {
        public static MyPackage Instance { get; private set; }
        private DTE _envDte;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _envDte = await GetServiceAsync(typeof(DTE)) as DTE;

            await AddMenuButtonsAsync();

            Instance = this;
        }

        public const int topLevelMenuCommandId = 0x0001;
        public const int openSnippetsCommandId = 0x00e0;
        public const int openHmiPublisherCommandId = 0x00f0;

        private async Task AddMenuButtonsAsync()
        {
            var commandService = (IMenuCommandService)await GetServiceAsync(typeof(OleMenuCommandService));

            if (commandService == null)
            {
                return;
            }

            AddCommand(GuidList.guidDaxEditorCmdSet, topLevelMenuCommandId, OnCommandsQueryStatusEnabled, null, commandService);
            AddCommand(GuidList.guidDaxEditorCmdSet, openSnippetsCommandId, OnOpenSnippetsCommandQueryStatusEnabled, OpenSnippetsCommandExecute, commandService);
            AddCommand(GuidList.guidDaxEditorCmdSet, openHmiPublisherCommandId, OnOpenHmiPublisherCommandQueryStatusEnabled, OpenHmiPublisherCommandExecute, commandService);

        }

        private void AddCommand(Guid cmdSet, int cmdId, EventHandler beforeQueryStatus, EventHandler invoke, IMenuCommandService commandService)
        {
            var commandID = new CommandID(cmdSet, cmdId);
            var command = new OleMenuCommand(invoke, commandID);
            command.BeforeQueryStatus += new EventHandler(beforeQueryStatus);
            commandService.AddCommand(command);
        }

        private void OnCommandsQueryStatusEnabled(object sender, EventArgs e)
        {
            try
            {
                OleMenuCommand command = sender as OleMenuCommand;
                if (command == null)
                {
                    return;
                }

                bool isVisible = _envDte != null;
                command.Visible = isVisible;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.StackTrace);
            }
        }

        private void OnOpenSnippetsCommandQueryStatusEnabled(object sender, EventArgs e)
        {

        }

        private void OnOpenHmiPublisherCommandQueryStatusEnabled(object sender, EventArgs e)
        {

        }

        private void OpenSnippetsCommandExecute(object sender, EventArgs e)
        {

        }

        private void OpenHmiPublisherCommandExecute(object sender, EventArgs e)
        {

        }
    }
}
