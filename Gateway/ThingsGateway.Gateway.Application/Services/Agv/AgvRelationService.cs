//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Common.Extension;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

internal sealed class AgvRelationService : BaseService<AgvRelation>, IAgvRelationService
{
    #region 查询

    public async Task<List<AgvRelation>> GetRelationByCategoryAsync(RelationCategoryEnum category)
    {
        var key = $"{ThingsGatewayCacheConst.Cache_Prefix}Cache_AgvRelation{category}";
        var agvRelations = App.CacheService.Get<List<AgvRelation>>(key);
        if (agvRelations == null)
        {
            using var db = GetDB();
            agvRelations = await db.Queryable<AgvRelation>().Where(it => it.Category == category).ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(key, agvRelations ?? new());
        }

        return agvRelations;
    }

    public async Task<IEnumerable<AgvRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, RelationCategoryEnum category)
    {
        var agvRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = agvRelations.Where(it => it.ObjectId == objectId);
        return result;
    }

    public async Task<IEnumerable<AgvRelation>> GetRelationListByObjectIdListAndCategoryAsync(IEnumerable<long> objectIds, RelationCategoryEnum category)
    {
        var agvRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = agvRelations.Where(it => objectIds.Contains(it.ObjectId));
        return result;
    }

    public async Task<IEnumerable<AgvRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, RelationCategoryEnum category)
    {
        var agvRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = agvRelations.Where(it => it.TargetId == targetId);
        return result;
    }

    public async Task<IEnumerable<AgvRelation>> GetRelationListByTargetIdListAndCategoryAsync(IEnumerable<string> targetIds, RelationCategoryEnum category)
    {
        var agvRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = agvRelations.Where(it => targetIds.Contains(it.TargetId));
        return result;
    }

    public async Task<IEnumerable<long>> GetUserModuleId(IEnumerable<long> roleIdList, long userId)
    {
        IEnumerable<long>? moduleIds = Enumerable.Empty<long>();
        var roleRelation = await GetRelationByCategoryAsync(RelationCategoryEnum.RoleHasModule).ConfigureAwait(false);
        if (roleRelation?.Count > 0)
        {
            moduleIds = roleRelation.Where(it => roleIdList.Contains(it.ObjectId)).Select(it => it.TargetId.ToLong());
        }
        var userRelation = await GetRelationByCategoryAsync(RelationCategoryEnum.UserHasModule).ConfigureAwait(false);
        var userModuleIds = userRelation.Where(it => it.ObjectId == userId).Select(it => it.TargetId.ToLong());
        if (userModuleIds.Any())
        {
            moduleIds = (userModuleIds);
        }
        return moduleIds;
    }

    #endregion 查询

    #region 保存

    public async Task SaveRelationAsync(RelationCategoryEnum category, long objectId, string? targetId,
        string extJson, bool clear, bool refreshCache = true)
    {
        var agvRelation = new AgvRelation
        {
            ObjectId = objectId,
            TargetId = targetId,
            Category = category,
            ExtJson = extJson
        };
        using var db = GetDB();
        var result = await db.UseTranAsync(async () =>
        {
            if (clear)
                await db.Deleteable<AgvRelation>().Where(it => it.ObjectId == objectId && it.Category == category).ExecuteCommandAsync().ConfigureAwait(false);
            await db.InsertableT(agvRelation).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            if (refreshCache)
                RefreshCache(category);
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    public async Task SaveRelationBatchAsync(RelationCategoryEnum category, long objectId, IEnumerable<(string targetId, string extJson)> targetIdAndExtJsons, bool clear)
    {
        var agvRelations = targetIdAndExtJsons.Select(a => new AgvRelation
        {
            ObjectId = objectId,
            TargetId = a.targetId,
            Category = category,
            ExtJson = a.extJson
        });

        using var db = GetDB();
        var result = await db.UseTranAsync(async () =>
        {
            if (clear)
                await db.Deleteable<AgvRelation>().Where(it => it.ObjectId == objectId && it.Category == category).ExecuteCommandAsync().ConfigureAwait(false);
            await db.Insertable(agvRelations.ToList()).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            RefreshCache(category);
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    #endregion 保存

    #region 缓存

    public void RefreshCache(RelationCategoryEnum category)
    {
        var key = $"{ThingsGatewayCacheConst.Cache_Prefix}Cache_AgvRelation{category}";
        App.CacheService.Remove(key);
    }

    #endregion 缓存
}