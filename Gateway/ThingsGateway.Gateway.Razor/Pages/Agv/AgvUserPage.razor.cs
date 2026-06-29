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

public partial class AgvUserPage
{
    private AgvUser? SearchModel { get; set; } = new();
    private long OrgId { get; set; }

    [Inject]
    [NotNull]
    private IAgvUserService? AgvUserService { get; set; }

    [Inject]
    [NotNull]
    private IAgvResourceService? AgvResourceService { get; set; }

    #region ��ѯ

    private async Task<QueryData<AgvUser>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await AgvUserService.PageAsync(options, new Admin.Application.UserSelectorInput() { OrgId = OrgId });
        return data;
    }
    private Task TreeChangedAsync(long id)
    {
        OrgId = id;
        return table.QueryAsync();
    }
    #endregion ��ѯ

    #region �޸�

    private async Task<bool> Delete(IEnumerable<AgvUser> sysUsers)
    {
        try
        {
            return await AgvUserService.DeleteUserAsync(sysUsers.Select(a => a.Id).ToHashSet());
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task GrantApi(long id)
    {
        var hasResources = (await AgvUserService.ApiOwnPermissionAsync(id))?.GrantInfoList;
        var ids = new List<string>();
        ids.AddRange(hasResources.Select(a => a.ApiUrl));

        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["UserGrantApiPermission"],
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
                    await AgvUserService.GrantApiPermissionAsync(data);
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
        var grantInfoList = (await AgvUserService.OwnResourceAsync(id))?.GrantInfoList.ToList();

        var menuData = grantInfoList.Select(a => a.MenuId);
        var buttonData = grantInfoList.SelectMany(a => a.ButtonIds);
        var value = menuData.Concat(buttonData).ToList();
        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["UserGrantResource"],
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

                    await AgvUserService.GrantResourceAsync(data);

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

    public HashSet<long> GrantRoleChoiceValues { get; set; }
    private async Task GrantRole(long id)
    {
        var data = (await AgvUserService.OwnRoleAsync(id)).ToHashSet();
        GrantRoleChoiceValues = data;
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraExtraLarge,
            Title = OperDescLocalizer["UserGrantRole"],
            ShowMaximizeButton = true,
            Class = "dialog-table",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await OnGrantRoleValueChanged(GrantRoleChoiceValues, id, true);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<RoleChoiceDialog>(new Dictionary<string, object?>
            {
                [nameof(RoleChoiceDialog.Values)] = data,
                [nameof(RoleChoiceDialog.ValuesChanged)] = (HashSet<long> v) => OnGrantRoleValueChanged(v, id),
            }).Render(),
        };
        await DialogService.Show(option);
    }

    private async Task OnGrantRoleValueChanged(HashSet<long> values, long userId, bool change = false)
    {
        if (GrantRoleChoiceValues != values)
        {
            GrantRoleChoiceValues = values;
        }
        if (change)
        {
            GrantUserOrRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = userId;
            userGrantRoleInput.GrantInfoList = GrantRoleChoiceValues;
            await AgvUserService.GrantRoleAsync(userGrantRoleInput);
        }
    }

    private async Task ResetPassword(long id)
    {
        try
        {
            await AgvUserService.ResetPasswordAsync(id);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    private async Task<bool> Save(AgvUser sysUser, ItemChangedType itemChangedType)
    {
        try
        {
            return await AgvUserService.SaveUserAsync(sysUser, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion �޸�
}
