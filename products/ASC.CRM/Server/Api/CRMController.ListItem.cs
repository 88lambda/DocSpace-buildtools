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


using ASC.CRM.ApiModels;
using ASC.Common.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;
using ASC.MessagingSystem;
using ASC.Web.Api.Routing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace ASC.Api.CRM
{
    public partial class CRMController
    {
        /// <summary>
        ///   Creates an opportunity stage with the parameters (title, description, success probability, etc.) specified in the request
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="description">Description</param>
        /// <param name="color">Color</param>
        /// <param name="successProbability">Success probability</param>
        /// <param name="stageType" remark="Allowed values: 0 (Open), 1 (ClosedAndWon),2 (ClosedAndLost)">Stage type</param>
        /// <short>Create opportunity stage</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Opportunity stage
        /// </returns>
        [Create(@"opportunity/stage")]
        public DealMilestoneDto CreateDealMilestone(
            string title,
            string description,
            string color,
            int successProbability,
            DealMilestoneStatus stageType)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            if (successProbability < 0) successProbability = 0;

            var dealMilestone = new DealMilestone
            {
                Title = title,
                Color = color,
                Description = description,
                Probability = successProbability,
                Status = stageType
            };

            dealMilestone.ID = DaoFactory.GetDealMilestoneDao().Create(dealMilestone);
            MessageService.Send(MessageAction.OpportunityStageCreated, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneDto(dealMilestone);
        }

        /// <summary>
        ///    Updates the selected opportunity stage with the parameters (title, description, success probability, etc.) specified in the request
        /// </summary>
        /// <param name="id">Opportunity stage ID</param>
        /// <param name="title">Title</param>
        /// <param name="description">Description</param>
        /// <param name="color">Color</param>
        /// <param name="successProbability">Success probability</param>
        /// <param name="stageType" remark="Allowed values: Open, ClosedAndWon, ClosedAndLost">Stage type</param>
        /// <short>Update opportunity stage</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Opportunity stage
        /// </returns>
        [Update(@"opportunity/stage/{id:int}")]
        public DealMilestoneDto UpdateDealMilestone(
            int id,
            string title,
            string description,
            string color,
            int successProbability,
            DealMilestoneStatus stageType)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            if (successProbability < 0) successProbability = 0;

            var curDealMilestoneExist = DaoFactory.GetDealMilestoneDao().IsExist(id);
            if (!curDealMilestoneExist) throw new ItemNotFoundException();

            var dealMilestone = new DealMilestone
            {
                Title = title,
                Color = color,
                Description = description,
                Probability = successProbability,
                Status = stageType,
                ID = id
            };

            DaoFactory.GetDealMilestoneDao().Edit(dealMilestone);
            MessageService.Send(MessageAction.OpportunityStageUpdated, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneDto(dealMilestone);
        }

        /// <summary>
        ///    Updates the selected opportunity stage with the color specified in the request
        /// </summary>
        /// <param name="id">Opportunity stage ID</param>
        /// <param name="color">Color</param>
        /// <short>Update opportunity stage color</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Opportunity stage
        /// </returns>
        [Update(@"opportunity/stage/{id:int}/color")]
        public DealMilestoneDto UpdateDealMilestoneColor(int id, string color)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.GetDealMilestoneDao().GetByID(id);
            if (dealMilestone == null) throw new ItemNotFoundException();

            dealMilestone.Color = color;

            DaoFactory.GetDealMilestoneDao().ChangeColor(id, color);
            MessageService.Send(MessageAction.OpportunityStageUpdatedColor, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return ToDealMilestoneDto(dealMilestone);
        }

        /// <summary>
        ///    Updates the available opportunity stages order with the list specified in the request
        /// </summary>
        /// <short>
        ///    Update opportunity stages order
        /// </short>
        /// <param name="ids">Opportunity stage ID list</param>
        /// <category>Opportunities</category>
        /// <returns>
        ///    Opportunity stages
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"opportunity/stage/reorder")]
        public IEnumerable<DealMilestoneDto> UpdateDealMilestonesOrder(IEnumerable<int> ids)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (ids == null) throw new ArgumentException();

            var idsList = ids.ToList();

            var result = idsList.Select(id => DaoFactory.GetDealMilestoneDao().GetByID(id)).ToList();

            DaoFactory.GetDealMilestoneDao().Reorder(idsList.ToArray());
            MessageService.Send(MessageAction.OpportunityStagesUpdatedOrder, MessageTarget.Create(idsList), result.Select(x => x.Title));

            return result.Select(ToDealMilestoneDto);
        }

        /// <summary>
        ///   Deletes the opportunity stage with the ID specified in the request
        /// </summary>
        /// <short>Delete opportunity stage</short> 
        /// <category>Opportunities</category>
        /// <param name="id">Opportunity stage ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Opportunity stage
        /// </returns>
        [Delete(@"opportunity/stage/{id:int}")]
        public DealMilestoneDto DeleteDealMilestone(int id)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.GetDealMilestoneDao().GetByID(id);
            if (dealMilestone == null) throw new ItemNotFoundException();

            var result = ToDealMilestoneDto(dealMilestone);

            DaoFactory.GetDealMilestoneDao().Delete(id);
            MessageService.Send(MessageAction.OpportunityStageDeleted, MessageTarget.Create(dealMilestone.ID), dealMilestone.Title);

            return result;
        }

        /// <summary>
        ///   Creates a new history category with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="sortOrder">Order</param>
        ///<param name="imageName">Image name</param>
        ///<short>Create history category</short> 
        /// <category>History</category>
        ///<returns>History category</returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"history/category")]
        public HistoryCategoryDto CreateHistoryCategory(string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (string.IsNullOrEmpty(title)) throw new ArgumentException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName
            };

            listItem.ID = DaoFactory.GetListItemDao().CreateItem(ListType.HistoryCategory, listItem);
            MessageService.Send(MessageAction.HistoryEventCategoryCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToHistoryCategoryDto(listItem);
        }

        /// <summary>
        ///   Updates the selected history category with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="id">History category ID</param>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="sortOrder">Order</param>
        ///<param name="imageName">Image name</param>
        ///<short>Update history category</short> 
        ///<category>History</category>
        ///<returns>History category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"history/category/{id:int}")]
        public HistoryCategoryDto UpdateHistoryCategory(int id, string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curHistoryCategoryExist = DaoFactory.GetListItemDao().IsExist(id);
            if (!curHistoryCategoryExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName,
                ID = id
            };

            DaoFactory.GetListItemDao().EditItem(ListType.HistoryCategory, listItem);
            MessageService.Send(MessageAction.HistoryEventCategoryUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToHistoryCategoryDto(listItem);
        }

        /// <summary>
        ///    Updates the icon of the selected history category
        /// </summary>
        /// <param name="id">History category ID</param>
        /// <param name="imageName">icon name</param>
        /// <short>Update history category icon</short> 
        /// <category>History</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    History category
        /// </returns>
        [Update(@"history/category/{id:int}/icon")]
        public HistoryCategoryDto UpdateHistoryCategoryIcon(int id, string imageName)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var historyCategory = DaoFactory.GetListItemDao().GetByID(id);
            if (historyCategory == null) throw new ItemNotFoundException();

            historyCategory.AdditionalParams = imageName;

            DaoFactory.GetListItemDao().ChangePicture(id, imageName);
            MessageService.Send(MessageAction.HistoryEventCategoryUpdatedIcon, MessageTarget.Create(historyCategory.ID), historyCategory.Title);

            return ToHistoryCategoryDto(historyCategory);
        }

        /// <summary>
        ///    Updates the history categories order with the list specified in the request
        /// </summary>
        /// <short>
        ///    Update history categories order
        /// </short>
        /// <param name="titles">History category title list</param>
        /// <category>History</category>
        /// <returns>
        ///    History categories
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"history/category/reorder")]
        public IEnumerable<HistoryCategoryDto> UpdateHistoryCategoriesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.GetListItemDao().GetByTitle(ListType.HistoryCategory, title)).ToList();

            DaoFactory.GetListItemDao().ReorderItems(ListType.HistoryCategory, titles.ToArray());
            MessageService.Send(MessageAction.HistoryEventCategoriesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToHistoryCategoryDto);
        }

        /// <summary>
        ///   Deletes the selected history category with the ID specified in the request
        /// </summary>
        /// <short>Delete history category</short> 
        /// <category>History</category>
        /// <param name="id">History category ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns>History category</returns>
        [Delete(@"history/category/{id:int}")]
        public HistoryCategoryDto DeleteHistoryCategory(int id)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var dao = DaoFactory.GetListItemDao();
            var listItem = dao.GetByID(id);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.HistoryCategory) < 2)
            {
                throw new ArgumentException("The last history category cannot be deleted");
            }

            var result = ToHistoryCategoryDto(listItem);

            dao.DeleteItem(ListType.HistoryCategory, id, 0);
            MessageService.Send(MessageAction.HistoryEventCategoryDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return result;
        }

        /// <summary>
        ///   Creates a new task category with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="sortOrder">Order</param>
        ///<param name="imageName">Image name</param>
        ///<short>Create task category</short> 
        ///<category>Tasks</category>
        ///<returns>Task category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<returns>
        ///    Task category
        ///</returns>
        [Create(@"task/category")]
        public TaskCategoryDto CreateTaskCategory(string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName
            };

            listItem.ID = DaoFactory.GetListItemDao().CreateItem(ListType.TaskCategory, listItem);
            MessageService.Send(MessageAction.CrmTaskCategoryCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryDto(listItem);
        }

        /// <summary>
        ///   Updates the selected task category with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="id">Task category ID</param>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="sortOrder">Order</param>
        ///<param name="imageName">Image name</param>
        ///<short>Update task category</short> 
        ///<category>Tasks</category>
        ///<returns>Task category</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        ///<returns>
        ///    Task category
        ///</returns>
        [Update(@"task/category/{id:int}")]
        public TaskCategoryDto UpdateTaskCategory(int id, string title, string description, string imageName, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curTaskCategoryExist = DaoFactory.GetListItemDao().IsExist(id);
            if (!curTaskCategoryExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                SortOrder = sortOrder,
                AdditionalParams = imageName,
                ID = id
            };

            DaoFactory.GetListItemDao().EditItem(ListType.TaskCategory, listItem);
            MessageService.Send(MessageAction.CrmTaskCategoryUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryDto(listItem);
        }

        /// <summary>
        ///    Updates the icon of the task category with the ID specified in the request
        /// </summary>
        /// <param name="id">Task category ID</param>
        /// <param name="imageName">icon name</param>
        /// <short>Update task category icon</short> 
        /// <category>Tasks</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Task category
        /// </returns>
        [Update(@"task/category/{id:int}/icon")]
        public TaskCategoryDto UpdateTaskCategoryIcon(int id, string imageName)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var taskCategory = DaoFactory.GetListItemDao().GetByID(id);
            if (taskCategory == null) throw new ItemNotFoundException();

            taskCategory.AdditionalParams = imageName;

            DaoFactory.GetListItemDao().ChangePicture(id, imageName);
            MessageService.Send(MessageAction.CrmTaskCategoryUpdatedIcon, MessageTarget.Create(taskCategory.ID), taskCategory.Title);

            return ToTaskCategoryDto(taskCategory);
        }

        /// <summary>
        ///    Updates the task categories order with the list specified in the request
        /// </summary>
        /// <short>
        ///    Update task categories order
        /// </short>
        /// <param name="titles">Task category title list</param>
        /// <category>Tasks</category>
        /// <returns>
        ///    Task categories
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"task/category/reorder")]
        public IEnumerable<TaskCategoryDto> UpdateTaskCategoriesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.GetListItemDao().GetByTitle(ListType.TaskCategory, title)).ToList();

            DaoFactory.GetListItemDao().ReorderItems(ListType.TaskCategory, titles.ToArray());
            MessageService.Send(MessageAction.CrmTaskCategoriesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToTaskCategoryDto);
        }

        /// <summary>
        ///   Deletes the task category with the ID specified in the request
        /// </summary>
        /// <short>Delete task category</short> 
        /// <category>Tasks</category>
        /// <param name="categoryid">Task category ID</param>
        /// <param name="newcategoryid">Task category ID for replace in task with current category stage</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        [Delete(@"task/category/{categoryid:int}")]
        public TaskCategoryDto DeleteTaskCategory(int categoryid, int newcategoryid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (categoryid <= 0 || newcategoryid < 0) throw new ArgumentException();

            var dao = DaoFactory.GetListItemDao();
            var listItem = dao.GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.TaskCategory) < 2)
            {
                throw new ArgumentException("The last task category cannot be deleted");
            }

            dao.DeleteItem(ListType.TaskCategory, categoryid, newcategoryid);
            MessageService.Send(MessageAction.CrmTaskCategoryDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToTaskCategoryDto(listItem);
        }

        /// <summary>
        ///   Creates a new contact status with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="color">Color</param>
        ///<param name="sortOrder">Order</param>
        ///<returns>Contact status</returns>
        /// <short>Create contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Contact status
        /// </returns>
        [Create(@"contact/status")]
        public ContactStatusDto CreateContactStatus(string title, string description, string color, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = description,
                Color = color,
                SortOrder = sortOrder
            };

            listItem.ID = DaoFactory.GetListItemDao().CreateItem(ListType.ContactStatus, listItem);
            MessageService.Send(MessageAction.ContactTemperatureLevelCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactStatusDto(listItem);
        }

        /// <summary>
        ///   Updates the selected contact status with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="id">Contact status ID</param>
        ///<param name="title">Title</param>
        ///<param name="description">Description</param>
        ///<param name="color">Color</param>
        ///<param name="sortOrder">Order</param>
        ///<returns>Contact status</returns>
        /// <short>Update contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns>
        ///    Contact status
        /// </returns>
        [Update(@"contact/status/{id:int}")]
        public ContactStatusDto UpdateContactStatus(int id, string title, string description, string color, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curListItemExist = DaoFactory.GetListItemDao().IsExist(id);
            if (!curListItemExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                ID = id,
                Title = title,
                Description = description,
                Color = color,
                SortOrder = sortOrder
            };

            DaoFactory.GetListItemDao().EditItem(ListType.ContactStatus, listItem);
            MessageService.Send(MessageAction.ContactTemperatureLevelUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactStatusDto(listItem);
        }

        /// <summary>
        ///    Updates the color of the selected contact status with the new color specified in the request
        /// </summary>
        /// <param name="id">Contact status ID</param>
        /// <param name="color">Color</param>
        /// <short>Update contact status color</short> 
        /// <category>Contacts</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Contact status
        /// </returns>
        [Update(@"contact/status/{id:int}/color")]
        public ContactStatusDto UpdateContactStatusColor(int id, string color)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0) throw new ArgumentException();

            var contactStatus = DaoFactory.GetListItemDao().GetByID(id);
            if (contactStatus == null) throw new ItemNotFoundException();

            contactStatus.Color = color;

            DaoFactory.GetListItemDao().ChangeColor(id, color);
            MessageService.Send(MessageAction.ContactTemperatureLevelUpdatedColor, MessageTarget.Create(contactStatus.ID), contactStatus.Title);

            return ToContactStatusDto(contactStatus);
        }

        /// <summary>
        ///    Updates the contact statuses order with the list specified in the request
        /// </summary>
        /// <short>
        ///    Update contact statuses order
        /// </short>
        /// <param name="titles">Contact status title list</param>
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact statuses
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"contact/status/reorder")]
        public IEnumerable<ContactStatusDto> UpdateContactStatusesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.GetListItemDao().GetByTitle(ListType.ContactStatus, title)).ToList();

            DaoFactory.GetListItemDao().ReorderItems(ListType.ContactStatus, titles.ToArray());
            MessageService.Send(MessageAction.ContactTemperatureLevelsUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToContactStatusDto);
        }

        /// <summary>
        ///   Deletes the contact status with the ID specified in the request
        /// </summary>
        /// <short>Delete contact status</short> 
        /// <category>Contacts</category>
        /// <param name="contactStatusid">Contact status ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns>
        ///  Contact status
        /// </returns>
        [Delete(@"contact/status/{contactStatusid:int}")]
        public ContactStatusDto DeleteContactStatus(int contactStatusid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (contactStatusid <= 0) throw new ArgumentException();

            var dao = DaoFactory.GetListItemDao();
            var listItem = dao.GetByID(contactStatusid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.ContactStatus) < 2)
            {
                throw new ArgumentException("The last contact status cannot be deleted");
            }

            var contactStatus = ToContactStatusDto(listItem);

            dao.DeleteItem(ListType.ContactStatus, contactStatusid, 0);
            MessageService.Send(MessageAction.ContactTemperatureLevelDeleted, MessageTarget.Create(contactStatus.Id), contactStatus.Title);

            return contactStatus;
        }

        /// <summary>
        ///   Returns the status of the contact for the ID specified in the request
        /// </summary>
        /// <param name="contactStatusid">Contact status ID</param>
        /// <returns>Contact status</returns>
        /// <short>Get contact status</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"contact/status/{contactStatusid:int}")]
        public ContactStatusDto GetContactStatusByID(int contactStatusid)
        {
            if (contactStatusid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.GetListItemDao().GetByID(contactStatusid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToContactStatusDto(listItem);
        }

        /// <summary>
        ///   Creates a new contact type with the parameters (title, etc.) specified in the request
        /// </summary>
        ///<param name="title">Title</param>
        ///<param name="sortOrder">Order</param>
        ///<returns>Contact type</returns>
        /// <short>Create contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Contact type
        /// </returns>
        [Create(@"contact/type")]
        public ContactTypeDto CreateContactType(string title, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            var listItem = new ListItem
            {
                Title = title,
                Description = string.Empty,
                SortOrder = sortOrder
            };

            listItem.ID = DaoFactory.GetListItemDao().CreateItem(ListType.ContactType, listItem);
            MessageService.Send(MessageAction.ContactTypeCreated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactTypeDto(listItem);
        }

        /// <summary>
        ///   Updates the selected contact type with the parameters (title, description, etc.) specified in the request
        /// </summary>
        ///<param name="id">Contact type ID</param>
        ///<param name="title">Title</param>
        ///<param name="sortOrder">Order</param>
        ///<returns>Contact type</returns>
        /// <short>Update contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <returns>
        ///    Contact type
        /// </returns>
        [Update(@"contact/type/{id:int}")]
        public ContactTypeDto UpdateContactType(int id, string title, int sortOrder)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (id <= 0 || string.IsNullOrEmpty(title)) throw new ArgumentException();

            var curListItemExist = DaoFactory.GetListItemDao().IsExist(id);
            if (!curListItemExist) throw new ItemNotFoundException();

            var listItem = new ListItem
            {
                ID = id,
                Title = title,
                SortOrder = sortOrder
            };

            DaoFactory.GetListItemDao().EditItem(ListType.ContactType, listItem);
            MessageService.Send(MessageAction.ContactTypeUpdated, MessageTarget.Create(listItem.ID), listItem.Title);

            return ToContactTypeDto(listItem);
        }

        /// <summary>
        ///    Updates the contact types order with the list specified in the request
        /// </summary>
        /// <short>
        ///    Update contact types order
        /// </short>
        /// <param name="titles">Contact type title list</param>
        /// <category>Contacts</category>
        /// <returns>
        ///    Contact types
        /// </returns>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update(@"contact/type/reorder")]
        public IEnumerable<ContactTypeDto> UpdateContactTypesOrder(IEnumerable<string> titles)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (titles == null) throw new ArgumentException();

            var result = titles.Select(title => DaoFactory.GetListItemDao().GetByTitle(ListType.ContactType, title)).ToList();

            DaoFactory.GetListItemDao().ReorderItems(ListType.ContactType, titles.ToArray());
            MessageService.Send(MessageAction.ContactTypesUpdatedOrder, MessageTarget.Create(result.Select(x => x.ID)), result.Select(x => x.Title));

            return result.ConvertAll(ToContactTypeDto);
        }

        /// <summary>
        ///   Deletes the contact type with the ID specified in the request
        /// </summary>
        /// <short>Delete contact type</short> 
        /// <category>Contacts</category>
        /// <param name="contactTypeid">Contact type ID</param>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="SecurityException"></exception>
        /// <returns>
        ///  Contact type
        /// </returns>
        [Delete(@"contact/type/{contactTypeid:int}")]
        public ContactTypeDto DeleteContactType(int contactTypeid)
        {
            if (!(CRMSecurity.IsAdmin)) throw CRMSecurity.CreateSecurityException();

            if (contactTypeid <= 0) throw new ArgumentException();
            var dao = DaoFactory.GetListItemDao();

            var listItem = dao.GetByID(contactTypeid);
            if (listItem == null) throw new ItemNotFoundException();

            if (dao.GetItemsCount(ListType.ContactType) < 2)
            {
                throw new ArgumentException("The last contact type cannot be deleted");
            }

            var contactType = ToContactTypeDto(listItem);

            dao.DeleteItem(ListType.ContactType, contactTypeid, 0);
            MessageService.Send(MessageAction.ContactTypeDeleted, MessageTarget.Create(listItem.ID), listItem.Title);

            return contactType;
        }

        /// <summary>
        ///   Returns the type of the contact for the ID specified in the request
        /// </summary>
        /// <param name="contactTypeid">Contact type ID</param>
        /// <returns>Contact type</returns>
        /// <short>Get contact type</short> 
        /// <category>Contacts</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"contact/type/{contactTypeid:int}")]
        public ContactTypeDto GetContactTypeByID(int contactTypeid)
        {
            if (contactTypeid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.GetListItemDao().GetByID(contactTypeid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToContactTypeDto(listItem);
        }

        /// <summary>
        ///  Returns the stage of the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="stageid">Opportunity stage ID</param>
        /// <returns>Opportunity stage</returns>
        /// <short>Get opportunity stage</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"opportunity/stage/{stageid:int}")]
        public DealMilestoneDto GetDealMilestoneByID(int stageid)
        {
            if (stageid <= 0) throw new ArgumentException();

            var dealMilestone = DaoFactory.GetDealMilestoneDao().GetByID(stageid);
            if (dealMilestone == null) throw new ItemNotFoundException();

            return ToDealMilestoneDto(dealMilestone);
        }

        /// <summary>
        ///    Returns the category of the task with the ID specified in the request
        /// </summary>
        /// <param name="categoryid">Task category ID</param>
        /// <returns>Task category</returns>
        /// <short>Get task category</short> 
        /// <category>Tasks</category>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<exception cref="ArgumentException"></exception>
        [Read(@"task/category/{categoryid:int}")]
        public TaskCategoryDto GetTaskCategoryByID(int categoryid)
        {
            if (categoryid <= 0) throw new ArgumentException();

            var listItem = DaoFactory.GetListItemDao().GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException();

            return ToTaskCategoryDto(listItem);
        }

        /// <summary>
        ///    Returns the list of all history categories available on the portal
        /// </summary>
        /// <short>Get all history categories</short> 
        /// <category>History</category>
        /// <returns>
        ///    List of all history categories
        /// </returns>
        [Read(@"history/category")]
        public IEnumerable<HistoryCategoryDto> GetHistoryCategoryDto()
        {
            var result = DaoFactory.GetListItemDao().GetItems(ListType.HistoryCategory).ConvertAll(item => new HistoryCategoryDto(item));

            var relativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.HistoryCategory);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.Id))
                        x.RelativeItemsCount = relativeItemsCount[x.Id];
                });
            return result;
        }

        /// <summary>
        ///    Returns the list of all task categories available on the portal
        /// </summary>
        /// <short>Get all task categories</short> 
        /// <category>Tasks</category>
        /// <returns>
        ///    List of all task categories
        /// </returns>
        [Read(@"task/category")]
        public IEnumerable<TaskCategoryDto> GetTaskCategories()
        {
            var result = DaoFactory.GetListItemDao().GetItems(ListType.TaskCategory).ConvertAll(item => (TaskCategoryDto)TaskCategoryDtoHelper.Get(item));

            var relativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.TaskCategory);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.Id))
                        x.RelativeItemsCount = relativeItemsCount[x.Id];
                });
            return result;
        }

        /// <summary>
        ///    Returns the list of all contact statuses available on the portal
        /// </summary>
        /// <short>Get all contact statuses</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    List of all contact statuses
        /// </returns>
        [Read(@"contact/status")]
        public IEnumerable<ContactStatusDto> GetContactStatuses()
        {
            var result = DaoFactory.GetListItemDao().GetItems(ListType.ContactStatus).ConvertAll(item => new ContactStatusDto(item));

            var relativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.ContactStatus);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.Id))
                        x.RelativeItemsCount = relativeItemsCount[x.Id];
                });
            return result;
        }

        /// <summary>
        ///    Returns the list of all contact types available on the portal
        /// </summary>
        /// <short>Get all contact types</short> 
        /// <category>Contacts</category>
        /// <returns>
        ///    List of all contact types
        /// </returns>
        [Read(@"contact/type")]
        public IEnumerable<ContactTypeDto> GetContactTypes()
        {
            var result = DaoFactory.GetListItemDao().GetItems(ListType.ContactType).ConvertAll(item => new ContactTypeDto(item));

            var relativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.ContactType);

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.Id))
                        x.RelativeItemsCount = relativeItemsCount[x.Id];
                });

            return result;
        }

        /// <summary>
        ///    Returns the list of all opportunity stages available on the portal
        /// </summary>
        /// <short>Get all opportunity stages</short> 
        /// <category>Opportunities</category>
        /// <returns>
        ///   List of all opportunity stages
        /// </returns>
        [Read(@"opportunity/stage")]
        public IEnumerable<DealMilestoneDto> GetDealMilestones()
        {
            var result = DaoFactory.GetDealMilestoneDao().GetAll().ConvertAll(item => new DealMilestoneDto(item));

            var relativeItemsCount = DaoFactory.GetDealMilestoneDao().GetRelativeItemsCount();

            result.ForEach(x =>
                {
                    if (relativeItemsCount.ContainsKey(x.Id))
                        x.RelativeItemsCount = relativeItemsCount[x.Id];
                });

            return result;
        }

        public ContactStatusDto ToContactStatusDto(ListItem listItem)
        {
            var result = new ContactStatusDto(listItem)
            {
                RelativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.ContactStatus, listItem.ID)
            };

            return result;
        }

        public ContactTypeDto ToContactTypeDto(ListItem listItem)
        {
            var result = new ContactTypeDto(listItem)
            {
                RelativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.ContactType, listItem.ID)
            };

            return result;
        }

        public HistoryCategoryDto ToHistoryCategoryDto(ListItem listItem)
        {
            var result = (HistoryCategoryDto)HistoryCategoryDtoHelper.Get(listItem);

            result.RelativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.HistoryCategory, listItem.ID);

            return result;
        }

        public TaskCategoryDto ToTaskCategoryDto(ListItem listItem)
        {
            var result = (TaskCategoryDto)TaskCategoryDtoHelper.Get(listItem);

            result.RelativeItemsCount = DaoFactory.GetListItemDao().GetRelativeItemsCount(ListType.TaskCategory, listItem.ID);

            return result;
        }

        private DealMilestoneDto ToDealMilestoneDto(DealMilestone dealMilestone)
        {
            var result = new DealMilestoneDto(dealMilestone)
            {
                RelativeItemsCount = DaoFactory.GetDealMilestoneDao().GetRelativeItemsCount(dealMilestone.ID)
            };
            return result;
        }
    }
}