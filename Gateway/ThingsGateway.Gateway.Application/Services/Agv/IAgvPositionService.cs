// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ְλ����
/// </summary>
public interface IAgvPositionService
{
    #region ��ѯ

    /// <summary>
    /// ��ȡְλ�б�
    /// </summary>
    /// <returns>ְλ�б�</returns>
    Task<List<AgvPosition>> GetAllAsync(bool showDisabled = true);
    /// <summary>
    /// �����λ
    /// </summary>
    /// <param name="input">����</param>
    /// <param name="type">��������</param>
    Task<bool> SavePositionAsync(AgvPosition input, ItemChangedType type);

    /// <summary>
    /// ɾ����λ
    /// </summary>
    /// <param name="ids">id�б�</param>
    Task<bool> DeletePositionAsync(IEnumerable<long> ids);

    /// <summary>
    /// ������ѯ
    /// </summary>
    /// <param name="option">��ѯ����</param>
    /// <param name="queryFunc">��������</param>
    Task<QueryData<AgvPosition>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<AgvPosition>, ISugarQueryable<AgvPosition>>? queryFunc = null);

    /// <summary>
    /// ��ȡְλ��Ϣ
    /// </summary>
    /// <param name="id">ְλID</param>
    /// <returns>ְλ��Ϣ</returns>
    Task<AgvPosition> GetAgvPositionById(long id);

    /// <summary>
    /// ְλ���νṹ
    /// </summary>
    /// <returns></returns>
    Task<List<PositionTreeOutput>> TreeAsync();

    /// <summary>
    /// ְλѡ����
    /// </summary>
    /// <param name="input">��ѯ����</param>
    /// <returns></returns>
    Task<List<PositionSelectorOutput>> SelectorAsync(PositionSelectorInput input);

    #endregion

}
