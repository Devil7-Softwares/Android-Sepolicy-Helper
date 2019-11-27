using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace AndroidSepolicyHelper.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Constructor
        public MainWindowViewModel()
        {
            this.dlgOpen = new OpenFileDialog();
            this.Source = SourceType.LogFile;

            this.SelectFile = ReactiveCommand.CreateFromTask<Window>(selectFile);
            this.RefreshDevices = ReactiveCommand.CreateFromTask(refreshDevices);
        }
        #endregion

        #region Variables
        private OpenFileDialog dlgOpen;

        private SourceType source;
        private string logFilePath;
        private ObservableCollection<Models.Device> devices;
        #endregion

        #region Properties
        private SourceType Source { get => source; set => this.RaiseAndSetIfChanged(ref source, value); }
        public string LogFilePath { get => logFilePath; set => this.RaiseAndSetIfChanged(ref logFilePath, value); }
        public ObservableCollection<Models.Device> Devices { get => devices; set => this.RaiseAndSetIfChanged(ref devices, value); }
        #endregion

        #region Enums
        public enum SourceType
        {
            LogFile,
            Device
        }
        #endregion

        #region Commands
        public ReactiveCommand<Window, Unit> SelectFile { get; }
        private Task selectFile(Window parent)
        {
            return Task.Run(async () =>
            {
                string[] filename = (await dlgOpen.ShowAsync(parent));
                if (filename.Length > 0) this.LogFilePath = filename[0];
            });
        }

        public ReactiveCommand<Unit, Unit> RefreshDevices { get; }
        private Task refreshDevices()
        {
            return Task.Run(() =>
            {
                this.Devices = new ObservableCollection<Models.Device>(Utils.ADB.GetDevices());
            });
        }
        #endregion
    }
}
