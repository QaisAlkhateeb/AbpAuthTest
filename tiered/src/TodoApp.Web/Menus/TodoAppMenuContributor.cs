﻿using System;
using System.Threading.Tasks;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TodoApp.Localization;
using TodoApp.Permissions;
using Volo.Abp.Account.AuthorityDelegation;
using Volo.Abp.Account.Localization;
using Volo.Abp.AuditLogging.Web.Navigation;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.LanguageManagement.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TextTemplateManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.OpenIddict.Pro.Web.Menus;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;
using Volo.Saas.Host.Navigation;

namespace TodoApp.Web.Menus;

public class TodoAppMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public TodoAppMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<TodoAppResource>();
        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                TodoAppMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        //Host Dashboard
        context.Menu.AddItem(
            new ApplicationMenuItem(
                TodoAppMenus.HostDashboard,
                l["Menu:Dashboard"],
                "~/HostDashboard",
                icon: "fa fa-line-chart",
                order: 2
            ).RequirePermissions(TodoAppPermissions.Dashboard.Host)
        );

        //Tenant Dashboard
        context.Menu.AddItem(
            new ApplicationMenuItem(
                TodoAppMenus.TenantDashboard,
                l["Menu:Dashboard"],
                "~/Dashboard",
                icon: "fa fa-line-chart",
                order: 2
            ).RequirePermissions(TodoAppPermissions.Dashboard.Tenant)
        );

        //Saas
        context.Menu.SetSubItemOrder(SaasHostMenuNames.GroupName, 3);


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 5;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);

        //Administration->OpenIddict
        administration.SetSubItemOrder(OpenIddictProMenus.GroupName, 2);

        //Administration->Language Management
        administration.SetSubItemOrder(LanguageManagementMenuNames.GroupName, 3);

        //Administration->Text Template Management
        administration.SetSubItemOrder(TextTemplateManagementMainMenuNames.GroupName, 4);

        //Administration->Audit Logs
        administration.SetSubItemOrder(AbpAuditLoggingMainMenuNames.GroupName, 5);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 6);

        return Task.CompletedTask;
    }

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var authServerUrl = _configuration["AuthServer:Authority"] ?? "~";
        var uiResource = context.GetLocalizer<AbpUiResource>();
        var accountResource = context.GetLocalizer<AccountResource>();
        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpAccountAuthorityDelegationOptions>>();
        if (currentUser.FindImpersonatorUserId() == null && options.Value.EnableDelegatedImpersonation)
        {
            context.Menu.AddItem(new ApplicationMenuItem("Account.AuthorityDelegation", accountResource["AuthorityDelegation"], url: "javascript:void(0)", icon: "fa fa-users"));
        }
        context.Menu.AddItem(new ApplicationMenuItem("Account.Manage", accountResource["MyAccount"], $"{authServerUrl.EnsureEndsWith('/')}Account/Manage", icon: "fa fa-cog", order: 1000, null, "_blank").RequireAuthenticated());
        context.Menu.AddItem(new ApplicationMenuItem("Account.SecurityLogs", accountResource["MySecurityLogs"], $"{authServerUrl.EnsureEndsWith('/')}Account/SecurityLogs", icon: "fa fa-cog", target: "_blank").RequireAuthenticated());
        context.Menu.AddItem(new ApplicationMenuItem("Account.Logout", uiResource["Logout"], url: "~/Account/Logout", icon: "fa fa-power-off", order: int.MaxValue - 1000).RequireAuthenticated());

        return Task.CompletedTask;
    }
}
