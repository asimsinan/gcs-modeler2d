using FuzzyMsc.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto
{
	public class SessionDTO
	{

		public Rule rule { get; set; }
		public List<RuleListText> rules { get; set; }
		public Variable resistivityVariable { get; set; }
		public List<VariableItem> resistivityItem { get; set; }
		public Variable groundVariable { get; set; }
		public List<VariableItem> groundItem { get; set; }
		public List<RuleList> ruleList { get; set; }
		public List<RuleListItem> ruleListItem { get; set; }

		public List<Variable> variables { get; set; }
		public List<VariableItem> variableItems { get; set; }
	}
}
