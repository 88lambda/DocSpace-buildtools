﻿using ASC.Api.Core;
using ASC.Api.CRM.Wrappers;
using ASC.Common.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;
using ASC.CRM.Resources;
using ASC.MessagingSystem;
using ASC.Web.Api.Routing;
using ASC.Web.CRM.Services.NotifyService;

using System;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Api.CRM
{
    public partial class CRMController
    {

        /// <summary>
        ///  Returns the detailed information about the task with the ID specified in the request
        /// </summary>
        /// <param name="taskid">Task ID</param>
        /// <returns>Task</returns>
        /// <short>Get task by ID</short> 
        /// <category>Tasks</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"task/{taskid:int}")]
        public TaskWrapper GetTaskByID(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            var task = DaoFactory.GetTaskDao().GetByID(taskid);
            if (task == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(task))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return TaskWrapperHelper.GetTaskWrapper(task);
        }

        /// <summary>
        ///   Returns the list of tasks matching the creteria specified in the request
        /// </summary>
        /// <param optional="true" name="responsibleid">Task responsible</param>
        /// <param optional="true" name="categoryid">Task category ID</param>
        /// <param optional="true" name="isClosed">Show open or closed tasks only</param>
        /// <param optional="true" name="fromDate">Earliest task due date</param>
        /// <param optional="true" name="toDate">Latest task due date</param>
        /// <param name="entityType" remark="Allowed values: opportunity, contact or case">Related entity type</param>
        /// <param name="entityid">Related entity ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Get task list</short> 
        /// <category>Tasks</category>
        /// <returns>
        ///   Task list
        /// </returns>
        [Read(@"task/filter")]
        public IEnumerable<TaskWrapper> GetAllTasks(
            Guid responsibleid,
            int categoryid,
            bool? isClosed,
            ApiDateTime fromDate,
            ApiDateTime toDate,
            string entityType,
            int entityid)
        {
            TaskSortedByType taskSortedByType;

            if (!string.IsNullOrEmpty(entityType) &&
                !(
                     string.Compare(entityType, "contact", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0)
                )
                throw new ArgumentException();

            var searchText = ApiContext.FilterValue;

            IEnumerable<TaskWrapper> result;

            OrderBy taskOrderBy;

            if (ASC.CRM.Classes.EnumExtension.TryParse(ApiContext.SortBy, true, out taskSortedByType))
            {
                taskOrderBy = new OrderBy(taskSortedByType, !ApiContext.SortDescending);
            }
            else if (string.IsNullOrEmpty(ApiContext.SortBy))
            {
                taskOrderBy = new OrderBy(TaskSortedByType.DeadLine, true);
            }
            else
            {
                taskOrderBy = null;
            }

            var fromIndex = (int)ApiContext.StartIndex;
            var count = (int)ApiContext.Count;

            if (taskOrderBy != null)
            {
                result = ToTaskListWrapper(
                    DaoFactory.GetTaskDao()
                        .GetTasks(
                            searchText,
                            responsibleid,
                            categoryid,
                            isClosed,
                            fromDate,
                            toDate,
                            ToEntityType(entityType),
                            entityid,
                            fromIndex,
                            count,
                            taskOrderBy)).ToList();

                ApiContext.SetDataPaginated();
                ApiContext.SetDataFiltered();
                ApiContext.SetDataSorted();
            }
            else
                result = ToTaskListWrapper(
                    DaoFactory
                        .GetTaskDao()
                        .GetTasks(
                            searchText,
                            responsibleid,
                            categoryid,
                            isClosed,
                            fromDate,
                            toDate,
                            ToEntityType(entityType),
                            entityid,
                            0,
                            0, null)).ToList();


            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory
                    .GetTaskDao()
                    .GetTasksCount(
                        searchText,
                        responsibleid,
                        categoryid,
                        isClosed,
                        fromDate,
                        toDate,
                        ToEntityType(entityType),
                        entityid);
            }

            ApiContext.SetTotalCount(totalCount);

            return result;
        }

        /// <summary>
        ///   Open anew the task with the ID specified in the request
        /// </summary>
        /// <short>Resume task</short> 
        /// <category>Tasks</category>
        /// <param name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Task
        /// </returns>
        [Update(@"task/{taskid:int}/reopen")]
        public TaskWrapper ReOpenTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            DaoFactory.GetTaskDao().OpenTask(taskid);

            var task = DaoFactory.GetTaskDao().GetByID(taskid);

            MessageService.Send(MessageAction.CrmTaskOpened, MessageTarget.Create(task.ID), task.Title);

            return TaskWrapperHelper.GetTaskWrapper(task);

        }

        /// <summary>
        ///   Close the task with the ID specified in the request
        /// </summary>
        /// <short>Close task</short> 
        /// <category>Tasks</category>
        /// <param name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///   Task
        /// </returns>
        [Update(@"task/{taskid:int}/close")]
        public TaskWrapper CloseTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            DaoFactory.GetTaskDao().CloseTask(taskid);

            var task = DaoFactory.GetTaskDao().GetByID(taskid);
            MessageService.Send(MessageAction.CrmTaskClosed, MessageTarget.Create(task.ID), task.Title);

            return TaskWrapperHelper.GetTaskWrapper(task);

        }

        /// <summary>
        ///   Delete the task with the ID specified in the request
        /// </summary>
        /// <short>Delete task</short> 
        /// <category>Tasks</category>
        /// <param name="taskid">Task ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///  Deleted task
        /// </returns>
        [Delete(@"task/{taskid:int}")]
        public TaskWrapper DeleteTask(int taskid)
        {
            if (taskid <= 0) throw new ArgumentException();

            var task = DaoFactory.GetTaskDao().GetByID(taskid);
            if (task == null) throw new ItemNotFoundException();

            DaoFactory.GetTaskDao().DeleteTask(taskid);
            MessageService.Send(MessageAction.CrmTaskDeleted, MessageTarget.Create(task.ID), task.Title);

            return TaskWrapperHelper.GetTaskWrapper(task);

        }

        /// <summary>
        ///  Creates the task with the parameters (title, description, due date, etc.) specified in the request
        /// </summary>
        /// <param name="title">Task title</param>
        /// <param optional="true"  name="description">Task description</param>
        /// <param name="deadline">Task due date</param>
        /// <param name="responsibleId">Task responsible ID</param>
        /// <param name="categoryId">Task category ID</param>
        /// <param optional="true"  name="contactId">Contact ID</param>
        /// <param optional="true"  name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param optional="true"  name="entityId">Related entity ID</param>
        /// <param optional="true"  name="isNotify">Notify the responsible about the task</param>
        /// <param optional="true"  name="alertValue">Time period in minutes for reminder to the responsible about the task</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Create task</short> 
        /// <category>Tasks</category>
        /// <returns>Task</returns>
        [Create(@"task")]
        public TaskWrapper CreateTask(
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleId,
            int categoryId,
            int contactId,
            string entityType,
            int entityId,
            bool isNotify,
            int alertValue
            )
        {
            if (!string.IsNullOrEmpty(entityType) &&
                !(
                     string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0
                 )
                || categoryId <= 0)
                throw new ArgumentException();

            var listItem = DaoFactory.GetListItemDao().GetByID(categoryId);
            if (listItem == null) throw new ItemNotFoundException(CRMErrorsResource.TaskCategoryNotFound);

            var task = new Task
            {
                Title = title,
                Description = description,
                ResponsibleID = responsibleId,
                CategoryID = categoryId,
                DeadLine = deadline,
                ContactID = contactId,
                EntityType = ToEntityType(entityType),
                EntityID = entityId,
                IsClosed = false,
                AlertValue = alertValue
            };

            task = DaoFactory.GetTaskDao().SaveOrUpdateTask(task);

            if (isNotify)
            {
                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (task.ContactID > 0)
                {
                    taskContact = DaoFactory.GetContactDao().GetByID(task.ContactID);
                }

                if (task.EntityID > 0)
                {
                    switch (task.EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.GetCasesDao().GetByID(task.EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.GetDealDao().GetByID(task.EntityID);
                            break;
                    }
                }

                NotifyClient.SendAboutResponsibleByTask(task, listItem.Title, taskContact, taskCase, taskDeal, null);
            }

            MessageService.Send(MessageAction.CrmTaskCreated, MessageTarget.Create(task.ID), task.Title);

            return TaskWrapperHelper.GetTaskWrapper(task);
        }

        /// <summary>
        ///  Creates the group of the same task with the parameters (title, description, due date, etc.) specified in the request for several contacts
        /// </summary>
        /// <param name="title">Task title</param>
        /// <param optional="true"  name="description">Task description</param>
        /// <param name="deadline">Task due date</param>
        /// <param name="responsibleId">Task responsible ID</param>
        /// <param name="categoryId">Task category ID</param>
        /// <param name="contactId">contact ID list</param>
        /// <param optional="true"  name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param optional="true"  name="entityId">Related entity ID</param>
        /// <param optional="true"  name="isNotify">Notify the responsible about the task</param>
        /// <param optional="true"  name="alertValue">Time period in minutes for reminder to the responsible about the task</param>
        /// <exception cref="ArgumentException"></exception>
        /// <short>Create task list</short> 
        /// <category>Tasks</category>
        /// <returns>Tasks</returns>
        /// <visible>false</visible>
        [Create(@"contact/task/group")]
        public IEnumerable<TaskWrapper> CreateTaskGroup(
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleId,
            int categoryId,
            int[] contactId,
            string entityType,
            int entityId,
            bool isNotify,
            int alertValue)
        {
            var tasks = new List<Task>();

            if (
                !string.IsNullOrEmpty(entityType) &&
                !(string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                  string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0)
                )
                throw new ArgumentException();

            foreach (var cid in contactId)
            {
                tasks.Add(new Task
                {
                    Title = title,
                    Description = description,
                    ResponsibleID = responsibleId,
                    CategoryID = categoryId,
                    DeadLine = deadline,
                    ContactID = cid,
                    EntityType = ToEntityType(entityType),
                    EntityID = entityId,
                    IsClosed = false,
                    AlertValue = alertValue
                });
            }

            tasks = DaoFactory.GetTaskDao().SaveOrUpdateTaskList(tasks).ToList();

            string taskCategory = null;
            if (isNotify)
            {
                if (categoryId > 0)
                {
                    var listItem = DaoFactory.GetListItemDao().GetByID(categoryId);
                    if (listItem == null) throw new ItemNotFoundException();

                    taskCategory = listItem.Title;
                }
            }

            for (var i = 0; i < tasks.Count; i++)
            {
                if (!isNotify) continue;

                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (tasks[i].ContactID > 0)
                {
                    taskContact = DaoFactory.GetContactDao().GetByID(tasks[i].ContactID);
                }

                if (tasks[i].EntityID > 0)
                {
                    switch (tasks[i].EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.GetCasesDao().GetByID(tasks[i].EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.GetDealDao().GetByID(tasks[i].EntityID);
                            break;
                    }
                }

                NotifyClient.SendAboutResponsibleByTask(tasks[i], taskCategory, taskContact, taskCase, taskDeal, null);
            }

            if (tasks.Any())
            {
                var contacts = DaoFactory.GetContactDao().GetContacts(contactId);
                var task = tasks.First();
                MessageService.Send(MessageAction.ContactsCreatedCrmTasks, MessageTarget.Create(tasks.Select(x => x.ID)), contacts.Select(x => x.GetTitle()), task.Title);
            }

            return ToTaskListWrapper(tasks);
        }


        /// <summary>
        ///   Updates the selected task with the parameters (title, description, due date, etc.) specified in the request
        /// </summary>
        /// <param name="taskid">Task ID</param>
        /// <param name="title">Task title</param>
        /// <param name="description">Task description</param>
        /// <param name="deadline">Task due date</param>
        /// <param name="responsibleid">Task responsible ID</param>
        /// <param name="categoryid">Task category ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <param name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param name="entityid">Related entity ID</param>
        /// <param name="isNotify">Notify or not</param>
        /// <param optional="true"  name="alertValue">Time period in minutes for reminder to the responsible about the task</param>
        /// <short> Update task</short> 
        /// <category>Tasks</category>
        /// <returns>Task</returns>
        [Update(@"task/{taskid:int}")]
        public TaskWrapper UpdateTask(
            int taskid,
            string title,
            string description,
            ApiDateTime deadline,
            Guid responsibleid,
            int categoryid,
            int contactid,
            string entityType,
            int entityid,
            bool isNotify,
            int alertValue)
        {
            if (!string.IsNullOrEmpty(entityType) &&
                !(string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                  string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0
                 ) || categoryid <= 0)
                throw new ArgumentException();

            var listItem = DaoFactory.GetListItemDao().GetByID(categoryid);
            if (listItem == null) throw new ItemNotFoundException(CRMErrorsResource.TaskCategoryNotFound);

            var task = new Task
            {
                ID = taskid,
                Title = title,
                Description = description,
                DeadLine = deadline,
                AlertValue = alertValue,
                ResponsibleID = responsibleid,
                CategoryID = categoryid,
                ContactID = contactid,
                EntityID = entityid,
                EntityType = ToEntityType(entityType)
            };


            task = DaoFactory.GetTaskDao().SaveOrUpdateTask(task);

            if (isNotify)
            {
                Contact taskContact = null;
                Cases taskCase = null;
                Deal taskDeal = null;

                if (task.ContactID > 0)
                {
                    taskContact = DaoFactory.GetContactDao().GetByID(task.ContactID);
                }

                if (task.EntityID > 0)
                {
                    switch (task.EntityType)
                    {
                        case EntityType.Case:
                            taskCase = DaoFactory.GetCasesDao().GetByID(task.EntityID);
                            break;
                        case EntityType.Opportunity:
                            taskDeal = DaoFactory.GetDealDao().GetByID(task.EntityID);
                            break;
                    }
                }

                NotifyClient.SendAboutResponsibleByTask(task, listItem.Title, taskContact, taskCase, taskDeal, null);
            }

            MessageService.Send(MessageAction.CrmTaskUpdated, MessageTarget.Create(task.ID), task.Title);

            return TaskWrapperHelper.GetTaskWrapper(task);
        }

        /// <visible>false</visible>
        [Update(@"task/{taskid:int}/creationdate")]
        public void SetTaskCreationDate(int taskId, ApiDateTime creationDate)
        {
            var dao = DaoFactory.GetTaskDao();
            var task = dao.GetByID(taskId);

            if (task == null || !CRMSecurity.CanAccessTo(task))
                throw new ItemNotFoundException();

            dao.SetTaskCreationDate(taskId, creationDate);
        }

        /// <visible>false</visible>
        [Update(@"task/{taskid:int}/lastmodifeddate")]
        public void SetTaskLastModifedDate(int taskId, ApiDateTime lastModifedDate)
        {
            var dao = DaoFactory.GetTaskDao();
            var task = dao.GetByID(taskId);

            if (task == null || !CRMSecurity.CanAccessTo(task))
                throw new ItemNotFoundException();

            dao.SetTaskLastModifedDate(taskId, lastModifedDate);
        }

        private IEnumerable<TaskWrapper> ToTaskListWrapper(IEnumerable<Task> itemList)
        {
            var result = new List<TaskWrapper>();

            var contactIDs = new List<int>();
            var taskIDs = new List<int>();
            var categoryIDs = new List<int>();
            var entityWrappersIDs = new Dictionary<EntityType, List<int>>();

            foreach (var item in itemList)
            {
                taskIDs.Add(item.ID);

                if (!categoryIDs.Contains(item.CategoryID))
                {
                    categoryIDs.Add(item.CategoryID);
                }

                if (item.ContactID > 0 && !contactIDs.Contains(item.ContactID))
                {
                    contactIDs.Add(item.ContactID);
                }

                if (item.EntityID > 0)
                {
                    if (item.EntityType != EntityType.Opportunity && item.EntityType != EntityType.Case) continue;

                    if (!entityWrappersIDs.ContainsKey(item.EntityType))
                    {
                        entityWrappersIDs.Add(item.EntityType, new List<int>
                            {
                                item.EntityID
                            });
                    }
                    else if (!entityWrappersIDs[item.EntityType].Contains(item.EntityID))
                    {
                        entityWrappersIDs[item.EntityType].Add(item.EntityID);
                    }
                }
            }

            var entityWrappers = new Dictionary<string, EntityWrapper>();

            foreach (var entityType in entityWrappersIDs.Keys)
            {
                switch (entityType)
                {
                    case EntityType.Opportunity:
                        DaoFactory.GetDealDao().GetDeals(entityWrappersIDs[entityType].Distinct().ToArray())
                                  .ForEach(item =>
                                  {
                                      if (item == null) return;

                                      entityWrappers.Add(
                                          string.Format("{0}_{1}", (int)entityType, item.ID),
                                          new EntityWrapper
                                          {
                                              EntityId = item.ID,
                                              EntityTitle = item.Title,
                                              EntityType = "opportunity"
                                          });
                                  });
                        break;
                    case EntityType.Case:
                        DaoFactory.GetCasesDao().GetByID(entityWrappersIDs[entityType].ToArray())
                                  .ForEach(item =>
                                  {
                                      if (item == null) return;

                                      entityWrappers.Add(
                                          string.Format("{0}_{1}", (int)entityType, item.ID),
                                          new EntityWrapper
                                          {
                                              EntityId = item.ID,
                                              EntityTitle = item.Title,
                                              EntityType = "case"
                                          });
                                  });
                        break;
                }
            }

            var categories = DaoFactory.GetListItemDao().GetItems(categoryIDs.ToArray()).ToDictionary(x => x.ID, x => TaskCategoryWrapperHelper.Get(x));
            var contacts = DaoFactory.GetContactDao().GetContacts(contactIDs.ToArray()).ToDictionary(item => item.ID, x => ContactWrapperHelper.GetContactBaseWithEmailWrapper(x));
            var restrictedContacts = DaoFactory.GetContactDao().GetRestrictedContacts(contactIDs.ToArray()).ToDictionary(item => item.ID, x => ContactWrapperHelper.GetContactBaseWithEmailWrapper(x));

            foreach (var item in itemList)
            {
                var taskWrapper = TaskWrapperHelper.GetTaskWrapper(item);
                taskWrapper.CanEdit = CRMSecurity.CanEdit(item);

                if (contacts.ContainsKey(item.ContactID))
                {
                    taskWrapper.Contact = contacts[item.ContactID];
                }

                if (restrictedContacts.ContainsKey(item.ContactID))
                {
                    taskWrapper.Contact = restrictedContacts[item.ContactID];
                    /*Hide some fields. Should be refactored! */
                    taskWrapper.Contact.Currency = null;
                    taskWrapper.Contact.Email = null;
                    taskWrapper.Contact.AccessList = null;
                }

                if (item.EntityID > 0)
                {
                    var entityStrKey = string.Format("{0}_{1}", (int)item.EntityType, item.EntityID);

                    if (entityWrappers.ContainsKey(entityStrKey))
                    {
                        taskWrapper.Entity = entityWrappers[entityStrKey];
                    }
                }

                if (categories.ContainsKey(item.CategoryID))
                {
                    taskWrapper.Category = categories[item.CategoryID];
                }

                result.Add(taskWrapper);
            }

            return result;
        }


    }

}