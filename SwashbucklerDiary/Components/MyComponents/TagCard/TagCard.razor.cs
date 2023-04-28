﻿using Microsoft.AspNetCore.Components;
using SwashbucklerDiary.Models;

namespace SwashbucklerDiary.Components
{
    public partial class TagCard : MyComponentBase
    {
        private bool ShowMenu;
        private List<ListItemModel> ListItemModels = new();

        [Parameter]
        public TagModel? Value { get; set; }
        [Parameter]
        public EventCallback<TagModel> OnDelete { get; set; }
        [Parameter]
        public EventCallback<TagModel> OnRename { get; set; }
        [Parameter]
        public EventCallback<TagModel> OnClick { get; set; }

        protected override void OnInitialized()
        {
            LoadView();
            base.OnInitialized();
        }

        void LoadView()
        {
            ListItemModels = new()
            {
                new("Share.Rename","mdi-rename-outline",EC(()=>OnRename.InvokeAsync(Value))),
                new("Share.Delete","mdi-delete-outline",EC(()=>OnDelete.InvokeAsync(Value))),
            };
        }
    }
}