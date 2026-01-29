using Microsoft.AspNetCore.Mvc;

namespace DossieImobiliario.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string TraceId => HttpContext.Items["TraceId"]?.ToString() ?? string.Empty;
    }
}
