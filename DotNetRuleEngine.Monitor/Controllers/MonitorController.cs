using System;
using System.Threading.Tasks;
using DotNetRuleEngine.Monitor.Models;
using DotNetRuleEngine.Monitor.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DotNetRuleEngine.Monitor.Controllers
{
    [Route("api/[controller]")]
    public class MonitorController : Controller
    {
        private readonly IModelRepository _modelRepository;

        public MonitorController(IModelRepository modelRepository)
        {
            _modelRepository = modelRepository;
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]RuleModel ruleModel)
        {
            Guid dotnetRuleEngineId;
            if (!Guid.TryParse(id, out dotnetRuleEngineId))
            {
                return BadRequest();
            }

            _modelRepository.Put(Guid.Parse(id), ruleModel);
            return NoContent();
        }


        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            Guid dotnetRuleEngineId;
            if (!Guid.TryParse(id, out dotnetRuleEngineId))
            {
                return BadRequest();
            }

            _modelRepository.Get(Guid.Parse(id));
            return NoContent();
        }
    }
}
