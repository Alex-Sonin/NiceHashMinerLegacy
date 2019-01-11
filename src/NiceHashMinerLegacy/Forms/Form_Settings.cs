﻿using Microsoft.Win32;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using NiceHashMiner.Devices.Algorithms;
using NiceHashMiner.Stats;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Interfaces.DataVisualizer;

namespace NiceHashMiner.Forms
{
    public partial class Form_Settings : Form, IDataVisualizer, IBTCDisplayer, IWorkerNameDisplayer, IServiceLocationDisplayer
    {
        private readonly bool _isInitFinished = false;
        private bool _isChange = false;

        public bool IsChange
        {
            get => _isChange;
            private set => _isChange = _isInitFinished && value;
        }

        private bool _isCredChange = false;
        public bool IsChangeSaved { get; private set; }
        public bool IsRestartNeeded { get; private set; }

        // most likely we wil have settings only per unique devices
        private const bool ShowUniqueDeviceList = true;

        private ComputeDevice _selectedComputeDevice;

        private readonly RegistryKey _rkStartup;

        private bool _isStartupChanged = false;

        ~Form_Settings()
        {
            ApplicationStateManager.UnsubscribeStateDisplayer(this);
        }

        public Form_Settings()
        {
            InitializeComponent();
            ApplicationStateManager.SubscribeStateDisplayer(this);
            Icon = Properties.Resources.logo;

            //ret = 1; // default
            IsChange = false;
            IsChangeSaved = false;

            // backup settings
            ConfigManager.CreateBackup();

            // initialize form
            InitializeFormTranslations();

            // Initialize toolTip
            InitializeToolTip();

            // Initialize tabs
            InitializeGeneralTab();

            // initialization calls 
            InitializeDevicesTab();
            // link algorithm list with algorithm settings control
            algorithmSettingsControl1.Enabled = false;
            algorithmsListView1.ComunicationInterface = algorithmSettingsControl1;
            //algorithmsListView1.RemoveRatioRates();


            // set first device selected {
            if (ComputeDeviceManager.Available.Devices.Count > 0)
            {
                _selectedComputeDevice = ComputeDeviceManager.Available.Devices[0];
                algorithmsListView1.SetAlgorithms(_selectedComputeDevice, _selectedComputeDevice.Enabled);
                groupBoxAlgorithmSettings.Text = string.Format(Translations.Tr("FormSettings_AlgorithmsSettings"),
                    _selectedComputeDevice.Name);
            }

            // At the very end set to true
            _isInitFinished = true;

            try
            {
                _rkStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            catch (SecurityException)
            {
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SETTINGS", e.ToString());
            }
        }

        #region Initializations

        private void InitializeToolTip()
        {
            // Setup Tooltips
            toolTip1.SetToolTip(comboBox_Language, Translations.Tr("Form_Settings_ToolTip_Language"));
            toolTip1.SetToolTip(label_Language, Translations.Tr("Form_Settings_ToolTip_Language"));
            toolTip1.SetToolTip(pictureBox_Language, Translations.Tr("Form_Settings_ToolTip_Language"));

            toolTip1.SetToolTip(checkBox_DebugConsole,
                Translations.Tr("Form_Settings_ToolTip_checkBox_DebugConsole"));
            toolTip1.SetToolTip(pictureBox_DebugConsole,
                Translations.Tr("Form_Settings_ToolTip_checkBox_DebugConsole"));

            toolTip1.SetToolTip(textBox_BitcoinAddress, Translations.Tr("Form_Settings_ToolTip_BitcoinAddress"));
            toolTip1.SetToolTip(label_BitcoinAddress, Translations.Tr("Form_Settings_ToolTip_BitcoinAddress"));
            toolTip1.SetToolTip(pictureBox_Info_BitcoinAddress,
                Translations.Tr("Form_Settings_ToolTip_BitcoinAddress"));

            toolTip1.SetToolTip(textBox_WorkerName, Translations.Tr("Form_Settings_ToolTip_WorkerName"));
            toolTip1.SetToolTip(label_WorkerName, Translations.Tr("Form_Settings_ToolTip_WorkerName"));
            toolTip1.SetToolTip(pictureBox_WorkerName, Translations.Tr("Form_Settings_ToolTip_WorkerName"));

            toolTip1.SetToolTip(comboBox_ServiceLocation,
                Translations.Tr("Form_Settings_ToolTip_ServiceLocation"));
            toolTip1.SetToolTip(label_ServiceLocation, Translations.Tr("Form_Settings_ToolTip_ServiceLocation"));
            toolTip1.SetToolTip(pictureBox_ServiceLocation,
                Translations.Tr("Form_Settings_ToolTip_ServiceLocation"));

            toolTip1.SetToolTip(comboBox_TimeUnit, Translations.Tr("Form_Settings_ToolTip_TimeUnit"));
            toolTip1.SetToolTip(label_TimeUnit, Translations.Tr("Form_Settings_ToolTip_TimeUnit"));
            toolTip1.SetToolTip(pictureBox_TimeUnit, Translations.Tr("Form_Settings_ToolTip_TimeUnit"));

            toolTip1.SetToolTip(checkBox_HideMiningWindows,
                Translations.Tr("Form_Settings_ToolTip_checkBox_HideMiningWindows"));
            toolTip1.SetToolTip(pictureBox_HideMiningWindows,
                Translations.Tr("Form_Settings_ToolTip_checkBox_HideMiningWindows"));

            toolTip1.SetToolTip(checkBox_MinimizeToTray,
                Translations.Tr("Form_Settings_ToolTip_checkBox_MinimizeToTray"));
            toolTip1.SetToolTip(pictureBox_MinimizeToTray,
                Translations.Tr("Form_Settings_ToolTip_checkBox_MinimizeToTray"));

            toolTip1.SetToolTip(checkBox_Use3rdPartyMiners,
                Translations.Tr("Form_Settings_General_3rdparty_ToolTip"));
            toolTip1.SetToolTip(pictureBox_Use3rdPartyMiners,
                Translations.Tr("Form_Settings_General_3rdparty_ToolTip"));

            toolTip1.SetToolTip(checkBox_AllowMultipleInstances,
                Translations.Tr("Form_Settings_General_AllowMultipleInstances_ToolTip"));
            toolTip1.SetToolTip(pictureBox_AllowMultipleInstances,
                Translations.Tr("Form_Settings_General_AllowMultipleInstances_ToolTip"));
            
            toolTip1.SetToolTip(label_MinProfit, Translations.Tr("Form_Settings_ToolTip_MinimumProfit"));
            toolTip1.SetToolTip(pictureBox_MinProfit, Translations.Tr("Form_Settings_ToolTip_MinimumProfit"));
            toolTip1.SetToolTip(textBox_MinProfit, Translations.Tr("Form_Settings_ToolTip_MinimumProfit"));

            toolTip1.SetToolTip(textBox_SwitchMaxSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMaxSeconds"));
            toolTip1.SetToolTip(label_SwitchMaxSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMaxSeconds"));
            toolTip1.SetToolTip(pictureBox_SwitchMaxSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMaxSeconds"));

            toolTip1.SetToolTip(textBox_SwitchMinSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMinSeconds"));
            toolTip1.SetToolTip(label_SwitchMinSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMinSeconds"));
            toolTip1.SetToolTip(pictureBox_SwitchMinSeconds,
                Translations.Tr("Form_Settings_ToolTip_SwitchMinSeconds"));

            toolTip1.SetToolTip(textBox_MinerAPIQueryInterval,
                Translations.Tr("Form_Settings_ToolTip_MinerAPIQueryInterval"));
            toolTip1.SetToolTip(label_MinerAPIQueryInterval,
                Translations.Tr("Form_Settings_ToolTip_MinerAPIQueryInterval"));
            toolTip1.SetToolTip(pictureBox_MinerAPIQueryInterval,
                Translations.Tr("Form_Settings_ToolTip_MinerAPIQueryInterval"));

            toolTip1.SetToolTip(textBox_MinerRestartDelayMS,
                Translations.Tr("Form_Settings_ToolTip_MinerRestartDelayMS"));
            toolTip1.SetToolTip(label_MinerRestartDelayMS,
                Translations.Tr("Form_Settings_ToolTip_MinerRestartDelayMS"));
            toolTip1.SetToolTip(pictureBox_MinerRestartDelayMS,
                Translations.Tr("Form_Settings_ToolTip_MinerRestartDelayMS"));

            toolTip1.SetToolTip(textBox_APIBindPortStart,
                Translations.Tr("Form_Settings_ToolTip_APIBindPortStart"));
            toolTip1.SetToolTip(label_APIBindPortStart,
                Translations.Tr("Form_Settings_ToolTip_APIBindPortStart"));
            toolTip1.SetToolTip(pictureBox_APIBindPortStart,
                Translations.Tr("Form_Settings_ToolTip_APIBindPortStart"));

            toolTip1.SetToolTip(comboBox_DagLoadMode, Translations.Tr("Form_Settings_ToolTip_DagGeneration"));
            toolTip1.SetToolTip(label_DagGeneration, Translations.Tr("Form_Settings_ToolTip_DagGeneration"));
            toolTip1.SetToolTip(pictureBox_DagGeneration, Translations.Tr("Form_Settings_ToolTip_DagGeneration"));

            benchmarkLimitControlCPU.SetToolTip(ref toolTip1, "CPUs");
            benchmarkLimitControlNVIDIA.SetToolTip(ref toolTip1, "NVIDIA GPUs");
            benchmarkLimitControlAMD.SetToolTip(ref toolTip1, "AMD GPUs");

            toolTip1.SetToolTip(checkBox_DisableDetectionNVIDIA,
                string.Format(Translations.Tr("Form_Settings_ToolTip_checkBox_DisableDetection"), "NVIDIA"));
            toolTip1.SetToolTip(checkBox_DisableDetectionAMD,
                string.Format(Translations.Tr("Form_Settings_ToolTip_checkBox_DisableDetection"), "AMD"));
            toolTip1.SetToolTip(pictureBox_DisableDetectionNVIDIA,
                string.Format(Translations.Tr("Form_Settings_ToolTip_checkBox_DisableDetection"), "NVIDIA"));
            toolTip1.SetToolTip(pictureBox_DisableDetectionAMD,
                string.Format(Translations.Tr("Form_Settings_ToolTip_checkBox_DisableDetection"), "AMD"));

            toolTip1.SetToolTip(checkBox_AutoScaleBTCValues,
                Translations.Tr("Form_Settings_ToolTip_checkBox_AutoScaleBTCValues"));
            toolTip1.SetToolTip(pictureBox_AutoScaleBTCValues,
                Translations.Tr("Form_Settings_ToolTip_checkBox_AutoScaleBTCValues"));

            toolTip1.SetToolTip(checkBox_StartMiningWhenIdle,
                Translations.Tr("Form_Settings_ToolTip_checkBox_StartMiningWhenIdle"));
            toolTip1.SetToolTip(pictureBox_StartMiningWhenIdle,
                Translations.Tr("Form_Settings_ToolTip_checkBox_StartMiningWhenIdle"));

            toolTip1.SetToolTip(textBox_MinIdleSeconds, Translations.Tr("Form_Settings_ToolTip_MinIdleSeconds"));
            toolTip1.SetToolTip(label_MinIdleSeconds, Translations.Tr("Form_Settings_ToolTip_MinIdleSeconds"));
            toolTip1.SetToolTip(pictureBox_MinIdleSeconds,
                Translations.Tr("Form_Settings_ToolTip_MinIdleSeconds"));

            toolTip1.SetToolTip(checkBox_LogToFile, Translations.Tr("Form_Settings_ToolTip_checkBox_LogToFile"));
            toolTip1.SetToolTip(pictureBox_LogToFile,
                Translations.Tr("Form_Settings_ToolTip_checkBox_LogToFile"));


            toolTip1.SetToolTip(textBox_LogMaxFileSize, Translations.Tr("Form_Settings_ToolTip_LogMaxFileSize"));
            toolTip1.SetToolTip(label_LogMaxFileSize, Translations.Tr("Form_Settings_ToolTip_LogMaxFileSize"));
            toolTip1.SetToolTip(pictureBox_LogMaxFileSize,
                Translations.Tr("Form_Settings_ToolTip_LogMaxFileSize"));

            toolTip1.SetToolTip(checkBox_ShowDriverVersionWarning,
                Translations.Tr("Form_Settings_ToolTip_checkBox_ShowDriverVersionWarning"));
            toolTip1.SetToolTip(pictureBox_ShowDriverVersionWarning,
                Translations.Tr("Form_Settings_ToolTip_checkBox_ShowDriverVersionWarning"));

            toolTip1.SetToolTip(checkBox_DisableWindowsErrorReporting,
                Translations.Tr("Form_Settings_ToolTip_checkBox_DisableWindowsErrorReporting"));
            toolTip1.SetToolTip(pictureBox_DisableWindowsErrorReporting,
                Translations.Tr("Form_Settings_ToolTip_checkBox_DisableWindowsErrorReporting"));

            toolTip1.SetToolTip(checkBox_ShowInternetConnectionWarning,
                Translations.Tr("Form_Settings_ToolTip_checkBox_ShowInternetConnectionWarning"));
            toolTip1.SetToolTip(pictureBox_ShowInternetConnectionWarning,
                Translations.Tr("Form_Settings_ToolTip_checkBox_ShowInternetConnectionWarning"));

            toolTip1.SetToolTip(checkBox_NVIDIAP0State,
                Translations.Tr("Form_Settings_ToolTip_checkBox_NVIDIAP0State"));
            toolTip1.SetToolTip(pictureBox_NVIDIAP0State,
                Translations.Tr("Form_Settings_ToolTip_checkBox_NVIDIAP0State"));

            toolTip1.SetToolTip(checkBox_RunScriptOnCUDA_GPU_Lost,
                Translations.Tr("Form_Settings_ToolTip_checkBox_RunScriptOnCUDA_GPU_Lost"));
            toolTip1.SetToolTip(pictureBox_RunScriptOnCUDA_GPU_Lost,
                Translations.Tr("Form_Settings_ToolTip_checkBox_RunScriptOnCUDA_GPU_Lost"));

            toolTip1.SetToolTip(checkBox_RunAtStartup,
                Translations.Tr("Form_Settings_ToolTip_checkBox_RunAtStartup"));
            toolTip1.SetToolTip(pictureBox_RunAtStartup,
                Translations.Tr("Form_Settings_ToolTip_checkBox_RunAtStartup"));


            toolTip1.SetToolTip(checkBox_AutoStartMining,
                Translations.Tr("Form_Settings_ToolTip_checkBox_AutoStartMining"));
            toolTip1.SetToolTip(pictureBox_AutoStartMining,
                Translations.Tr("Form_Settings_ToolTip_checkBox_AutoStartMining"));


            toolTip1.SetToolTip(textBox_ethminerDefaultBlockHeight,
                Translations.Tr("Form_Settings_ToolTip_ethminerDefaultBlockHeight"));
            toolTip1.SetToolTip(label_ethminerDefaultBlockHeight,
                Translations.Tr("Form_Settings_ToolTip_ethminerDefaultBlockHeight"));
            toolTip1.SetToolTip(pictureBox_ethminerDefaultBlockHeight,
                Translations.Tr("Form_Settings_ToolTip_ethminerDefaultBlockHeight"));

            toolTip1.SetToolTip(label_displayCurrency, Translations.Tr("Form_Settings_ToolTip_DisplayCurrency"));
            toolTip1.SetToolTip(pictureBox_displayCurrency,
                Translations.Tr("Form_Settings_ToolTip_DisplayCurrency"));
            toolTip1.SetToolTip(currencyConverterCombobox,
                Translations.Tr("Form_Settings_ToolTip_DisplayCurrency"));

            // Setup Tooltips CPU
            toolTip1.SetToolTip(comboBox_CPU0_ForceCPUExtension,
                Translations.Tr("Form_Settings_ToolTip_CPU_ForceCPUExtension"));
            toolTip1.SetToolTip(label_CPU0_ForceCPUExtension,
                Translations.Tr("Form_Settings_ToolTip_CPU_ForceCPUExtension"));
            toolTip1.SetToolTip(pictureBox_CPU0_ForceCPUExtension,
                Translations.Tr("Form_Settings_ToolTip_CPU_ForceCPUExtension"));

            // amd disable temp control
            toolTip1.SetToolTip(checkBox_AMD_DisableAMDTempControl,
                Translations.Tr("Form_Settings_ToolTip_DisableAMDTempControl"));
            toolTip1.SetToolTip(pictureBox_AMD_DisableAMDTempControl,
                Translations.Tr("Form_Settings_ToolTip_DisableAMDTempControl"));

            // disable default optimizations
            toolTip1.SetToolTip(checkBox_DisableDefaultOptimizations,
                Translations.Tr("Form_Settings_ToolTip_DisableDefaultOptimizations"));
            toolTip1.SetToolTip(pictureBox_DisableDefaultOptimizations,
                Translations.Tr("Form_Settings_ToolTip_DisableDefaultOptimizations"));

            // internet connection mining check
            toolTip1.SetToolTip(checkBox_IdleWhenNoInternetAccess,
                Translations.Tr("Form_Settings_ToolTip_ContinueMiningIfNoInternetAccess"));
            toolTip1.SetToolTip(pictureBox_IdleWhenNoInternetAccess,
                Translations.Tr("Form_Settings_ToolTip_ContinueMiningIfNoInternetAccess"));

            // IFTTT notification check
            toolTip1.SetToolTip(checkBox_UseIFTTT, Translations.Tr("Form_Settings_ToolTip_UseIFTTT"));
            toolTip1.SetToolTip(pictureBox_UseIFTTT, Translations.Tr("Form_Settings_ToolTip_UseIFTTT"));

            toolTip1.SetToolTip(pictureBox_SwitchProfitabilityThreshold,
                Translations.Tr("Miner will not switch if the profitability is below SwitchProfitabilityThreshold. Value is in percentage [0 - 1]"));
            toolTip1.SetToolTip(label_SwitchProfitabilityThreshold,
                Translations.Tr("Miner will not switch if the profitability is below SwitchProfitabilityThreshold. Value is in percentage [0 - 1]"));

            toolTip1.SetToolTip(pictureBox_MinimizeMiningWindows,
                Translations.Tr("Form_Settings_ToolTip_MinimizeMiningWindows"));
            toolTip1.SetToolTip(checkBox_MinimizeMiningWindows,
                Translations.Tr("Form_Settings_ToolTip_MinimizeMiningWindows"));

            // Electricity cost
            toolTip1.SetToolTip(label_ElectricityCost, Translations.Tr("Form_Settings_ToolTip_ElectricityCost"));
            toolTip1.SetToolTip(textBox_ElectricityCost, Translations.Tr("Form_Settings_ToolTip_ElectricityCost"));
            toolTip1.SetToolTip(pictureBox_ElectricityCost, Translations.Tr("Form_Settings_ToolTip_ElectricityCost"));

            SetToolTip("Form_Settings_ToolTip_RunEthlargement", checkBox_RunEthlargement, pictureBox_RunEthlargement);

            Text = Translations.Tr("Form_Settings_Title");

            algorithmSettingsControl1.InitLocale(toolTip1);

            SetToolTip("Choose how to check if computer is idle when start mining on idle is enabled.\nSession Lock will start when the computer is locked (generally when the screen has turned off).\nInput Timeout will start when there has been no system input for the idle time seconds.", comboBox_IdleType, label_IdleType, pictureBox_IdleType);
        }

        private void SetToolTip(string internationalKey, params Control[] controls)
        {
            foreach (var control in controls)
            {
                toolTip1.SetToolTip(control, Translations.Tr(internationalKey));
            }
        }

        #region Form this

        private void InitializeFormTranslations()
        {
            buttonDefaults.Text = Translations.Tr("Form_Settings_buttonDefaultsText");
            buttonSaveClose.Text = Translations.Tr("Form_Settings_buttonSaveText");
            buttonCloseNoSave.Text = Translations.Tr("Form_Settings_buttonCloseNoSaveText");
        }

        #endregion //Form this

        #region Tab General

        private void InitializeGeneralTabTranslations()
        {
            checkBox_DebugConsole.Text = Translations.Tr("Form_Settings_General_DebugConsole");
            checkBox_AutoStartMining.Text = Translations.Tr("Form_Settings_General_AutoStartMining");
            checkBox_HideMiningWindows.Text = Translations.Tr("Form_Settings_General_HideMiningWindows");
            checkBox_MinimizeToTray.Text = Translations.Tr("Form_Settings_General_MinimizeToTray");
            checkBox_DisableDetectionNVIDIA.Text =
                string.Format(Translations.Tr("Form_Settings_General_DisableDetection"), "NVIDIA");
            checkBox_DisableDetectionAMD.Text =
                string.Format(Translations.Tr("Form_Settings_General_DisableDetection"), "AMD");
            checkBox_AutoScaleBTCValues.Text = Translations.Tr("Form_Settings_General_AutoScaleBTCValues");
            checkBox_StartMiningWhenIdle.Text = Translations.Tr("Form_Settings_General_StartMiningWhenIdle");
            checkBox_ShowDriverVersionWarning.Text =
                Translations.Tr("Form_Settings_General_ShowDriverVersionWarning");
            checkBox_DisableWindowsErrorReporting.Text =
                Translations.Tr("Form_Settings_General_DisableWindowsErrorReporting");
            checkBox_ShowInternetConnectionWarning.Text =
                Translations.Tr("Form_Settings_General_ShowInternetConnectionWarning");
            checkBox_Use3rdPartyMiners.Text = Translations.Tr("Form_Settings_General_3rdparty_Text");
            checkBox_NVIDIAP0State.Text = Translations.Tr("Form_Settings_General_NVIDIAP0State");
            checkBox_LogToFile.Text = Translations.Tr("Form_Settings_General_LogToFile");
            checkBox_AMD_DisableAMDTempControl.Text =
                Translations.Tr("Form_Settings_General_DisableAMDTempControl");
            checkBox_AllowMultipleInstances.Text =
                Translations.Tr("Form_Settings_General_AllowMultipleInstances_Text");
            checkBox_RunAtStartup.Text = Translations.Tr("Form_Settings_General_RunAtStartup");
            checkBox_MinimizeMiningWindows.Text = Translations.Tr("Form_Settings_General_MinimizeMiningWindows");
            checkBox_UseIFTTT.Text = Translations.Tr("Form_Settings_General_UseIFTTT");
            checkBox_RunScriptOnCUDA_GPU_Lost.Text =
                Translations.Tr("Form_Settings_General_RunScriptOnCUDA_GPU_Lost");

            label_Language.Text = Translations.Tr("Form_Settings_General_Language") + ":";
            label_BitcoinAddress.Text = Translations.Tr("Bitcoin Address") + ":";
            label_WorkerName.Text = Translations.Tr("Worker Name") + ":";
            label_ServiceLocation.Text = Translations.Tr("Service location") + ":";
            {
                var i = 0;
                foreach (var loc in StratumService.MiningLocations)
                {
                    comboBox_ServiceLocation.Items[i] = Translations.Tr((string)StratumService.MiningLocationNames[i]);
                    i++;
                }    
            }
            label_MinIdleSeconds.Text = Translations.Tr("Form_Settings_General_MinIdleSeconds") + ":";
            label_MinerRestartDelayMS.Text = Translations.Tr("Form_Settings_General_MinerRestartDelayMS") + ":";
            label_MinerAPIQueryInterval.Text =
                Translations.Tr("Form_Settings_General_MinerAPIQueryInterval") + ":";
            label_LogMaxFileSize.Text = Translations.Tr("Form_Settings_General_LogMaxFileSize") + ":";
            
            label_SwitchMaxSeconds.Text =
                Translations.Tr("Form_Settings_General_SwitchMaxSeconds") + ":";
            label_SwitchMinSeconds.Text = Translations.Tr("Form_Settings_General_SwitchMinSeconds") + ":";

            label_ethminerDefaultBlockHeight.Text =
                Translations.Tr("Form_Settings_General_ethminerDefaultBlockHeight") + ":";
            label_DagGeneration.Text = Translations.Tr("Form_Settings_DagGeneration") + ":";
            label_APIBindPortStart.Text = Translations.Tr("Form_Settings_APIBindPortStart") + ":";

            label_MinProfit.Text = Translations.Tr("Form_Settings_General_MinimumProfit") + ":";

            label_displayCurrency.Text = Translations.Tr("Form_Settings_DisplayCurrency");

            label_IFTTTAPIKey.Text = Translations.Tr("Form_Settings_IFTTTAPIKey");

            label_ElectricityCost.Text = Translations.Tr("Form_Settings_ElectricityCost");

            // Benchmark time limits
            // internationalization change
            groupBoxBenchmarkTimeLimits.Text =
                Translations.Tr("Form_Settings_General_BenchmarkTimeLimits_Title") + ":";
            benchmarkLimitControlCPU.GroupName =
                Translations.Tr("Form_Settings_General_BenchmarkTimeLimitsCPU_Group") + ":";
            benchmarkLimitControlNVIDIA.GroupName =
                Translations.Tr("Form_Settings_General_BenchmarkTimeLimitsNVIDIA_Group") + ":";
            benchmarkLimitControlAMD.GroupName =
                Translations.Tr("Form_Settings_General_BenchmarkTimeLimitsAMD_Group") + ":";
            // moved from constructor because of editor
            benchmarkLimitControlCPU.InitLocale();
            benchmarkLimitControlNVIDIA.InitLocale();
            benchmarkLimitControlAMD.InitLocale();

            // device enabled listview translation
            devicesListViewEnableControl1.InitLocale();
            algorithmsListView1.InitLocale();

            // Setup Tooltips CPU
            label_CPU0_ForceCPUExtension.Text =
                Translations.Tr("Form_Settings_General_CPU_ForceCPUExtension") + ":";
            // new translations
            tabControlGeneral.TabPages[0].Text = Translations.Tr("FormSettings_Tab_General");
            tabControlGeneral.TabPages[1].Text = Translations.Tr("FormSettings_Tab_Advanced");
            tabControlGeneral.TabPages[2].Text = Translations.Tr("FormSettings_Tab_Devices_Algorithms");
            groupBox_Main.Text = Translations.Tr("FormSettings_Tab_General_Group_Main");
            groupBox_Localization.Text = Translations.Tr("FormSettings_Tab_General_Group_Localization");
            groupBox_Logging.Text = Translations.Tr("FormSettings_Tab_General_Group_Logging");
            groupBox_Misc.Text = Translations.Tr("FormSettings_Tab_General_Group_Misc");
            // advanced
            groupBox_Miners.Text = Translations.Tr("FormSettings_Tab_Advanced_Group_Miners");
            groupBoxBenchmarkTimeLimits.Text =
                Translations.Tr("FormSettings_Tab_Advanced_Group_BenchmarkTimeLimits");

            buttonAllProfit.Text = Translations.Tr("FormSettings_Tab_Devices_Algorithms_Check_ALLProfitability");
            buttonSelectedProfit.Text =
                Translations.Tr("FormSettings_Tab_Devices_Algorithms_Check_SingleProfitability");

            checkBox_DisableDefaultOptimizations.Text =
                Translations.Tr("Form_Settings_Text_DisableDefaultOptimizations");
            checkBox_IdleWhenNoInternetAccess.Text =
                Translations.Tr("Form_Settings_Text_ContinueMiningIfNoInternetAccess");

            label_SwitchProfitabilityThreshold.Text =
                Translations.Tr("Switch Profitability Threshold");

            checkBox_RunEthlargement.Text = Translations.Tr("Form_Settings_General_RunEthlargement");
            checkBox_RunEthlargement.Enabled = Helpers.IsElevated && ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;

            label_IdleType.Text = Translations.Tr("Idle Check Method");
            foreach (var type in Enum.GetNames(typeof(IdleCheckType)))
            {
                comboBox_IdleType.Items.Add(Translations.Tr($"Form_Settings_IdleType_{type}"));
            }
            comboBox_IdleType.Enabled = ConfigManager.GeneralConfig.StartMiningWhenIdle;
        }

        private void InitializeGeneralTabCallbacks()
        {
            // Add EventHandler for all the general tab's checkboxes
            {
                checkBox_AutoScaleBTCValues.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableDetectionAMD.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableDetectionNVIDIA.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_MinimizeToTray.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_HideMiningWindows.CheckedChanged += CheckBox_HideMiningWindows_CheckChanged;
                checkBox_DebugConsole.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_ShowDriverVersionWarning.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_DisableWindowsErrorReporting.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_ShowInternetConnectionWarning.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_StartMiningWhenIdle.CheckedChanged += CheckBox_MineOnIdle_CheckChanged;
                checkBox_NVIDIAP0State.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_LogToFile.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_AutoStartMining.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_AllowMultipleInstances.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_MinimizeMiningWindows.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_UseIFTTT.CheckedChanged += CheckBox_UseIFTTT_CheckChanged;
                checkBox_RunScriptOnCUDA_GPU_Lost.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
                checkBox_RunEthlargement.CheckedChanged += GeneralCheckBoxes_CheckedChanged;
            }
            // Add EventHandler for all the general tab's textboxes
            {
                textBox_BitcoinAddress.Leave += textBox_BitcoinAddress_Leave;
                textBox_WorkerName.Leave += textBox_WorkerName_Leave;
                textBox_IFTTTKey.Leave += GeneralTextBoxes_Leave;
                // these are ints only
                textBox_SwitchMaxSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_SwitchMinSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_MinerAPIQueryInterval.Leave += GeneralTextBoxes_Leave;
                textBox_MinerRestartDelayMS.Leave += GeneralTextBoxes_Leave;
                textBox_MinIdleSeconds.Leave += GeneralTextBoxes_Leave;
                textBox_LogMaxFileSize.Leave += GeneralTextBoxes_Leave;
                textBox_ethminerDefaultBlockHeight.Leave += GeneralTextBoxes_Leave;
                textBox_APIBindPortStart.Leave += GeneralTextBoxes_Leave;
                textBox_MinProfit.Leave += GeneralTextBoxes_Leave;
                textBox_ElectricityCost.Leave += GeneralTextBoxes_Leave;
                // set int only keypress
                textBox_SwitchMaxSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_SwitchMinSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinerAPIQueryInterval.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinerRestartDelayMS.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_MinIdleSeconds.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_LogMaxFileSize.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_ethminerDefaultBlockHeight.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                textBox_APIBindPortStart.KeyPress += TextBoxKeyPressEvents.TextBoxIntsOnly_KeyPress;
                // set double only keypress
                textBox_MinProfit.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
                textBox_ElectricityCost.KeyPress += TextBoxKeyPressEvents.TextBoxDoubleOnly_KeyPress;
            }
            // Add EventHandler for all the general tab's textboxes
            {
                comboBox_Language.Leave += GeneralComboBoxes_Leave;
                comboBox_ServiceLocation.SelectedIndexChanged += comboBox_ServiceLocation_SelectedIndexChanged;
                comboBox_TimeUnit.Leave += GeneralComboBoxes_Leave;
                comboBox_DagLoadMode.Leave += GeneralComboBoxes_Leave;
                comboBox_IdleType.Leave += GeneralComboBoxes_Leave;
            }

            // CPU exceptions
            comboBox_CPU0_ForceCPUExtension.SelectedIndex = (int) ConfigManager.GeneralConfig.ForceCPUExtension;
            comboBox_CPU0_ForceCPUExtension.SelectedIndexChanged +=
                ComboBox_CPU0_ForceCPUExtension_SelectedIndexChanged;
            // fill dag dropdown
            comboBox_DagLoadMode.Items.Clear();
            for (var i = 0; i < (int) DagGenerationType.END; ++i)
            {
                comboBox_DagLoadMode.Items.Add(MinerEtherum.GetDagGenerationString((DagGenerationType) i));
            }

            // set selected
            comboBox_DagLoadMode.SelectedIndex = (int) ConfigManager.GeneralConfig.EthminerDagGenerationType;
        }

        private void InitializeGeneralTabFieldValuesReferences()
        {
            // Checkboxes set checked value
            {
                checkBox_DebugConsole.Checked = ConfigManager.GeneralConfig.DebugConsole;
                checkBox_AutoStartMining.Checked = ConfigManager.GeneralConfig.AutoStartMining;
                checkBox_HideMiningWindows.Checked = ConfigManager.GeneralConfig.HideMiningWindows;
                checkBox_MinimizeToTray.Checked = ConfigManager.GeneralConfig.MinimizeToTray;
                checkBox_DisableDetectionNVIDIA.Checked =
                    ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA;
                checkBox_DisableDetectionAMD.Checked = ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD;
                checkBox_AutoScaleBTCValues.Checked = ConfigManager.GeneralConfig.AutoScaleBTCValues;
                checkBox_StartMiningWhenIdle.Checked = ConfigManager.GeneralConfig.StartMiningWhenIdle;
                checkBox_ShowDriverVersionWarning.Checked = ConfigManager.GeneralConfig.ShowDriverVersionWarning;
                checkBox_DisableWindowsErrorReporting.Checked =
                    ConfigManager.GeneralConfig.DisableWindowsErrorReporting;
                checkBox_ShowInternetConnectionWarning.Checked =
                    ConfigManager.GeneralConfig.ShowInternetConnectionWarning;
                checkBox_NVIDIAP0State.Checked = ConfigManager.GeneralConfig.NVIDIAP0State;
                checkBox_LogToFile.Checked = ConfigManager.GeneralConfig.LogToFile;
                checkBox_AMD_DisableAMDTempControl.Checked = ConfigManager.GeneralConfig.DisableAMDTempControl;
                checkBox_DisableDefaultOptimizations.Checked = ConfigManager.GeneralConfig.DisableDefaultOptimizations;
                checkBox_IdleWhenNoInternetAccess.Checked = ConfigManager.GeneralConfig.IdleWhenNoInternetAccess;
                checkBox_Use3rdPartyMiners.Checked =
                    ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
                checkBox_AllowMultipleInstances.Checked = ConfigManager.GeneralConfig.AllowMultipleInstances;
                checkBox_RunAtStartup.Checked = IsInStartupRegistry();
                checkBox_MinimizeMiningWindows.Checked = ConfigManager.GeneralConfig.MinimizeMiningWindows;
                checkBox_MinimizeMiningWindows.Enabled = !ConfigManager.GeneralConfig.HideMiningWindows;
                checkBox_UseIFTTT.Checked = ConfigManager.GeneralConfig.UseIFTTT;
                checkBox_RunScriptOnCUDA_GPU_Lost.Checked = ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost;
                checkBox_RunEthlargement.Checked = ConfigManager.GeneralConfig.UseEthlargement;
            }

            // Textboxes
            {
                textBox_BitcoinAddress.Text = ConfigManager.GeneralConfig.BitcoinAddress;
                textBox_WorkerName.Text = ConfigManager.GeneralConfig.WorkerName;
                textBox_IFTTTKey.Text = ConfigManager.GeneralConfig.IFTTTKey;
                textBox_IFTTTKey.Enabled = ConfigManager.GeneralConfig.UseIFTTT;
                textBox_SwitchMaxSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper.ToString();
                textBox_SwitchMinSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower.ToString();
                textBox_MinerAPIQueryInterval.Text = ConfigManager.GeneralConfig.MinerAPIQueryInterval.ToString();
                textBox_MinerRestartDelayMS.Text = ConfigManager.GeneralConfig.MinerRestartDelayMS.ToString();
                textBox_MinIdleSeconds.Text = ConfigManager.GeneralConfig.MinIdleSeconds.ToString();
                textBox_LogMaxFileSize.Text = ConfigManager.GeneralConfig.LogMaxFileSize.ToString();
                textBox_ethminerDefaultBlockHeight.Text =
                    ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
                textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
                textBox_MinProfit.Text =
                    ConfigManager.GeneralConfig.MinimumProfit.ToString("F2").Replace(',', '.'); // force comma;
                textBox_SwitchProfitabilityThreshold.Text = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold
                    .ToString("F2").Replace(',', '.'); // force comma;
                textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
            }

            // set custom control referances
            {
                benchmarkLimitControlCPU.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.CPU;
                benchmarkLimitControlNVIDIA.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.NVIDIA;
                benchmarkLimitControlAMD.TimeLimits = ConfigManager.GeneralConfig.BenchmarkTimeLimits.AMD;

                // here we want all devices
                devicesListViewEnableControl1.SetComputeDevices(ComputeDeviceManager.Available.Devices);
                devicesListViewEnableControl1.SetAlgorithmsListView(algorithmsListView1);
            }

            // Add language selections list
            {
                var lang = Translations.GetAvailableLanguages();

                comboBox_Language.Items.Clear();
                for (var i = 0; i < lang.Count; i++)
                {
                    comboBox_Language.Items.Add(lang[(LanguageType) i]);
                }
            }

            // Add time unit selection list
            {
                var timeunits = new Dictionary<TimeUnitType, string>();

                foreach (TimeUnitType timeunit in Enum.GetValues(typeof(TimeUnitType)))
                {
                    timeunits.Add(timeunit, Translations.Tr(timeunit.ToString()));
                    comboBox_TimeUnit.Items.Add(timeunits[timeunit]);
                }
            }

            // ComboBox
            {
                comboBox_Language.SelectedIndex = (int) ConfigManager.GeneralConfig.Language;
                comboBox_ServiceLocation.SelectedIndex = ConfigManager.GeneralConfig.ServiceLocation;
                comboBox_TimeUnit.SelectedItem = Translations.Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                currencyConverterCombobox.SelectedItem = ConfigManager.GeneralConfig.DisplayCurrency;
                comboBox_IdleType.SelectedIndex = (int) ConfigManager.GeneralConfig.IdleCheckType;
            }
        }

        private void InitializeGeneralTab()
        {
            InitializeGeneralTabTranslations();
            InitializeGeneralTabCallbacks();
            InitializeGeneralTabFieldValuesReferences();
        }

        #endregion //Tab General

        #region Tab Devices

        private void InitializeDevicesTab()
        {
            InitializeDevicesCallbacks();
        }

        private void InitializeDevicesCallbacks()
        {
            devicesListViewEnableControl1.SetDeviceSelectionChangedCallback(DevicesListView1_ItemSelectionChanged);
            minDeviceProfitField.Leave += MinDeviceProfitFieldLeft;
        }

        #endregion //Tab Devices

        #endregion // Initializations

        #region Form Callbacks

        #region Tab General

        private void GeneralCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DebugConsole = checkBox_DebugConsole.Checked;
            ConfigManager.GeneralConfig.AutoStartMining = checkBox_AutoStartMining.Checked;
            ConfigManager.GeneralConfig.HideMiningWindows = checkBox_HideMiningWindows.Checked;
            ConfigManager.GeneralConfig.MinimizeToTray = checkBox_MinimizeToTray.Checked;
            ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA =
                checkBox_DisableDetectionNVIDIA.Checked;
            ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionAMD = checkBox_DisableDetectionAMD.Checked;
            ConfigManager.GeneralConfig.AutoScaleBTCValues = checkBox_AutoScaleBTCValues.Checked;
            ConfigManager.GeneralConfig.ShowDriverVersionWarning = checkBox_ShowDriverVersionWarning.Checked;
            ConfigManager.GeneralConfig.DisableWindowsErrorReporting = checkBox_DisableWindowsErrorReporting.Checked;
            ConfigManager.GeneralConfig.ShowInternetConnectionWarning = checkBox_ShowInternetConnectionWarning.Checked;
            ConfigManager.GeneralConfig.NVIDIAP0State = checkBox_NVIDIAP0State.Checked;
            ConfigManager.GeneralConfig.LogToFile = checkBox_LogToFile.Checked;
            ConfigManager.GeneralConfig.IdleWhenNoInternetAccess = checkBox_IdleWhenNoInternetAccess.Checked;
            ConfigManager.GeneralConfig.UseIFTTT = checkBox_UseIFTTT.Checked;
            ConfigManager.GeneralConfig.AllowMultipleInstances = checkBox_AllowMultipleInstances.Checked;
            ConfigManager.GeneralConfig.MinimizeMiningWindows = checkBox_MinimizeMiningWindows.Checked;
            ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost = checkBox_RunScriptOnCUDA_GPU_Lost.Checked;
            ConfigManager.GeneralConfig.UseEthlargement = checkBox_RunEthlargement.Checked;
        }

        private void CheckBox_AMD_DisableAMDTempControl_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;

            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DisableAMDTempControl = checkBox_AMD_DisableAMDTempControl.Checked;
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                if (cDev.DeviceType == DeviceType.AMD)
                {
                    foreach (var algorithm in cDev.GetAlgorithmSettings())
                    {
                        if (algorithm.NiceHashID != AlgorithmType.DaggerHashimoto)
                        {
                            algorithm.ExtraLaunchParameters += AmdGpuDevice.TemperatureParam;
                            algorithm.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, algorithm), algorithm.NiceHashID, DeviceType.AMD, false);
                        }
                    }
                }
            }
        }

        private void CheckBox_DisableDefaultOptimizations_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;

            // indicate there has been a change
            IsChange = true;
            ConfigManager.GeneralConfig.DisableDefaultOptimizations = checkBox_DisableDefaultOptimizations.Checked;
            if (ConfigManager.GeneralConfig.DisableDefaultOptimizations)
            {
                foreach (var cDev in ComputeDeviceManager.Available.Devices)
                {
                    foreach (var algorithm in cDev.GetAlgorithmSettings())
                    {
                        algorithm.ExtraLaunchParameters = "";
                        if (cDev.DeviceType == DeviceType.AMD && algorithm.NiceHashID != AlgorithmType.DaggerHashimoto)
                        {
                            algorithm.ExtraLaunchParameters += AmdGpuDevice.TemperatureParam;
                            algorithm.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, algorithm), algorithm.NiceHashID, cDev.DeviceType, false);
                        }
                    }
                }
            }
            else
            {
                foreach (var cDev in ComputeDeviceManager.Available.Devices)
                {
                    if (cDev.DeviceType == DeviceType.CPU) continue; // cpu has no defaults
                    var deviceDefaultsAlgoSettings = GroupAlgorithms.CreateForDeviceList(cDev);
                    foreach (var defaultAlgoSettings in deviceDefaultsAlgoSettings)
                    {
                        var toSetAlgo = cDev.GetAlgorithm(defaultAlgoSettings);
                        if (toSetAlgo != null)
                        {
                            toSetAlgo.ExtraLaunchParameters = defaultAlgoSettings.ExtraLaunchParameters;
                            toSetAlgo.ExtraLaunchParameters = ExtraLaunchParametersParser.ParseForMiningPair(
                                new MiningPair(cDev, toSetAlgo), toSetAlgo.NiceHashID, cDev.DeviceType, false);
                        }
                    }
                }
            }
        }

        private void CheckBox_RunAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            _isStartupChanged = true;
        }

        private bool IsInStartupRegistry()
        {
            // Value is stored in registry
            var startVal = "";
            try
            {
                startVal = (string) _rkStartup?.GetValue(Application.ProductName, "");
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("REGISTRY", e.ToString());
            }

            return startVal == Application.ExecutablePath;
        }

        // TODO copy paste from Form_Main
        private void textBox_BitcoinAddress_Leave(object sender, EventArgs e)
        {
            var trimmedBtcText = textBox_BitcoinAddress.Text.Trim();
            var result = ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            // TODO get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    _isCredChange = true;
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        // TODO copy paste from Form_Main
        private void textBox_WorkerName_Leave(object sender, EventArgs e)
        {
            var trimmedWorkerNameText = textBox_WorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    // TODO workername invalid handling
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    _isCredChange = true;
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void comboBox_ServiceLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var locationIndex = comboBox_ServiceLocation.SelectedIndex;
            var result = ApplicationStateManager.SetServiceLocationIfValidOrDifferent(locationIndex);
            // TODO GUI stuff get back to this, here we can't really break anything
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    _isCredChange = true;
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void GeneralTextBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            if (ConfigManager.GeneralConfig.BitcoinAddress != textBox_BitcoinAddress.Text.Trim()) _isCredChange = true;
            ConfigManager.GeneralConfig.BitcoinAddress = textBox_BitcoinAddress.Text.Trim();
            if (ConfigManager.GeneralConfig.WorkerName != textBox_WorkerName.Text.Trim()) _isCredChange = true;
            ConfigManager.GeneralConfig.WorkerName = textBox_WorkerName.Text.Trim();
            
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper =
                Helpers.ParseInt(textBox_SwitchMaxSeconds.Text);
            ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower = Helpers.ParseInt(textBox_SwitchMinSeconds.Text);
            ConfigManager.GeneralConfig.MinerAPIQueryInterval = Helpers.ParseInt(textBox_MinerAPIQueryInterval.Text);
            ConfigManager.GeneralConfig.MinerRestartDelayMS = Helpers.ParseInt(textBox_MinerRestartDelayMS.Text);
            ConfigManager.GeneralConfig.MinIdleSeconds = Helpers.ParseInt(textBox_MinIdleSeconds.Text);
            ConfigManager.GeneralConfig.LogMaxFileSize = Helpers.ParseLong(textBox_LogMaxFileSize.Text);
            ConfigManager.GeneralConfig.ethminerDefaultBlockHeight =
                Helpers.ParseInt(textBox_ethminerDefaultBlockHeight.Text);
            ConfigManager.GeneralConfig.ApiBindPortPoolStart = Helpers.ParseInt(textBox_APIBindPortStart.Text);
            // min profit
            ConfigManager.GeneralConfig.MinimumProfit = Helpers.ParseDouble(textBox_MinProfit.Text);
            ConfigManager.GeneralConfig.SwitchProfitabilityThreshold =
                Helpers.ParseDouble(textBox_SwitchProfitabilityThreshold.Text);

            ConfigManager.GeneralConfig.IFTTTKey = textBox_IFTTTKey.Text.Trim();

            ConfigManager.GeneralConfig.KwhPrice = Helpers.ParseDouble(textBox_ElectricityCost.Text);

            // Fix bounds
            ConfigManager.GeneralConfig.FixSettingBounds();
            // update strings
            textBox_MinProfit.Text =
                ConfigManager.GeneralConfig.MinimumProfit.ToString("F2").Replace(',', '.'); // force comma
            textBox_SwitchProfitabilityThreshold.Text = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold
                .ToString("F2").Replace(',', '.'); // force comma
            textBox_SwitchMaxSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Upper.ToString();
            textBox_SwitchMinSeconds.Text = ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds.Lower.ToString();
            textBox_MinerAPIQueryInterval.Text = ConfigManager.GeneralConfig.MinerAPIQueryInterval.ToString();
            textBox_MinerRestartDelayMS.Text = ConfigManager.GeneralConfig.MinerRestartDelayMS.ToString();
            textBox_MinIdleSeconds.Text = ConfigManager.GeneralConfig.MinIdleSeconds.ToString();
            textBox_LogMaxFileSize.Text = ConfigManager.GeneralConfig.LogMaxFileSize.ToString();
            textBox_ethminerDefaultBlockHeight.Text = ConfigManager.GeneralConfig.ethminerDefaultBlockHeight.ToString();
            textBox_APIBindPortStart.Text = ConfigManager.GeneralConfig.ApiBindPortPoolStart.ToString();
            textBox_ElectricityCost.Text = ConfigManager.GeneralConfig.KwhPrice.ToString("0.0000");
        }

        private void GeneralComboBoxes_Leave(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            ConfigManager.GeneralConfig.Language = (LanguageType) comboBox_Language.SelectedIndex;
            //ConfigManager.GeneralConfig.ServiceLocation = comboBox_ServiceLocation.SelectedIndex;
            ConfigManager.GeneralConfig.TimeUnit = (TimeUnitType) comboBox_TimeUnit.SelectedIndex;
            ConfigManager.GeneralConfig.EthminerDagGenerationType =
                (DagGenerationType) comboBox_DagLoadMode.SelectedIndex;
            ConfigManager.GeneralConfig.IdleCheckType = (IdleCheckType) comboBox_IdleType.SelectedIndex;
        }

        private void ComboBox_CPU0_ForceCPUExtension_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cmbbox = (ComboBox) sender;
            ConfigManager.GeneralConfig.ForceCPUExtension = (CpuExtensionType) cmbbox.SelectedIndex;
        }

        #endregion //Tab General


        #region Tab Device

        private void DevicesListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            algorithmSettingsControl1.Deselect();
            // show algorithms
            _selectedComputeDevice =
                ComputeDeviceManager.Available.GetCurrentlySelectedComputeDevice(e.ItemIndex, ShowUniqueDeviceList);
            algorithmsListView1.SetAlgorithms(_selectedComputeDevice, _selectedComputeDevice.Enabled);
            groupBoxAlgorithmSettings.Text = string.Format(Translations.Tr("FormSettings_AlgorithmsSettings"),
                _selectedComputeDevice.Name);
            minDeviceProfitField.Enabled = true;
            minDeviceProfitField.EntryText = _selectedComputeDevice.MinimumProfit.ToString("F2").Replace(',', '.');
        }

        private void MinDeviceProfitFieldLeft(object sender, EventArgs e)
        {
            if (_selectedComputeDevice != null && 
                double.TryParse(minDeviceProfitField.EntryText, out var min))
            {
                if (min < 0) min = 0;

                _selectedComputeDevice.MinimumProfit = min;
            }
        }

        private void ButtonSelectedProfit_Click(object sender, EventArgs e)
        {
            if (_selectedComputeDevice == null)
            {
                MessageBox.Show(Translations.Tr("FormSettings_ButtonProfitSingle"),
                    Translations.Tr("Warning!"),
                    MessageBoxButtons.OK);
                return;
            }

            var url = Links.NhmProfitCheck + _selectedComputeDevice.Name;
            foreach (var algorithm in _selectedComputeDevice.GetAlgorithmSettingsFastest())
            {
                var id = (int) algorithm.NiceHashID;
                url += "&speed" + id + "=" + ProfitabilityCalculator
                           .GetFormatedSpeed(algorithm.BenchmarkSpeed, algorithm.NiceHashID)
                           .ToString("F2", CultureInfo.InvariantCulture);
            }

            url += "&nhmver=" + Application.ProductVersion; // Add version info
            url += "&cost=1&power=1"; // Set default power and cost to 1
            System.Diagnostics.Process.Start(url);
        }

        private void ButtonAllProfit_Click(object sender, EventArgs e)
        {
            var url = Links.NhmProfitCheck + "CUSTOM";
            var total = new Dictionary<AlgorithmType, double>();
            foreach (var curCDev in ComputeDeviceManager.Available.Devices)
            {
                foreach (var algorithm in curCDev.GetAlgorithmSettingsFastest())
                {
                    if (total.ContainsKey(algorithm.NiceHashID))
                    {
                        total[algorithm.NiceHashID] += algorithm.BenchmarkSpeed;
                    }
                    else
                    {
                        total[algorithm.NiceHashID] = algorithm.BenchmarkSpeed;
                    }
                }
            }

            foreach (var algorithm in total)
            {
                var id = (int) algorithm.Key;
                url += "&speed" + id + "=" + ProfitabilityCalculator.GetFormatedSpeed(algorithm.Value, algorithm.Key)
                           .ToString("F2", CultureInfo.InvariantCulture);
            }

            url += "&nhmver=" + Application.ProductVersion; // Add version info
            url += "&cost=1&power=1"; // Set default power and cost to 1
            System.Diagnostics.Process.Start(url);
        }

        #endregion //Tab Device


        private void ToolTip1_Popup(object sender, PopupEventArgs e)
        {
            toolTip1.ToolTipTitle = Translations.Tr("Form_Settings_ToolTip_Explaination");
        }

        #region Form Buttons

        private void ButtonDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Translations.Tr("Form_Settings_buttonDefaultsMsg"),
                Translations.Tr("Form_Settings_buttonDefaultsTitle"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IsChange = true;
                IsChangeSaved = true;
                ConfigManager.GeneralConfig.SetDefaults();

                Translations.SetLanguage(ConfigManager.GeneralConfig.Language);
                InitializeGeneralTabFieldValuesReferences();
                InitializeGeneralTabTranslations();
            }
        }

        private void ButtonSaveClose_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Translations.Tr("Form_Settings_buttonSaveMsg"),
                Translations.Tr("Form_Settings_buttonSaveTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            IsChange = true;
            IsChangeSaved = true;

            if (_isCredChange)
            {
                NiceHashStats.SetCredentials(ConfigManager.GeneralConfig.BitcoinAddress.Trim(),
                    ConfigManager.GeneralConfig.WorkerName.Trim());
            }

            Close();
        }

        private void ButtonCloseNoSave_Click(object sender, EventArgs e)
        {
            IsChangeSaved = false;
            Close();
        }

        #endregion // Form Buttons

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsChange && !IsChangeSaved)
            {
                var result = MessageBox.Show(Translations.Tr("Form_Settings_buttonCloseNoSaveMsg"),
                    Translations.Tr("Form_Settings_buttonCloseNoSaveTitle"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // check restart parameters change
            IsRestartNeeded = ConfigManager.IsRestartNeeded();

            if (IsChangeSaved)
            {
                ConfigManager.GeneralConfigFileCommit();
                ConfigManager.CommitBenchmarks();
                Translations.SetLanguage(ConfigManager.GeneralConfig.Language);

                if (_isStartupChanged)
                {
                    // Commit to registry
                    try
                    {
                        if (checkBox_RunAtStartup.Checked)
                        {
                            // Add NHML to startup registry
                            _rkStartup?.SetValue(Application.ProductName, Application.ExecutablePath);
                        }
                        else
                        {
                            _rkStartup?.DeleteValue(Application.ProductName, false);
                        }
                    }
                    catch (Exception er)
                    {
                        Helpers.ConsolePrint("REGISTRY", er.ToString());
                    }
                }
            }
            else
            {
                ConfigManager.RestoreBackup();
            }
        }

        private void CurrencyConverterCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = currencyConverterCombobox.SelectedItem.ToString();
            ConfigManager.GeneralConfig.DisplayCurrency = selected;
        }

        #endregion Form Callbacks

        private void TabControlGeneral_Selected(object sender, TabControlEventArgs e)
        {
            // set first device selected {
            if (ComputeDeviceManager.Available.Devices.Count > 0)
            {
                algorithmSettingsControl1.Deselect();
            }
        }

        private void CheckBox_Use3rdPartyMiners_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            if (checkBox_Use3rdPartyMiners.Checked)
            {
                // Show TOS
                Form tos = new Form_3rdParty_TOS();
                tos.ShowDialog(this);
                checkBox_Use3rdPartyMiners.Checked =
                    ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            }
            else
            {
                ConfigManager.GeneralConfig.Use3rdPartyMiners = Use3rdPartyMiners.NO;
            }
        }

        private void CheckBox_HideMiningWindows_CheckChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            ConfigManager.GeneralConfig.HideMiningWindows = checkBox_HideMiningWindows.Checked;
            checkBox_MinimizeMiningWindows.Enabled = !checkBox_HideMiningWindows.Checked;
        }

        private void CheckBox_UseIFTTT_CheckChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;

            ConfigManager.GeneralConfig.UseIFTTT = checkBox_UseIFTTT.Checked;

            textBox_IFTTTKey.Enabled = checkBox_UseIFTTT.Checked;
        }

        private void CheckBox_MineOnIdle_CheckChanged(object sender, EventArgs e)
        {
            if (!_isInitFinished) return;
            IsChange = true;
            ConfigManager.GeneralConfig.StartMiningWhenIdle = checkBox_StartMiningWhenIdle.Checked;
            comboBox_IdleType.Enabled = checkBox_StartMiningWhenIdle.Checked;
        }

        void IBTCDisplayer.DisplayBTC(string btc)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBox_BitcoinAddress.Text = btc;
            });
        }

        void IWorkerNameDisplayer.DisplayWorkerName(string workerName)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                textBox_WorkerName.Text = workerName;
            });
        }

        void IServiceLocationDisplayer.DisplayServiceLocation(int serviceLocation)
        {
            FormHelpers.SafeInvoke(this, () =>
            {
                comboBox_ServiceLocation.SelectedIndex = serviceLocation;
            });
        }
    }
}
