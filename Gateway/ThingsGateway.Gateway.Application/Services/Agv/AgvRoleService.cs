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
using ThingsGateway.Common.Extension;
using ThingsGateway.Extension.Generic;
using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

internal sealed class AgvRoleService : BaseService<AgvRole>, IAgvRoleService
{
    private readonly IAgvRelationService _relationService;
    private readonly IAgvResourceService _sysResourceService;
    private readonly ISysOrgService _sysOrgService;
    private IAgvUserService _sysUserService;
    private IAgvUserService AgvUserService
    {
        get
        {
            if (_sysUserService == null)
            {
                _sysUserService = App.GetService<IAgvUserService>();
            }
            return _sysUserService;
        }
    }
    private IDispatchService<AgvRole> _dispatchService;

    public AgvRoleService(IAgvRelationService relationService, IAgvResourceService sysResourceService, ISysOrgService sysOrgService, IDispatchService<AgvRole> dispatchService)
    {
        _relationService = relationService;
        _sysResourceService = sysResourceService;
        _sysOrgService = sysOrgService;
        _dispatchService = dispatchService;
    }

    #region ��ѯ

    /// <inheritdoc/>
    public async Task<List<RoleTreeOutput>> TreeAsync()
    {
        var result = new List<RoleTreeOutput>();//���ؽ��
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//��ȡ���л���
        var sysRoles = await GetAllAsync().ConfigureAwait(false);//��ȡ���н�ɫ
        var dataScope = await AgvUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//��ָ����֯�б���ѯ
        sysRoles = sysRoles
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)

            .ToList();//��ָ��ְλ�б���ѯ

        var topOrgList = sysOrgList.Where(it => it.ParentId == 0);//��ȡ��������
        var globalRole = sysRoles.Where(it => it.Category == RoleCategoryEnum.Global);//��ȡȫ�ֽ�ɫ
        var children = globalRole.Select(it => new RoleTreeOutput
        {
            Id = it.Id,
            Name = it.Name,
            IsRole = true
        }).ToList();

