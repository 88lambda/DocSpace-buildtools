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

namespace ASC.Web.Core.Utility
{
    public static class UrlSwitcher
    {
        public static string SelectCurrentUriScheme(this HttpContext httpContext, string uri)
        {
            return httpContext != null ? SelectUriScheme(uri, httpContext.Request.GetUrlRewriter().Scheme) : uri;
        }

        public static string SelectUriScheme(string uri, string scheme)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute) ? SelectUriScheme(new Uri(uri, UriKind.Absolute), scheme).ToString() : uri;
        }

        public static Uri SelectCurrentUriScheme(this HttpContext httpContext, Uri uri)
        {
            if (httpContext != null)
            {
                return SelectUriScheme(uri, httpContext.Request.GetUrlRewriter().Scheme);
            }
            return uri;
        }

        public static Uri SelectUriScheme(Uri uri, string scheme)
        {
            if (!string.IsNullOrEmpty(scheme) && !scheme.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                //Switch
                var builder = new UriBuilder(uri) { Scheme = scheme.ToLowerInvariant(), Port = scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80 };//Set proper port!
                return builder.Uri;
            }
            return uri;
        }

        public static Uri ToCurrentScheme(this Uri uri, HttpContext httpContext)
        {
            return SelectCurrentUriScheme(httpContext, uri);
        }

        public static Uri ToScheme(this Uri uri, string scheme)
        {
            return SelectUriScheme(uri, scheme);
        }
    }
}