﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SwashbucklerDiary.Rcl.Essentials;
using SwashbucklerDiary.Rcl.Layout;
using SwashbucklerDiary.Shared;
using SwashbucklerDiary.WebAssembly.Essentials;

namespace SwashbucklerDiary.WebAssembly.Layout
{
    public partial class MainLayout : MainLayoutBase
    {
        [Inject]
        private SystemThemeJSModule SystemThemeJSModule { get; set; } = default!;

        [Inject]
        private IAppLifecycle AppLifecycle { get; set; } = default!;

        [Inject]
        private IVersionTracking VersionTracking { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await InitSettingsAsync();
            await InitVersionUpdateAsync();
        }

        protected override void OnDispose()
        {
            ThemeService.OnChanged -= ThemeChanged;
            base.OnDispose();
        }

        protected override async Task InitSettingsAsync()
        {
            await base.InitSettingsAsync();
            await Task.WhenAll(
                InitThemeAsync(),
                InitLanguageAsync(),
                ((AppLifecycle)AppLifecycle).InitializedAsync());
        }

        private async Task InitThemeAsync()
        {
            ThemeService.OnChanged += ThemeChanged;
            await SystemThemeJSModule.InitializedAsync();
            var themeState = SettingService.Get<int>(Setting.Theme);
            await ThemeService.SetThemeAsync((Theme)themeState);
        }

        private Task InitLanguageAsync()
        {
            var language = SettingService.Get<string>(Setting.Language);
            I18n.SetCulture(language);
            return Task.CompletedTask;
        }

        private async Task InitVersionUpdateAsync()
        {
            await ((VersionTracking)VersionTracking).Track(typeof(App).Assembly);
            await VersionUpdataManager.HandleVersionUpdate();
        }

        private async Task RefreshPage()
        {
            await RefreshToSkipWaiting();
        }

        private async Task RefreshToSkipWaiting()
        {
            await JSRuntime.InvokeVoidAsync("swSkipWaiting");
        }
    }
}
