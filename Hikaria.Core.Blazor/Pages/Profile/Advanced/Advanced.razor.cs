﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign.ProLayout;
using Hikaria.Core.Blazor.Models;
using Hikaria.Core.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace Hikaria.Core.Blazor.Pages.Profile {
  public partial class Advanced {
    private readonly IList<TabPaneItem> _tabList = new List<TabPaneItem>
        {
            new TabPaneItem {Key = "detail", Tab = "Details"},
            new TabPaneItem {Key = "rules", Tab = "Rules"}
        };

    private AdvancedProfileData _data = new AdvancedProfileData();

    [Inject] protected IProfileService ProfileService { get; set; }

    protected override async Task OnInitializedAsync() {
      await base.OnInitializedAsync();
      _data = await ProfileService.GetAdvancedAsync();
    }
  }
}