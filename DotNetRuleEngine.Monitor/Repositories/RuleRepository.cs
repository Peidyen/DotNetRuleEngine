using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRuleEngine.Monitor.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DotNetRuleEngine.Monitor.Repositories
{
    public class RuleRepository : IModelRepository
    {
        private readonly DotNetRuleEngineModelContext _context;

        public RuleRepository(DotNetRuleEngineModelContext context)
        {
            _context = context;
        }

        public void Put(Guid ruleEngineId, Model model)
        {
            RuleEngine dotnetRuleEngineModel = null;
            try
            {
                dotnetRuleEngineModel = _context.DotNetRuleEngineModel
                    .Include(rm => rm.RuleModels)
                    .SingleOrDefault(d => d.RuleEngineId == ruleEngineId);

                if (dotnetRuleEngineModel == null)
                {
                    dotnetRuleEngineModel = new RuleEngine
                    {
                        RuleEngineId = ruleEngineId,
                        RuleModels = new List<Model> {model}
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

        private void HandleDbUpdateException(Guid ruleEngineId, Model model, RuleEngine dotnetRuleEngineModel)
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

        public RuleEngine Get(Guid ruleEngineId)
        {
            return _context.DotNetRuleEngineModel
                .Include(rm => rm.RuleModels)
                .SingleOrDefault(d => d.RuleEngineId == ruleEngineId);
        }
    }
}