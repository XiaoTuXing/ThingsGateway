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
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Razor;

public partial class AgvResourcePage
{
    private Admin.Application.ResourceTableSearchModel CustomerSearchModel { get; set; } = new Admin.Application.ResourceTableSearchModel();

    private List<SelectedItem> ModuleSelectedItems { get; set; }

    private List<TreeViewItem<AgvResource>> MenuTreeItems { get; set; }
    private List<SelectedItem> MenuItems { get; set; }

    [CascadingParameter(Name = "ReloadMenu")]
    private Func<Task>? ReloadMenu { get; set; }

    [CascadingParameter(Name = "ReloadUser")]
    private Func<Task>? ReloadUser { get; set; }

    [Inject]
    [NotNull]
    private IAgvResourceService? AgvResourceService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CustomerSearchModel.Module = (await AgvResourceService.GetAllAsync()).FirstOrDefault(a => a.Category == Admin.Application.ResourceCategoryEnum.Module)?.Id ?? ResourceConst.SystemId;
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        ModuleSelectedItems = AgvResourceUtil.BuildModuleSelectList((await AgvResourceService.GetAllAsync())).ToList();
        MenuItems = AgvResourceUtil.BuildMenuSelectList((await AgvResourceService.GetAllAsync())).Concat(new List<SelectedItem>() { new("0", AdminLocalizer["Root"]) }).ToList();

        await base.OnParametersSetAsync();
    }

    #region 查询

    private async Task<QueryData<AgvResource>> OnQueryAsync(QueryPageOptions options)
    {
        MenuTreeItems = new List<TreeViewItem<AgvResource>>() { new TreeViewItem<AgvResource>(new AgvResource()) { Text = AdminLocalizer["Root"] } }.Concat(AgvResourceUtil.BuildTreeItemList((await AgvResourceService.GetAllAsync()).Where(a => a.Module == CustomerSearchModel.Module), new(), null)).ToList();

        var data = await AgvResourceService.PageAsync(options, CustomerSearchModel);
        return data;
    }

    #endregion ��ѯ

    #region �޸�
    private List<AgvResource> SelectedRows { get; set; } = new();
    private long CopyModule { get; set; }
    private long ChangeParentId { get; set; }

    private async Task OnCopy()
    {
        try
        {
            await AgvResourceService.CopyAsync(SelectedRows.Select(a => a.Id), CopyModule);
            await table.QueryAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
    private async Task OnChangeParent()
    {
        try
        {
            await AgvResourceService.ChangeParentAsync(SelectedRows.Select(a => a.Id).FirstOrDefault(), ChangeParentId);
            await table.QueryAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    private async Task<bool> Delete(IEnumerable<AgvResource> sysResources)
    {
        try
        {
            var result = await AgvResourceService.DeleteResourceAsync(sysResources.Select(a => a.Id).ToHashSet());
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            return result;
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task<bool> Save(AgvResource sysResource, ItemChangedType itemChangedType)
    {
        try
        {
            if (itemChangedType == ItemChangedType.Add && sysResource.Category != ResourceCategoryEnum.Module)
                sysResource.Module = CustomerSearchModel.Module;
            var result = await AgvResourceService.SaveResourceAsync(sysResource, itemChangedType);
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            return result;
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion �޸�

    #region ���ڵ�

    private static bool ModelEqualityComparer(AgvResource x, AgvResource y) => x.Id == y.Id;

    private async Task<IEnumerable<TableTreeNode<AgvResource>>> OnTreeExpand(AgvResource menu)
    {
        var sysResources = await AgvResourceService.GetAllAsync();
        var result = AgvResourceUtil.BuildTableTrees(sysResources, menu.Id);
        return result;
    }

    private static async Task<IEnumerable<TableTreeNode<AgvResource>>> TreeNodeConverter(IEnumerable<AgvResource> items)
    {
        await Task.CompletedTask;
        var result = AgvResourceUtil.BuildTableTrees(items, 0);
        return result;
    }

    #endregion ���ڵ�

    #region ����ҳ��

    private async Task OnAfterModifyAsync()
    {
        if (ReloadMenu != null)
        {
            await ReloadMenu();
        }
        await OnParametersSetAsync();
    }

    #endregion ����ҳ��
}
