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
using System.Linq;
using ThingsGateway.Common.Extension;
using ThingsGateway.DataEncryption;
using ThingsGateway.Extension.Generic;
using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

internal sealed class AgvUserService : BaseService<AgvUser>, IAgvUserService
{
    private readonly IAgvRelationService _relationService;
    private readonly IAgvResourceService _sysResourceService;
    private readonly IAgvRoleService _roleService;
    private readonly ISysDictService _configService;
    private readonly IAgvPositionService _sysPositionService;
    private readonly ISysOrgService _sysOrgService;
    private readonly IVerificatInfoService _verificatInfoService;

    public AgvUserService(
        IVerificatInfoService verificatInfoService,
        IAgvRelationService relationService,
        IAgvPositionService sysPositionService,
        ISysOrgService sysOrgService,
        IAgvResourceService sysResourceService,
        IAgvRoleService roleService,
        ISysDictService configService)
    {
        _sysOrgService = sysOrgService;
        _sysPositionService = sysPositionService;
        _relationService = relationService;
        _sysResourceService = sysResourceService;
        _roleService = roleService;
        _configService = configService;
        _verificatInfoService = verificatInfoService;
    }

    #region ���ݷ�Χ���

    /// <inheritdoc/>
    public async Task<HashSet<long>?> GetCurrentUserDataScopeAsync()
    {
        if (UserManager.SuperAdmin || UserManager.UserId == 0)
            return null;
        var userInfo = await GetUserByIdAsync(UserManager.UserId).ConfigureAwait(false);//��ȡ�û���Ϣ
        var roles = await _roleService.GetRoleListByUserIdAsync(UserManager.UserId).ConfigureAwait(false);
        if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ALL))
        {
            return null;
        }
        else
        {
            var scopeDefineOrgIdList = roles.Where(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_DEFINE).SelectMany(a => a.DefaultDataScope.ScopeDefineOrgIdList);

            HashSet<long> orgChilds = new();
            HashSet<long> orgs = new();
            if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_CHILD))
            {
                orgChilds = userInfo.ScopeOrgChildList;
            }
            if (roles.Any(a => a.DefaultDataScope.ScopeCategory == DataScopeEnum.SCOPE_ORG_CHILD))
            {
                orgs = new HashSet<long>() { userInfo.OrgId };
            }
            return scopeDefineOrgIdList.Concat(orgChilds).Concat(orgs).ToHashSet();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CheckApiDataScopeAsync(long? orgId, long createUerId, bool throwEnable = true)
    {
        var hasPermission = true;
        //�ж����ݷ�Χ
        var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        if (dataScope is { Count: > 0 })//����л���
        {
            if (orgId == null || !dataScope.Contains(orgId.Value))//�жϻ���id�Ƿ������ݷ�Χ
                hasPermission = false;
        }
        else if (dataScope is { Count: 0 })// ��ʾ���Լ�
        {
            if (createUerId != 0 && createUerId != UserManager.UserId)
                hasPermission = false;//�����Ĵ����˲����Լ��򱨴�
        }
        if (!hasPermission && throwEnable)
        {
            throw Oops.Bah(App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Application.OperDescAttribute))["NoPermission"]);
        }
        return hasPermission;
    }

    public async Task<bool> CheckApiDataScopeAsync(IEnumerable<long> orgIds, IEnumerable<long> createUerIds, bool throwEnable = true)
    {
        var hasPermission = true;
        //�ж����ݷ�Χ
        var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        if (dataScope is { Count: > 0 })//����л���
        {
            if (orgIds == null || !dataScope.IsSupersetOf(orgIds))//�жϻ���id�б��Ƿ�ȫ�����ݷ�Χ
                hasPermission = false;
        }
        else if (dataScope is { Count: 0 })// ��ʾ���Լ�
        {
            if (createUerIds.Any(it => it != 0 && it != UserManager.UserId))//���������id�����κβ����Լ������Ļ���
                hasPermission = false;
        }
        if (!hasPermission && throwEnable)
        {
            throw Oops.Bah(App.CreateLocalizerByType(typeof(ThingsGateway.Admin.Application.OperDescAttribute))["NoPermission"]);
        }
        return hasPermission;
    }

    #endregion

    #region ��ѯ

    /// <inheritdoc/>
    public async Task<AgvUser?> GetUserByAccountAsync(string account, long? tenantId)
    {
        var userId = await GetIdByAccountAsync(account, tenantId).ConfigureAwait(false);//��ȡ�û�ID
        if (userId > 0)
        {
            var sysUser = await GetUserByIdAsync(userId).ConfigureAwait(false);//��ȡ�û���Ϣ
            if (sysUser?.Account == account)
                return sysUser;
            else
                return null;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// �����û�id��ȡ�û��������ڷ���null
    /// </summary>
    /// <param name="userId">�û�id</param>
    /// <returns>�û�</returns>
    public async Task<AgvUser?> GetUserByIdAsync(long userId)
    {
        //�ȴ�Cache��
        var sysUser = App.CacheService.HashGetOne<AgvUser>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUser", userId.ToString());
        sysUser ??= await GetUserFromDbAsync(userId).ConfigureAwait(false);//�����ݿ����û���Ϣ
        return sysUser;
    }

    /// <summary>
    /// �����˺Ż�ȡ�û�id
    /// </summary>
    /// <param name="account">�˺�</param>
    /// <param name="tenantId">�⻧id</param>
    /// <returns>�û�id</returns>
    public async Task<long> GetIdByAccountAsync(string account, long? tenantId)
    {
        var key = ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUserAccount";
        var orgIds = new HashSet<long>();
        if (tenantId > 0)
        {
            key += $":{tenantId}";
            orgIds = await _sysOrgService.GetOrgChildIdsAsync(tenantId.Value).ConfigureAwait(false);//��ȡ�¼�����
        }
        //�ȴ�Cache��
        var userId = App.CacheService.HashGetOne<long>(key, account);
        if (userId == 0)
        {
            //�����ȡ�û��˺Ŷ�ӦID
            using var db = GetDB();
            userId = await db.Queryable<AgvUser>()
                .Where(it => it.Account == account)
                .WhereIF(orgIds.Count > 0, it => orgIds.Contains(it.OrgId))
                .Select(it => it.Id).FirstAsync().ConfigureAwait(false);
            if (userId != 0)
            {
                //����Cache
                App.CacheService.HashAdd(key, account, userId);
            }
        }
        return userId;
    }

    /// <summary>
    /// ��ȡ�û�ӵ�еİ�ť����
    /// </summary>
    /// <param name="userId">�û�id</param>
    /// <returns>��ť����</returns>
    public async Task<Dictionary<string, List<string>>> GetButtonCodeListAsync(long userId)
    {
        //��ȡ�û���Դ����
        var resourceList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasResource).ConfigureAwait(false);
        if (!resourceList.Any())//����б�ʾ�û�������Ȩ�˲����û���ɫ
        {
            //��ȡ�û���ɫ��ϵ����
            var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
            var roleIdList = roleList.Select(x => x.TargetId.ToLong());//��ɫID�б�
            if (roleIdList.Any())//������û��н�ɫ
            {
                resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList,
                    RelationCategoryEnum.RoleHasResource).ConfigureAwait(false);//��ȡ��Դ����
            }
        }
        var relationResourcePermissions = resourceList.Select(it => it.ExtJson?.FromJsonNetString<RelationResourcePermission>());
        var allResources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);

        var ids = relationResourcePermissions.Select(a => a.MenuId);
        var menus = allResources.Where(it => it.Category == ResourceCategoryEnum.Menu && ids.Contains(it.Id)).ToDictionary(a => a, a => relationResourcePermissions.FirstOrDefault(b => b.MenuId == a.Id));

        Dictionary<string, List<string>> buttonCodeList = new();
        foreach (var item in menus)
        {
            if (buttonCodeList.TryGetValue(item.Key.Href, out var buttonCode))
            {
                var buttonS = allResources.Where(a => item.Value.ButtonIds.Contains(a.Id));
                buttonCode.AddRange(buttonS.Select(a => a.Title));
            }
            else
            {
                var buttonS = allResources.Where(a => item.Value.ButtonIds.Contains(a.Id));
                buttonCodeList.Add(item.Key.Href, buttonS.Select(a => a.Title).ToList());
            }
        }

        var firstbuttons = allResources.Where(it => it.Category == ResourceCategoryEnum.Button && relationResourcePermissions.FirstOrDefault(a => a.MenuId == 0)?.ButtonIds?.Contains(it.Id) == true);
        buttonCodeList.Add(string.Empty, firstbuttons?.Select(a => a.Title).ToList());

        return buttonCodeList!;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataScope>> GetPermissionListByUserIdAsync(long userId)
    {
        List<DataScope>? permissions = new();

        #region Razorҳ��Ȩ��

        {
            var sysRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasPermission).ConfigureAwait(false);//�����û�ID��ȡ�û�Ȩ��
            if (!sysRelations.Any())//����б�ʾ�û�������Ȩ�˲����û���ɫ
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//�����û�ID��ȡ��ɫID
                if (roleIdList.Any())//�����ɫID��Ϊ��
                {
                    //��ȡ��ɫȨ����Ϣ
                    sysRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()),
                        RelationCategoryEnum.RoleHasPermission).ConfigureAwait(false);
                }
            }
            var relationGroup = sysRelations.GroupBy(it => it.TargetId);//����Ŀ��ID,Ҳ���ǽӿ������飬��Ϊ����һ���û������ɫ

            //��������
            foreach (var it in relationGroup)
            {
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });
            }
        }

        #endregion Razorҳ��Ȩ��

        #region APIȨ��

        {
            var apiRelations =
                await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasOpenApiPermission).ConfigureAwait(false);//�����û�ID��ȡ�û�Ȩ��
            if (!apiRelations.Any())//����б�ʾ�û�������Ȩ�˲����û���ɫ
            {
                var roleIdList =
                    await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);//�����û�ID��ȡ��ɫID
                if (roleIdList.Any())//�����ɫID��Ϊ��
                {
                    //��ȡ��ɫȨ����Ϣ
                    apiRelations = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(roleIdList.Select(it => it.TargetId.ToLong()),
                        RelationCategoryEnum.RoleHasOpenApiPermission).ConfigureAwait(false);
                }
            }
            var relationGroup = apiRelations.GroupBy(it => it.TargetId);//����Ŀ��ID,Ҳ���ǽӿ������飬��Ϊ����һ���û������ɫ

            //��������
            foreach (var it in relationGroup)
            {
                permissions.Add(new DataScope
                {
                    ApiUrl = it.Key,
                });
            }
        }

        #endregion APIȨ��

        return permissions;
    }

    /// <summary>
    /// �����ѯ
    /// </summary>
    /// <param name="option">��ѯ����</param>
    /// <param name="input">��ѯ����</param>
    public async Task<QueryData<AgvUser>> PageAsync(QueryPageOptions option, UserSelectorInput input)
    {
        if (input != null)
        {
            option.SortName = "u." + option.SortName;
            var orgIds = await _sysOrgService.GetOrgChildIdsAsync(input.OrgId).ConfigureAwait(false);//��ȡ�¼�����
            var dataScope = await GetCurrentUserDataScopeAsync().ConfigureAwait(false);

            return await QueryAsync(option, query =>
            query.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText))
             .WhereIF(input.OrgId > 0, u => orgIds.Contains(u.OrgId))//ָ������
                .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.OrgId))//��ָ�������б���ѯ
                .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
                .WhereIF(input.PositionId > 0, u => u.PositionId == input.PositionId)//ָ��ְλ

                .WhereIF(input.RoleId > 0,
                    u => SqlFunc.Subqueryable<AgvRelation>()
                        .Where(r => r.TargetId == input.RoleId.ToString() && r.ObjectId == u.Id && r.Category == RelationCategoryEnum.UserHasRole)
                        .Any())//ָ����ɫ

       .LeftJoin<SysOrg>((u, o) => u.OrgId == o.Id).LeftJoin<AgvPosition>((u, o, p) => u.PositionId == p.Id)
          .Select((u, o, p) => new AgvUser
          {
              Id = u.Id.SelectAll(),
              OrgName = o.Name,
              PositionName = p.Name,
              OrgNames = o.Names
          }
                  )
            .Mapper(u =>
            {
#pragma warning disable CS8625 // �޷��� null ������ת��Ϊ�� null ���������͡�
                u.Password = null;//�������
                u.Phone = DESEncryption.Decrypt(u.Phone);//�����ֻ���
#pragma warning restore CS8625 // �޷��� null ������ת��Ϊ�� null ���������͡�
            })).ConfigureAwait(false);
        }
        else
        {
            return await QueryAsync(option, query =>
        query.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText)).Mapper(u =>
                {
#pragma warning disable CS8625 // �޷��� null ������ת��Ϊ�� null ���������͡�
                    u.Password = null;//�������
                    u.Phone = DESEncryption.Decrypt(u.Phone);//�����ֻ���
#pragma warning restore CS8625 // �޷��� null ������ת��Ϊ�� null ���������͡�
                })).ConfigureAwait(false);
        }


    }

    /// <summary>
    /// ��ȡ�û�ӵ�еĽ�ɫ
    /// </summary>
    /// <param name="id">�û�id</param>
    /// <returns>��ɫid�б�</returns>
    public async Task<IEnumerable<long>> OwnRoleAsync(long id)
    {
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.UserHasRole).ConfigureAwait(false);
        return relations.Select(it => it.TargetId.ToLong());
    }

    /// <summary>
    /// ��ȡ�û�ӵ�е���Դ
    /// </summary>
    /// <param name="id">�û�id</param>
    public Task<GrantResourceData> OwnResourceAsync(long id)
    {
        return _roleService.OwnResourceAsync(id, RelationCategoryEnum.UserHasResource);
    }

    /// <summary>
    /// �����û�id��ȡ�û��б�
    /// </summary>
    /// <param name="input">�û�id�б�</param>
    /// <returns>�û��б�</returns>
    public async Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IEnumerable<long> input)
    {
        using var db = GetDB();
        var userList = await db.Queryable<AgvUser>().Where(it => input.Contains(it.Id)).Select<UserSelectorOutput>().ToListAsync().ConfigureAwait(false);
        return userList;
    }

    #endregion ��ѯ

    #region OPENAPI

    /// <summary>
    /// ��ȡ�û�ӵ�е�OpenApiȨ��
    /// </summary>
    /// <param name="id">�û�id</param>
    public async Task<GrantPermissionData> ApiOwnPermissionAsync(long id)
    {
        var roleOwnPermission = new GrantPermissionData { Id = id };//��������
        //��ȡ��ϵ�б�
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(id, RelationCategoryEnum.UserHasOpenApiPermission).ConfigureAwait(false);
        roleOwnPermission.GrantInfoList = relations.Select(it => it.ExtJson?.FromJsonNetString<RelationPermission>()!).Where(a => a != null);
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("UserGrantApiPermission")]
    public async Task GrantApiPermissionAsync(GrantPermissionData input)
    {
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//��ȡ�û�

        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.UserHasOpenApiPermission, input.Id,
                 input.GrantInfoList.Select(a => (a.ApiUrl, a.ToSystemTextJsonString())),
                true).ConfigureAwait(false);//���ӵ����ݿ�
            DeleteUserFromCache(input.Id);
        }
    }

    #endregion OPENAPI

    #region ����

    /// <inheritdoc/>
    [OperDesc("SaveUser", isRecordPar: false)]
    public async Task<bool> SaveUserAsync(AgvUser input, ItemChangedType changedType)
    {
        await CheckInput(input).ConfigureAwait(false);//������

        if (changedType == ItemChangedType.Add)
        {
            var agvUser = Newtonsoft.Json.JsonConvert.DeserializeObject<AgvUser>(Newtonsoft.Json.JsonConvert.SerializeObject(input));
            //��ȡĬ������
            agvUser.Password = await GetDefaultPassWord(true).ConfigureAwait(false);//��������
            agvUser.Status = true;//Ĭ��״̬
            return await SaveAsync(agvUser, changedType).ConfigureAwait(false);//��������
        }
        else
        {
            await CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);
            var exist = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//��ȡ�û���Ϣ
            if (exist != null)
            {
                var isSuperAdmin = exist.Id == RoleConst.SuperAdminId;//�ж��Ƿ��г���
                if (isSuperAdmin && !UserManager.SuperAdmin)
                    throw Oops.Bah(Localizer["CanotEditAdminUser"]);

                if (input.Status != exist.Status)
                    CheckSelf(input.Id, input.Status ? Localizer["Enable"] : Localizer["Disable"]);//�ж��ǲ����Լ�

                var agvUser = input;//ʵ��ת��
                using var db = GetDB();
                var result = await db.UpdateableT(agvUser).IgnoreColumns(it =>
                        new
                        {
                            //���Ը����ֶ�
                            it.Password,
                            it.LastLoginDevice,
                            it.LastLoginIp,
                            it.LastLoginTime,
                            it.LatestLoginDevice,
                            it.LatestLoginIp,
                            it.LatestLoginTime
                        }).ExecuteCommandAsync().ConfigureAwait(false) > 0;
                if (result)//�</minimax:tool_call>
                {
                    DeleteUserFromCache(agvUser.Id);//ɾ���û�����

                    var verificatInfoIds = _verificatInfoService.GetListByUserId(agvUser.Id);

                    //���б���ɾ��
                    //ɾ���û�verificat����
                    _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
                    await NoticeUtil.UserLoginOut(new UserLoginOutEvent() { ClientIds = verificatInfoIds.SelectMany(a => a.ClientIds).ToList(), Message = Localizer["ExitVerificat"] }).ConfigureAwait(false);
                }
                return result;
            }
        }
        return false;
    }

    #endregion ����

    #region �༭

    /// <inheritdoc/>
    [OperDesc("ResetPassword")]
    public async Task ResetPasswordAsync(long id)
    {
        var sysUser = await GetUserByIdAsync(id).ConfigureAwait(false);

        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);

        var password = await GetDefaultPassWord(true).ConfigureAwait(false);//��ȡĬ������,���ﲻ��Aop������Ҫ����һ��
        using var db = GetDB();
        //��������
        if ((await db.UpdateSetColumnsTrueAsync<AgvUser>(it => new AgvUser
        {
            Password = password
        }, it => it.Id == id).ConfigureAwait(false)) > 0)
        {
            DeleteUserFromCache(id);//��cacheɾ���û���Ϣ
            var verificatInfoIds = _verificatInfoService.GetListByUserId(id);
            //ɾ���û�verificat����
            _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
            await NoticeUtil.UserLoginOut(new UserLoginOutEvent() { ClientIds = verificatInfoIds.SelectMany(a => a.ClientIds).ToList(), Message = Localizer["ExitVerificat"] }).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [OperDesc("UserGrantRole")]
    public async Task GrantRoleAsync(GrantUserOrRoleInput input)
    {
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//��ȡ�û���Ϣ
        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            var isSuperAdmin = (sysUser.Id == RoleConst.SuperAdminId || input.GrantInfoList.Any(a => a == RoleConst.SuperAdminRoleId)) && !UserManager.SuperAdmin;//�ж��Ƿ��г���
            if (isSuperAdmin)
                throw Oops.Bah(Localizer["CanotGrantAdmin"]);

            CheckSelf(input.Id, Localizer["GrantRole"]);//�ж��ǲ����Լ�

            //���û�����ɫ
            await _relationService.SaveRelationBatchAsync(RelationCategoryEnum.UserHasRole, input.Id, input.GrantInfoList.Select(it => (it.ToString(), string.Empty)), true).ConfigureAwait(false);
            DeleteUserFromCache(input.Id);//��cacheɾ���û���Ϣ
        }
    }

    /// <inheritdoc />
    [OperDesc("UserGrantResource")]
    public async Task GrantResourceAsync(GrantResourceData input)
    {
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//�˵�ID
        var extJsons = input.GrantInfoList.Select(it => it.ToSystemTextJsonString()).ToList();//��չ��Ϣ
        var relationUsers = new List<AgvRelation>();//Ҫ���ӵ��û���Դ����Ȩ��ϵ��
        var sysUser = await GetUserByIdAsync(input.Id).ConfigureAwait(false);//��ȡ�û�
        await CheckApiDataScopeAsync(sysUser.OrgId, sysUser.CreateUserId).ConfigureAwait(false);
        if (sysUser != null)
        {
            var resources = await _sysResourceService.GetAllAsync().ConfigureAwait(false);
            var menusList = resources.Where(a => a.Category == ResourceCategoryEnum.Menu && menuIds.Contains(a.Id));

            #region �û�ģ�鴦��

            //��ȡ�ҵ�ģ����ϢId�б�
            var moduleIds = menusList.Select(it => it.Module).Distinct();
            foreach (var item in moduleIds)
            {
                //����ɫ��Դ���ӵ��б�
                relationUsers.Add(new AgvRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = item.ToString(),
                    Category = RelationCategoryEnum.UserHasModule
                });
            }

            #endregion �û�ģ�鴦��

            #region �û���Դ����

            for (var i = 0; i < menuIds.Count; i++)
            {
                //����ɫ��Դ���ӵ��б�
                relationUsers.Add(new AgvRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = menuIds[i].ToString(),
                    Category = RelationCategoryEnum.UserHasResource,
                    ExtJson = extJsons?[i]
                });
            }
            #endregion �û���Դ����

            #region �û�Ȩ�޴���.

            //��ȡ�˵���Ϣ
            if (relationUsers.Count != 0)
            {
                //��ȡȨ����Ȩ��
                var permissions = App.GetService<IApiPermissionService>().PermissionTreeSelector(menusList.Select(it => it.Href));
                //Ҫ���ӵĽ�ɫ����ЩȨ���б�
                var relationUserPer = permissions.Select(it => new AgvRelation
                {
                    ObjectId = sysUser.Id,
                    TargetId = it.ApiRoute,
                    Category = RelationCategoryEnum.UserHasPermission,
                    ExtJson = new RelationPermission { ApiUrl = it.ApiRoute }
                            .ToSystemTextJsonString()
                });
                relationUsers.AddRange(relationUserPer);//�ϲ��б�
            }

            #endregion �û�Ȩ�޴���.

            #region �������ݿ�

            using var db = GetDB();

            //����
            var result = await db.UseTranAsync(async () =>
            {
                await db.Deleteable<AgvRelation>(it =>
                    it.ObjectId == sysUser.Id && (it.Category == RelationCategoryEnum.UserHasPermission
                    || it.Category == RelationCategoryEnum.UserHasResource
                    || it.Category == RelationCategoryEnum.UserHasModule

                    )).ExecuteCommandAsync().ConfigureAwait(false);
                await db.Insertable(relationUsers).ExecuteCommandAsync().ConfigureAwait(false);//�����µ�
            }).ConfigureAwait(false);
            if (result.IsSuccess)//����ɹ���
            {
                _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);//ˢ�¹�ϵ����
                _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//ˢ�¹�ϵ����
                _relationService.RefreshCache(RelationCategoryEnum.UserHasModule);//ˢ�¹�ϵ����
                DeleteUserFromCache(input.Id);//ɾ�����û�����
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion �������ݿ�
        }
    }

    #endregion �༭

    #region ɾ��

    /// <inheritdoc/>
    [OperDesc("DeleteUser")]
    public async Task<bool> DeleteUserAsync(HashSet<long> ids)
    {
        using var db = GetDB();
        var containsSuperAdmin = await db.Queryable<AgvUser>().Where(it => it.Id == RoleConst.SuperAdminId && ids.Contains(it.Id)).AnyAsync().ConfigureAwait(false);//�ж��Ƿ��г���
        if (containsSuperAdmin)
            throw Oops.Bah(Localizer["CanotDeleteAdminUser"]);
        if (ids.Contains(UserManager.UserId))
            throw Oops.Bah(Localizer["CanotDeleteSelf"]);

        var sysUsers = await GetUserListByIdListAsync(ids).ConfigureAwait(false);//��ȡ�û���Ϣ
        await CheckApiDataScopeAsync(sysUsers.Select(a => a.OrgId).ToList(), sysUsers.Select(a => a.CreateUserId).ToList()).ConfigureAwait(false);

        //����ɾ���Ĺ�ϵ
        var delRelations = new List<RelationCategoryEnum>
            {
                RelationCategoryEnum.UserHasResource, RelationCategoryEnum.UserHasPermission, RelationCategoryEnum.UserHasRole, RelationCategoryEnum.UserHasOpenApiPermission
                , RelationCategoryEnum.UserHasModule
            };
        //����
        var result = await db.UseTranAsync(async () =>
        {
            //������û���Ϊ������Ϣ
            await db.Updateable<AgvUser>().SetColumns(it => new AgvUser
            {
                DirectorId = null
            })
            .Where(it => ids.Contains(it.DirectorId.Value))
            .ExecuteCommandAsync().ConfigureAwait(false);

            //ɾ���û�
            await db.Deleteable<AgvUser>().In(ids).ExecuteCommandHasChangeAsync().ConfigureAwait(false);//ɾ��

            //ɾ����ϵ���û�����Դ��ϵ���û���Ȩ�޹�ϵ,�û����ɫ��ϵ
            await db.Deleteable<AgvRelation>(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync().ConfigureAwait(false);

            //ɾ����֯��������Ϣ
            await db.Deleteable<SysOrg>(it => ids.Contains(it.DirectorId.Value)).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);

        if (result.IsSuccess)//����ɹ���
        {
            DeleteUserFromCache(ids);//cacheɾ���û�
            _relationService.RefreshCache(RelationCategoryEnum.UserHasRole);//��ϵ��ˢ��UserHasRole����
            _relationService.RefreshCache(RelationCategoryEnum.UserHasResource);//��ϵ��ˢ��UserHasRole����
            _relationService.RefreshCache(RelationCategoryEnum.UserHasModule);//��ϵ��ˢ��UserHasModule����
            _relationService.RefreshCache(RelationCategoryEnum.UserHasPermission);//��ϵ��ˢ��UserHasRole����
            _relationService.RefreshCache(RelationCategoryEnum.UserHasOpenApiPermission);//��ϵ��ˢ��Relation_SYS_USER_HAS_OPENAPIPERMISSION����
            //����Щ�û������ߣ�������ע����Щ�û�
            foreach (var id in ids)
            {
                var verificatInfoIds = _verificatInfoService.GetListByUserId(id);
                _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
                await UserLoginOut(id, verificatInfoIds.SelectMany(a => a.ClientIds).ToList()).ConfigureAwait(false);
            }

            return true;
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(long userId)
    {
        DeleteUserFromCache(new List<long>
        {
            userId
        });
    }

    /// <inheritdoc />
    public void DeleteUserFromCache(IEnumerable<long> ids)
    {
        var userIds = ids.Select(it => it.ToString()).ToArray();//idתstring�б�
        var sysUsers = App.CacheService.HashGet<AgvUser>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUser", userIds);//��ȡ�û��б�
        if (sysUsers.Count != 0)
        {
            var accounts = sysUsers.Where(it => it != null).Select(it => it.Account).ToArray();//�˺ż���
            var phones = sysUsers.Select(it => it?.Phone);//�ֻ��ż���

            if (sysUsers.Any(it => it?.TenantId != null))//������⻧id���ǿյı�ʾ�Ƕ��⻧ģʽ
            {
                var userAccountKey = ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUserAccount";
                var tenantIds = sysUsers.Where(it => it?.TenantId != null).Select(it => it.TenantId.Value).Distinct().ToArray();//�⻧id�б�
                foreach (var tenantId in tenantIds)
                {
                    userAccountKey = $"{userAccountKey}:{tenantId}";
                    //ɾ���˺�
                    App.CacheService.HashDel<long>(userAccountKey, accounts);
                }
            }
            //ɾ���û���Ϣ
            App.CacheService.HashDel<AgvUser>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUser", userIds);
            //ɾ���˺�
            App.CacheService.HashDel<long>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUserAccount", accounts);

            App.CacheService.HashDel<VerificatInfo>(ThingsGatewayCacheConst.Cache_Prefix + "Cache_Token", userIds.Select(it => it.ToString()).ToArray());
        }
    }

    #endregion ɾ��

    #region ����

    /// <summary>
    /// ֪ͨ�û�����
    /// </summary>
    /// <param name="userId">�û�ID</param>
    /// <param name="verificatInfoIds">Token�б�</param>
    private async Task UserLoginOut(long userId, List<string>? verificatInfoIds)
    {
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent
        {
            Message = Localizer["ExitVerificat"],
            ClientIds = verificatInfoIds,
        }).ConfigureAwait(false);//֪ͨ�û�����
    }

    /// <summary>
    /// ��ȡĬ������
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetDefaultPassWord(bool isEncrypt = false)
    {
        //��ȡĬ������
        var appConfig = await _configService.GetAppConfigAsync().ConfigureAwait(false);
        return isEncrypt ? DESEncryption.Encrypt(appConfig.PasswordPolicy.DefaultPassword) : appConfig.PasswordPolicy.DefaultPassword;//�ж��Ƿ���Ҫ����
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="sysUser"></param>
    private async Task CheckInput(AgvUser sysUser)
    {
        var sysOrgList = await _sysOrgService.GetAllAsync().ConfigureAwait(false);//��ȡ��֯�б�
        var userOrg = sysOrgList.FirstOrDefault(it => it.Id == sysUser.OrgId);
        if (userOrg == null)
            throw Oops.Bah(Localizer[$"NoOrg"]);
        var tenantId = await _sysOrgService.GetTenantIdByOrgIdAsync(sysUser.OrgId, sysOrgList).ConfigureAwait(false);

        //�ж��˺��ظ�,ֱ�Ӵ�cache��
        var accountId = await GetIdByAccountAsync(sysUser.Account, tenantId).ConfigureAwait(false);
        if (accountId > 0 && accountId != sysUser.Id)
            throw Oops.Bah(Localizer["AccountDup", sysUser.Account]);
        //������䲻�ǿ�
        if (!string.IsNullOrEmpty(sysUser.Email))
        {
            var isMatch = sysUser.Email.MatchEmail();//��֤�����ʽ
            if (!isMatch)
                throw Oops.Bah(Localizer["EmailError", sysUser.Email]);

            using var db = GetDB();
            if (await db.Queryable<AgvUser>().Where(it => it.Email == sysUser.Email && it.Id != sysUser.Id).AnyAsync().ConfigureAwait(false))
                throw Oops.Bah(Localizer["EmailDup", sysUser.Email]);
        }
        //����ֻ��Ų��ǿ�
        if (!string.IsNullOrEmpty(sysUser.Phone))
        {
            if (!sysUser.Phone.MatchPhoneNumber())//��֤�ֻ���ʽ
                throw Oops.Bah(Localizer["PhoneError", sysUser.Phone]);
            sysUser.Phone = DESEncryption.Encrypt(sysUser.Phone);
        }

        if (sysUser.DirectorId == UserManager.UserId)
            throw Oops.Bah(Localizer["DirectorSelf"]);
    }

    /// <summary>
    /// ����Ƿ�Ϊ�Լ�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="operate">��������</param>
    private void CheckSelf(long id, string operate)
    {
        if (id == UserManager.UserId)//������Լ�
        {
            throw Oops.Bah(Localizer["CheckSelf", operate]);
        }
    }

    /// <summary>
    /// ���ݿ��ȡ�û���Ϣ
    /// </summary>
    /// <param name="userId">�û�ID</param>
    /// <returns></returns>
    private async Task<AgvUser?> GetUserFromDbAsync(long userId)
    {
        using var db = GetDB();
        var sysUser = await db.Queryable<AgvUser>()
             .LeftJoin<SysOrg>((u, o) => u.OrgId == o.Id)//����
            .LeftJoin<AgvPosition>((u, o, p) => u.PositionId == p.Id)//����
            .Where(u => u.Id == userId)
            .Select((u, o, p) => new AgvUser
            {
                Id = u.Id.SelectAll(),
                OrgName = o.Name,
                OrgNames = o.Names,
                PositionName = p.Name,
                OrgAndPosIdList = o.ParentIdList
            }).FirstAsync()
            .ConfigureAwait(false);
        if (sysUser != null)
        {
            sysUser.Password = DESEncryption.Decrypt(sysUser.Password);//��������
            sysUser.Phone = DESEncryption.Decrypt(sysUser.Phone);//�����ֻ���

            sysUser.OrgAndPosIdList.AddRange(sysUser.OrgId, sysUser.PositionId ?? 0);//������֯��ְλId
            if (sysUser.DirectorId != null)
            {
                sysUser.DirectorInfo = (await GetUserByIdAsync(sysUser.DirectorId.Value).ConfigureAwait(false)).AdaptUserSelectorOutput();//��ȡ������Ϣ
            }

            //��ȡ��ť��
            var buttonCodeList = await GetButtonCodeListAsync(sysUser.Id).ConfigureAwait(false);
            //��ȡ����Ȩ��
            var dataScopeList = await GetPermissionListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //��ȡȨ����
            var permissionCodeList = dataScopeList.Select(it => it.ApiUrl).ToHashSet();
            //��ȡ��ɫ��
            var roleCodeList = await _roleService.GetRoleListByUserIdAsync(sysUser.Id).ConfigureAwait(false);
            //Ȩ���븳ֵ
            sysUser.ButtonCodeList = buttonCodeList;
            sysUser.RoleIdList = roleCodeList.Select(it => it.Id).ToHashSet();
            sysUser.PermissionCodeList = permissionCodeList;
            sysUser.IsGlobal = roleCodeList.Any(a => a.Category == RoleCategoryEnum.Global);

            var sysOrgList = await _sysOrgService.GetAllAsync().ConfigureAwait(false);
            var scopeOrgChildList =
                (await _sysOrgService.GetChildListByIdAsync(sysUser.OrgId, true, sysOrgList).ConfigureAwait(false)).Select(it => it.Id).ToHashSet();//��ȡ�����������¼�����Id�б�
            sysUser.ScopeOrgChildList = scopeOrgChildList;
            var tenantId = await _sysOrgService.GetTenantIdByOrgIdAsync(sysUser.OrgId, sysOrgList).ConfigureAwait(false);
            sysUser.TenantId = tenantId;

            if (sysUser.Id == RoleConst.SuperAdminId)
            {
                var modules = (await _sysResourceService.GetAllAsync().ConfigureAwait(false)).Where(a => a.Category == ResourceCategoryEnum.Module).OrderBy(a => a.SortCode);
                sysUser.ModuleList = modules.ToList();//ģ���б���ֵ���û�
            }
            else
            {
                var moduleIds = await _relationService.GetUserModuleId(sysUser.RoleIdList, sysUser.Id).ConfigureAwait(false);//��ȡģ��ID�б�
                var modules = (await _sysResourceService.GetMuduleByMuduleIdsAsync(moduleIds).ConfigureAwait(false)).OrderBy(a => a.SortCode);//��ȡģ���б�
                sysUser.ModuleList = modules.ToList();//ģ���б���ֵ���û�
            }

            //����Cache
            App.CacheService.HashAdd(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUserAccount", sysUser.Account, sysUser.Id);
            App.CacheService.HashAdd(ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvUser", sysUser.Id.ToString(), sysUser);

            return sysUser;
        }
        return null;
    }

    #endregion ����
}
