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

public partial class AgvResourceEdit
{
    [Parameter]
    [NotNull]
    public AgvResource? Model { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long ModuleId { get; set; }

    [Parameter]
    [NotNull]
    public IEnumerable<SelectedItem>? MenuItems { get; set; }

    [Inject]
    [NotNull]
    private DialogService DialogService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<MenuIconList> Localizer { get; set; }

    private Task OnToggleIconDialog() => DialogService.Show(new DialogOption()
    {
        IsScrolling = false,
        Title = Localizer["ChoiceIcon"],
        ShowFooter = false,
        Component = BootstrapDynamicComponent.CreateComponent<MenuIconList>(new Dictionary<string, object?>()
        {
            [nameof(MenuIconList.Value)] = Model.Icon,
            [nameof(MenuIconList.ValueChanged)] = EventCallback.Factory.Create<string?>(this, v => Model.Icon = v!)
        })
    });
}
