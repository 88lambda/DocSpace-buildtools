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


using ASC.Api.Collections;
using ASC.CRM.ApiModels;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;

using ASC.ElasticSearch;
using ASC.MessagingSystem;
using ASC.Web.Api.Routing;
using ASC.Web.Files.Services.WCFService;

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.CRM;
using ASC.CRM.Core.Dao;
using ASC.Api.Core;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Core.Users;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace ASC.CRM.Api
{
    public class CasesController : BaseApiController
    {
        private IMapper _mapper;

        public CasesController(CRMSecurity cRMSecurity,
                     DaoFactory daoFactory,
                     ApiContext apiContext,
                     MessageTarget messageTarget,
                     MessageService messageService,
                     NotifyClient notifyClient,
                     ContactDtoHelper contactBaseDtoHelper,
                     SecurityContext securityContext,
                     DisplayUserSettingsHelper displayUserSettingsHelper,
                     UserManager userManager,
                     IMapper mapper)
            : base(daoFactory, cRMSecurity)
        {
            ApiContext = apiContext;
            MessageTarget = messageTarget;
            MessageService = messageService;
            NotifyClient = notifyClient;
            ContactDtoHelper = contactBaseDtoHelper;
            SecurityContext = securityContext;
            DisplayUserSettingsHelper = displayUserSettingsHelper;
            UserManager = userManager;
            _mapper = mapper;
        }

        public DisplayUserSettingsHelper DisplayUserSettingsHelper { get; }
        public SecurityContext SecurityContext { get; }
        public ContactDtoHelper ContactDtoHelper { get; }
        public NotifyClient NotifyClient { get; }
        private ApiContext ApiContext { get; }
        public MessageService MessageService { get; }
        public MessageTarget MessageTarget { get; }
        public UserManager UserManager { get; }

        /// <summary>
        ///   Close the case with the ID specified in the request
        /// </summary>
        /// <short>Close case</short> 
        /// <category>Cases</category>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case
        /// </returns>
        [Update(@"case/{caseid:int}/close")]
        public CasesDto CloseCases([FromQuery] int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().CloseCases(caseid);
            if (cases == null) throw new ItemNotFoundException();

            MessageService.Send(MessageAction.CaseClosed, MessageTarget.Create(cases.ID), cases.Title);
            

            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///   Resume the case with the ID specified in the request
        /// </summary>
        /// <short>Resume case</short> 
        /// <category>Cases</category>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case
        /// </returns>
        [Update(@"case/{caseid:int}/reopen")]
        public CasesDto ReOpenCases([FromQuery] int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().ReOpenCases(caseid);
            if (cases == null) throw new ItemNotFoundException();

            MessageService.Send(MessageAction.CaseOpened, MessageTarget.Create(cases.ID), cases.Title);
            
            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///    Creates the case with the parameters specified in the request
        /// </summary>
        /// <short>Create case</short> 
        /// <param name="title" optional="false">Case title</param>
        /// <param name="members" optional="true">Participants</param>
        /// <param name="customFieldList" optional="true">User field list</param>
        /// <param name="isPrivate" optional="true">Case privacy: private or not</param>
        /// <param name="accessList" optional="true">List of users with access to the case</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the case</param>
        /// <returns>Case</returns>
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    title: "Exhibition organization",
        ///    isPrivate: false,
        ///    customFieldList: [{1: "value for text custom field with id = 1"}]
        /// }
        /// 
        /// ]]>
        /// </example>
        [Create(@"case")]
        public CasesDto CreateCases([FromForm] CreateOrUpdateCasesRequestDto inDto)
        {

            var title = inDto.Title;
            var customFieldList = inDto.CustomFieldList;
            var isPrivate = inDto.isPrivate;
            var isNotify = inDto.isNotify;
            var members = inDto.Members;
            var accessList = inDto.accessList;

            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            var casesID = DaoFactory.GetCasesDao().CreateCases(title);

            var cases = new Cases
            {
                ID = casesID,
                Title = title,
                CreateBy = SecurityContext.CurrentAccount.ID,
                CreateOn = DateTime.UtcNow
            };

            SetAccessToCases(cases, isPrivate, accessList, isNotify, false);

            var membersList = members != null ? members.ToList() : new List<int>();
            if (membersList.Any())
            {
                var contacts = DaoFactory.GetContactDao().GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();
                DaoFactory.GetCasesDao().SetMembers(cases.ID, membersList.ToArray());
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Case).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.GetCustomFieldDao().SetFieldValue(EntityType.Case, cases.ID, field.Key, field.Value);
                }
            }

            var casesToResult = DaoFactory.GetCasesDao().GetByID(casesID);

            return _mapper.Map<CasesDto>(casesToResult);
        }

        /// <summary>
        ///   Updates the selected case with the parameters specified in the request
        /// </summary>
        /// <short>Update case</short> 
        /// <param name="caseid" optional="false">Case ID</param>
        /// <param name="title" optional="false">Case title</param>
        /// <param name="members" optional="true">Participants</param>
        /// <param name="customFieldList" optional="true">User field list</param>
        /// <param name="isPrivate" optional="true">Case privacy: private or not</param>
        /// <param name="accessList" optional="true">List of users with access to the case</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the case</param>
        /// <category>Cases</category>
        /// <returns>Case</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    caseid: 0,
        ///    title: "Exhibition organization",
        ///    isPrivate: false,
        ///    customFieldList: [{1: "value for text custom field with id = 1"}]
        /// }
        /// 
        /// ]]>
        /// </example>
        [Update(@"case/{caseid:int}")]
        public CasesDto UpdateCases([FromQuery] int caseid, [FromForm] CreateOrUpdateCasesRequestDto inDto)
        {
            var title = inDto.Title;
            var isPrivate = inDto.isPrivate;
            var isNotify = inDto.isNotify;
            var accessList = inDto.accessList;
            var members = inDto.Members;
            var customFieldList = inDto.CustomFieldList;

            if ((caseid <= 0) || (string.IsNullOrEmpty(title))) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().GetByID(caseid);
            if (cases == null) throw new ItemNotFoundException();

            cases.Title = title;

            DaoFactory.GetCasesDao().UpdateCases(cases);

            if (CRMSecurity.IsAdmin || cases.CreateBy == SecurityContext.CurrentAccount.ID)
            {
                SetAccessToCases(cases, isPrivate, accessList, isNotify, false);
            }

            var membersList = members != null ? members.ToList() : new List<int>();
            if (membersList.Any())
            {
                var contacts = DaoFactory.GetContactDao().GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();
                DaoFactory.GetCasesDao().SetMembers(cases.ID, membersList.ToArray());
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Case).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.GetCustomFieldDao().SetFieldValue(EntityType.Case, cases.ID, field.Key, field.Value);
                }
            }

            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///   Sets access rights for the selected case with the parameters specified in the request
        /// </summary>
        /// <param name="caseid" optional="false">Case ID</param>
        /// <param name="isPrivate" optional="false">Case privacy: private or not</param>
        /// <param name="accessList" optional="false">List of users with access to the case</param>
        /// <short>Set rights to case</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case 
        /// </returns>
        [Update(@"case/{caseid:int}/access")]
        public CasesDto SetAccessToCases([FromQuery] int caseid, bool isPrivate, IEnumerable<Guid> accessList)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().GetByID(caseid);
            if (cases == null) throw new ItemNotFoundException();

            if (!(CRMSecurity.IsAdmin || cases.CreateBy == SecurityContext.CurrentAccount.ID)) throw CRMSecurity.CreateSecurityException();

            return SetAccessToCases(cases, isPrivate, accessList, false, true);
        }

        private CasesDto SetAccessToCases(Cases cases, bool isPrivate, IEnumerable<Guid> accessList, bool isNotify, bool isMessageServicSende)
        {
            var accessListLocal = accessList != null ? accessList.Distinct().ToList() : new List<Guid>();
            if (isPrivate && accessListLocal.Any())
            {
                if (isNotify)
                {
                    accessListLocal = accessListLocal.Where(u => u != SecurityContext.CurrentAccount.ID).ToList();
                    NotifyClient.SendAboutSetAccess(EntityType.Case, cases.ID, DaoFactory, accessListLocal.ToArray());
                }

                if (!accessListLocal.Contains(SecurityContext.CurrentAccount.ID))
                {
                    accessListLocal.Add(SecurityContext.CurrentAccount.ID);
                }

                CRMSecurity.SetAccessTo(cases, accessListLocal);
                if (isMessageServicSende)
                {
                    var users = GetUsersByIdList(accessListLocal);
                    MessageService.Send(MessageAction.CaseRestrictedAccess, MessageTarget.Create(cases.ID), cases.Title, users.Select(x => x.DisplayUserName(false, DisplayUserSettingsHelper)));
                }
            }
            else
            {
                CRMSecurity.MakePublic(cases);
                if (isMessageServicSende)
                {
                    MessageService.Send(MessageAction.CaseOpenedAccess, MessageTarget.Create(cases.ID), cases.Title);
                }
            }

            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of cases with the IDs specified in the request
        /// </summary>
        /// <param name="casesid">Case ID list</param>
        /// <param name="isPrivate">Case privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set case access rights</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case/access")]
        public IEnumerable<CasesDto> SetAccessToBatchCases([FromForm] SetAccessToBatchCasesRequestDto inDto)
        {
            var casesid = inDto.CasesId;
            var isPrivate = inDto.isPrivate;
            var accessList = inDto.AccessList;

            var result = new List<Cases>();

            var cases = DaoFactory.GetCasesDao().GetCases(casesid);

            if (!cases.Any()) return new List<CasesDto>();

            foreach (var c in cases)
            {
                if (c == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || c.CreateBy == SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToCases(c, isPrivate, accessList, false, true);
                result.Add(c);
            }

            return ToListCasesDtos(result);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param name="isPrivate">Case privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set case access rights</short> 
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case/filter/access")]
        public IEnumerable<CasesDto> SetAccessToBatchCases([FromForm] SetAccessToBatchCasesByFilterInDto inDto)
        {
            int contactid = inDto.Contactid;
            bool? isClosed = inDto.isClosed;
            IEnumerable<string> tags = inDto.Tags;
            bool isPrivate = inDto.isPrivate;
            IEnumerable<Guid> accessList = inDto.AccessList;

            var result = new List<Cases>();

            var caseses = DaoFactory.GetCasesDao().GetCases(ApiContext.FilterValue, contactid, isClosed, tags, 0, 0, null);

            if (!caseses.Any()) return new List<CasesDto>();

            foreach (var casese in caseses)
            {
                if (casese == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || casese.CreateBy == SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToCases(casese, isPrivate, accessList, false, true);
                result.Add(casese);
            }

            return ToListCasesDtos(result);
        }

        /// <summary>
        ///    Returns the detailed information about the case with the ID specified in the request
        /// </summary>
        /// <short>Get case by ID</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"case/{caseid:int}")]
        public CasesDto GetCaseByID([FromQuery] int caseid)
        {
            if (caseid <= 0) throw new ItemNotFoundException();

            var cases = DaoFactory.GetCasesDao().GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///     Returns the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <short>Get case list</short> 
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <category>Cases</category>
        /// <returns>
        ///    Case list
        /// </returns>
        [Read(@"case/filter")]
        public IEnumerable<CasesDto> GetCases(int contactid, bool? isClosed, IEnumerable<string> tags)
        {
            IEnumerable<CasesDto> result;
            SortedByType sortBy;
            OrderBy casesOrderBy;

            var searchString = ApiContext.FilterValue;

            if (ASC.CRM.Classes.EnumExtension.TryParse(ApiContext.SortBy, true, out sortBy))
            {
                casesOrderBy = new OrderBy(sortBy, !ApiContext.SortDescending);
            }
            else if (string.IsNullOrEmpty(ApiContext.SortBy))
            {
                casesOrderBy = new OrderBy(SortedByType.Title, true);
            }
            else
            {
                casesOrderBy = null;
            }

            var fromIndex = (int)ApiContext.StartIndex;
            var count = (int)ApiContext.Count;

            if (casesOrderBy != null)
            {
                result = ToListCasesDtos(
                    DaoFactory
                        .GetCasesDao()
                        .GetCases(
                            searchString,
                            contactid,
                            isClosed,
                            tags,
                            fromIndex,
                            count,
                            casesOrderBy)).ToList();

                ApiContext.SetDataPaginated();
                ApiContext.SetDataFiltered();
                ApiContext.SetDataSorted();
            }
            else
            {
                result = ToListCasesDtos(
                    DaoFactory
                        .GetCasesDao()
                        .GetCases(
                            searchString, contactid, isClosed,
                            tags,
                            0,
                            0,
                            null)).ToList();
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.GetCasesDao().GetCasesCount(searchString, contactid, isClosed, tags);
            }

            ApiContext.SetTotalCount(totalCount);
            return result;
        }

        /// <summary>
        ///   Deletes the case with the ID specified in the request
        /// </summary>
        /// <short>Delete case</short> 
        /// <param name="caseid">Case ID</param>
        /// <category>Cases</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Case
        /// </returns>
        [Delete(@"case/{caseid:int}")]
        public CasesDto DeleteCase([FromQuery] int caseid)
        {
            if (caseid <= 0) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().DeleteCases(caseid);

            if (cases == null) throw new ItemNotFoundException();

            MessageService.Send(MessageAction.CaseDeleted, MessageTarget.Create(cases.ID), cases.Title);

            return _mapper.Map<CasesDto>(cases);
        }

        /// <summary>
        ///   Deletes the group of cases with the IDs specified in the request
        /// </summary>
        /// <param name="casesids">Case ID list</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete case group</short> 
        /// <category>Cases</category>
        /// <returns>
        ///   Case list
        /// </returns>
        [Update(@"case")]
        public IEnumerable<CasesDto> DeleteBatchCases([FromForm] IEnumerable<int> casesids)
        {
            if (casesids == null) throw new ArgumentException();

            casesids = casesids.Distinct();
            var caseses = DaoFactory.GetCasesDao().DeleteBatchCases(casesids.ToArray());

            if (caseses == null || !caseses.Any()) return new List<CasesDto>();

            MessageService.Send(MessageAction.CasesDeleted, MessageTarget.Create(casesids), caseses.Select(c => c.Title));

            return ToListCasesDtos(caseses);
        }

        /// <summary>
        ///   Deletes the list of all cases matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="isClosed">Case status</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete case group</short> 
        /// <category>Cases</category>
        /// <returns>
        ///   Case list
        /// </returns>
        [Delete(@"case/filter")]
        public IEnumerable<CasesDto> DeleteBatchCases(int contactid, bool? isClosed, IEnumerable<string> tags)
        {
            var caseses = DaoFactory.GetCasesDao().GetCases(ApiContext.FilterValue, contactid, isClosed, tags, 0, 0, null);
            if (!caseses.Any()) return new List<CasesDto>();

            caseses = DaoFactory.GetCasesDao().DeleteBatchCases(caseses);

            MessageService.Send(MessageAction.CasesDeleted, MessageTarget.Create(caseses.Select(c => c.ID)), caseses.Select(c => c.Title));

            return ToListCasesDtos(caseses);
        }

        /// <summary>
        ///    Returns the list of all contacts associated with the case with the ID specified in the request
        /// </summary>
        /// <short>Get all case contacts</short> 
        /// <param name="caseid">Case ID</param>
        /// <category>Cases</category>
        /// <returns>Contact list</returns>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"case/{caseid:int}/contact")]
        public IEnumerable<ContactDto> GetCasesMembers(int caseid)
        {
            var contactIDs = DaoFactory.GetCasesDao().GetMembers(caseid);
            return contactIDs == null
                       ? new ItemList<ContactDto>()
                       : ContactDtoHelper.ToListContactDto(DaoFactory.GetContactDao().GetContacts(contactIDs));
        }

        /// <summary>
        ///   Adds the selected contact to the case with the ID specified in the request
        /// </summary>
        /// <short>Add case contact</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Create(@"case/{caseid:int}/contact")]
        public ContactDto AddMemberToCases(int caseid, int contactid)
        {
            if ((caseid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            DaoFactory.GetCasesDao().AddMember(caseid, contactid);

            var messageAction = contact is Company ? MessageAction.CaseLinkedCompany : MessageAction.CaseLinkedPerson;
            MessageService.Send(messageAction, MessageTarget.Create(cases.ID), cases.Title, contact.GetTitle());

            return ContactDtoHelper.ToContactDto(contact);
        }

        /// <summary>
        ///   Delete the selected contact from the case with the ID specified in the request
        /// </summary>
        /// <short>Delete case contact</short> 
        /// <category>Cases</category>
        /// <param name="caseid">Case ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Delete(@"case/{caseid:int}/contact/{contactid:int}")]
        public ContactDto DeleteMemberFromCases(int caseid, int contactid)
        {
            if ((caseid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var cases = DaoFactory.GetCasesDao().GetByID(caseid);
            if (cases == null || !CRMSecurity.CanAccessTo(cases)) throw new ItemNotFoundException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var result = ContactDtoHelper.ToContactDto(contact);

            DaoFactory.GetCasesDao().RemoveMember(caseid, contactid);

            var messageAction = contact is Company ? MessageAction.CaseUnlinkedCompany : MessageAction.CaseUnlinkedPerson;
            MessageService.Send(messageAction, MessageTarget.Create(cases.ID), cases.Title, contact.GetTitle());

            return result;
        }

        /// <summary>
        ///    Returns the list of 30 cases in the CRM module with prefix
        /// </summary>
        /// <param optional="true" name="prefix"></param>
        /// <param optional="true" name="contactID"></param>
        /// <category>Cases</category>
        /// <returns>
        ///    Cases list
        /// </returns>
        /// <visible>false</visible>
        [Read(@"case/byprefix")]
        public IEnumerable<CasesDto> GetCasesByPrefix(string prefix, int contactID)
        {
            var result = new List<CasesDto>();

            if (contactID > 0)
            {
                var findedCases = DaoFactory.GetCasesDao().GetCases(string.Empty, contactID, null, null, 0, 0, null);

                foreach (var item in findedCases)
                {
                    if (item.Title.IndexOf(prefix, StringComparison.Ordinal) != -1)
                    {
                        result.Add(_mapper.Map<CasesDto>(item));
                    }
                }

                ApiContext.SetTotalCount(findedCases.Count);
            }
            else
            {
                const int maxItemCount = 30;
                var findedCases = DaoFactory.GetCasesDao().GetCasesByPrefix(prefix, 0, maxItemCount);

                foreach (var item in findedCases)
                {
                    result.Add(_mapper.Map<CasesDto>(item));
                }
            }

            return result;
        }

        private IEnumerable<CasesDto> ToListCasesDtos(ICollection<Cases> items)
        {
            if (items == null || items.Count == 0) return new List<CasesDto>();

            var result = new List<CasesDto>();

            var contactIDs = new List<int>();
            var casesIDs = items.Select(item => item.ID).ToArray();

            var customFields = DaoFactory.GetCustomFieldDao()
                                         .GetEnityFields(EntityType.Case, casesIDs)
                                         .GroupBy(item => item.EntityID)
                                         .ToDictionary(item => item.Key, item => item.Select(x => new CustomFieldBaseDto(x)));

            var casesMembers = DaoFactory.GetCasesDao().GetMembers(casesIDs);

            foreach (var value in casesMembers.Values)
            {
                contactIDs.AddRange(value);
            }

            var contacts = DaoFactory
                .GetContactDao()
                .GetContacts(contactIDs.Distinct().ToArray())
                .ToDictionary(item => item.ID, x => ContactDtoHelper.GetContactBaseDto(x));

            foreach (var cases in items)
            {
                var casesDto = _mapper.Map<CasesDto>(cases);

                casesDto.CustomFields = customFields.ContainsKey(cases.ID)
                                   ? customFields[cases.ID]
                                   : new List<CustomFieldBaseDto>();

                casesDto.Members = casesMembers.ContainsKey(cases.ID)
                                      ? casesMembers[cases.ID].Where(contacts.ContainsKey).Select(item => contacts[item])
                                      : new List<ContactBaseDto>();

                result.Add(casesDto);
            }

            return result;
        }

        private IEnumerable<UserInfo> GetUsersByIdList(IEnumerable<Guid> ids)
        {
            return UserManager.GetUsers().Where(x => ids.Contains(x.ID));
        }
    }
}