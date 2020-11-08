using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using demo.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demo.Controllers
{
    public class DataController : Controller
    {
        private readonly JarrtDbContext DbContext;
        private readonly ICache Cache;

        public DataController(JarrtDbContext dbContext, ICache cache)
        {
            this.DbContext = dbContext;
            this.Cache = cache;
        }

        public async Task<IActionResult> Promotions(int pageNumber = 1, int pageSize = 10)
        {
            var results = await this.DbContext.Promotions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return this.StatusCode(200, new PagedResult<Promotion>(results, 1, 5, 1000));
        }
    }
}