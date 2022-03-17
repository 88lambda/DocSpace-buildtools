/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/

namespace ASC.Web.Files.ThirdPartyApp;

public class BoxApp : Consumer, IThirdPartyApp, IOAuthProvider
{
    public const string AppAttr = "box";

    private const string _boxUrlUserInfo = "https://api.box.com/2.0/users/me";
    private const string _boxUrlFile = "https://api.box.com/2.0/files/{fileId}";
    private const string _boxUrlUpload = "https://upload.box.com/api/2.0/files/{fileId}/content";

    public ILog Logger { get; }
    public IHttpClientFactory ClientFactory { get; }
    public string Scopes => string.Empty;
    public string CodeUrl => string.Empty;
    public string AccessTokenUrl => "https://www.box.com/api/oauth2/token";
    public string RedirectUri => string.Empty;
    public string ClientID => this["boxAppClientId"];
    public string ClientSecret => this["boxAppSecretKey"];
    public bool IsEnabled => !string.IsNullOrEmpty(ClientID) && !string.IsNullOrEmpty(ClientSecret);

    private readonly PathProvider _pathProvider;
    private readonly TenantUtil _tenantUtil;
    private readonly AuthContext _authContext;
    private readonly SecurityContext _securityContext;
    private readonly UserManager _userManager;
    private readonly UserManagerWrapper _userManagerWrapper;
    private readonly CookiesManager _cookiesManager;
    private readonly MessageService _messageService;
    private readonly Global _global;
    private readonly EmailValidationKeyProvider _emailValidationKeyProvider;
    private readonly FilesLinkUtility _filesLinkUtility;
    private readonly SettingsManager _settingsManager;
    private readonly PersonalSettingsHelper _personalSettingsHelper;
    private readonly BaseCommonLinkUtility _baseCommonLinkUtility;
    private readonly IOptionsSnapshot<AccountLinker> _snapshot;
    private readonly SetupInfo _setupInfo;
    private readonly TokenHelper _tokenHelper;
    private readonly DocumentServiceConnector _documentServiceConnector;
    private readonly ThirdPartyAppHandlerService _thirdPartyAppHandlerService;
    private readonly IServiceProvider _serviceProvider;

    public BoxApp() { }

    public BoxApp(
        PathProvider pathProvider,
        TenantUtil tenantUtil,
        IOptionsMonitor<ILog> option,
        AuthContext authContext,
        SecurityContext securityContext,
        UserManager userManager,
        UserManagerWrapper userManagerWrapper,
        CookiesManager cookiesManager,
        MessageService messageService,
        Global global,
        EmailValidationKeyProvider emailValidationKeyProvider,
        FilesLinkUtility filesLinkUtility,
        SettingsManager settingsManager,
        PersonalSettingsHelper personalSettingsHelper,
        BaseCommonLinkUtility baseCommonLinkUtility,
        IOptionsSnapshot<AccountLinker> snapshot,
        SetupInfo setupInfo,
        TokenHelper tokenHelper,
        DocumentServiceConnector documentServiceConnector,
        ThirdPartyAppHandlerService thirdPartyAppHandlerService,
        IServiceProvider serviceProvider,
        TenantManager tenantManager,
        CoreBaseSettings coreBaseSettings,
        CoreSettings coreSettings,
        IConfiguration configuration,
        ICacheNotify<ConsumerCacheItem> cache,
        ConsumerFactory consumerFactory,
        IHttpClientFactory clientFactory,
        string name, int order, Dictionary<string, string> additional)
        : base(tenantManager, coreBaseSettings, coreSettings, configuration, cache, consumerFactory, name, order, additional)
    {
        _pathProvider = pathProvider;
        _tenantUtil = tenantUtil;
        _authContext = authContext;
        _securityContext = securityContext;
        _userManager = userManager;
        _userManagerWrapper = userManagerWrapper;
        _cookiesManager = cookiesManager;
        _messageService = messageService;
        _global = global;
        _emailValidationKeyProvider = emailValidationKeyProvider;
        _filesLinkUtility = filesLinkUtility;
        _settingsManager = settingsManager;
        _personalSettingsHelper = personalSettingsHelper;
        _baseCommonLinkUtility = baseCommonLinkUtility;
        _snapshot = snapshot;
        _setupInfo = setupInfo;
        _tokenHelper = tokenHelper;
        _documentServiceConnector = documentServiceConnector;
        _thirdPartyAppHandlerService = thirdPartyAppHandlerService;
        _serviceProvider = serviceProvider;
        Logger = option.CurrentValue;
        ClientFactory = clientFactory;
    }

