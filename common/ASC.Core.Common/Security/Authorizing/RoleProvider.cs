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

using Constants = ASC.Common.Security.Authorizing.Constants;

namespace ASC.Core.Security.Authorizing;

[Scope]
class RoleProvider : IRoleProvider
{
    //circ dep
    private readonly IServiceProvider _serviceProvider;
    public RoleProvider(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public List<IRole> GetRoles(ISubject account)
    {
        var roles = new List<IRole>();
        if (account is not ISystemAccount)
        {
            if (account is IRole)
            {
                roles = GetParentRoles(account.ID).ToList();
            }
            else if (account is IUserAccount)
            {
                roles = _serviceProvider.GetService<UserManager>()
                                   .GetUserGroups(account.ID, IncludeType.Distinct | IncludeType.InParent)
                                   .Select(g => (IRole)g)
                                   .ToList();
            }
        }
        
        if (roles.Any(r => r.ID == Constants.Collaborator.ID || r.ID == Constants.User.ID))
        {
            roles = roles.Where(r => r.ID != Constants.RoomAdmin.ID).ToList();
        }

        return roles;
    }

    public bool IsSubjectInRole(ISubject account, IRole role)
    {
        return _serviceProvider.GetService<UserManager>().IsUserInGroup(account.ID, role.ID);
    }

    private List<IRole> GetParentRoles(Guid roleID)
    {
        var roles = new List<IRole>();
        var gi = _serviceProvider.GetService<UserManager>().GetGroupInfo(roleID);
        if (gi != null)
        {
            var parent = gi.Parent;
            while (parent != null)
            {
                roles.Add(parent);
                parent = parent.Parent;
            }
        }

        return roles;
    }
}
