namespace ASC.Api.Documents;

[Scope]
[DefaultRoute]
[ApiController]
public class PrivacyRoomController : ControllerBase
{
    private readonly AuthContext _authContext;
    private readonly EncryptionKeyPairDtoHelper _encryptionKeyPairHelper;
    private readonly FileStorageService<string> _fileStorageService;
    private readonly FileStorageService<int> _fileStorageServiceInt;
    private readonly ILog _logger;
    private readonly MessageService _messageService;
    private readonly PermissionContext _permissionContext;
    private readonly SettingsManager _settingsManager;
    private readonly TenantManager _tenantManager;

    public PrivacyRoomController(
        AuthContext authContext,
        PermissionContext permissionContext,
        SettingsManager settingsManager,
        TenantManager tenantManager,
        EncryptionKeyPairDtoHelper encryptionKeyPairHelper,
        FileStorageService<int> fileStorageServiceInt,
        FileStorageService<string> fileStorageService,
        MessageService messageService,
        IOptionsMonitor<ILog> option)
    {
        _authContext = authContext;
        _permissionContext = permissionContext;
        _settingsManager = settingsManager;
        _tenantManager = tenantManager;
        _encryptionKeyPairHelper = encryptionKeyPairHelper;
        _fileStorageServiceInt = fileStorageServiceInt;
        _fileStorageService = fileStorageService;
        _messageService = messageService;
        _logger = option.Get("ASC.Api.Documents");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <visible>false</visible>
    [Read("keys")]
    public EncryptionKeyPairDto GetKeys()
    {
        _permissionContext.DemandPermissions(new UserSecurityProvider(_authContext.CurrentAccount.ID), Constants.Action_EditUser);

        if (!PrivacyRoomSettings.GetEnabled(_settingsManager))
        {
            throw new System.Security.SecurityException();
        }

        return _encryptionKeyPairHelper.GetKeyPair();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <visible>false</visible>
    [Read("access/{fileId}")]
    public Task<IEnumerable<EncryptionKeyPairDto>> GetPublicKeysWithAccess(string fileId)
    {
        if (!PrivacyRoomSettings.GetEnabled(_settingsManager))
        {
            throw new System.Security.SecurityException();
        }

        return _encryptionKeyPairHelper.GetKeyPairAsync(fileId, _fileStorageService);
    }

    [Read("access/{fileId:int}")]
    public Task<IEnumerable<EncryptionKeyPairDto>> GetPublicKeysWithAccess(int fileId)
    {
        if (!PrivacyRoomSettings.GetEnabled(_settingsManager))
        {
            throw new System.Security.SecurityException();
        }

        return _encryptionKeyPairHelper.GetKeyPairAsync(fileId, _fileStorageServiceInt);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <visible>false</visible>
    [Read("")]
    public bool PrivacyRoom()
    {
        _permissionContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

        return PrivacyRoomSettings.GetEnabled(_settingsManager);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <visible>false</visible>
    [Update("keys")]
    public object SetKeysFromBody([FromBody] PrivacyRoomRequestDto inDto)
    {
        return SetKeys(inDto);
    }

    [Update("keys")]
    [Consumes("application/x-www-form-urlencoded")]
    public object SetKeysFromForm([FromForm] PrivacyRoomRequestDto inDto)
    {
        return SetKeys(inDto);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enable"></param>
    /// <returns></returns>
    /// <visible>false</visible>
    [Update("")]
    public bool SetPrivacyRoomFromBody([FromBody] PrivacyRoomRequestDto inDto)
    {
        return SetPrivacyRoom(inDto);
    }

    [Update("")]
    [Consumes("application/x-www-form-urlencoded")]
    public bool SetPrivacyRoomFromForm([FromForm] PrivacyRoomRequestDto inDto)
    {
        return SetPrivacyRoom(inDto);
    }

    private object SetKeys(PrivacyRoomRequestDto inDto)
    {
        _permissionContext.DemandPermissions(new UserSecurityProvider(_authContext.CurrentAccount.ID), Constants.Action_EditUser);

        if (!PrivacyRoomSettings.GetEnabled(_settingsManager))
        {
            throw new System.Security.SecurityException();
        }

        var keyPair = _encryptionKeyPairHelper.GetKeyPair();
        if (keyPair != null)
        {
            if (!string.IsNullOrEmpty(keyPair.PublicKey) && !inDto.Update)
            {
                return new { isset = true };
            }

            _logger.InfoFormat("User {0} updates address", _authContext.CurrentAccount.ID);
        }

        _encryptionKeyPairHelper.SetKeyPair(inDto.PublicKey, inDto.PrivateKeyEnc);

        return new
        {
            isset = true
        };
    }

    private bool SetPrivacyRoom(PrivacyRoomRequestDto inDto)
    {
        _permissionContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

        if (inDto.Enable)
        {
            if (!PrivacyRoomSettings.IsAvailable(_tenantManager))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "PrivacyRoom");
            }
        }

        PrivacyRoomSettings.SetEnabled(_tenantManager, _settingsManager, inDto.Enable);

        _messageService.Send(inDto.Enable ? MessageAction.PrivacyRoomEnable : MessageAction.PrivacyRoomDisable);

        return inDto.Enable;
    }
}
