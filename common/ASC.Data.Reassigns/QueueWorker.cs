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

namespace ASC.Data.Reassigns
{
    public static class QueueWorker
    {
        public static IDictionary<string, StringValues> GetHttpHeaders(HttpRequest httpRequest)
        {
            return httpRequest?.Headers;
        }
    }

    public class QueueWorker<T> where T : DistributedTaskProgress
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected IServiceProvider ServiceProvider { get; }

        private readonly object _synchRoot = new object();
        protected readonly DistributedTaskQueue Queue;

        public QueueWorker(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            DistributedTaskQueueOptionsManager options)
        {
            HttpContextAccessor = httpContextAccessor;
            ServiceProvider = serviceProvider;
            Queue = options.Get<T>();
        }

        public static string GetProgressItemId(int tenantId, Guid userId)
        {
            return string.Format("{0}_{1}_{2}", tenantId, userId, typeof(T).Name);
        }

        public T GetProgressItemStatus(int tenantId, Guid userId)
        {
            var id = GetProgressItemId(tenantId, userId);

            return Queue.GetTask<T>(id);
        }

        public void Terminate(int tenantId, Guid userId)
        {
            var item = GetProgressItemStatus(tenantId, userId);

            if (item != null)
            {
                Queue.CancelTask(item.Id);
            }
        }

        protected DistributedTaskProgress Start(int tenantId, Guid userId, Func<T> constructor)
        {
            lock (_synchRoot)
            {
                var task = GetProgressItemStatus(tenantId, userId);

                if (task != null && task.IsCompleted)
                {
                    Queue.RemoveTask(task.Id);
                    task = null;
                }

                if (task == null)
                {
                    task = constructor();
                    Queue.QueueTask(task);
                }

                return task;
            }
        }
    }

    [Scope(Additional = typeof(ReassignProgressItemExtension))]
    public class QueueWorkerReassign : QueueWorker<ReassignProgressItem>
    {
        public QueueWorkerReassign(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            DistributedTaskQueueOptionsManager options) :
            base(httpContextAccessor, serviceProvider, options)
        {
        }

        public ReassignProgressItem Start(int tenantId, Guid fromUserId, Guid toUserId, Guid currentUserId, bool deleteProfile)
        {
            return Start(tenantId, fromUserId, () =>
            {
                var result = ServiceProvider.GetService<ReassignProgressItem>();
                result.Init(tenantId, fromUserId, toUserId, currentUserId, deleteProfile);

                return result;
            }) as ReassignProgressItem;
        }
    }

    [Scope(Additional = typeof(RemoveProgressItemExtension))]
    public class QueueWorkerRemove : QueueWorker<RemoveProgressItem>
    {
        public QueueWorkerRemove(
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            DistributedTaskQueueOptionsManager options) :
            base(httpContextAccessor, serviceProvider, options)
        {
        }

        public RemoveProgressItem Start(int tenantId, UserInfo user, Guid currentUserId, bool notify)
        {
            return Start(tenantId, user.Id, () =>
            {
                var result = ServiceProvider.GetService<RemoveProgressItem>();
                result.Init(tenantId, user, currentUserId, notify);

                return result;
            }) as RemoveProgressItem;
        }
    }
}
