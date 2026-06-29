//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public interface IAgvRelationService
{
    Task<List<AgvRelation>> GetRelationByCategoryAsync(RelationCategoryEnum category);

    Task<IEnumerable<AgvRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, RelationCategoryEnum category);

    Task<IEnumerable<AgvRelation>> GetRelationListByObjectIdListAndCategoryAsync(IEnumerable<long> objectIds, RelationCategoryEnum category);

    Task<IEnumerable<AgvRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, RelationCategoryEnum category);

    Task<IEnumerable<AgvRelation>> GetRelationListByTargetIdListAndCategoryAsync(IEnumerable<string> targetIds, RelationCategoryEnum category);

    Task<IEnumerable<long>> GetUserModuleId(IEnumerable<long> roleIdList, long userId);

    void RefreshCache(RelationCategoryEnum category);

    Task SaveRelationAsync(RelationCategoryEnum category, long objectId, string? targetId, string extJson, bool clear, bool refreshCache = true);

    Task SaveRelationBatchAsync(RelationCategoryEnum category, long objectId, IEnumerable<(string targetId, string extJson)> targetIdAndExtJsons, bool clear);
}