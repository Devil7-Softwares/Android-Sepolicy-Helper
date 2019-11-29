using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Linq;
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
            this.dlgFolder = new OpenFolderDialog();
            this.dlgSave = new SaveFileDialog();
            this.Source = SourceType.LogFile;

            this.SelectFile = ReactiveCommand.CreateFromTask<Window>(selectFile);
            this.Save = ReactiveCommand.CreateFromTask<Window>(save);
            this.SplitSave = ReactiveCommand.CreateFromTask<Window>(splitSave);
            this.RefreshDevices = ReactiveCommand.CreateFromTask(refreshDevices);
            this.StartProcess = ReactiveCommand.CreateFromTask(startProcess);
            this.StopProcess = ReactiveCommand.CreateFromTask(stopProcess);

            Utils.ADB.LogcatEnded += LogcatEnded;
            Utils.ADB.LogcatReceived += LogcatReceived;
        }
        #endregion

        #region Variables
        private OpenFileDialog dlgOpen;
        private OpenFolderDialog dlgFolder;
        private SaveFileDialog dlgSave;
        private List<String> SepoliciesStrList;

        private bool showStopButton;
        private bool isBusy;
        private string status;

        private SourceType source;
        private LineEndings lineEnding;
        private string logFilePath;
        private ObservableCollection<Models.Device> devices;
        private bool ignoreExistingPolicies;
        private ObservableCollection<Models.SepolicyInfo> sepolicies;
        private Models.Device selectedDevice;
        private Models.SepolicyInfo selectedSepolicy;
        #endregion

        #region Properties
        public bool ShowStopButton { get => showStopButton; set => this.RaiseAndSetIfChanged(ref showStopButton, value); }
        public bool IsBusy { get => isBusy; set => this.RaiseAndSetIfChanged(ref isBusy, value); }
        public string Status { get => status; set => this.RaiseAndSetIfChanged(ref status, value); }

        private SourceType Source { get => source; set => this.RaiseAndSetIfChanged(ref source, value); }
        private LineEndings LineEnding { get => lineEnding; set => this.RaiseAndSetIfChanged(ref lineEnding, value); }
        public string LogFilePath { get => logFilePath; set => this.RaiseAndSetIfChanged(ref logFilePath, value); }
        public ObservableCollection<Models.Device> Devices { get => devices; set => this.RaiseAndSetIfChanged(ref devices, value); }
        public bool IgnoreExistingPolicies { get => ignoreExistingPolicies; set => this.RaiseAndSetIfChanged(ref ignoreExistingPolicies, value); }
        public ObservableCollection<Models.SepolicyInfo> Sepolicies { get => sepolicies; set => this.RaiseAndSetIfChanged(ref sepolicies, value); }
        public Models.Device SelectedDevice { get => selectedDevice; set => this.RaiseAndSetIfChanged(ref selectedDevice, value); }
        public Models.SepolicyInfo SelectedSepolicy { get => selectedSepolicy; set => this.RaiseAndSetIfChanged(ref selectedSepolicy, value); }
        #endregion

        #region Enums
        public enum SourceType
        {
            LogFile,
            Device
        }

        public enum LineEndings
        {
            CR,
            CRLF
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
                try
                {
                    this.IsBusy = true;
                    this.Status = "Scanning for devices...";
                    this.Devices = new ObservableCollection<Models.Device>(Utils.ADB.GetDevices());
                    if (this.Devices.Count > 0)
                    {
                        if (this.SelectedDevice == null || (this.SelectedDevice = this.Devices.ToList().Find(device => this.SelectedDevice.DeviceName == device.DeviceName)) == null)
                        {
                            this.SelectedDevice = this.Devices[0];
                        }
                    }
                    else
                    {
                        this.SelectedDevice = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        public ReactiveCommand<Unit, Unit> StartProcess { get; }
        private Task startProcess()
        {
            return Task.Run(() =>
            {
                if (this.Source == SourceType.LogFile)
                {
                    try
                    {
                        if (File.Exists(this.LogFilePath))
                        {
                            this.IsBusy = true;
                            this.Status = "Processing Log File...";

                            ObservableCollection<Models.SepolicyInfo> sepolicies = new ObservableCollection<Models.SepolicyInfo>();
                            if (this.IgnoreExistingPolicies)
                            {
                                this.SepoliciesStrList = new List<string>();
                            }

                            using (StreamReader reader = new StreamReader(this.LogFilePath))
                            {
                                while (reader.Peek() != -1)
                                {
                                    AddSepolicy(sepolicies, reader.ReadLine());
                                }
                            }

                            this.Sepolicies = sepolicies;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        this.IsBusy = false;
                    }
                }
                else if (this.Source == SourceType.Device)
                {
                    if (this.SelectedDevice != null)
                    {
                        this.ShowStopButton = true;
                        Utils.ADB.StartLogcat(this.SelectedDevice);
                    }
                }
            });
        }

        public ReactiveCommand<Unit, Unit> StopProcess { get; }
        private Task stopProcess()
        {
            return Task.Run(() =>
            {
                if (this.Source == SourceType.Device)
                {
                    this.ShowStopButton = false;
                    Utils.ADB.StopLogcat();
                }
            });
        }

        public ReactiveCommand<Window, Unit> Save { get; }
        private Task save(Window parent)
        {
            this.dlgSave.DefaultExtension = "te";
            this.dlgSave.Filters.Clear();
            this.dlgSave.Filters.Add(new FileDialogFilter()
            {
                Name = "Type Enforcement Files",
                Extensions = new List<string>() { "te" }
            }); ;

            return Task.Run(async () =>
            {
                try
                {
                    this.IsBusy = true;
                    this.Status = "Saving Sepolicies to File...";

                    string fileName = await this.dlgSave.ShowAsync(parent);
                    if (this.Sepolicies != null && this.Sepolicies.Count > 0 && fileName.Length > 0)
                    {
                        List<string> list = new List<string>();
                        if (File.Exists(fileName))
                        {
                            using (StreamReader reader = new StreamReader(fileName))
                            {
                                while (reader.Peek() != -1)
                                {
                                    list.Add(reader.ReadLine());
                                }
                            }
                        }
                        using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.ASCII))
                        {
                            writer.NewLine = LineEnding == LineEndings.CR ? "\n" : "\r\n";

                            foreach (Models.SepolicyInfo current in this.Sepolicies)
                            {
                                if (!list.Contains(current.Sepolicy))
                                {
                                    writer.WriteLine(current.Sepolicy);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        public ReactiveCommand<Window, Unit> SplitSave { get; }
        private Task splitSave(Window parent)
        {
            return Task.Run(async () =>
            {
                try
                {
                    this.IsBusy = true;
                    this.Status = "Splitting & Saving Sepolicies to Files...";

                    string folderName = await this.dlgFolder.ShowAsync(parent);

                    if (this.Sepolicies != null && this.Sepolicies.Count > 0 && folderName.Length > 0)
                    {
                        List<string> existingPolicies = new List<string>();
                        foreach (Models.SepolicyInfo current in this.Sepolicies)
                        {
                            string file = Path.Combine(folderName, current.Source + ".te");
                            if (File.Exists(file))
                            {
                                using (StreamReader reader = new StreamReader(file))
                                {
                                    while (reader.Peek() != -1)
                                    {
                                        existingPolicies.Add(reader.ReadLine().Replace(" ", ""));
                                    }
                                }
                            }
                            if (!existingPolicies.Contains(current.Sepolicy.Replace(" ", "")))
                            {
                                using (StreamWriter writer = new StreamWriter(file, true, Encoding.ASCII))
                                {
                                    writer.NewLine = LineEnding == LineEndings.CR ? "\n" : "\r\n";
                                    writer.WriteLine(current.Sepolicy);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }
        #endregion

        #region Private Methods
        private void AddSepolicy(IList<Models.SepolicyInfo> sepolicies, string logString)
        {
            if (logString.Contains("avc: denied"))
            {
                Models.SepolicyInfo sepolicy = Utils.Sepolicy.GetSepolicy(logString);
                if (sepolicy != null)
                {
                    if (this.IgnoreExistingPolicies)
                    {
                        if (this.SepoliciesStrList.Contains(sepolicy.Sepolicy))
                        {
                            return;
                        }
                        this.SepoliciesStrList.Add(sepolicy.Sepolicy);
                    }
                    sepolicies.Add(sepolicy);
                }
                else
                {
                    Console.WriteLine("Unable to write sepolicy for log: " + logString);
                }
            }
        }
        #endregion

        #region Events
        private void LogcatEnded(object sender, EventArgs e)
        {
            this.ShowStopButton = false;
        }

        private void LogcatReceived(Utils.ADB.LogReceivedEventArgs e)
        {
            if (this.Sepolicies == null) this.Sepolicies = new ObservableCollection<Models.SepolicyInfo>();
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                AddSepolicy(this.Sepolicies, e.LogString);
            });
        }
        #endregion
    }
}
