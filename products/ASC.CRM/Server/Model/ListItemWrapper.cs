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
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Configuration;
using System;
using System.Runtime.Serialization;

namespace ASC.Api.CRM.Wrappers
{

    #region History Category

    [DataContract(Name = "historyCategoryBase", Namespace = "")]
    public class HistoryCategoryBaseWrapper : ListItemWrapper
    {
        public HistoryCategoryBaseWrapper()
        {

        }

        public HistoryCategoryBaseWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember]
        public String ImagePath { get; set; }

        public static HistoryCategoryBaseWrapper GetSample()
        {
            return new HistoryCategoryBaseWrapper
                {
                    Title = "Lunch",
                    SortOrder = 10,
                    Color = String.Empty,
                    Description = "",
                    ImagePath = "path to image"
                };
        }
    }

    [DataContract(Name = "historyCategory", Namespace = "")]
    public class HistoryCategoryWrapper : HistoryCategoryBaseWrapper
    {
        public HistoryCategoryWrapper()
        {
        }

        public HistoryCategoryWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static HistoryCategoryWrapper GetSample()
        {
            return new HistoryCategoryWrapper
                {
                    Title = "Lunch",
                    SortOrder = 10,
                    Color = String.Empty,
                    Description = "",
                    ImagePath = "path to image",
                    RelativeItemsCount = 1
                };
        }
    }

    [Scope]
    public sealed class HistoryCategoryWrapperHelper
    {
        public HistoryCategoryWrapperHelper(WebImageSupplier webImageSupplier)
        {
            WebImageSupplier = webImageSupplier;
        }

        public WebImageSupplier WebImageSupplier { get; }

        public HistoryCategoryBaseWrapper Get(ListItem listItem)
        {
            return new HistoryCategoryBaseWrapper(listItem)
            {
                ImagePath = WebImageSupplier.GetAbsoluteWebPath(listItem.AdditionalParams, ProductEntryPoint.ID)
            };
        }
    }

    #endregion

    #region Deal Milestone

    [DataContract(Name = "opportunityStagesBase", Namespace = "")]
    public class DealMilestoneBaseWrapper : ListItemWrapper
    {
        public DealMilestoneBaseWrapper()
        {
        }

        public DealMilestoneBaseWrapper(DealMilestone dealMilestone)
        {
            SuccessProbability = dealMilestone.Probability;
            StageType = dealMilestone.Status;
            Color = dealMilestone.Color;
            Description = dealMilestone.Description;
            Title = dealMilestone.Title;
        }

        [DataMember]
        public int SuccessProbability { get; set; }

        [DataMember]
        public DealMilestoneStatus StageType { get; set; }

        public static DealMilestoneBaseWrapper GetSample()
        {
            return new DealMilestoneBaseWrapper
                {
                    Title = "Discussion",
                    SortOrder = 2,
                    Color = "#B9AFD3",
                    Description = "The potential buyer showed his/her interest and sees how your offering meets his/her goal",
                    StageType = DealMilestoneStatus.Open,
                    SuccessProbability = 20
                };
        }
    }

    [DataContract(Name = "opportunityStages", Namespace = "")]
    public class DealMilestoneWrapper : DealMilestoneBaseWrapper
    {
        public DealMilestoneWrapper()
        {
        }

        public DealMilestoneWrapper(DealMilestone dealMilestone)
            : base(dealMilestone)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static DealMilestoneWrapper GetSample()
        {
            return new DealMilestoneWrapper
                {
                    Title = "Discussion",
                    SortOrder = 2,
                    Color = "#B9AFD3",
                    Description = "The potential buyer showed his/her interest and sees how your offering meets his/her goal",
                    StageType = DealMilestoneStatus.Open,
                    SuccessProbability = 20,
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Task Category

    [DataContract(Name = "taskCategoryBase", Namespace = "")]
    public class TaskCategoryBaseWrapper : ListItemWrapper
    {
        public TaskCategoryBaseWrapper()
        {
        }

        public TaskCategoryBaseWrapper(ListItem listItem) : base(listItem)
        {

        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String ImagePath { get; set; }

        public static TaskCategoryBaseWrapper GetSample()
        {
            return new TaskCategoryBaseWrapper
                {
                    Title = "Appointment",
                    SortOrder = 2,
                    Description = "",
                    ImagePath = "path to image"
                };
        }
    }

    [DataContract(Name = "taskCategory", Namespace = "")]
    public class TaskCategoryWrapper : TaskCategoryBaseWrapper
    {
        public TaskCategoryWrapper()
        {
        }

        public TaskCategoryWrapper(ListItem listItem): base(listItem)
        {
        }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static TaskCategoryWrapper GetSample()
        {
            return new TaskCategoryWrapper
                {
                    Id = 30,
                    Title = "Appointment",
                    SortOrder = 2,
                    Description = "",
                    ImagePath = "path to image",
                    RelativeItemsCount = 1
                };
        }
    }


    [Scope]
    public sealed class TaskCategoryWrapperHelper
    {
        public TaskCategoryWrapperHelper(WebImageSupplier webImageSupplier)
        {
            WebImageSupplier = webImageSupplier;                
        }

        public WebImageSupplier WebImageSupplier { get; }

        public TaskCategoryBaseWrapper Get(ListItem listItem)
        {
            return new TaskCategoryBaseWrapper(listItem)
            {
                ImagePath = WebImageSupplier.GetAbsoluteWebPath(listItem.AdditionalParams, ProductEntryPoint.ID)
            };
        }
    }







    #endregion

    #region Contact Status

    [DataContract(Name = "contactStatusBase", Namespace = "")]
    public class ContactStatusBaseWrapper : ListItemWrapper
    {
        public ContactStatusBaseWrapper() 
        {
        }

        public ContactStatusBaseWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        public static ContactStatusBaseWrapper GetSample()
        {
            return new ContactStatusBaseWrapper
                {
                    Title = "Cold",
                    SortOrder = 2,
                    Description = ""
                };
        }
    }

    [DataContract(Name = "contactStatus", Namespace = "")]
    public class ContactStatusWrapper : ContactStatusBaseWrapper
    {
        public ContactStatusWrapper()
        {
        }

        public ContactStatusWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static ContactStatusWrapper GetSample()
        {
            return new ContactStatusWrapper
                {
                    Title = "Cold",
                    SortOrder = 2,
                    Description = "",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Contact Type

    [DataContract(Name = "contactTypeBase", Namespace = "")]
    public class ContactTypeBaseWrapper : ListItemWrapper
    {
        public ContactTypeBaseWrapper()
        {

        }

        public ContactTypeBaseWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        public static ContactTypeBaseWrapper GetSample()
        {
            return new ContactTypeBaseWrapper
                {
                    Id = 30,
                    Title = "Client",
                    SortOrder = 2,
                    Description = ""
                };
        }
    }

    [DataContract(Name = "contactType", Namespace = "")]
    public class ContactTypeWrapper : ContactTypeBaseWrapper
    {
        public ContactTypeWrapper()
        {
        }

        public ContactTypeWrapper(ListItem listItem)
            : base(listItem)
        {
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public new static ContactTypeWrapper GetSample()
        {
            return new ContactTypeWrapper
                {
                    Id= 30,
                    Title = "Client",
                    SortOrder = 2,
                    Description = "",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    #region Tags

    [DataContract(Name = "tagWrapper", Namespace = "")]
    public class TagWrapper
    {
        public TagWrapper()
        {
            Title = String.Empty;
            RelativeItemsCount = 0;
        }

        public TagWrapper(String tag, int relativeItemsCount = 0)
        {
            Title = tag;
            RelativeItemsCount = relativeItemsCount;
        }

        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int RelativeItemsCount { get; set; }

        public static TagWrapper GetSample()
        {
            return new TagWrapper
                {
                    Title = "Tag",
                    RelativeItemsCount = 1
                };
        }
    }

    #endregion

    [DataContract(Name = "listItem", Namespace = "")]
    public abstract class ListItemWrapper 
    {
        protected ListItemWrapper()
        {

        }

        protected ListItemWrapper(ListItem listItem)
        {
            Title = listItem.Title;
            Description = listItem.Description;
            Color = listItem.Color;
            SortOrder = listItem.SortOrder;
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Title { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public String Color { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int SortOrder { get; set; }
    }
}