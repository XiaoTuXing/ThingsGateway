//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://thingsgateway.cn/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Common.Extension;
using ThingsGateway.Extension.Generic;
using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

internal sealed class AgvResourceService : BaseService<AgvResource>, IAgvResourceService
{
    private readonly IAgvRelationService _relationService;

    private string CacheKey = $"{ThingsGatewayCacheConst.Cache_Prefix}Cache_AgvResource";

    public AgvResourceService(IAgvRelationService relationService)
    {
        _relationService = relationService;
    }

    #region ��ɾ�Ĳ�

    [OperDesc("CopyResource")]
    public async Task CopyAsync(IEnumerable<long> ids, long moduleId)
    {
        var resourceList = await GetAllAsync().ConfigureAwait(false);
        var myResourceList = resourceList.Where(a => ids.Contains(a.Id)).ToList();

        var parent = GetMyParentResources(resourceList, myResourceList);
        myResourceList = myResourceList.Concat(parent).Where(a => a.Category != ResourceCategoryEnum.Module).DistinctBy(a => a.Id).ToList();
        var tree = ConstructMenuTrees(myResourceList).ToList();
        AgvResourceService.SetTreeValue(tree, moduleId, 0);
        var data = MenuTreesToSaveLevel(tree);
        using var db = GetDB();
        var result = await db.Insertable(data).ExecuteCommandAsync().ConfigureAwait(false);
        RefreshCache();//ˢ�»���
    }

    private static void SetTreeValue(List<AgvResource> tree, long moduleId, long parentId)
    {
        if (tree == null) return;
        foreach (var item in tree)
        {
            item.Id = CommonUtils.GetSingleId();
            item.ParentId = parentId;
            item.Code = RandomHelper.CreateRandomString(10);
            item.Module = moduleId;
            AgvResourceService.SetTreeValue(item.Children, moduleId, item.Id);
        }
    }

    [OperDesc("ChangeParentResource")]
    public async Task ChangeParentAsync(long id, long parentMenuId)
    {
        var resourceList = await GetAllAsync().ConfigureAwait(false);
        var resource = resourceList.First(a => a.Id == id);
        resource.ParentId = parentMenuId;
        using var db = GetDB();
        var result = await db.UpdateableT(resource).ExecuteCommandAsync().ConfigureAwait(false);
        RefreshCache();//ˢ�»���
        _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//��ϵ��ˢ�»���
        _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//��ϵ��ˢ�»���
    }

