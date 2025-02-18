﻿using MaterialDesignThemes.Wpf;
using ReactiveUI;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDl.Wpf.Models;

namespace YoutubeDl.Wpf.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        public HomeViewModel HomeVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public object[] Tabs { get; }

        public ReactiveCommand<CancelEventArgs?, bool> SaveSettingsAsyncCommand { get; }

        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            var (settings, loadSettingsErrMsg) = Settings.LoadSettingsAsync().GetAwaiter().GetResult();
            if (loadSettingsErrMsg is not null)
                snackbarMessageQueue.Enqueue(loadSettingsErrMsg);

            _settings = settings;
            _snackbarMessageQueue = snackbarMessageQueue;

            HomeVM = new(settings, snackbarMessageQueue);
            SettingsVM = new(settings, snackbarMessageQueue);
            Tabs = new object[]
            {
                HomeVM,
                SettingsVM,
            };

            SaveSettingsAsyncCommand = ReactiveCommand.CreateFromTask<CancelEventArgs?, bool>(SaveSettingsAsync);
        }

        public async Task<bool> SaveSettingsAsync(CancelEventArgs? cancelEventArgs = null, CancellationToken cancellationToken = default)
        {
            var errMsg = await Settings.SaveSettingsAsync(_settings, cancellationToken);
            if (errMsg is not null)
            {
                _snackbarMessageQueue.Enqueue(errMsg);

                // Cancel window closing
                if (cancelEventArgs is not null)
                    cancelEventArgs.Cancel = true;

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
