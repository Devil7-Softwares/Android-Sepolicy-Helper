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
            this.Source = SourceType.LogFile;

            this.SelectFile = ReactiveCommand.CreateFromTask<Window>(selectFile);
            this.RefreshDevices = ReactiveCommand.CreateFromTask(refreshDevices);
            this.StartProcess = ReactiveCommand.CreateFromTask(startProcess);
        }
        #endregion

        #region Variables
        private OpenFileDialog dlgOpen;
        private List<String> SepoliciesStrList;

        private bool isBusy;
        private string status;

        private SourceType source;
        private string logFilePath;
        private ObservableCollection<Models.Device> devices;
        private bool ignoreExistingPolicies;
        private ObservableCollection<Models.SepolicyInfo> sepolicies;
        private Models.Device selectedDevice;
        #endregion

        #region Properties
        public bool IsBusy { get => isBusy; set => this.RaiseAndSetIfChanged(ref isBusy, value); }
        public string Status { get => status; set => this.RaiseAndSetIfChanged(ref status, value); }

        private SourceType Source { get => source; set => this.RaiseAndSetIfChanged(ref source, value); }
        public string LogFilePath { get => logFilePath; set => this.RaiseAndSetIfChanged(ref logFilePath, value); }
        public ObservableCollection<Models.Device> Devices { get => devices; set => this.RaiseAndSetIfChanged(ref devices, value); }
        public bool IgnoreExistingPolicies { get => ignoreExistingPolicies; set => this.RaiseAndSetIfChanged(ref ignoreExistingPolicies, value); }
        public ObservableCollection<Models.SepolicyInfo> Sepolicies { get => sepolicies; set => this.RaiseAndSetIfChanged(ref sepolicies, value); }
        public Models.Device SelectedDevice { get => selectedDevice; set => this.RaiseAndSetIfChanged(ref selectedDevice, value); }
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
                                    string logString = reader.ReadLine();
                                    if (logString.Contains("avc: denied"))
                                    {
                                        Models.SepolicyInfo sepolicy = Utils.Sepolicy.GetSepolicy(logString);
                                        if (sepolicy != null)
                                        {
                                            if (this.IgnoreExistingPolicies)
                                            {
                                                if (this.SepoliciesStrList.Contains(sepolicy.Sepolicy))
                                                {
                                                    continue;
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
                    throw new NotImplementedException();
                }
            });
        }
        #endregion
    }
}
