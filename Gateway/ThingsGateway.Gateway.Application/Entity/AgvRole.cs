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
using System.ComponentModel.DataAnnotations;
using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ϵͳ��ɫ��
///</summary>
[SugarTable("agv_role", TableDescription = "ϵͳ��ɫ��")]
[Tenant(SqlSugarConst.DB_Custom)]
public class AgvRole : BaseEntity
{
    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual string Name { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", Length = 200)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string Code { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual RoleCategoryEnum Category { get; set; }

    /// <summary>
    /// ��֯id
    ///</summary>
    [SugarColumn(ColumnName = "OrgId", ColumnDescription = "��֯id", IsNullable = false)]
    [AutoGenerateColumn(Ignore = true)]
    public long OrgId { get; set; }

    /// <summary>
    /// Ĭ�����ݷ�Χ
    ///</summary>
    [SugarColumn(ColumnName = "DefaultDataScope", ColumnDescription = "Ĭ�����ݷ�Χ", IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = false)]
    [AutoGenerateColumn(Ignore = true)]
    public virtual DefaultDataScope DefaultDataScope { get; set; } = new();

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is AgvRole))
        {
            return false;
        }

        return Id == ((AgvRole)obj).Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

/// <summary>
/// Ĭ�����ݷ�Χ
/// </summary>
public class DefaultDataScope
{
    /// <summary>
    /// ���ݷ�Χ
    /// </summary>
    public DataScopeEnum ScopeCategory { get; set; }

    /// <summary>
    /// �Զ��������Χ�б�
    /// </summary>
    public List<long> ScopeDefineOrgIdList { get; set; } = new List<long>();
}
