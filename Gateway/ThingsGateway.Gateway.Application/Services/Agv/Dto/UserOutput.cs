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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ѡ���û��������
/// </summary>
public class UserSelectorOutput : PrimaryIdEntity
{
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string Account { get; set; }

    /// <summary>
    /// ��֯ID
    /// </summary>
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false)]
    public long OrgId { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    public long CreateUserId { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is UserSelectorOutput))
        {
            return false;
        }

        return Id == ((UserSelectorOutput)obj).Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
