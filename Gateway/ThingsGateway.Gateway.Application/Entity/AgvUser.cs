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
using Riok.Mapperly.Abstractions;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ϵͳ�û���
///</summary>
[SugarTable("agv_user", TableDescription = "ϵͳ�û���")]
[Tenant(SqlSugarConst.DB_Custom)]
public class AgvUser : BaseEntity
{
    /// <summary>
    /// ͷ��
    ///</summary>
    [SugarColumn(ColumnDescription = "ͷ��", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = false, Filterable = false)]
    [MapperIgnore]
    public virtual string? Avatar { get; set; }

    /// <summary>
    /// �˺�
    ///</summary>
    [SugarColumn(ColumnDescription = "�˺�", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual string Account { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [AutoGenerateColumn(Ignore = true)]
    public string Password { get; set; }

    /// <summary>
    /// ״̬
    ///</summary>
    [SugarColumn(ColumnDescription = "״̬")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public bool Status { get; set; } = true;

    /// <summary>
    /// �ֻ�
    /// ����ʹ����SM4�Զ����ܽ���
    ///</summary>
    [SugarColumn(ColumnDescription = "�ֻ�", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string? Email { get; set; }

    /// <summary>
    /// �ϴε�¼ip
    ///</summary>
    [SugarColumn(ColumnDescription = "�ϴε�¼ip", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// �ϴε�¼�豸
    ///</summary>
    [SugarColumn(ColumnDescription = "�ϴε�¼�豸", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LastLoginDevice { get; set; }

    /// <summary>
    /// �ϴε�¼ʱ��
    ///</summary>
    [SugarColumn(ColumnDescription = "�ϴε�¼ʱ��", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// �ϴε�¼�ص�
    ///</summary>
    [SugarColumn(ColumnDescription = "�ϴε�¼�ص�", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string LastLoginAddress { get; set; }

    /// <summary>
    /// ���µ�¼ip
    ///</summary>
    [SugarColumn(ColumnDescription = "���µ�¼ip", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LatestLoginIp { get; set; }

    /// <summary>
    /// ���µ�¼ʱ��
    ///</summary>
    [SugarColumn(ColumnDescription = "���µ�¼ʱ��", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public DateTime? LatestLoginTime { get; set; }

    /// <summary>
    /// ���µ�¼�豸
    ///</summary>
    [SugarColumn(ColumnDescription = "���µ�¼�豸", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string? LatestLoginDevice { get; set; }

    /// <summary>
    /// ���µ�¼�ص�
    ///</summary>
    [SugarColumn(ColumnDescription = "���µ�¼�ص�", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public string LatestLoginAddress { get; set; }

    /// <summary>
    /// ����id
    ///</summary>
    [SugarColumn(ColumnName = "OrgId", ColumnDescription = "����id", IsNullable = false)]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long OrgId { get; set; }

    /// <summary>
    /// ְλid
    ///</summary>
    [SugarColumn(ColumnName = "PositionId", ColumnDescription = "ְλid", IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    [Required]
    [NotNull]
    public virtual long? PositionId { get; set; }

    /// <summary>
    /// ����id
    ///</summary>
    [SugarColumn(ColumnName = "DirectorId", ColumnDescription = "����id", IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    public long? DirectorId { get; set; }

    #region other
    /// <summary>
    /// ������Ϣ
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public string OrgName { get; set; }

    /// <summary>
    /// ������Ϣȫ��
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string OrgNames { get; set; }

    /// <summary>
    /// ְλ��Ϣ
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string PositionName { get; set; }

    /// <summary>
    /// ��֯�ͻ���ID�б�,��֯ID���ϵ��������ְλ
    /// </summary>
    [SugarColumn(IsIgnore = true, IsJson = true)]
    [AutoGenerateColumn(Ignore = true)]
    public List<long>? OrgAndPosIdList { get; set; } = new List<long>();

    /// <summary>
    /// ������Ϣ
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public UserSelectorOutput? DirectorInfo { get; set; }

    #endregion

    #region other

    /// <summary>
    /// ��ť�뼯��
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<string, List<string>>? ButtonCodeList { get; set; } = new();

    /// <summary>
    /// Ȩ���뼯��
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public HashSet<string>? PermissionCodeList { get; set; } = new();

    /// <summary>
    /// ��ɫID����
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public HashSet<long>? RoleIdList { get; set; } = new();

    /// <summary>
    /// ���������»���ID����
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public HashSet<long>? ScopeOrgChildList { get; set; }

    /// <summary>
    /// ģ�鼯��
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public List<AgvResource>? ModuleList { get; set; } = new();

    /// <summary>
    /// �⻧Id
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public long? TenantId { get; set; }

    /// <summary>
    /// ȫ���Ñ�
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public bool IsGlobal { get; set; }

    #endregion other
}

/// <summary>
/// ���ݷ�Χ��
/// </summary>
public class DataScope
{
    /// <summary>
    /// API�ӿ�
    /// </summary>
    public string ApiUrl { get; set; }
}
