//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class AgvPositionUtil
{
    /// <summary>
    /// 构建级联数据
    /// </summary>
    public static List<CascaderItem> BuildCascaderItemList(IEnumerable<Admin.Application.PositionSelectorOutput> positions)
    {
        if (positions == null) return null;
        var trees = new List<CascaderItem>();
        foreach (var node in positions)
        {
            var item = new CascaderItem()
            {
                Text = node.Name,
                Value = node.Id.ToString(),
            };
            var data = BuildCascaderItemList(node.Children);
            foreach (var children in data)
            {
                item.AddItem(children);
            }
            trees.Add(item);
        }
        return trees;
    }
}