    /// <summary>
    /// ɾ����Դ
    /// </summary>
    /// <param name="ids">id�б�</param>
    /// <returns></returns>
    [OperDesc("DeleteResource")]
    public async Task<bool> DeleteResourceAsync(HashSet<long> ids)
    {
        //ɾ��
        if (ids.Count != 0)
        {
            //��ȡ���в˵��Ͱ�ť
            var resourceList = await GetAllAsync().ConfigureAwait(false);
            //�ҵ�Ҫɾ���Ĳ˵�
            var delAgvResources = resourceList.Where(it => ids.Contains(it.Id));
            //�ҵ�Ҫɾ����ģ��
            var delModules = resourceList.Where(a => a.Category == ResourceCategoryEnum.Module).Where(it => ids.Contains(it.Id));

            //��ȡģ���µ������б�
            var delHashSet = delModules.Select(a => a.Id).ToHashSet();
            if (delHashSet.Count != 0)
            {
                var delModuleResources = resourceList.Where(it => delHashSet.Contains(it.Module));
                delAgvResources = delAgvResources.Concat(delModuleResources).ToHashSet();
            }
            //�������ò˵�
            var system = delAgvResources.FirstOrDefault(it => it.Code == ResourceConst.System);
            if (system != null)
                throw Oops.Bah(Localizer["CanotDeleteSystemResource", system.Title]);

            //��Ҫɾ������ԴID�б�
            var resourceIds = delAgvResources.SelectMany(it =>
            {
                var child = GetResourceChilden(resourceList, it.Id);
                return child.Select(c => c.Id).Concat(new List<long>() { it.Id });
            });
            var deleteIds = ids.Concat(resourceIds).ToHashSet();//���ӵ�ɾ��ID�б�

            using var db = GetDB();
            //����
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<AgvResource>().In(deleteIds.ToList()).ExecuteCommandAsync().ConfigureAwait(false);//ɾ���˵��Ͱ�ť
                await db.Deleteable<AgvRelation>()//��ϵ��ɾ����ӦRoleHasResource
                 .Where(it => it.Category == RelationCategoryEnum.RoleHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Deleteable<AgvRelation>()//��ϵ��ɾ����ӦUserHasResource
               .Where(it => it.Category == RelationCategoryEnum.UserHasResource && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (result.IsSuccess)//����ɹ���
            {
                RefreshCache();//��Դ���˵�ˢ�»���
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//��ϵ��ˢ�»���
                _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//��ϵ��ˢ�»���
                return true;
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
        return false;
    }

    /// <summary>
    /// �ӻ���/���ݿ��ȡȫ����Դ�б�
    /// </summary>
    /// <returns>ȫ����Դ�б�</returns>
    public async Task<List<AgvResource>> GetAllAsync()
    {
        var sysResources = App.CacheService.Get<List<AgvResource>>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvResource");
        if (sysResources == null)
        {
            using var db = GetDB();
            sysResources = await db.Queryable<AgvResource>().ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvResource", sysResources);
        }
        return sysResources;
    }

    /// <summary>
    /// ���ݲ˵�Id��ȡ�˵��б�
    /// </summary>
    /// <param name="menuIds">�˵�id�б�</param>
    /// <returns>�˵��б�</returns>
    public async Task<IEnumerable<AgvResource>> GetMenuByMenuIdsAsync(IEnumerable<long> menuIds)
    {
        var menuList = await GetAllAsync().ConfigureAwait(false);
        var menus = menuList.Where(it => it.Category == ResourceCategoryEnum.Menu && menuIds.Contains(it.Id));
        return menus;
    }

    /// <summary>
    /// ����ģ��Id��ȡģ���б�
    /// </summary>
    /// <param name="moduleIds">ģ��id�б�</param>
    /// <returns>�˵��б�</returns>
    public async Task<IEnumerable<AgvResource>> GetMuduleByMuduleIdsAsync(IEnumerable<long> moduleIds)
    {
        var moduleList = await GetAllAsync().ConfigureAwait(false);
        var modules = moduleList.Where(it => it.Category == ResourceCategoryEnum.Module && moduleIds.Contains(it.Id));
        return modules;
    }

    /// <summary>
    /// �����ѯ
    /// </summary>
    /// <param name="options">��ѯ����</param>
    /// <param name="searchModel">��ѯ����</param>
    /// <returns></returns>
    public Task<QueryData<AgvResource>> PageAsync(QueryPageOptions options, ResourceTableSearchModel searchModel)
    {
        return QueryAsync(options, b => b.Where(a => (a.Category == ResourceCategoryEnum.Module && a.Id == searchModel.Module) || (a.Category != ResourceCategoryEnum.Module && a.Module == searchModel.Module)));
    }

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="input">��Դ</param>
    /// <param name="type">��������</param>
    [OperDesc("SaveResource")]
    public async Task<bool> SaveResourceAsync(AgvResource input, ItemChangedType type)
    {
        var resource = await CheckInput(input).ConfigureAwait(false);//������
        using var db = GetDB();

        if (type == ItemChangedType.Add)
        {
            var result = await db.InsertableT(input).ExecuteCommandAsync().ConfigureAwait(false);
            RefreshCache();//ˢ�»���
            return result > 0;
        }
        else
        {
            var permissions = new List<AgvRelation>();
            if (resource.Href != input.Href)
            {
                //��ȡ���н�ɫ���û���Ȩ�޹�ϵ
                var rolePermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.RoleHasPermission).ConfigureAwait(false);
                var userPermissions = await _relationService.GetRelationByCategoryAsync(RelationCategoryEnum.UserHasPermission).ConfigureAwait(false);
                //�ҵ�����ƥ���Ȩ��
                rolePermissions = rolePermissions.Where(it => it.TargetId!.Contains(resource.Href)).ToList();
                userPermissions = userPermissions.Where(it => it.TargetId!.Contains(resource.Href)).ToList();
                //����·��
                rolePermissions.ForEach(it => it.TargetId = it.TargetId!.Replace(resource.Href, input.Href));
                userPermissions.ForEach(it => it.TargetId = it.TargetId!.Replace(resource.Href, input.Href));
                //���ӵ�Ȩ���б�
                permissions.AddRange(rolePermissions);
                permissions.AddRange(userPermissions);
            }
            //����
            var result = await db.UseTranAsync(async () =>
            {
                await db.UpdateableT(input).ExecuteCommandAsync().ConfigureAwait(false);//��������
                if (permissions.Count > 0)//���Ȩ���б�����0�͸���
                {
                    await db.Updateable(permissions).ExecuteCommandAsync().ConfigureAwait(false);//���¹�ϵ��
                }
            }).ConfigureAwait(false);
            if (result.IsSuccess)//����ɹ���
            {
                RefreshCache();//ˢ�²˵�����
                if (resource.Href != input.Href)
                {
                    _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);
                    _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);
                }
                return true;
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
    }

    #endregion ��ɾ�Ĳ�

    #region ����

    /// <summary>
    /// ˢ�»���
    /// </summary>
    public void RefreshCache()
    {
        App.CacheService.Remove(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvResource");
        //ɾ����������Ա�Ļ���
        App.RootServices.GetRequiredService<IAgvUserService>().DeleteUserFromCache(RoleConst.SuperAdminId);
    }

    #endregion ����

    #region ����

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="sysResource">��Դ</param>
    private async Task<AgvResource> CheckInput(AgvResource sysResource)
    {
        if (sysResource.Code.IsNullOrWhiteSpace()) //Ĭ�ϱ���
        {
            sysResource.Code = RandomHelper.CreateRandomString(10);
        }

        //����˵������ǲ˵�
        //if (sysResource.Category == ResourceCategoryEnum.Menu)
        //{
        //    if (string.IsNullOrEmpty(sysResource.Href))
        //        throw Oops.Bah("ResourceMenuHrefNotNull");
        //}

        //��ȡ�����б�
        var menList = await GetAllAsync().ConfigureAwait(false);
        //�ж��Ƿ���ͬ����ͬ��
        if (menList.Any(it => it.ParentId == sysResource.ParentId && it.Title == sysResource.Title && it.Id != sysResource.Id && it.Module == sysResource.Module))
            throw Oops.Bah(Localizer["ResourceDup", sysResource.Title]);
        if (sysResource.ParentId != 0)
        {
            //��ȡ����,�жϸ���ID������ȷ
            var parent = menList.Where(it => it.Id == sysResource.ParentId).FirstOrDefault();
            if (parent != null)
            {
                if (parent.Module != sysResource.Module)//���������ģ��͵�ǰģ�鲻һ��
                    throw Oops.Bah(Localizer["ModuleIdDiff"]);
                if (parent.Id == sysResource.Id)
                    throw Oops.Bah(Localizer["ResourceChoiceSelf"]);
            }
            else
            {
                throw Oops.Bah(Localizer["ResourceParentNull", sysResource.ParentId]);
            }
        }

        //���ID����0��ʾ�༭
        if (sysResource.Id > 0)
        {
            var resource = menList.FirstOrDefault(it => it.Id == sysResource.Id);
            if (resource == null)
                throw Oops.Bah(Localizer["NotFoundResource"]);
            return resource;
        }

        return null;
    }

    #endregion ����

    /// <inheritdoc/>
    private static List<AgvResource> MenuTreesToSaveLevel(IEnumerable<AgvResource> resourceList)
    {
        var flatList = new List<AgvResource>();

        void TraverseTree(AgvResource node)
        {
            // ���ӵ�ǰ�ڵ㵽ƽ���б�
            flatList.Add(node);

            // �����ǰ�ڵ����ӽڵ㣬��ݹ鴦��ÿ���ӽڵ�
            if (node.Children?.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    TraverseTree(child);
                }
            }
        }

        // ������Դ�б��е�ÿ�������ڵ�
        foreach (var resource in resourceList)
        {
            TraverseTree(resource);
        }

        return flatList;
    }

    /// <inheritdoc/>
    public IEnumerable<AgvResource> ConstructMenuTrees(List<AgvResource> resourceList, long parentId = 0)
    {
        //���¼���ԴID�б�
        var resources = resourceList.Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode);
        foreach (var item in resources)//������Դ
        {
            var children = ConstructMenuTrees(resourceList, item.Id).ToList();//�����ӽڵ�
            item.Children = children.Count > 0 ? children : null;
        }
        return resources;
    }

    /// <inheritdoc/>
    public IEnumerable<AgvResource> GetMyParentResources(IEnumerable<AgvResource> allMenuList, IEnumerable<AgvResource> myMenus)
    {
        var parentList = myMenus
            .SelectMany(it => GetResourceParent(allMenuList, it.ParentId))
                                .Where(parent => parent != null
                                && !myMenus.Contains(parent)
                                && !myMenus.Any(m => m.Id == parent.Id))
                                .Distinct();
        return parentList;
    }

    /// <inheritdoc/>
    public IEnumerable<AgvResource> GetResourceChilden(IEnumerable<AgvResource> resourceList, long parentId)
    {
        //���¼���ԴID�б�
        return resourceList.Where(it => it.ParentId == parentId)
                           .SelectMany(item => new List<AgvResource> { item }.Concat(GetResourceChilden(resourceList, item.Id)));
    }

    /// <inheritdoc/>
    public IEnumerable<AgvResource> GetResourceParent(IEnumerable<AgvResource> resourceList, long resourceId)
    {
        //���ϼ���ԴID�б�
        return resourceList.Where(it => it.Id == resourceId)
                           .SelectMany(item => new List<AgvResource> { item }.Concat(GetResourceParent(resourceList, item.ParentId)));
    }

}
