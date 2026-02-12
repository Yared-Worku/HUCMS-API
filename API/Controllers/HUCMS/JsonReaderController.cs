using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace HU_api.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class JsonReaderController : ControllerBase
    {
        private readonly string _formsDir;

        public JsonReaderController(IConfiguration config)
        {
            _formsDir = config["FileSettings:FormsDirectory"];
        }

        [HttpGet("{formName}")]
        public IActionResult GetFormData(string formName)
        {
            if (!formName.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
                formName += ".json";

            var formPath = Path.Combine(_formsDir, formName);
            if (!System.IO.File.Exists(formPath))
                return NotFound(new { error = "Form not found" });

            var formContent = System.IO.File.ReadAllText(formPath);

            return Ok(new
            {
                json = formContent
            });
        }
    }
}
