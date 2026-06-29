//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class AgvPositionPage
{
    private long OrgId { get; set; }

    [Inject]
    [NotNull]
    private IAgvPositionService? AgvPositionService { get; set; }

    #region 查询

    private async Task<QueryData<AgvPosition>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await AgvPositionService.PageAsync(options, a =>
        a.WhereIF(OrgId != 0, b => b.OrgId == OrgId));
        return data;
    }

    private Task TreeChangedAsync(long id)
    {
        OrgId = id;
        return table.QueryAsync();
    }

    #endregion 查询

    #region 修改

    private async Task<bool> Delete(IEnumerable<AgvPosition> sysPositions)
    {
        try
        {
            return await AgvPositionService.DeletePositionAsync(sysPositions.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task<bool> Save(AgvPosition sysPosition, ItemChangedType itemChangedType)
    {
        try
        {
            return await AgvPositionService.SavePositionAsync(sysPosition, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

}
