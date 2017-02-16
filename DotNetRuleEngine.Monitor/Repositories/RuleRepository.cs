using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRuleEngine.Monitor.Models;
using Microsoft.EntityFrameworkCore;


namespace DotNetRuleEngine.Monitor.Repositories
{
    public class RuleRepository : IModelRepository
    {
        private readonly DotNetRuleEngineModelContext _context;

        public RuleRepository(DotNetRuleEngineModelContext context)
        {
            _context = context;
        }

        public void Put(Guid ruleEngineId, RuleModel model)
        {
            DotNetRuleEngineModel dotnetRuleEngineModel = null;
            try
            {
                dotnetRuleEngineModel = _context.DotNetRuleEngineModel
                    .Include(rm => rm.RuleModels)
                    .SingleOrDefault(d => d.RuleEngineId == ruleEngineId);

                if (dotnetRuleEngineModel == null)
                {
                    dotnetRuleEngineModel = new DotNetRuleEngineModel
                    {
                        RuleEngineId = ruleEngineId,
                        RuleModels = new List<RuleModel> {model}
                    };

                    _context.DotNetRuleEngineModel.Add(dotnetRuleEngineModel);
                }
                else
                {
                    dotnetRuleEngineModel.RuleModels.Add(model);
                }

                _context.RuleModel.Add(model);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                HandleDbUpdateException(ruleEngineId, model, dotnetRuleEngineModel);
            }
            catch (DbUpdateException)
            {
                HandleDbUpdateException(ruleEngineId, model, dotnetRuleEngineModel);
            }
        }

        private void HandleDbUpdateException(Guid ruleEngineId, RuleModel model, DotNetRuleEngineModel dotnetRuleEngineModel)
        {
            if (dotnetRuleEngineModel != null &&
                _context.Entry(dotnetRuleEngineModel).State == EntityState.Added)
            {
                _context.Remove(dotnetRuleEngineModel);

                if (_context.Entry(model).State == EntityState.Added)
                {
                    _context.Remove(model);
                }

                dotnetRuleEngineModel = Get(ruleEngineId);

                dotnetRuleEngineModel.RuleModels.Add(model);

                _context.RuleModel.Add(model);
                _context.SaveChanges();
            }
        }

        public DotNetRuleEngineModel Get(Guid ruleEngineId)
        {
            return _context.DotNetRuleEngineModel
                .Include(rm => rm.RuleModels)
                .SingleOrDefault(d => d.RuleEngineId == ruleEngineId);
        }
    }
}