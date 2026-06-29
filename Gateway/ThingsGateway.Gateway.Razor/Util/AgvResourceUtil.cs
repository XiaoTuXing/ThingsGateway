//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Razor;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class AgvResourceUtil
{
    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    public static IEnumerable<SelectedItem> BuildMenuSelectList(IEnumerable<AgvResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Menu)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    public static IEnumerable<SelectedItem> BuildModuleSelectList(IEnumerable<AgvResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Module)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
            {
                GroupName = items.FirstOrDefault(a => a.Id == item.ParentId)?.Title!
            }
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造树形数据
    /// </summary>
    public static IEnumerable<TableTreeNode<AgvResource>> BuildTableTrees(IEnumerable<AgvResource> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new TableTreeNode<AgvResource>(item)
            {
                HasChildren = items.Any(i => i.ParentId == item.Id),
                IsExpand = items.Any(i => i.ParentId == item.Id),
                Items = BuildTableTrees(items, item.Id).ToList()
            }
        );
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<AgvResource>> BuildTreeItemList(IEnumerable<AgvResource> resources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<AgvResource> render, long parentId = 0, TreeViewItem<AgvResource>? parent = null, Func<AgvResource, bool>? disableFunc = null)
    {
        if (resources == null) return null;
        var trees = new List<TreeViewItem<AgvResource>>();
        var roots = resources.Where(i => i.ParentId == parentId).OrderBy(a => a.Module).ThenBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<AgvResource>(node)
            {
                Text = node.Title,
                Icon = node.Icon,
                IsDisabled = disableFunc == null ? false : disableFunc(node),
                IsActive = selectedItems.Any(v => node.Id == v),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(resources, selectedItems, render, node.Id, item, disableFunc) ?? new();
            trees.Add(item);
        }
        return trees;
    }
}