    public async Task<bool> RequestAsync(HttpContext context)
    {
        if ((context.Request.Query[FilesLinkUtility.Action].FirstOrDefault() ?? "").Equals("stream", StringComparison.InvariantCultureIgnoreCase))
        {
            await StreamFileAsync(context);

            return true;
        }

        if (!string.IsNullOrEmpty(context.Request.Query["code"]))
        {
            RequestCode(context);

            return true;
        }

        return false;
    }

    public string GetRefreshUrl()
    {
        return AccessTokenUrl;
    }

    public File<string> GetFile(string fileId, out bool editable)
    {
        Logger.Debug("BoxApp: get file " + fileId);
        fileId = ThirdPartySelector.GetFileId(fileId);

        var token = _tokenHelper.GetToken(AppAttr);

        var boxFile = GetBoxFile(fileId, token);
        editable = true;

        if (boxFile == null)
        {
            return null;
        }

        var jsonFile = JObject.Parse(boxFile);

        var file = _serviceProvider.GetService<File<string>>();
        file.ID = ThirdPartySelector.BuildAppFileId(AppAttr, jsonFile.Value<string>("id"));
        file.Title = Global.ReplaceInvalidCharsAndTruncate(jsonFile.Value<string>("name"));
        file.CreateOn = _tenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("created_at"));
        file.ModifiedOn = _tenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("modified_at"));
        file.ContentLength = Convert.ToInt64(jsonFile.Value<string>("size"));
        file.ProviderKey = "Box";

        var modifiedBy = jsonFile.Value<JObject>("modified_by");
        if (modifiedBy != null)
        {
            file.ModifiedByString = modifiedBy.Value<string>("name");
        }

        var createdBy = jsonFile.Value<JObject>("created_by");
        if (createdBy != null)
        {
            file.CreateByString = createdBy.Value<string>("name");
        }


        var locked = jsonFile.Value<JObject>("lock");
        if (locked != null)
        {
            var lockedBy = locked.Value<JObject>("created_by");
            if (lockedBy != null)
            {
                var lockedUserId = lockedBy.Value<string>("id");
                Logger.Debug("BoxApp: locked by " + lockedUserId);

                editable = CurrentUser(lockedUserId);
            }
        }

        return file;
    }

    public string GetFileStreamUrl(File<string> file)
    {
        if (file == null)
        {
            return string.Empty;
        }

        var fileId = ThirdPartySelector.GetFileId(file.ID);

        Logger.Debug("BoxApp: get file stream url " + fileId);

        var uriBuilder = new UriBuilder(_baseCommonLinkUtility.GetFullAbsolutePath(_thirdPartyAppHandlerService.HandlerPath));
        if (uriBuilder.Uri.IsLoopback)
        {
            uriBuilder.Host = Dns.GetHostName();
        }

        var query = uriBuilder.Query;
        query += FilesLinkUtility.Action + "=stream&";
        query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId) + "&";
        query += CommonLinkUtility.ParamName_UserUserID + "=" + HttpUtility.UrlEncode(_authContext.CurrentAccount.ID.ToString()) + "&";
        query += FilesLinkUtility.AuthKey + "=" + _emailValidationKeyProvider.GetEmailKey(fileId + _authContext.CurrentAccount.ID) + "&";
        query += ThirdPartySelector.AppAttr + "=" + AppAttr;

        return uriBuilder.Uri + "?" + query;
    }

    public async Task SaveFileAsync(string fileId, string fileType, string downloadUrl, Stream stream)
    {
        Logger.Debug("BoxApp: save file stream " + fileId +
                            (stream == null
                                 ? " from - " + downloadUrl
                                 : " from stream"));
        fileId = ThirdPartySelector.GetFileId(fileId);

        var token = _tokenHelper.GetToken(AppAttr);

        var boxFile = GetBoxFile(fileId, token);
        if (boxFile == null)
        {
            Logger.Error("BoxApp: file is null");

            throw new Exception("File not found");
        }

        var jsonFile = JObject.Parse(boxFile);
        var title = Global.ReplaceInvalidCharsAndTruncate(jsonFile.Value<string>("name"));
        var currentType = FileUtility.GetFileExtension(title);
        if (!fileType.Equals(currentType))
        {
            try
            {
                if (stream != null)
                {
                    downloadUrl = await _pathProvider.GetTempUrlAsync(stream, fileType);
                    downloadUrl = _documentServiceConnector.ReplaceCommunityAdress(downloadUrl);
                }

                Logger.Debug("BoxApp: GetConvertedUri from " + fileType + " to " + currentType + " - " + downloadUrl);

                var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);

                var resultTuple = await _documentServiceConnector.GetConvertedUriAsync(downloadUrl, fileType, currentType, key, null, null, null, false);
                downloadUrl = resultTuple.ConvertedDocumentUri;

                stream = null;
            }
            catch (Exception e)
            {
                Logger.Error("BoxApp: Error convert", e);
            }
        }

        var httpClient = ClientFactory.CreateClient();

        var request = new HttpRequestMessage();
        request.RequestUri = new Uri(_boxUrlUpload.Replace("{fileId}", fileId));

        using (var tmpStream = new MemoryStream())
        {
            var boundary = DateTime.UtcNow.Ticks.ToString("x");

            var metadata = $"Content-Disposition: form-data; name=\"filename\"; filename=\"{title}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
            var metadataPart = $"--{boundary}\r\n{metadata}";
            var bytes = Encoding.UTF8.GetBytes(metadataPart);
            await tmpStream.WriteAsync(bytes, 0, bytes.Length);

            if (stream != null)
            {
                await stream.CopyToAsync(tmpStream);
            }
            else
            {
                var downloadRequest = new HttpRequestMessage();
                downloadRequest.RequestUri = new Uri(downloadUrl);
                using var response = await httpClient.SendAsync(request);
                using var downloadStream = new ResponseStream(response);
                await downloadStream.CopyToAsync(tmpStream);
            }

            var mediaPartEnd = $"\r\n--{boundary}--\r\n";
            bytes = Encoding.UTF8.GetBytes(mediaPartEnd);
            await tmpStream.WriteAsync(bytes, 0, bytes.Length);

            request.Method = HttpMethod.Post;
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data; boundary=" + boundary);
            Logger.Debug("BoxApp: save file totalSize - " + tmpStream.Length);

            tmpStream.Seek(0, SeekOrigin.Begin);
            request.Content = new StreamContent(tmpStream);
        }

        try
        {
            using var response = await httpClient.SendAsync(request);
            using var responseStream = await response.Content.ReadAsStreamAsync();
            string result = null;
            if (responseStream != null)
            {
                using var readStream = new StreamReader(responseStream);
                result = await readStream.ReadToEndAsync();
            }

            Logger.Debug("BoxApp: save file response - " + result);
        }
        catch (HttpRequestException e)
        {
            Logger.Error("BoxApp: Error save file", e);
            if (e.StatusCode == HttpStatusCode.Forbidden || e.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
            }

            throw;
        }
    }


    private void RequestCode(HttpContext context)
    {
        var token = GetToken(context.Request.Query["code"]);
        if (token == null)
        {
            Logger.Error("BoxApp: token is null");

            throw new SecurityException("Access token is null");
        }

        var boxUserId = context.Request.Query["userId"];

        if (_authContext.IsAuthenticated)
        {
            if (!CurrentUser(boxUserId))
            {
                Logger.Debug("BoxApp: logout for " + boxUserId);
                _cookiesManager.ClearCookies(CookiesType.AuthKey);
                _authContext.Logout();
            }
        }

        if (!_authContext.IsAuthenticated)
        {
            var userInfo = GetUserInfo(token, out var isNew);

            if (userInfo == null)
            {
                Logger.Error("BoxApp: UserInfo is null");

                throw new Exception("Profile is null");
            }

            var cookiesKey = _securityContext.AuthenticateMe(userInfo.Id);
            _cookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            _messageService.Send(MessageAction.LoginSuccessViaSocialApp);

            if (isNew)
            {
                var userHelpTourSettings = _settingsManager.LoadForCurrentUser<UserHelpTourSettings>();
                userHelpTourSettings.IsNewUser = true;
                _settingsManager.SaveForCurrentUser(userHelpTourSettings);

                _personalSettingsHelper.IsNewUser = true;
                _personalSettingsHelper.IsNotActivated = true;
            }

            if (!string.IsNullOrEmpty(boxUserId) && !CurrentUser(boxUserId))
            {
                AddLinker(boxUserId);
            }
        }

        _tokenHelper.SaveToken(token);

        var fileId = context.Request.Query["id"];

        context.Response.Redirect(_filesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
    }

    private async Task StreamFileAsync(HttpContext context)
    {
        try
        {
            var fileId = context.Request.Query[FilesLinkUtility.FileId];
            var auth = context.Request.Query[FilesLinkUtility.AuthKey];
            var userId = context.Request.Query[CommonLinkUtility.ParamName_UserUserID];

            Logger.Debug("BoxApp: get file stream " + fileId);

            var validateResult = _emailValidationKeyProvider.ValidateEmailKey(fileId + userId, auth, _global.StreamUrlExpire);
            if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
            {
                var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                Logger.Error(string.Format("BoxApp: validate error {0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url()), exc);

                throw exc;
            }

            Token token = null;

            if (Guid.TryParse(userId, out var userIdGuid))
            {
                token = _tokenHelper.GetToken(AppAttr, userIdGuid);
            }

            if (token == null)
            {
                Logger.Error("BoxApp: token is null");

                throw new SecurityException("Access token is null");
            }

            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(_boxUrlFile.Replace("{fileId}", fileId) + "/content");
            request.Method = HttpMethod.Get;
            request.Headers.Add("Authorization", "Bearer " + token);

            var httpClient = ClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request);
            using var stream = new ResponseStream(response);
            await stream.CopyToAsync(context.Response.Body);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(ex.Message);
            Logger.Error("BoxApp: Error request " + context.Request.Url(), ex);
        }

        try
        {
            await context.Response.Body.FlushAsync();
            //TODO
            //context.Response.Body.SuppressContent = true;
            //context.ApplicationInstance.CompleteRequest();
        }
        catch (HttpException ex)
        {
            Logger.Error("BoxApp StreamFile", ex);
        }
    }

    private bool CurrentUser(string boxUserId)
    {
        var linkedProfiles = _snapshot.Get("webstudio")
            .GetLinkedObjectsByHashId(HashHelper.MD5($"{ProviderConstants.Box}/{boxUserId}"));

        return linkedProfiles.Any(profileId => Guid.TryParse(profileId, out var tmp) && tmp == _authContext.CurrentAccount.ID);
    }

    private void AddLinker(string boxUserId)
    {
        Logger.Debug("BoxApp: AddLinker " + boxUserId);
        var linker = _snapshot.Get("webstudio");
        linker.AddLink(_authContext.CurrentAccount.ID.ToString(), boxUserId, ProviderConstants.Box);
    }

    private UserInfo GetUserInfo(Token token, out bool isNew)
    {
        isNew = false;
        if (token == null)
        {
            Logger.Error("BoxApp: token is null");

            throw new SecurityException("Access token is null");
        }

        var resultResponse = string.Empty;
        try
        {
            resultResponse = RequestHelper.PerformRequest(_boxUrlUserInfo,
                                                          headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token } });
            Logger.Debug("BoxApp: userinfo response - " + resultResponse);
        }
        catch (Exception ex)
        {
            Logger.Error("BoxApp: userinfo request", ex);
        }

        var boxUserInfo = JObject.Parse(resultResponse);
        if (boxUserInfo == null)
        {
            Logger.Error("Error in userinfo request");

            return null;
        }

        var email = boxUserInfo.Value<string>("login");
        var userInfo = _userManager.GetUserByEmail(email);
        if (Equals(userInfo, Constants.LostUser))
        {
            userInfo = new UserInfo
            {
                FirstName = boxUserInfo.Value<string>("name"),
                Email = email,
                MobilePhone = boxUserInfo.Value<string>("phone"),
            };

            var cultureName = boxUserInfo.Value<string>("language");
            if (string.IsNullOrEmpty(cultureName))
            {
                cultureName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            }

            var cultureInfo = _setupInfo.EnabledCultures.Find(c => string.Equals(c.TwoLetterISOLanguageName, cultureName, StringComparison.InvariantCultureIgnoreCase));
            if (cultureInfo != null)
            {
                userInfo.CultureName = cultureInfo.Name;
            }
            else
            {
                Logger.DebugFormat("From box app new personal user '{0}' without culture {1}", userInfo.Email, cultureName);
            }

            if (string.IsNullOrEmpty(userInfo.FirstName))
            {
                userInfo.FirstName = FilesCommonResource.UnknownFirstName;
            }
            if (string.IsNullOrEmpty(userInfo.LastName))
            {
                userInfo.LastName = FilesCommonResource.UnknownLastName;
            }

            try
            {
                _securityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);
                userInfo = _userManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword());
            }
            finally
            {
                _authContext.Logout();
            }

            isNew = true;

            Logger.Debug("BoxApp: new user " + userInfo.Id);
        }

        return userInfo;
    }

    private string GetBoxFile(string boxFileId, Token token)
    {
        if (token == null)
        {
            Logger.Error("BoxApp: token is null");

            throw new SecurityException("Access token is null");
        }

        try
        {
            var resultResponse = RequestHelper.PerformRequest(_boxUrlFile.Replace("{fileId}", boxFileId),
                                                              headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token } });
            Logger.Debug("BoxApp: file response - " + resultResponse);

            return resultResponse;
        }
        catch (Exception ex)
        {
            Logger.Error("BoxApp: file request", ex);
        }
        return null;
    }

    private Token GetToken(string code)
    {
        try
        {
            Logger.Debug("BoxApp: GetAccessToken by code " + code);
            var token = OAuth20TokenHelper.GetAccessToken<BoxApp>(ConsumerFactory, code);

            return new Token(token, AppAttr);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }

        return null;
    }
}
