//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://thingsgateway.cn/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Razor;

public partial class AgvUserEdit
{
    private List<SelectedItem> ModuleSelectedItems { get; set; }
    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }
    [Inject]
    private IAgvPositionService? AgvPositionService { get; set; }

    [Parameter]
    [NotNull]
    public AgvUser? Model { get; set; }

    private List<CascaderItem> Items { get; set; }
    private List<SelectedItem> BoolItems;
    [Inject]
    private IAgvResourceService AgvResourceService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        BoolItems = LocalizerUtil.GetBoolItems(Model.GetType(), nameof(Model.Status));
        var items = await AgvPositionService.SelectorAsync(new Admin.Application.PositionSelectorInput());
        Items = BuildCascaderItems(items);
        ModuleSelectedItems = AgvResourceUtil.BuildModuleSelectList((await AgvResourceService.GetAllAsync())).ToList();
        await InvokeAsync(StateHasChanged);
        await base.OnInitializedAsync();
    }

    private List<CascaderItem> BuildCascaderItems(IEnumerable<Gateway.Application.PositionSelectorOutput> positions)
    {
        if (positions == null) return new List<CascaderItem>();
        var trees = new List<CascaderItem>();
        foreach (var node in positions)
        {
            var item = new CascaderItem()
            {
                Text = node.Name,
                Value = node.Id.ToString(),
            };
            var data = BuildCascaderItems(node.Children);
            foreach (var children in data)
            {
                item.AddItem(children);
            }
            trees.Add(item);
        }
        return trees;
    }

    private Task OnSelectedItemChanged(CascaderItem[] items)
    {
        Model.OrgId = items.LastOrDefault()?.Parent?.Value?.ToLong() ?? 0;
        return Task.CompletedTask;
    }
    [Inject]
    ToastService ToastService { get; set; }

    #region ͷ��

    private List<UploadFile> PreviewFileList;

    [FileValidation(Extensions = [".png", ".jpg", ".jpeg"], FileSize = 200 * 1024)]
    public IBrowserFile? Picture { get; set; }

    private CancellationTokenSource? ReadAvatarToken { get; set; }

    public void Dispose()
    {
        ReadAvatarToken?.Cancel();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        PreviewFileList = new(new[] { new UploadFile { PrevUrl = Model.Avatar } });
        base.OnInitialized();
    }

    private async Task OnAvatarUpload(UploadFile file)
    {
        if (file?.File != null)
        {
            var format = file.File.ContentType;
            ReadAvatarToken ??= new CancellationTokenSource();
            if (ReadAvatarToken.IsCancellationRequested)
            {
                ReadAvatarToken.Dispose();
                ReadAvatarToken = new CancellationTokenSource();
            }

            await file.RequestBase64ImageFileAsync(format, 640, 480, 1024 * 200, token: ReadAvatarToken.Token);

            if (file.Code != 0)
            {
                await ToastService.Error($"{file.Error} ");
            }
            else
            {
                Model.Avatar = file.PrevUrl;
            }
        }
    }

    #endregion ͷ��
}
