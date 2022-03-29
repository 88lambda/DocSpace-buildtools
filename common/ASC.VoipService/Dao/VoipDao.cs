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

namespace ASC.VoipService.Dao;

[Scope(Additional = typeof(EventTypeConverterExtension))]
public class VoipDao : AbstractDao
{
    private readonly AuthContext _authContext;
    private readonly TenantUtil _tenantUtil;
    private readonly SecurityContext _securityContext;
    private readonly BaseCommonLinkUtility _baseCommonLinkUtility;
    private readonly ConsumerFactory _consumerFactory;
    private readonly IMapper _mapper;

    public VoipDao(
        TenantManager tenantManager,
        DbContextManager<VoipDbContext> dbOptions,
        AuthContext authContext,
        TenantUtil tenantUtil,
        SecurityContext securityContext,
        BaseCommonLinkUtility baseCommonLinkUtility,
        ConsumerFactory consumerFactory,
        IMapper mapper)
        : base(dbOptions, tenantManager)
    {
        _authContext = authContext;
        _tenantUtil = tenantUtil;
        _securityContext = securityContext;
        _baseCommonLinkUtility = baseCommonLinkUtility;
        _consumerFactory = consumerFactory;
        _mapper = mapper;
    }

    public virtual VoipPhone SaveOrUpdateNumber(VoipPhone phone)
    {
        if (!string.IsNullOrEmpty(phone.Number))
        {
            phone.Number = phone.Number.TrimStart('+');
        }

        var voipNumber = new VoipNumber
        {
            Id = phone.Id,
            Number = phone.Number,
            Alias = phone.Alias,
            Settings = phone.Settings.ToString(),
            TenantId = TenantID
        };

        VoipDbContext.VoipNumbers.Add(voipNumber);
        VoipDbContext.SaveChanges();

        return phone;
    }

    public virtual void DeleteNumber(string phoneId = "")
    {
        var number = VoipDbContext.VoipNumbers.Where(r => r.Id == phoneId && r.TenantId == TenantID).FirstOrDefault();
        VoipDbContext.VoipNumbers.Remove(number);
        VoipDbContext.SaveChanges();
    }

    public virtual IEnumerable<VoipPhone> GetAllNumbers()
    {
        return VoipDbContext.VoipNumbers
            .Where(r => r.TenantId == TenantID)
            .ToList()
            .ConvertAll(ToPhone);
    }

    public virtual IEnumerable<VoipPhone> GetNumbers(params string[] ids)
    {
        var numbers = VoipDbContext.VoipNumbers.Where(r => r.TenantId == TenantID);

        if (ids.Length > 0)
        {
            numbers = numbers.Where(r => ids.Any(a => a == r.Number || a == r.Id));
        }

        return numbers.ToList().ConvertAll(ToPhone);
    }

    public VoipPhone GetNumber(string id)
    {
        return GetNumbers(id.TrimStart('+')).FirstOrDefault();
    }

    public virtual VoipPhone GetCurrentNumber()
    {
        return GetNumbers().FirstOrDefault(r => r.Caller != null);
    }


    public VoipCall SaveOrUpdateCall(VoipCall call)
    {
        var voipCall = new DbVoipCall
        {
            TenantId = TenantID,
            Id = call.Id,
            NumberFrom = call.NumberFrom,
            NumberTo = call.NumberTo,
            ContactId = call.ContactId
        };

        if (!string.IsNullOrEmpty(call.ParentCallId))
        {
            voipCall.ParentCallId = call.ParentCallId;
        }

        if (call.Status.HasValue)
        {
            voipCall.Status = (int)call.Status.Value;
        }

        if (!call.AnsweredBy.Equals(Guid.Empty))
        {
            voipCall.AnsweredBy = call.AnsweredBy;
        }

        if (call.DialDate == DateTime.MinValue)
        {
            call.DialDate = DateTime.UtcNow;
        }

        voipCall.DialDate = _tenantUtil.DateTimeToUtc(call.DialDate);

        if (call.DialDuration > 0)
        {
            voipCall.DialDuration = call.DialDuration;
        }

        if (call.Price > decimal.Zero)
        {
            voipCall.Price = call.Price;
        }

        if (call.VoipRecord != null)
        {
            if (!string.IsNullOrEmpty(call.VoipRecord.Sid))
            {
                voipCall.Sid = call.VoipRecord.Sid;
            }

            if (!string.IsNullOrEmpty(call.VoipRecord.Uri))
            {
                voipCall.Uri = call.VoipRecord.Uri;
            }

            if (call.VoipRecord.Duration != 0)
            {
                voipCall.Duration = call.VoipRecord.Duration;
            }

            if (call.VoipRecord.Price != default)
            {
                voipCall.RecordPrice = call.VoipRecord.Price;
            }
        }

        VoipDbContext.VoipCalls.Add(voipCall);
        VoipDbContext.SaveChanges();

        return call;
    }

