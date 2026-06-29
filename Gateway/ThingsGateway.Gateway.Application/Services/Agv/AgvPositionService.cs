// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using ThingsGateway.Common.Extension;
using ThingsGateway.Extension.Generic;
using ThingsGateway.FriendlyException;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ְλ����
/// </summary>
public class AgvPositionService : BaseService<AgvPosition>, IAgvPositionService
{
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
    private readonly ISysOrgService _sysOrgService;
    private IDispatchService<AgvPosition> _dispatchService;

    public AgvPositionService(ISysOrgService sysOrgService, IDispatchService<AgvPosition> dispatchService)
    {
        _sysOrgService = sysOrgService;
        _dispatchService = dispatchService;
    }

    public async Task<List<AgvPosition>> GetAllAsync(bool showDisabled = true)
    {
        var key = $"{ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvPosition"}";//ϵͳ����key
        var sysPositions = App.CacheService.Get<List<AgvPosition>>(key);
        if (sysPositions == null)
        {
            using var db = GetDB();
            sysPositions = (await db.Queryable<AgvPosition>().ToListAsync().ConfigureAwait(false));
            App.CacheService.Set(key, sysPositions);
        }
        if (!showDisabled)
        {
            sysPositions = sysPositions.Where(it => it.Status).ToList();
        }
        return sysPositions;
    }

    public async Task<AgvPosition> GetAgvPositionById(long id)
    {
        var list = await GetAllAsync().ConfigureAwait(false);
        return list.FirstOrDefault(x => x.Id == id);
    }

    [OperDesc("DeletePosition")]
    public async Task<bool> DeletePositionAsync(IEnumerable<long> ids)
    {
        //��ȡ����ID
        if (ids.Any())
        {
            using var db = GetDB();
            //�����֯�����û�����ɾ��
            if (await db.Queryable<AgvUser>().AnyAsync(it => ids.Contains(it.PositionId.Value)).ConfigureAwait(false))
            {
                throw Oops.Bah(Localizer["DeleteAgvUserFirst"]);
            }

            var dels = (await GetAllAsync().ConfigureAwait(false)).Where(a => ids.Contains(a.Id));
            await AgvUserService.CheckApiDataScopeAsync(dels.Select(a => a.OrgId), dels.Select(a => a.CreateUserId)).ConfigureAwait(false);
            //ɾ��ְλ
            var result = await base.DeleteAsync(ids).ConfigureAwait(false);
            if (result)
                RefreshCache();
            return result;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// �����ѯ
    /// </summary>
    public async Task<QueryData<AgvPosition>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<AgvPosition>, ISugarQueryable<AgvPosition>>? queryFunc = null)
    {
        var dataScope = await AgvUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false); //��ȡ����ID��Χ
        queryFunc += a => a
                    .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);

        return await QueryAsync(option, queryFunc).ConfigureAwait(false);
    }

    [OperDesc("SavePosition")]
    public async Task<bool> SavePositionAsync(AgvPosition input, ItemChangedType type)
    {
        await CheckInput(input).ConfigureAwait(false);//������
        if (type == ItemChangedType.Update)
            await AgvUserService.CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);

        var reuslt = await base.SaveAsync(input, type).ConfigureAwait(false);
        if (reuslt)
            RefreshCache();

        return reuslt;
    }

    /// <summary>
    /// ˢ�»���
    /// </summary>
    /// <returns></returns>
    private void RefreshCache()
    {
        App.CacheService.Remove($"{ThingsGatewayCacheConst.Cache_Prefix + "Cache_AgvPosition"}");
        _dispatchService.Dispatch(null);
    }

    /// <inheritdoc/>
    public async Task<List<PositionTreeOutput>> TreeAsync()
    {
        var result = new List<PositionTreeOutput>();//���ؽ��
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//��ȡ������֯
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//��ȡ����ְλ
        var dataScope = await AgvUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
           .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
           .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
           .ToList();//��ָ����֯�б���ѯ
        sysPositions = sysPositions
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//��ָ��ְλ�б���ѯ

        var posCategory = typeof(PositionCategoryEnum).GetEnumNames();//��ȡְλ����
        var topOrgList = sysOrgList.Where(it => it.ParentId == 0).ToList();//��ȡ������֯
        //����������֯
        foreach (var org in topOrgList)
        {
            var childIds = await _sysOrgService.GetOrgChildIdsAsync(org.Id, true, sysOrgList).ConfigureAwait(false);//��ȡ��֯�µ������Ӽ�ID
            var orgPositions = sysPositions.Where(it => childIds.Contains(it.OrgId)).ToList();//��ȡ��֯�µ�ְλ
            if (orgPositions.Count == 0) continue;
            var positionTreeOutput = new PositionTreeOutput
            {
                Id = org.Id,
                Name = org.Name,
                IsPosition = false
            };//ʵ������֯��
            //��ȡ��֯�µ�ְλְλ����
            foreach (var category in posCategory)
            {
                var id = CommonUtils.GetSingleId();//����ΨһID��ʱ��,��Ϊǰ����ҪID
                var categoryTreeOutput = new PositionTreeOutput
                {
                    Id = id,
                    Name = category,
                    IsPosition = false
                };//ʵ����ְλ������
                var positions = orgPositions.Where(it => it.Category.ToString() == category).ToList();//��ȡְλ�����µ�ְλ
                //����ְλ��ʵ����ְλ��
                positions.ForEach(it =>
                {
                    categoryTreeOutput.Children.Add(new PositionTreeOutput()
                    {
                        Id = it.Id,
                        Name = it.Name,
                        IsPosition = true
                    });//����ְλ
                });
                positionTreeOutput.Children.Add(categoryTreeOutput);
            }
            result.Add(positionTreeOutput);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<PositionSelectorOutput>> SelectorAsync(PositionSelectorInput input)
    {
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//��ȡ������֯
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//��ȡ����ְλ
        var dataScope = await AgvUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
           .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
           .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
           .ToList();//��ָ����֯�б���ѯ
        sysPositions = sysPositions
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//��ָ��ְλ�б���ѯ

        var result = await ConstructPositionSelector(sysOrgList, sysPositions).ConfigureAwait(false);//������
        return result;
    }

    /// <summary>
    /// ����ְλѡ����
    /// </summary>
    /// <param name="orgList">��֯�б�</param>
    /// <param name="sysPositions">ְλ�б�</param>
    /// <param name="parentId">��Id</param>
    /// <returns></returns>
    public async Task<List<PositionSelectorOutput>> ConstructPositionSelector(List<SysOrg> orgList, List<AgvPosition> sysPositions,
        long parentId = 0)
    {
        //���¼���֯�б�
        var orgInfos = orgList.Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode).ToList();
        var data = new List<PositionSelectorOutput>();
        if (orgInfos.Count > 0)//�����������0
        {
            foreach (var item in orgInfos)//������֯
            {
                var childIds = await _sysOrgService.GetOrgChildIdsAsync(item.Id, true, orgList).ConfigureAwait(false);//��ȡ��֯�µ������Ӽ�ID
                var orgPositions = sysPositions.Where(it => childIds.Contains(it.OrgId)).ToList();//��ȡ��֯�µ�ְλ
                if (orgPositions.Count > 0)//�����֯����֯�¼���ְλ
                {
                    var positionSelectorOutput = new PositionSelectorOutput
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Children = await ConstructPositionSelector(orgList, sysPositions, item.Id).ConfigureAwait(false)//�ݹ�
                    };//ʵ����ְλ��
                    var positions = orgPositions.Where(it => it.OrgId == item.Id).ToList();//��ȡ��֯�µ�ְλ
                    if (positions.Count > 0)//�����������0
                    {
                        foreach (var position in positions)
                        {
                            positionSelectorOutput.Children.Add(new PositionSelectorOutput
                            {
                                Id = position.Id,
                                Name = position.Name
                            });//����ְλ
                        }
                    }
                    data.Add(positionSelectorOutput);//���ӵ��б�
                }
            }
            return data;//���ؽ��
        }
        return new List<PositionSelectorOutput>();
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="input"></param>
    private async Task CheckInput(AgvPosition input)
    {
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//��ȡȫ��
        if (sysPositions.Any(it => it.OrgId == input.OrgId && it.Name == input.Name && it.Id != input.Id))//�ж�ͬ���Ƿ��������ظ���
            throw Oops.Bah(Localizer["AgvNameDup", input.Name]);
        if (input.Id > 0)//���ID����0��ʾ�༭
        {
            var position = sysPositions.Where(it => it.Id == input.Id).FirstOrDefault();//��ȡ��ǰְλ
            if (position == null)
                throw Oops.Bah(Localizer["AgvPositionNull", input.Name]);
        }
        //���codeû��
        if (string.IsNullOrEmpty(input.Code))
        {
            input.Code = RandomHelper.CreateRandomString(10);//��ֵCode
        }
        else
        {
            //�ж��Ƿ�����ͬ��Code
            if (sysPositions.Any(it => it.Code == input.Code && it.Id != input.Id))
                throw Oops.Bah(Localizer["AgvCodeDup", input.Code]);
        }
    }


}
