using System;
using MaintenanceSchedule.Data;
using System.Collections.Generic;
using yellowx.Framework.UnitWork;
using MaintenanceSchedule.Core.Web.Data;
using MaintenanceSchedule.Entity.Vienauto;
using MaintenanceSchedule.Library.Data.Caching;

namespace MaintenanceSchedule.Core.Queries.Vienauto
{
    public interface IGetAllManufacturerListQuery : IQuery<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse>
    { }

    public class GetAllManufacturerListQuery : IGetAllManufacturerListQuery
    {
        private readonly IBaseRepository _repository;
        private ICacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse> _cacheAllManufacture;

        public GetAllManufacturerListQuery(IBaseRepository repository)
        {
            _repository = repository;
        }

        private GetAllManufacturerListQueryResponse getAllManufacture(GetAllManufacturerListQueryRequest request)
        {
            try
            {
                IList<Manufacturer> manufacturers = null;

                using (var session = _repository.GetSession("nhibernate.vienauto_factory_key"))
                {
                    manufacturers = _repository.ListAll<Manufacturer>();
                }

                return new GetAllManufacturerListQueryResponse()
                {
                    Items = manufacturers,
                    ResponseStatus = GetAllManufacturerStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new GetAllManufacturerListQueryResponse()
                {
                    Exception = ex,
                    ResponseStatus = GetAllManufacturerStatus.Fail
                };
            }
        }

        public GetAllManufacturerListQueryResponse Invoke(GetAllManufacturerListQueryRequest request)
        {
            try
            {
                var result = new GetAllManufacturerListQueryResponse();
                var cache = new CacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse>();
                _cacheAllManufacture = (ICacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse>)cache;
                Func<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse> delegateGetAllManufacture = getAllManufacture;
                result = _cacheAllManufacture.Fetch("allManufacture", request, delegateGetAllManufacture, DateTime.Now.AddHours(4), null);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class GetAllManufacturerListQueryRequest
    {

    }

    public class GetAllManufacturerListQueryResponse : DataResponse<Manufacturer, GetAllManufacturerStatus>
    { }

    public enum GetAllManufacturerStatus
    {
        Success = 1,
        Fail
    }
}
