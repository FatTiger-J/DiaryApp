﻿using BlazorComponent;
using Masa.Blazor;
using Microsoft.AspNetCore.Components;
using SwashbucklerDiary.Components;
using SwashbucklerDiary.IServices;
using SwashbucklerDiary.Models;
using SwashbucklerDiary.Services;

namespace SwashbucklerDiary.Pages
{
    public partial class WritePage : PageComponentBase, IDisposable
    {
        private bool ShowTitle;
        private bool ShowMenu;
        private bool ShowSelectTag;
        private bool ShowWeather;
        private bool ShowMood;
        private bool ShowLocation;
        private bool Markdown;
        private DiaryModel Diary = new()
        {
            CreateTime = DateTime.Now
        };

        [Inject]
        public MasaBlazor MasaBlazor { get; set; } = default!;
        [Inject]
        public IDiaryService DiaryService { get; set; } = default!;
        [Inject]
        public ITagService TagService { get; set; } = default!;
        [Inject]
        public IconService IconService { get; set; } = default!;
        [Inject]
        public IAchievementService AchievementService { get; set; } = default!;

        [Parameter]
        [SupplyParameterFromQuery]
        public int? TagId { get; set; }
        [Parameter]
        [SupplyParameterFromQuery]
        public int? DiaryId { get; set; }

        public void Dispose()
        {
            MasaBlazor.Breakpoint.OnUpdate -= InvokeStateHasChangedAsync;
            NavigateService.Action -= OnBack;
            GC.SuppressFinalize(this);
        }

        protected override async Task OnInitializedAsync()
        {
            MasaBlazor.Breakpoint.OnUpdate += InvokeStateHasChangedAsync;
            NavigateService.Action += OnBack;
            await LoadSettings();
            await SetTag();
            await SetDiary();
        }

        private List<TagModel> SelectedTags
        {
            get => Diary.Tags ?? new();
            set => Diary.Tags = value;
        }
        private bool Desktop => MasaBlazor.Breakpoint.SmAndUp;
        private bool Mobile => !MasaBlazor.Breakpoint.SmAndUp;
        private Dictionary<string, string> WeatherIcons => IconService!.WeatherIcon;
        private Dictionary<string, string> MoodIcons => IconService!.MoodIcon;
        private StringNumber WeatherIndex
        {
            get
            {
                if (string.IsNullOrEmpty(Diary.Weather))
                {
                    return -1;
                }
                return WeatherIcons.Keys.ToList().IndexOf(Diary.Weather);
            }
            set
            {
                if (value != null)
                {
                    Diary.Weather = WeatherIcons.ElementAt(value.ToInt32()).Key;
                }
                else
                {
                    Diary.Weather = string.Empty;
                }
            }
        }
        private StringNumber MoodIndex
        {
            get
            {
                if (string.IsNullOrEmpty(Diary.Mood))
                {
                    return -1;
                }
                return MoodIcons.Keys.ToList().IndexOf(Diary.Mood);
            }
            set
            {
                if (value != null)
                {
                    Diary.Mood = MoodIcons.ElementAt(value.ToInt32()).Key;
                }
                else
                {
                    Diary.Mood = string.Empty;
                }
            }
        }
        private string Weather =>
            string.IsNullOrEmpty(Diary.Weather) ? I18n.T("Write.Weather") : I18n.T("Weather." + Diary.Weather);
        private string Mood =>
            string.IsNullOrEmpty(Diary.Mood) ? I18n.T("Write.Mood") : I18n.T("Mood." + Diary.Mood);

        private async Task SetTag()
        {
            if (TagId != null)
            {
                var tag = await TagService.FindAsync((int)TagId);
                if (tag != null)
                {
                    SelectedTags.Add(tag);
                }
            }
        }

        private async Task SetDiary()
        {
            if (DiaryId == null)
            {
                return;
            }

            var diary = await DiaryService.FindIncludesAsync((int)DiaryId);
            if (diary == null)
            {
                return;
            }

            Diary = diary;
            ShowTitle = !string.IsNullOrEmpty(diary.Title);
        }

        private async Task LoadSettings()
        {
            Markdown = await SettingsService.Get(nameof(Markdown), false);
        }

        private void RemoveSelectedTag(TagModel tag)
        {
            int index = SelectedTags.IndexOf(tag);
            if (index > -1)
            {
                SelectedTags.RemoveAt(index);
            }
        }

        private void SaveSelectTags()
        {
            ShowSelectTag = false;
        }

        private async Task OnSave()
        {
            if (string.IsNullOrWhiteSpace(Diary.Content))
            {
                return;
            }
            await SaveDiary();
        }

        private async void OnBack()
        {
            if (string.IsNullOrWhiteSpace(Diary.Content))
            {
                NavigateToBack();
                return;
            }

            await SaveDiary();
        }

        private void OnClear()
        {
            Diary.Content = string.Empty;
            this.StateHasChanged();
        }

        private async Task SaveDiary()
        {
            if (DiaryId == null)
            {
                bool flag = await DiaryService.AddAsync(Diary);
                if (flag)
                {
                    await PopupService.ToastAsync(it =>
                    {
                        it.Type = AlertTypes.Success;
                        it.Title = I18n.T("Share.AddSuccess");
                    });
                    var messages = await AchievementService.UpdateUserState(Models.Data.AchievementType.Diary);
                    foreach (var item in messages)
                    {
                        await PopupService.ToastAsync(it =>
                        {
                            it.Type = AlertTypes.Success;
                            it.Title = "达成成就";
                            it.Content = item;
                        });
                    }
                }
                else
                {
                    await PopupService.ToastAsync(it =>
                    {
                        it.Type = AlertTypes.Error;
                        it.Title = I18n.T("Share.AddFail");
                    });
                }
            }
            else
            {
                bool flag = await DiaryService.UpdateIncludesAsync(Diary);
                if (flag)
                {
                    await PopupService.ToastAsync(it =>
                    {
                        it.Type = AlertTypes.Success;
                        it.Title = I18n.T("Share.EditSuccess");
                    });

                }
                else
                {
                    await PopupService.ToastAsync(it =>
                    {
                        it.Type = AlertTypes.Error;
                        it.Title = I18n.T("Share.EditFail");
                    });
                }
            }

            NavigateToBack();
        }

        private async Task InvokeStateHasChangedAsync()
        {
            await InvokeAsync(StateHasChanged);
        }

        private string GetWeatherIcon(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return "mdi-weather-cloudy";
            }
            return IconService!.GetWeatherIcon(key);
        }

        private string GetMoodIcon(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return "mdi-emoticon-outline";
            }
            return IconService!.GetMoodIcon(key);
        }

        private string CounterValue(string? value)
        {
            int len = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return len + " " + I18n.T("Write.CountUnit");
            }

            value = value.Trim();
            if (I18n.T("Write.Word") == "1")
            {
                len = value.Split(' ').Length;
            }

            if (I18n.T("Write.Character") == "1")
            {
                len = value.Length;
            }

            return len + " " + I18n.T("Write.CountUnit");
        }

        private async Task MarkdownChanged()
        {
            Markdown = !Markdown;
            await SettingsService!.Save(nameof(Markdown), Markdown);
        }
    }
}