    public IEnumerable<VoipCall> GetCalls(VoipCallFilter filter)
    {
        var query = GetCallsQuery(filter);

        if (filter.SortByColumn != null)
        {
            query.OrderBy(filter.SortByColumn, filter.SortOrder);
        }

        query = query.Skip((int)filter.Offset);
        query = query.Take((int)filter.Max * 3);

        var calls = _mapper.Map<List<CallContact>, IEnumerable<VoipCall>>(query.ToList());

        calls = calls.GroupJoin(calls, call => call.Id, h => h.ParentCallId, (call, h) =>
        {
            call.ChildCalls.AddRange(h);
            return call;
        }).Where(r => string.IsNullOrEmpty(r.ParentCallId)).ToList();

        return calls;
    }

    public VoipCall GetCall(string id)
    {
        return GetCalls(new VoipCallFilter { Id = id }).FirstOrDefault();
    }

    public int GetCallsCount(VoipCallFilter filter)
    {
        return GetCallsQuery(filter).Where(r => r.DbVoipCall.ParentCallId == "").Count();
    }

    public IEnumerable<VoipCall> GetMissedCalls(Guid agent, long count = 0, DateTime? from = null)
    {
        var query = GetCallsQuery(new VoipCallFilter { Agent = agent, SortBy = "date", SortOrder = true, Type = "missed" });

        if (from.HasValue)
        {
            query = query.Where(r => r.DbVoipCall.DialDate >= _tenantUtil.DateTimeFromUtc(from.Value));
        }

        if (count != 0)
        {
            query = query.Take((int)count);
        }

        query = query.Select(ca => new
        {
            dbVoipCall = ca,
            tmpDate = VoipDbContext.VoipCalls
            .Where(tmp => tmp.TenantId == ca.DbVoipCall.TenantId)
            .Where(tmp => tmp.NumberFrom == ca.DbVoipCall.NumberFrom || tmp.NumberTo == ca.DbVoipCall.NumberFrom)
            .Where(tmp => tmp.Status <= (int)VoipCallStatus.Missed)
            .Max(tmp => tmp.DialDate)
        }).Where(r => r.dbVoipCall.DbVoipCall.DialDate >= r.tmpDate || r.tmpDate == default)
        .Select(q=> q.dbVoipCall);

        return _mapper.Map<List<CallContact>, IEnumerable<VoipCall>>(query.ToList());
    }

    private IQueryable<CallContact> GetCallsQuery(VoipCallFilter filter)
    {
        var q = VoipDbContext.VoipCalls
            .Where(r => r.TenantId == TenantID);

        if (!string.IsNullOrEmpty(filter.Id))
        {
            q = q.Where(r => r.Id == filter.Id || r.ParentCallId == filter.Id);
        }

        if (filter.ContactID.HasValue)
        {
            q = q.Where(r => r.ContactId == filter.ContactID.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            q = q.Where(r => r.Id.StartsWith(filter.SearchText));
        }

        if (filter.TypeStatus.HasValue)
        {
            q = q.Where(r => r.Status == filter.TypeStatus.Value);
        }

        if (filter.FromDate.HasValue)
        {
            q = q.Where(r => r.DialDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            q = q.Where(r => r.DialDate <= filter.ToDate.Value);
        }

        if (filter.Agent.HasValue)
        {
            q = q.Where(r => r.AnsweredBy == filter.Agent.Value);
        }
        
        return from voipCalls in q
               join crmContact in VoipDbContext.CrmContact on voipCalls.ContactId equals crmContact.Id into grouping
               from g in grouping.DefaultIfEmpty()
               select new CallContact { DbVoipCall = voipCalls, CrmContact = g };
    }

    private VoipPhone ToPhone(VoipNumber r)
    {
        return GetProvider().GetPhone(r);
    }

    public Consumer Consumer
    {
        get { return _consumerFactory.GetByKey("twilio"); }
    }

    public TwilioProvider GetProvider()
    {
        return new TwilioProvider(Consumer["twilioAccountSid"], Consumer["twilioAuthToken"], _authContext, _tenantUtil, _securityContext, _baseCommonLinkUtility);
    }

    public bool ConfigSettingsExist
    {
        get
        {
            return !string.IsNullOrEmpty(Consumer["twilioAccountSid"]) &&
                    !string.IsNullOrEmpty(Consumer["twilioAuthToken"]);
        }
    }
}

public class CallContact
{
    public DbVoipCall DbVoipCall { get; set; }
    public CrmContact CrmContact { get; set; }
}