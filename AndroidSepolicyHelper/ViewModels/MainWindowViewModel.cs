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
using System.IO.Compression;

namespace Devil7.Android.SepolicyHelper.ViewModels
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
            this.RefreshDevices = ReactiveCommand.CreateFromTask<Window>(refreshDevices);
            this.StartProcess = ReactiveCommand.CreateFromTask<Window>(startProcess);
            this.StopProcess = ReactiveCommand.CreateFromTask<Window>(stopProcess);
            this.ExtractADB = ReactiveCommand.CreateFromTask<EventArgs>(extractADB);

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
            LF,
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

        public ReactiveCommand<Window, Unit> RefreshDevices { get; }
        private Task refreshDevices(Window parent)
        {
            return Task.Run(async () =>
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
                    await Utils.MessageBoxHelper.Error("Failed to refresh devices list! " + ex.Message, "Error", parent);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }

        public ReactiveCommand<Window, Unit> StartProcess { get; }
        private Task startProcess(Window parent)
        {
            return Task.Run(async () =>
            {
                if (this.Source == SourceType.LogFile)
                {
                    try
                    {
                        if (this.LogFilePath.Trim() == "")
                        {
                            await Utils.MessageBoxHelper.Error("No log file selected!", "Error", parent);
                        }
                        else
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

                                if (sepolicies.Count == 0)
                                    await Utils.MessageBoxHelper.Info("No sepolicy denials found in selected log file!", "Done", parent);
                                else
                                    await Utils.MessageBoxHelper.Info(string.Format("Successfully genarated {0} sepolicies from selected log file.", sepolicies.Count), "Done", parent);
                            }
                            else
                            {
                                await Utils.MessageBoxHelper.Error("Selected log file doesn't exist!", "Error", parent);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await Utils.MessageBoxHelper.Error("Error on reading log file! " + ex.Message, "Error", parent);
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
                        await Utils.MessageBoxHelper.Info("Generating sepolicies from device is a continuous. This application will continue to read logcat from device as long as device is connected/locat exits. Press 'Stop' button to stop the process!", "Warning", parent);
                        Utils.ADB.StartLogcat(this.SelectedDevice);
                    }
                    else
                    {
                        await Utils.MessageBoxHelper.Error("No device selected!", "Error", parent);
                    }
                }
                else
                {
                    await Utils.MessageBoxHelper.Error("Select source to generate sepolicies from..!", "Error", parent);
                }
            });
        }

        public ReactiveCommand<Window, Unit> StopProcess { get; }
        private Task stopProcess(Window parent)
        {
            return Task.Run(async () =>
            {
                if (this.Source == SourceType.Device)
                {
                    this.ShowStopButton = false;
                    Utils.ADB.StopLogcat();
                    await Utils.MessageBoxHelper.Info("Generation of sepolicy from device stopped successfully!", "Done", parent);
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
                    if (this.Sepolicies != null && this.Sepolicies.Count > 0)
                    {
                        this.IsBusy = true;
                        this.Status = "Saving Sepolicies to File...";

                        string fileName = await this.dlgSave.ShowAsync(parent);
                        if (fileName.Length > 0)
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
                                writer.NewLine = LineEnding == LineEndings.LF ? "\n" : "\r\n";

                                foreach (Models.SepolicyInfo current in this.Sepolicies)
                                {
                                    if (!list.Contains(current.Sepolicy))
                                    {
                                        writer.WriteLine(current.Sepolicy);
                                    }
                                }
                            }
                        }

                        await Utils.MessageBoxHelper.Info("Successfully written sepolicies to file.", "Done", parent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Utils.MessageBoxHelper.Error("Error on saving sepolicies to file! " + ex.Message, "Error", parent);
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
                    if (this.Sepolicies != null && this.Sepolicies.Count > 0)
                    {
                        this.IsBusy = true;
                        this.Status = "Splitting & Saving Sepolicies to Files...";

                        string folderName = await this.dlgFolder.ShowAsync(parent);

                        if (folderName.Length > 0)
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
                                        writer.NewLine = LineEnding == LineEndings.LF ? "\n" : "\r\n";
                                        writer.WriteLine(current.Sepolicy);
                                    }
                                }
                            }
                        }

                        await Utils.MessageBoxHelper.Info("Successfully splitted & written sepolicies to file(s).", "Done", parent);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Utils.MessageBoxHelper.Error("Error on splitting/saving sepolicies to file(s)! " + ex.Message, "Error", parent);
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

        public ReactiveCommand<EventArgs, Unit> ExtractADB { get; }
        private Task extractADB(EventArgs e)
        {
            return Task.Run(() => {
                try
                {
                    this.IsBusy = true;
                    this.Status = "Extracting ADB Binaries...";

                    FileInfo adbZip = new FileInfo(Path.Combine(AppContext.BaseDirectory, "adb.zip"));
                    DirectoryInfo adbDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "adb"));
                    if (!adbDir.Exists && adbZip.Exists)
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(adbZip.FullName))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (entry.Name != "")
                                {
                                    string destinationPath = Path.GetFullPath(Path.Combine(adbDir.FullName, entry.FullName));
                                    FileInfo destinationFile = new FileInfo(destinationPath);
                                    if (!destinationFile.Directory.Exists)
                                    {
                                        destinationFile.Directory.Create();
                                    }
                                    if (destinationPath.StartsWith(adbDir.FullName, StringComparison.Ordinal))
                                        entry.ExtractToFile(destinationPath);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to extract ADB binaries. " + ex.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            });
        }
        #endregion

        #region Events
        private async void LogcatEnded(object sender, EventArgs e)
        {
            if (this.ShowStopButton)
            {
                this.ShowStopButton = false;
                await Utils.MessageBoxHelper.Info("Logcat process ended!", "Done", null);
            }
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
