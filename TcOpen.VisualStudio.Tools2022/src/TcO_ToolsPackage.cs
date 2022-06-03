using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace TcOpen.VisualStudio.Tools
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("TcOpen Visual Studio Tools", "", "1.0")]
    [Guid(GuidList.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class TcO_ToolsPackage : AsyncPackage
    {
        public static TcO_ToolsPackage Instance { get; private set; }
        private TcO_DTE _dte;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _dte = new TcO_DTE(await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE);

            await RegisterCommandsAsync();

            Instance = this;
        }

        private async Task RegisterCommandsAsync()
        {
            var commandService = (IMenuCommandService)await GetServiceAsync(typeof(IMenuCommandService));

            if (commandService == null)
            {
                return;
            }

            Utils.AddCommand(GuidList.guidToolsCmdSet, 0x5000, OpenHmiPublisher, commandService);
            Utils.AddCommand(GuidList.guidToolsCmdSet, 0x5020, OpenSnippets, commandService);

        }

        private void OpenHmiPublisher()
        {
            Utils.RunWithErrorHandling(() =>
            {
                _dte.ExecuteCommand(Command.SaveAll);

                var solutionPath = _dte.GetSolutionPath();
                var exePath = @"C:\Program Files (x86)\MTS spol s.r.o\HmiPublisherV3\HmiPublisher.UI.exe";

                Utils.RunExternalProgram(Path.GetDirectoryName(exePath), exePath, "\"" + solutionPath + "\"");
            });
        }

        private void OpenSnippets()
        {
            var snippets = new Snippets(_dte);
            snippets.Show();
        }
    }
}
