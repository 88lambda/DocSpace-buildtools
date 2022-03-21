// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.AuditTrail.Mappers;

internal static class LoginActionsMapper
{
    public static Dictionary<MessageAction, MessageMaps> GetMaps() =>
        new Dictionary<MessageAction, MessageMaps>
        {
                    { MessageAction.LoginSuccess, new MessageMaps { ActionTextResourceName = "LoginSuccess"} },
                    { MessageAction.LoginSuccessViaSocialAccount, new MessageMaps { ActionTextResourceName = "LoginSuccessSocialAccount"} },
                    { MessageAction.LoginSuccessViaSocialApp, new MessageMaps { ActionTextResourceName = "LoginSuccessSocialApp"} },
                    { MessageAction.LoginSuccessViaSms, new MessageMaps { ActionTextResourceName = "LoginSuccessViaSms"} },
                    { MessageAction.LoginSuccessViaApi, new MessageMaps { ActionTextResourceName = "LoginSuccessViaApi"} },
                    { MessageAction.LoginSuccessViaApiSms, new MessageMaps { ActionTextResourceName = "LoginSuccessViaApiSms"} },
                    { MessageAction.LoginSuccessViaApiTfa, new MessageMaps { ActionTextResourceName = "LoginSuccessViaApiTfa"} },
                    { MessageAction.LoginSuccessViaApiSocialAccount, new MessageMaps { ActionTextResourceName = "LoginSuccessViaSocialAccount"} },
                    { MessageAction.LoginSuccessViaSSO, new MessageMaps { ActionTextResourceName = "LoginSuccessViaSSO"} },
                    { MessageAction.LoginSuccesViaTfaApp, new MessageMaps { ActionTextResourceName = "LoginSuccesViaTfaApp"} },
                    { MessageAction.LoginFailInvalidCombination, new MessageMaps { ActionTextResourceName = "LoginFailInvalidCombination" } },
                    { MessageAction.LoginFailSocialAccountNotFound, new MessageMaps { ActionTextResourceName = "LoginFailSocialAccountNotFound" } },
                    { MessageAction.LoginFailDisabledProfile, new MessageMaps { ActionTextResourceName = "LoginFailDisabledProfile" } },
                    { MessageAction.LoginFail, new MessageMaps { ActionTextResourceName = "LoginFail" } },
                    { MessageAction.LoginFailViaSms, new MessageMaps { ActionTextResourceName = "LoginFailViaSms" } },
                    { MessageAction.LoginFailViaApi, new MessageMaps { ActionTextResourceName = "LoginFailViaApi" } },
                    { MessageAction.LoginFailViaApiSms, new MessageMaps { ActionTextResourceName = "LoginFailViaApiSms" } },
                    { MessageAction.LoginFailViaApiTfa, new MessageMaps { ActionTextResourceName = "LoginFailViaApiTfa" } },
                    { MessageAction.LoginFailViaApiSocialAccount, new MessageMaps { ActionTextResourceName = "LoginFailViaApiSocialAccount" } },
                    { MessageAction.LoginFailViaTfaApp, new MessageMaps { ActionTextResourceName = "LoginFailViaTfaApp" } },
                    { MessageAction.LoginFailIpSecurity, new MessageMaps { ActionTextResourceName = "LoginFailIpSecurity" } },
                    { MessageAction.LoginFailViaSSO, new MessageMaps { ActionTextResourceName = "LoginFailViaSSO"}},
                    { MessageAction.LoginFailBruteForce, new MessageMaps { ActionTextResourceName = "LoginFailBruteForce" } },
                    { MessageAction.LoginFailRecaptcha, new MessageMaps { ActionTextResourceName = "LoginFailRecaptcha" } },
                    { MessageAction.Logout, new MessageMaps { ActionTextResourceName = "Logout" } },
                    { MessageAction.SessionStarted, new MessageMaps { ActionTextResourceName = "SessionStarted" } },
                    { MessageAction.SessionCompleted, new MessageMaps { ActionTextResourceName = "SessionCompleted" } }
        };
}