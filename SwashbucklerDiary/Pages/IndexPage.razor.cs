﻿using BlazorComponent;
using Microsoft.AspNetCore.Components;
using SwashbucklerDiary.Components;
using SwashbucklerDiary.IServices;
using SwashbucklerDiary.Models;

namespace SwashbucklerDiary.Pages
{
    public partial class IndexPage : PageComponentBase
    {
        private StringNumber tab = 0;
        private List<DiaryModel> Diaries = new();
        private List<TagModel> Tags = new();
        private readonly List<string> Types = new() { "All", "Tags" };

        [Inject]
        private IDiaryService DiaryService { get; set; } = default!;
        [Inject]
        private ITagService TagService { get; set; } = default!;

        [Parameter]
        [SupplyParameterFromQuery]
        public string? Type { get; set; }

        protected override async Task OnInitializedAsync()
        {
            InitTab();
            SetCurrentUrl();
            await UpdateTags();
            await UpdateDiaries();
            await base.OnInitializedAsync();
        }

        private bool ShowAddTag { get; set; }
        private async Task UpdateDiaries()
        {
            Diaries = await DiaryService.QueryTakeAsync(50, it => !it.Private);
        }

        private async Task UpdateTags()
        {
            Tags = await TagService.QueryAsync();
        }

        private void InitTab()
        {
            if (string.IsNullOrEmpty(Type))
            {
                Type = Types[0];
            }
            tab = Types.IndexOf(Type!);
        }

        private void SetCurrentUrl()
        {
            NavigateService.CurrentUrl += () => {
                return Navigation.GetUriWithQueryParameter("Type", Types[tab.ToInt32()]);
            };
        }

        private async Task SaveAddTag(string tagName)
        {
            ShowAddTag = false;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return;
            }

            if (Tags.Any(it => it.Name == tagName))
            {
                await AlertService.Warning(I18n.T("Tag.Repeat.Title"), I18n.T("Tag.Repeat.Content"));
                return;
            }

            TagModel tagModel = new()
            {
                Name = tagName
            };
            var flag = await TagService.AddAsync(tagModel);
            if (!flag)
            {
                await AlertService.Error(I18n.T("Share.AddFail"));
                return;
            }

            await AlertService.Success(I18n.T("Share.AddSuccess"));
            Tags.Add(tagModel);
            this.StateHasChanged();
        }

        private string GetWelcomeText()
        {
            int hour = Convert.ToInt16(DateTime.Now.ToString("HH"));
            if (hour >= 6 && hour < 11)
            {
                return I18n.T("Index.Welcome.Morning")!;
            }
            else if (hour >= 11 && hour < 13)
            {
                return I18n.T("Index.Welcome.Noon")!;
            }
            else if (hour >= 13 && hour < 18)
            {
                return I18n.T("Index.Welcome.Afternoon")!;
            }
            else if (hour >= 18 && hour < 23)
            {
                return I18n.T("Index.Welcome.Night")!;
            }
            else if (hour >= 23 || hour < 6)
            {
                return I18n.T("Index.Welcome.BeforeDawn")!;
            }
            return "Hello World";
        }
    }
}
