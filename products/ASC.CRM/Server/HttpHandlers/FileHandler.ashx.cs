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


using ASC.Common;
using ASC.Common.Logging;
using ASC.CRM.Resources;
using ASC.Web.CRM.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASC.Web.CRM.HttpHandlers
{
    [Transient]
    public class FileHandler
    {
        public FileHandler(RequestDelegate next,
                           Global global,
                           ContactPhotoManager contactPhotoManager,
                           IOptionsMonitor<ILog> logger)
        {
            _next = next;
            Global = global;
            ContactPhotoManager = contactPhotoManager;
            Logger = logger.Get("ASC");
        }

        public ILog Logger { get; }
        public ContactPhotoManager ContactPhotoManager { get; }
        public Global Global { get; }

        private readonly RequestDelegate _next;

        public async Task InvokeAsync(HttpContext context)
        {
            var action = context.Request.Query["action"];

            switch (action)
            {
                case "contactphotoulr":
                    {
                        var contactId = Convert.ToInt32(context.Request.Query["cid"]);
                        var isCompany = Convert.ToBoolean(context.Request.Query["isc"]);
                        var photoSize = Convert.ToInt32(context.Request.Query["ps"]);

                        string photoUrl = string.Empty;

                        switch (photoSize)
                        {
                            case 1:
                                photoUrl = ContactPhotoManager.GetSmallSizePhoto(contactId, isCompany);
                                break;
                            case 2:
                                photoUrl = ContactPhotoManager.GetMediumSizePhoto(contactId, isCompany);
                                break;
                            case 3:
                                photoUrl = ContactPhotoManager.GetBigSizePhoto(contactId, isCompany);
                                break;
                            default:
                                throw new Exception(CRMErrorsResource.ContactPhotoSizeUnknown);
                        }

                        context.Response.Clear();

                        await context.Response.WriteAsync(photoUrl);                       
                    }
                    break;
                case "mailmessage":
                    {
                        var messageId = Convert.ToInt32(context.Request.Query["message_id"]);

                        var filePath = String.Format("folder_{0}/message_{1}.html", (messageId / 1000 + 1) * 1000, messageId);

                        string messageContent = string.Empty;

                        using (var streamReader = new StreamReader(Global.GetStore().GetReadStream("mail_messages", filePath)))
                        {
                            messageContent = streamReader.ReadToEnd();
                        }

                        context.Response.Clear();

                        await context.Response.WriteAsync(messageContent);
                    }
                    break;
                default:
                    throw new ArgumentException(String.Format("action='{0}' is not defined", action));
            }
        }
    }
} 