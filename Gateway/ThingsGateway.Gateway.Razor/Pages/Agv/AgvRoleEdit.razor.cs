//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://thingsgateway.cn/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class AgvRoleEdit
{
    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Inject]
    private IStringLocalizer<ThingsGateway.Gateway.Razor._Imports>? GatewayLocalizer { get; set; }

    [Parameter]
    [NotNull]
    public AgvRole? Model { get; set; }

    [NotNull]
    private List<TreeViewItem<SysOrg>> Items { get; set; }
    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }

    private static bool ModelEqualityComparer(SysOrg x, SysOrg y) => x.Id == y.Id;

    private Task OnTreeItemChecked(List<TreeViewItem<SysOrg>> items)
    {
        Model.DefaultDataScope.ScopeDefineOrgIdList = items.Select(a => a.Value.Id).ToList();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private List<SysOrg>? items;
    [Inject]
    private BlazorAppContext AppContext { get; set; }
    protected override async Task OnInitializedAsync()
    {
        items = (await SysOrgService.SelectorAsync());
        Items = OrgUtil.BuildTreeItemList(items, Model.DefaultDataScope.ScopeDefineOrgIdList).ToList();

        OrgItems = OrgUtil.BuildTreeIdItemList(items, new List<long> { Model.OrgId });

        if (!AppContext.CurrentUser.IsGlobal)
            Model.Category = Admin.Application.RoleCategoryEnum.Org;
        await base.OnInitializedAsync();
    }

    private string SearchText;
    private async Task<List<TreeViewItem<SysOrg>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;
        var items = (await SysOrgService.SelectorAsync());
        items = items.WhereIf(!string.IsNullOrEmpty(searchText), a => a.Name.Contains(searchText)).ToList();
        return OrgUtil.BuildTreeItemList(items, Model.DefaultDataScope.ScopeDefineOrgIdList);
    }
    [NotNull]
    private List<TreeViewItem<long>> OrgItems { get; set; }
}