        result.Add(new RoleTreeOutput()
        {
            Id = CommonUtils.GetSingleId(),
            Name = Localizer["Global"],
            Children = children
        });//����ȫ�ֽ�ɫ
        //������������
        foreach (var org in topOrgList)
        {
            var childIds = await _sysOrgService.GetOrgChildIdsAsync(org.Id, true, sysOrgList).ConfigureAwait(false);//��ȡ�����µ������Ӽ�ID
            var childRoles = sysRoles.Where(it => it.OrgId != 0 && childIds.Contains(it.OrgId));//��ȡ�����µ����н�ɫ

            List<RoleTreeOutput> childrenRoleTreeOutputs = new();

            foreach (var it in childRoles)
            {
                childrenRoleTreeOutputs.Add(new RoleTreeOutput()
                {
                    Id = it.Id,
                    Name = it.Name,
                    IsRole = true
                });
            }
            if (childrenRoleTreeOutputs.Count > 0)

            {
                var roleTreeOutput = new RoleTreeOutput
                {
                    Id = org.Id,
                    Name = org.Name,
                    IsRole = false,
                    Children = childrenRoleTreeOutputs
                };//ʵ������ɫ��
                result.Add(roleTreeOutput);
            }
        }
        return result;
    }

    /// <summary>
    /// �ӻ���/���ݿ��ȡȫ����ɫ��Ϣ
    /// </summary>
    /// <returns>��ɫ�б�</returns>
    public async Task<List<AgvRole>> GetAllAsync()
    {
        var key = ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvRole";
        var sysRoles = App.CacheService.Get<List<AgvRole>>(key);
        if (sysRoles == null)
        {
            using var db = GetDB();
            sysRoles = await db.Queryable<AgvRole>().ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(key, sysRoles);
        }
        return sysRoles;
    }

    /// <summary>
    /// �����û�id��ȡ��ɫ�б�
    /// </summary>
    /// <param name="userId">�û�id</param>
    /// <returns>��ɫ�б�</returns>
    public async Task<IEnumerable<AgvRole>> GetRoleListByUserIdAsync(long userId)
    {
        var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//�����û�ID��ȡ��ɫID
        var roleIdList = roleList.Select(x => x.TargetId.ToLong());//��ɫID�б�
        return (await GetAllAsync().ConfigureAwait(false)).Where(it => roleIdList.Contains(it.Id));
    }

    /// <inheritdoc/>
    public async Task<QueryData<AgvRole>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<AgvRole>, ISugarQueryable<AgvRole>>? queryFunc = null)
    {
        var dataScope = await AgvUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false); //��ȡ����ID��Χ
        queryFunc += a => a
             .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
             .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);

        return await QueryAsync(option, queryFunc).ConfigureAwait(false);
    }
    /// <summary>
    /// ���ݽ�ɫid��ȡ��ɫ�б�
    /// </summary>
    /// <param name="input">��ɫid�б�</param>
    /// <returns>��ɫ�б�</returns>
    public async Task<IEnumerable<AgvRole>> GetRoleListByIdListAsync(HashSet<long> input)
    {
        var roles = await GetAllAsync().ConfigureAwait(false);
        var roleList = roles.Where(it => input.Contains(it.Id));
        return roleList;
    }
    #endregion ��ѯ

    #region �޸�

    /// <summary>
    /// ɾ����ɫ
    /// </summary>
    /// <param name="ids">id�б�</param>
    [OperDesc("DeleteRole")]
    public async Task<bool> DeleteRoleAsync(HashSet<long> ids)
    {
        var sysRoles = await GetAllAsync().ConfigureAwait(false);//��ȡ���н�ɫ
        var hasSuperAdmin = sysRoles.Any(it => it.Id == RoleConst.SuperAdminRoleId && ids.Contains(it.Id));//�ж��Ƿ��г�������Ա
        if (hasSuperAdmin)
            throw Oops.Bah(Localizer["CanotDeleteAdmin"]);

        var dels = (await GetAllAsync().ConfigureAwait(false)).Where(a => ids.Contains(a.Id));
        await AgvUserService.CheckApiDataScopeAsync(dels.Select(a => a.OrgId), dels.Select(a => a.CreateUserId)).ConfigureAwait(false);

        //���ݿ���string��������ת��
        var targetIds = ids.Select(it => it.ToString()).ToList();
        //����ɾ���Ĺ�ϵ
        var delRelations = new List<RelationCategoryEnum> {
            RelationCategoryEnum.RoleHasResource,
            RelationCategoryEnum.RoleHasPermission,
            RelationCategoryEnum.RoleHasModule,
            RelationCategoryEnum.RoleHasOpenApiPermission };
        using var db = GetDB();
        //����
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<AgvRole>().In(ids).ExecuteCommandHasChangeAsync().ConfigureAwait(false);//ɾ��
            //ɾ����ϵ����ɫ����Դ��ϵ����ɫ��Ȩ�޹�ϵ
            await db.Deleteable<AgvRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync().ConfigureAwait(false);
            //ɾ����ϵ����ɫ���û���ϵ
            await db.Deleteable<AgvRelation>(it => targetIds.Contains(it.TargetId) && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//����ɹ���
        {
            RefreshCache();//ˢ�»���
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//��ϵ��ˢ��UserHasRole����
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//��ϵ��ˢ��RoleHasResource����
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//��ϵ��ˢ��RoleHasPermission����
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//��ϵ��ˢ��RoleHasModule����
            _relationService.RefreshCache(RelationCategoryEnum.RoleHasOpenApiPermission);//��ϵ��ˢ��RoleHasOpenApiPermission����
            await ClearTokenUtil.DeleteUserCacheByRoleIds(ids).ConfigureAwait(false);//�����ɫ���û�����
            return true;
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <summary>
    /// �����ɫ
    /// </summary>
    /// <param name="input">��ɫ</param>
    /// <param name="type">��������</param>
    [OperDesc("SaveRole")]
    public async Task<bool> SaveRoleAsync(AgvRole input, ItemChangedType type)
    {
        await CheckInput(input).ConfigureAwait(false);//������

        if (type == ItemChangedType.Add)
        {
            if (!((await AgvUserService.GetUserByIdAsync(UserManager.UserId).ConfigureAwait(false)).IsGlobal))
            {
                input.Category = RoleCategoryEnum.Org;
            }
        }
        else
        {
            await AgvUserService.CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);
        }

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {
            RefreshCache();
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//�����ɫ���û�����
            return true;
        }
        return false;
    }

    #endregion �޸�

    #region ��Ȩ

    #region ��Դ

    /// <summary>
    /// ��ȡӵ�е���Դ
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="category">����</param>
    public async Task<GrantResourceData> OwnResourceAsync(long id, RelationCategoryEnum category = RelationCategoryEnum.RoleHasResource)
    {
        var roleOwnResource = new GrantResourceData() { Id = id };//��������

        //��ȡ��ϵ�б�
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, category).ConfigureAwait(false);
        roleOwnResource.GrantInfoList = relations.Select(it => (it.ExtJson?.FromJsonNetString<RelationResourcePermission?>())).Where(a => a != null);
        return roleOwnResource;
    }
    /// <summary>
    /// ��Ȩ��Դ
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ</param>
    [OperDesc("RoleGrantResource")]
    public async Task GrantResourceAsync(GrantResourceData input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//�ж��Ƿ��г���
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//�˵�ID
        var extJsons = input.GrantInfoList.Select(it => it.ToSystemTextJsonString()).ToList();//��չ��Ϣ
        var relationRoles = new List<AgvRelation>();//Ҫ���ӵĽ�ɫ��Դ����Ȩ��ϵ��
        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(it => it.Id == input.Id);//��ȡ��ɫ

        await AgvUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

        if (sysRole != null)
        {
            var resources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);
            var menusList = resources.Where(a => a.Category == ResourceCategoryEnum.Menu && menuIds.Contains(a.Id));

            #region ��ɫģ�鴦��

            //��ȡ�ҵ�ģ����ϢId�б�
            var moduleIds = menusList.Select(it => it.Module).Distinct();
            foreach (var item in moduleIds)
            {
                //����ɫ��Դ���ӵ��б�
                relationRoles.Add(new AgvRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.RoleHasModule
                });
            }

            #endregion ��ɫģ�鴦��

            #region ��ɫ��Դ����

            //�����˵��б�
            for (var i = 0; i < menuIds.Count; i++)
            {
                //����ɫ��Դ���ӵ��б�
                relationRoles.Add(new AgvRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = menuIds[i].ToString(),
                    Category = RelationCategoryEnum.RoleHasResource,
                    ExtJson = extJsons?[i]
                });
            }

            #endregion ��ɫ��Դ����

            #region ��ɫȨ�޴���.
            var defaultDataScope = sysRole.DefaultDataScope;//��ȡĬ�����ݷ�Χ

            if (relationRoles.Count != 0)
            {
                //��ȡȨ����Ȩ��
                var permissions = App.GetService<IApiPermissionService>().PermissionTreeSelector(menusList.Select(it => it.Href));
                //Ҫ���ӵĽ�ɫ����ЩȨ���б�
                var relationRolePer = permissions.Select(it => new AgvRelation
                {
                    ObjectId = sysRole.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.RoleHasPermission,
                    ExtJson = new RelationPermission
                    {
                        ApiUrl = it.ApiRoute,
                    }.ToSystemTextJsonString()
                });
                relationRoles.AddRange(relationRolePer);//�ϲ��б�
            }

            #endregion ��ɫȨ�޴���.

            #region �������ݿ�

            using var db = GetDB();

            //����
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<AgvRelation>(it =>
                    it.ObjectId == sysRole.Id && (it.Category == RelationCategoryEnum.RoleHasPermission || it.Category == RelationCategoryEnum.RoleHasResource
                    || it.Category == RelationCategoryEnum.RoleHasModule

                    )).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Insertable(relationRoles).ExecuteCommandAsync().ConfigureAwait(false);//�����µ�
            }).ConfigureAwait(false);
            if (result.IsSuccess)//����ɹ���
            {
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasResource);//ˢ�¹�ϵ����
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasPermission);//ˢ�¹�ϵ����
                _relationService.RefreshCache(RelationCategoryEnum.RoleHasModule);//��ϵ��ˢ��
                await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//�����ɫ���û�����
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion �������ݿ�
        }
    }
    #endregion

    #region OPENAPI

    /// <summary>
    /// ��ȡ��ɫӵ�е�OpenApiȨ��
    /// </summary>
    /// <param name="id">��ɫid</param>
    public async Task<GrantPermissionData> ApiOwnPermissionAsync(long id)
    {
        var roleOwnPermission = new GrantPermissionData { Id = id };//��������
        //��ȡ��ϵ�б�
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.RoleHasOpenApiPermission).ConfigureAwait(false);

        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromJsonNetString<RelationPermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <summary>
    /// ��ȨOpenApiȨ��
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ</param>
    [OperDesc("RoleGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//�ж��Ƿ��г���
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);

        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(it => it.Id == input.Id);//��ȡ��ɫ

        await AgvUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

        if (sysRole != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.RoleHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToSystemTextJsonString()))
                , true).ConfigureAwait(false);//���ӵ����ݿ�
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//�����ɫ���û�����
        }
    }

    #endregion OPENAPI

    #region �û�

    /// <inheritdoc/>
    public async Task<IEnumerable<long>> OwnUserAsync(long id)
    {
        //��ȡ��ϵ�б�
        var relations = await _relationService.GetRelationListByTargetIdAndCategoryAsync(id.ToString(), RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
        return relations.Select(it => it.ObjectId);
    }

    /// <summary>
    /// ��Ȩ�û�
    /// </summary>
    /// <param name="input">��Ȩ����</param>
    [OperDesc("RoleGrantUser")]
    public async Task GrantUserAsync(GrantUserOrRoleInput input)
    {
        var isSuperAdmin = input.Id == RoleConst.SuperAdminRoleId;//�ж��Ƿ��г���
        if (isSuperAdmin)
            throw Oops.Bah(Localizer["CanotGrantAdmin"]);

        var sysRole = (await GetAllAsync().ConfigureAwait(false)).FirstOrDefault(a => a.Id == input.Id);
        await AgvUserService.CheckApiDataScopeAsync(sysRole.OrgId, sysRole.CreateUserId).ConfigureAwait(false);

        var sysRelations = input.GrantInfoList.Select(it =>
       new AgvRelation()
       {
           ObjectId = it,
           TargetId = input.Id.ToString(),
           Category = RelationCategoryEnum.UserHasRole
       }
       );
        using var db = GetDB();

        //����
        var result = await db.UseTranAsync(async () =>
        {
            var targetId = input.Id.ToString();
            await db.Deleteable<AgvRelation>(it => it.TargetId == targetId && it.Category == RelationCategoryEnum.UserHasRole).ExecuteCommandAsync().ConfigureAwait(false);//ɾ���ϵ�
            await db.Insertable(sysRelations.ToList()).ExecuteCommandAsync().ConfigureAwait(false);//�����µ�
        }).ConfigureAwait(false);
        if (result.IsSuccess)//����ɹ���
        {
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//ˢ�¹�ϵ��UserHasRole����
            await ClearTokenUtil.DeleteUserCacheByRoleIds(new List<long> { input.Id }).ConfigureAwait(false);//�����ɫ���û�����
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    #endregion

    /// <inheritdoc/>
    public void RefreshCache()
    {
        App.CacheService.Remove(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvRole");//ɾ��KEY

        _dispatchService.Dispatch(null);
    }

    #endregion ��Ȩ

    #region ����

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="sysRole"></param>
    private async Task CheckInput(AgvRole sysRole)
    {
        if (sysRole.Id == RoleConst.SuperAdminRoleId)
            throw Oops.Bah(Localizer["CanotEditAdmin"]);
        if (sysRole.Category == RoleCategoryEnum.Org && sysRole.OrgId == 0)
            throw Oops.Bah(Localizer["OrgNotNull"]);

        if (sysRole.Category == RoleCategoryEnum.Global)//�����ȫ��
            sysRole.OrgId = 0;//����id��0

        var sysRoles = await GetAllAsync().ConfigureAwait(false);//��ȡ����
        var repeatName = sysRoles.Any(it => it.OrgId == sysRole.OrgId && it.Name == sysRole.Name && it.Id != sysRole.Id);//�Ƿ����ظ���ɫ����
        if (repeatName)//�����
        {
            if (sysRole.OrgId == 0)
                throw Oops.Bah(Localizer["SameOrgAgvNameDup", sysRole.Name]);
            throw Oops.Bah(Localizer["AgvNameDup", sysRole.Name]);
        }

        if (!((await GetRoleListByUserIdAsync(UserManager.UserId).ConfigureAwait(false)).Any(a => a.Category == RoleCategoryEnum.Global)) && sysRole.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ALL)
            throw Oops.Bah(Localizer["CannotRoleScopeAll"]);

        //���codeû��
        if (string.IsNullOrEmpty(sysRole.Code))
        {
            sysRole.Code = RandomHelper.CreateRandomString(10);//��ֵCode
        }
        //�ж��Ƿ�����ͬ��Code
        if (sysRoles.Any(it => it.Code == sysRole.Code && it.Id != sysRole.Id))
            throw Oops.Bah(Localizer["AgvCodeDup", sysRole.Code]);
    }

    #endregion ����
}
