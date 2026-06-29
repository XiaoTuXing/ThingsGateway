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
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class AgvPositionEdit
{
    [Parameter]
    [NotNull]
    public AgvPosition? Model { get; set; }

    [NotNull]
    private List<TreeViewItem<long>> Items { get; set; }

    [Inject]
    [NotNull]
    private IAgvPositionService AgvPositionService { get; set; }

    [Inject]
    [NotNull]
    private ISysOrgService SysOrgService { get; set; }
    private List<SelectedItem> BoolItems;

    protected override async Task OnInitializedAsync()
    {
        BoolItems = LocalizerUtil.GetBoolItems(Model.GetType(), nameof(Model.Status));
        var items = (await SysOrgService.SelectorAsync());
        Items = OrgUtil.BuildTreeIdItemList(items, new List<long> { Model.OrgId });
        await base.OnInitializedAsync();
    }
}
