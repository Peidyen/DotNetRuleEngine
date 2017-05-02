using System;
using DotNetRuleEngine.Monitor.Converters;
using DotNetRuleEngine.Monitor.Models;
using DotNetRuleEngine.Monitor.Repositories;
using DotNetRuleEngine.Monitor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DotNetRuleEngine.Monitor.Controllers
{
    [Route("api/[controller]")]
    public class MonitorController : Controller
    {
        private readonly RuleEngineConverter _ruleEngineConverter;
        private readonly ModelConverter _modelConverter;
        private readonly RuleEngineService _ruleEngineService;

        public MonitorController(IModelRepository modelRepository, 
            RuleEngineConverter ruleEngineConverter,
            RuleEngineService ruleEngineService, ModelConverter modelConverter)
        {
            _ruleEngineConverter = ruleEngineConverter;
            _ruleEngineService = ruleEngineService;
            _modelConverter = modelConverter;
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]Model model)
        {
            Guid dotnetRuleEngineId;
            if (!Guid.TryParse(id, out dotnetRuleEngineId))
            {
                return BadRequest();
            }

            _ruleEngineService.Add(dotnetRuleEngineId, _modelConverter.Convert(model));
            
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

            var dotNetRuleEngineModel = _ruleEngineService.Get(dotnetRuleEngineId);

            return Ok(_ruleEngineConverter.Convert(dotNetRuleEngineModel));
        }
    }
}
