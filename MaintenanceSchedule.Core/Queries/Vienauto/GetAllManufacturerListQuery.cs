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
        private const string cacheKey = "ALL_MANUFACTURER_CACHE";

        private readonly IBaseRepository _repository;
        private ICacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse> _cacheProvider;

        public GetAllManufacturerListQuery(IBaseRepository repository)
        {
            _repository = repository;
            _cacheProvider = new CacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse>();
        }

        public GetAllManufacturerListQueryResponse Invoke(GetAllManufacturerListQueryRequest request)
        {
            try
            {
                var result = new GetAllManufacturerListQueryResponse();
                var cache = new CacheProvider<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse>();
                Func<GetAllManufacturerListQueryRequest, GetAllManufacturerListQueryResponse> getAllManufacturer = GetAllManufacturerFromDB;
                result = _cacheProvider.Fetch(cacheKey, request, getAllManufacturer, null, TimeSpan.FromHours(4));
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private GetAllManufacturerListQueryResponse GetAllManufacturerFromDB(GetAllManufacturerListQueryRequest request)
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
