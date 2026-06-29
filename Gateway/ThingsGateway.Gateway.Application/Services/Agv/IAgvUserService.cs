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
/// �û�����ӿڣ��ṩ�û���ز���������
/// </summary>
public interface IAgvUserService
{
    #region ���ݷ�Χ���

    /// <summary>
    /// ��ȡ��ǰAPI�û������ݷ�Χ
    /// null:����ӵ��ȫ������Ȩ��
    /// [xx,xx]:����ӵ�в��ֻ�����Ȩ��
    /// []���������Լ�Ȩ��
    /// </summary>
    /// <returns>�����б�</returns>
    Task<HashSet<long>?> GetCurrentUserDataScopeAsync();

    /// <summary>
    /// ����û��Ƿ��л���������Ȩ��
    /// </summary>
    /// <param name="orgId">����id</param>
    /// <param name="createUerId">������id</param>
    /// <param name="throwEnable"></param>
    /// <returns>�Ƿ���Ȩ��</returns>
    Task<bool> CheckApiDataScopeAsync(long? orgId, long createUerId, bool throwEnable = true);

    /// <summary>
    /// ����û��Ƿ��л���������Ȩ��
    /// </summary>
    /// <param name="orgIds">����id�б�</param>
    /// <param name="createUerIds">������id�б�</param>
    /// <param name="throwEnable"></param>
    /// <returns></returns>
    Task<bool> CheckApiDataScopeAsync(IEnumerable<long> orgIds, IEnumerable<long> createUerIds, bool throwEnable = true);

    #endregion

    /// <summary>
    /// ��ȡ�û�ӵ�е�OpenAPIȨ�ޡ�
    /// </summary>
    /// <param name="id">�û�ID��</param>
    /// <returns>�û�ӵ�е�OpenAPIȨ�ޡ�</returns>
    Task<GrantPermissionData> ApiOwnPermissionAsync(long id);

    /// <summary>
    /// ɾ���û���
    /// </summary>
    /// <param name="ids">�û�ID�б���</param>
    /// <returns>�Ƿ�ɾ���ɹ���</returns>
    Task<bool> DeleteUserAsync(HashSet<long> ids);

    /// <summary>
    /// �ӻ�����ɾ���û���Ϣ��
    /// </summary>
    /// <param name="userId">�û�ID��</param>
    void DeleteUserFromCache(long userId);

    /// <summary>
    /// �ӻ�����ɾ������û���Ϣ��
    /// </summary>
    /// <param name="ids">�û�ID�б���</param>
    void DeleteUserFromCache(IEnumerable<long> ids);

    /// <summary>
    /// ��ȡ�û�ӵ�еİ�ť���롣
    /// </summary>
    /// <param name="userId">�û�ID��</param>
    /// <returns>�Բ˵�����Ϊ������ť�����б�Ϊֵ���ֵ䡣</returns>
    Task<Dictionary<string, List<string>>> GetButtonCodeListAsync(long userId);

    /// <summary>
    /// �����˺Ż�ȡ�û�ID��
    /// </summary>
    /// <param name="account">�˺š�</param>
    /// <param name="tenantId">�⻧id��</param>
    /// <returns>�û�ID������û��������򷵻�0��</returns>
    Task<long> GetIdByAccountAsync(string account, long? tenantId);

    /// <summary>
    /// ��ȡ�û�ӵ�е�Ȩ�ޡ�
    /// </summary>
    /// <param name="userId">�û�ID��</param>
    /// <returns>Ȩ���б���</returns>
    Task<IEnumerable<DataScope>> GetPermissionListByUserIdAsync(long userId);

    /// <summary>
    /// �����˺Ż�ȡ�û���Ϣ��
    /// </summary>
    /// <param name="account">�˺š�</param>
    /// <param name="tenantId">�⻧id��</param>
    /// <returns>�û���Ϣ������û��������򷵻�null��</returns>
    Task<AgvUser?> GetUserByAccountAsync(string account, long? tenantId);

    /// <summary>
    /// �����û�ID��ȡ�û���Ϣ��
    /// </summary>
    /// <param name="userId">�û�ID��</param>
    /// <returns>�û���Ϣ������û��������򷵻�null��</returns>
    Task<AgvUser?> GetUserByIdAsync(long userId);

    /// <summary>
    /// �����û�ID�б���ȡ�û��б���
    /// </summary>
    /// <param name="input">�û�ID�б���</param>
    /// <returns>�û��б���</returns>
    Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IEnumerable<long> input);

    /// <summary>
    /// �����û�OpenAPIȨ�ޡ�
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ��</param>
    /// <returns>�첽����</returns>
    Task GrantApiPermissionAsync(GrantPermissionData input);

    /// <summary>
    /// �����û���Դ��
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ��</param>
    /// <returns>�첽����</returns>
    Task GrantResourceAsync(GrantResourceData input);

    /// <summary>
    /// �����û���ɫ��
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ��</param>
    /// <returns>�첽����</returns>
    Task GrantRoleAsync(GrantUserOrRoleInput input);

    /// <summary>
    /// ��ȡ�û�ӵ�е���Դ��
    /// </summary>
    /// <param name="id">�û�ID��</param>
    /// <returns>�û�ӵ�е���Դ���ݡ�</returns>
    Task<GrantResourceData> OwnResourceAsync(long id);

    /// <summary>
    /// ��ȡ�û�ӵ�еĽ�ɫID�б���
    /// </summary>
    /// <param name="id">�û�ID��</param>
    /// <returns>��ɫID�б���</returns>
    Task<IEnumerable<long>> OwnRoleAsync(long id);

    /// <summary>
    /// �����ѯ�û���Ϣ��
    /// </summary>
    /// <param name="option">��ѯѡ�</param>
    /// <param name="input">��ѯѡ�</param>
    /// <returns>�û���Ϣ�б���</returns>
    Task<QueryData<AgvUser>> PageAsync(QueryPageOptions option, UserSelectorInput input);

    /// <summary>
    /// �����û����롣
    /// </summary>
    /// <param name="id">�û�ID��</param>
    /// <returns>�첽����</returns>
    Task ResetPasswordAsync(long id);

    /// <summary>
    /// �����û���Ϣ��
    /// </summary>
    /// <param name="input">�û���Ϣ��</param>
    /// <param name="changedType">������͡�</param>
    /// <returns>�Ƿ񱣴�ɹ���</returns>
    Task<bool> SaveUserAsync(AgvUser input, ItemChangedType changedType);
}
