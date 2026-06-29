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

namespace ThingsGateway.Gateway.Razor;

public partial class AgvRolePage
{
    private AgvRole? SearchModel { get; set; } = new();
    private long OrgId { get; set; }

    [Inject]
    [NotNull]
    private IAgvRoleService? AgvRoleService { get; set; }

    [Inject]
    [NotNull]
    private IAgvResourceService? AgvResourceService { get; set; }

    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }

    #region ��ѯ

    private async Task<QueryData<AgvRole>> OnQueryAsync(QueryPageOptions options)
    {
        var orgIds = await SysOrgService.GetOrgChildIdsAsync(OrgId);//��ȡ�¼�����
        var data = await AgvRoleService.PageAsync(options, a => a.WhereIF(OrgId != 0, b => orgIds.Contains(b.OrgId)));
        return data;
    }
    private Task TreeChangedAsync(long id)
    {
        OrgId = id;
        return table.QueryAsync();
    }
    #endregion ��ѯ

    #region �޸�

    private async Task<bool> Delete(IEnumerable<AgvRole> sysRoles)
    {
        try
        {
            return await AgvRoleService.DeleteRoleAsync(sysRoles.Select(a => a.Id).ToHashSet());
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task<bool> Save(AgvRole sysRole, ItemChangedType itemChangedType)
    {
        try
        {
            return await AgvRoleService.SaveRoleAsync(sysRole, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion �޸�

    #region ��Ȩ

    private async Task GrantApi(long id)
    {
        var hasResources = (await AgvRoleService.ApiOwnPermissionAsync(id))?.GrantInfoList;
        var ids = new List<string>();
        ids.AddRange(hasResources.Select(a => a.ApiUrl));

        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantApiPermission"],
            ShowCloseButton = false,
            ShowMaximizeButton = true,
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    GrantPermissionData data = new();
                    data.Id = id;
                    data.GrantInfoList = ids.Select(a => new RelationPermission() { ApiUrl = a });
                    await AgvRoleService.GrantApiPermissionAsync(data);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            Class = "dialog-table",
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantApiDialog>(new Dictionary<string, object?>
            {
                [nameof(GrantApiDialog.Value)] = ids,
                [nameof(GrantApiDialog.ValueChanged)] = (List<string> v) => { ids = v; return Task.CompletedTask; },
            }).Render(),
        };

        await DialogService.Show(op);
    }

    private async Task GrantResource(long id)
    {
        var grantInfoList = (await AgvRoleService.OwnResourceAsync(id))?.GrantInfoList.ToList();

        var menuData = grantInfoList.Select(a => a.MenuId);
        var buttonData = grantInfoList.SelectMany(a => a.ButtonIds);
        var value = menuData.Concat(buttonData).ToList();

        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantResource"],
            ShowCloseButton = false,
            ShowMaximizeButton = true,
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    GrantResourceData data = new();

                    var allResource = await AgvResourceService.GetAllAsync();
                    var resources = allResource.Where(a => value.Contains(a.Id));
                    var pResources = AgvResourceService.GetMyParentResources(allResource, resources);
                    var grantInfoList = new List<Admin.Application.RelationResourcePermission>();
                    foreach (var item in pResources.Concat(resources).Distinct().Where(a => a.Category == Admin.Application.ResourceCategoryEnum.Menu && !string.IsNullOrEmpty(a.Href)))
                    {
                        var relationResourcePermission = new Admin.Application.RelationResourcePermission();
                        relationResourcePermission.MenuId = item.Id;
                        relationResourcePermission.ButtonIds = AgvResourceService.GetResourceChilden(allResource, item.Id).Where(a => value.Contains(a.Id)).Select(a => a.Id).ToHashSet();
                        grantInfoList.Add(relationResourcePermission);
                    }

                    var buttons = resources.Where(a => a.Category == Admin.Application.ResourceCategoryEnum.Button && a.ParentId == 0);
                    grantInfoList.Add(new Admin.Application.RelationResourcePermission()
                    {
                        MenuId = 0,
                        ButtonIds = buttons.Select(a => a.Id).ToHashSet()
                    });

                    data.GrantInfoList = grantInfoList;
                    data.Id = id;
                    await AgvRoleService.GrantResourceAsync(data);

                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            Class = "dialog-table",
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantResourceDialog>(new Dictionary<string, object?>
            {
                [nameof(GrantResourceDialog.Value)] = value,
                [nameof(GrantResourceDialog.ValueChanged)] = (List<long> v) => { value = v; return Task.CompletedTask; },
            }).Render(),
        };
        await DialogService.Show(op);
    }

    private async Task GrantUser(long id)
    {
        var data = (await AgvRoleService.OwnUserAsync(id)).ToHashSet();
        GrantUserChoiceValues = data;
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraExtraLarge,
            Title = OperDescLocalizer["RoleGrantUser"],
            ShowMaximizeButton = true,
            Class = "dialog-table",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await OnGrantUserValueChanged(GrantUserChoiceValues, id, true);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<UserChoiceDialog>(new Dictionary<string, object?>
            {
                [nameof(UserChoiceDialog.Values)] = data,
                [nameof(UserChoiceDialog.ValuesChanged)] = (HashSet<long> v) => OnGrantUserValueChanged(v, id)
            }).Render(),
        };
        await DialogService.Show(option);
    }
    private HashSet<long> GrantUserChoiceValues = new();
    private async Task OnGrantUserValueChanged(HashSet<long> values, long roleId, bool change = false)
    {
        GrantUserChoiceValues = values;
        if (change)
        {
            GrantUserOrRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = roleId;
            userGrantRoleInput.GrantInfoList = values;
            await AgvRoleService.GrantUserAsync(userGrantRoleInput);
        }
    }

    #endregion ��Ȩ
}
