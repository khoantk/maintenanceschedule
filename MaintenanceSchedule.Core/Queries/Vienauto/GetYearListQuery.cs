using System;
using NHibernate.Transform;
using MaintenanceSchedule.Data;
using yellowx.Framework.UnitWork;
using System.Collections.Generic;
using MaintenanceSchedule.Core.Web.Data;
using MaintenanceSchedule.Entity.Vienauto;
using MaintenanceSchedule.Core.Queries.Vienauto.Dto;
using MaintenanceSchedule.Library.Data.Caching;

namespace MaintenanceSchedule.Core.Queries.Vienauto
{
    public interface IGetYearListQuery : IQuery<GetYearListQueryRequest, GetYearListQueryResponse>
    { }

    public class GetYearListQuery : IGetYearListQuery
    {
        private readonly IBaseRepository _repository;
        private ICacheProvider<GetYearListQueryRequest, GetYearListQueryResponse> _cacheYearList;

        public GetYearListQuery(IBaseRepository repository)
        {
            _repository = repository;
        }

        private GetYearListQueryResponse getYearByModel(GetYearListQueryRequest request)
        {
            try
            {
                Year yearAlias = null;
                Model modelAlias = null;
                YearDto yearDto = null;

                using (var session = _repository.GetSession("nhibernate.vienauto_factory_key"))
                {
                    var yearDtos = session.QueryOver<Year>(() => yearAlias)
                                  .JoinAlias(year => year.Model, () => modelAlias)
                                  .Where(() => modelAlias.Id == request.ModelId)
                                  .SelectList(list => list
                                        .SelectGroup(() => yearAlias.Id).WithAlias(() => yearDto.YearId)
                                        .SelectGroup(() => yearAlias.Name).WithAlias(() => yearDto.YearName))
                                  .TransformUsing(Transformers.AliasToBean<YearDto>())
                                  .List<YearDto>();

                    var years = new List<Year>();
                    foreach (var dto in yearDtos)
                        years.Add(dto.ToYear());

                    return new GetYearListQueryResponse()
                    {
                        Items = years,
                        ResponseStatus = GetYearStatus.Success
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetYearListQueryResponse()
                {
                    Exception = ex,
                    ResponseStatus = GetYearStatus.Fail
                };
            }
        }

        public GetYearListQueryResponse Invoke(GetYearListQueryRequest request)
        {
            try
            {
                var result = new GetYearListQueryResponse();
                var cache = new CacheProvider<GetYearListQueryRequest, GetYearListQueryResponse>();
                _cacheYearList = (ICacheProvider<GetYearListQueryRequest, GetYearListQueryResponse>)cache;
                Func<GetYearListQueryRequest, GetYearListQueryResponse> delegateGetYearList = getYearByModel;
                result = _cacheYearList.Fetch(request.ModelId.ToString(), request, delegateGetYearList, DateTime.Now.AddHours(4), null);
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }

    public class GetYearListQueryRequest
    {
        public int ModelId { get; set; }
    }

    public class GetYearListQueryResponse : DataResponse<Year, GetYearStatus>
    { }

    public enum GetYearStatus
    {
        Success = 1,
        Fail
    }
}
