using System;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Transform;
using System.Threading.Tasks;
using System.Collections.Generic;
using yellowx.Framework.Data.Paging;
using MaintenanceSchedule.Entity.Datlichbaoduong;
using MaintenanceSchedule.Library.Data.Caching;

namespace MaintenanceSchedule.Data.Repositories.Datlichbaoduong
{
    public interface IServiceRepository : IBaseRepository
    {
        Service GetService(int serviceId);
        Service GetNextService(int serviceId);
        Service GetLastService(int serviceId);
        IList<Service> GetLatestServices();
        PagingResult<Service> ListServices(Paging paging);
        Dictionary<int, IList<Service>> ListAdminServices(int pageIndex, int pageSize);
    }

    public class ServiceRepository : BaseRepository, IServiceRepository
    {
        private ICacheProvider<Paging, PagingResult<Service>> _cacheListService;
        private ICacheProvider<Int32, Service> _cacheService;
                
        private Service _getService(Int32 serviceId)
        {
            try
            {
                using (var session = GetSession())
                {
                    return Get<Service>(serviceId);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Service GetService(int serviceId)
        {
            try
            {
                var result = new Service();
                var cache = new CacheProvider<Int32, Service>();
                _cacheService = (ICacheProvider<Int32, Service>)cache;
                Func<Int32, Service> delegateGetService = _getService;
                result = _cacheService.Fetch(serviceId.ToString(), serviceId, delegateGetService, DateTime.Now.AddHours(4), null);
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public Service GetNextService(int serviceId)
        {
            try
            {
                using (var session = GetSession())
                {
                    return First<Service>(s => s.Id > serviceId);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Service GetLastService(int serviceId)
        {
            try
            {
                using (var session = GetSession())
                {
                    //return Last<Service>(s => s.Id > serviceId);
                    return List<Service>(s => s.Id < serviceId).OrderByDescending(s => s.Id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IList<Service> GetLatestServices()
        {
            try
            {
                using (var session = GetSession())
                {
                    var services = List<Service>(s => s.EntryDate <= DateTime.Now && s.EntryDate >= DateTime.Now.AddMonths(-1))
                                                .OrderByDescending(s => s.EntryDate).Take(8).ToList();

                    return services;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private PagingResult<Service> _getListService(Paging paging)
        {
            var result = new PagingResult<Service>();

            try
            {
                using (var session = GetSession())
                {                    
                    var query = session.QueryOver<Service>();
                    var items = query.Skip(paging.Index * paging.Size)
                                       .Take(paging.Size)
                                       .List();
                    var count = query.RowCount();
                    result.Items = items;
                    result.TotalCount = count;
                }
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }
            return result;
        }

        public PagingResult<Service> ListServices(Paging paging) 
        {
            try
            {
                var result = new PagingResult<Service>();
                Func<Paging, PagingResult<Service>> delegateGetListSer = _getListService;
                var cache = new CacheProvider<Paging, PagingResult<Service>>();
                _cacheListService = (ICacheProvider<Paging, PagingResult<Service>>)cache;                
                result = _cacheListService.Fetch(paging.Index.ToString(), paging, delegateGetListSer, DateTime.Now.AddHours(4), null);
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public Dictionary<int, IList<Service>> ListAdminServices(int pageIndex, int pageSize)
        {
            try
            {
                using (var session = GetSession())
                {
                    var services = ListPage<Service>(pageIndex, pageSize);

                    return new Dictionary<int, IList<Service>>
                    {
                        { Count<Service>(), services }
                    };
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
