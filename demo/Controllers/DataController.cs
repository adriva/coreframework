using System.Linq;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Memory;
using demo.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demo.Controllers
{
    public class DataController : Controller
    {
        private readonly JarrtDbContext DbContext;
        private readonly ICache Cache;

        public DataController(JarrtDbContext dbContext, ICache<InMemoryCache> cacheWrapper)
        {
            this.DbContext = dbContext;
            this.Cache = cacheWrapper.Instance;
        }

        public async Task<IActionResult> Promotions(int pageNumber = 1, int pageSize = 10)
        {
            var results = await this.DbContext.Promotions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return this.StatusCode(200, new PagedResult<Promotion>(results, 1, 5, 1000));
        }
    }
}