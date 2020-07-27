using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Core.Enums;
using FuzzyMsc.Dto;
using FuzzyMsc.Dto.FuzzyDTOS;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.FuzzyLibrary;
using FuzzyMsc.Pattern.UnitOfWork;
using FuzzyMsc.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FuzzyMsc.Bll
{
    public class FuzzyManager : IFuzzyManager
    {

        IUnitOfWorkAsync _unitOfWork;
        IInputManager _inputManager;
        IUserService _userService;
        IRuleService _ruleService;
        IRuleListService _ruleListService;
        IRuleListItemService _ruleListItemService;
        IRuleListTextService _ruleListTextService;
        IVariableService _variableService;
        IVariableItemService _variableItemService;
        MamdaniFuzzySystem _fsGround = null;

        public FuzzyManager(
            IUnitOfWorkAsync unitOfWork,
            IUserService userService,
            IInputManager inputManager,
            IRuleService ruleService,
            IRuleListService ruleListService,
            IRuleListItemService ruleListItemService,
            IRuleListTextService ruleListTextService,
            IVariableService variableService,
            IVariableItemService variableItemService)
        {
            _unitOfWork = unitOfWork;
            _inputManager = inputManager;
            _userService = userService;
            _ruleService = ruleService;
            _ruleListService = ruleListService;
            _ruleListTextService = ruleListTextService;
            _variableService = variableService;
            _variableItemService = variableItemService;
            _ruleListItemService = ruleListItemService;
        }

        public ResultDTO SaveSet(RuleSetDTO ruleSet)
        {
            ResultDTO result = new ResultDTO();
           // var resistivity = EditVisibleName(ruleSet.ResistivityList);
           // var ground = EditVisibleName(ruleSet.GroundList);
            var resistivity =ruleSet.ResistivityList;
            var ground = ruleSet.GroundList;

            #region Database 

            try
            {
                _unitOfWork.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                #region Fuzzy Rule
                Rule rule = new Rule
                {
                    ruleName = ruleSet.SetName,
                    isActive = true,
                    addDate = DateTime.Now
                };
                _ruleService.BulkInsert(rule);
                #endregion

                #region RuleListText
                List<RuleListText> rules = new List<RuleListText>();
                foreach (var RuleListItem in ruleSet.RuleList)
                {
                    string ruleText = CreateRule(RuleListItem);
                    rules.Add(new RuleListText { ruleID = rule.ruleID, ruleText = ruleText });
                }
                _ruleListTextService.BulkInsertRange(rules);
                #endregion

                #region Input Variable
                Variable resistivityVariable = new Variable
                {
                    ruleID = rule.ruleID,
                    variableTypeID = (byte)Enums.VariableType.Input,
                    variableName = "Resistivity",
                    visibleVariableName = "Resistivity"
                };
                _variableService.BulkInsert(resistivityVariable);
                var resistivityItem = (from a in resistivity
                                    select new VariableItem()
                                    {
                                        variableID = resistivityVariable.variableID,
                                        variableItemName = a.VariableName,
                                        variableItemVisibleName = a.VisibleName,
                                        minValue = a.MinValue,
                                        maxValue = a.MaxValue
                                    });
                _variableItemService.BulkInsertRange(resistivityItem);
                #endregion

                #region Output Variable
                Variable groundVariable = new Variable
                {
                    ruleID = rule.ruleID,
                    variableTypeID = (byte)Enums.VariableType.Output,
                    variableName = "Ground",
                    visibleVariableName = "Ground"
                };
                _variableService.BulkInsert(groundVariable);
                var groundItem = (from a in ground
                                  select new VariableItem()
                                  {
                                      variableID = groundVariable.variableID,
                                      variableItemName = a.VariableName,
                                      variableItemVisibleName = a.VisibleName,
                                      minValue = a.MinValue,
                                      maxValue = a.MaxValue
                                  });
                _variableItemService.BulkInsertRange(groundItem);
                #endregion



                #region RuleList
                List<RuleListItem> ruleListItem = new List<RuleListItem>();
                for (int i = 0; i < ruleSet.RuleList.Count; i++)
                {
                    var ruleList = (new RuleList { ruleID = rule.ruleID, orderNumber = (byte)(i + 1) });
                    _ruleListService.BulkInsert(ruleList);

                    foreach (var item in ruleSet.RuleList)
                    {
                        var InputVariableID = _variableItemService.Queryable().FirstOrDefault(d => d.variable.variableTypeID == (byte)Enums.VariableType.Input && d.variableItemName == item.FuzzyRule.Resistivity).variableItemID;
                        ruleListItem.Add(new RuleListItem { ruleListID = ruleList.ruleListID, variableItemID = InputVariableID });

                        var OutputVariableID = _variableItemService.Queryable().FirstOrDefault(d => d.variable.variableTypeID == (byte)Enums.VariableType.Output && d.variableItemName == item.FuzzyRule.Ground).variableItemID;
                        ruleListItem.Add(new RuleListItem { ruleListID = ruleList.ruleListID, variableItemID = InputVariableID });
                    }
                }
                _ruleListItemService.BulkInsertRange(ruleListItem);
                #endregion

                _unitOfWork.Commit();
                result.Success = true;
                result.Message = "Rule set successfully added";
                result.ResultObject = null;
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                result.Success = false;
                result.Message = "Error while adding rule set" + ex.Message;
                result.ResultObject = null;
                return result;
            }

            #endregion
        }

        public string CreateFuzzyRuleAndReturnResult(FetchRuleDTO rules, double inputValue)
        {

            FuzzySystemResultDTO system = new FuzzySystemResultDTO();
            system = CreateSystem(rules, inputValue);
            _fsGround = system.System;
            inputValue = system.InputValue;


            FuzzyVariable fvInput = _fsGround.InputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Input).visibleVariableName);
            FuzzyVariable fvOutput = _fsGround.OutputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Output).visibleVariableName);

            Dictionary<FuzzyVariable, double> inputValues = new Dictionary<FuzzyVariable, double>();
            inputValues.Add(fvInput, inputValue);

            Dictionary<FuzzyVariable, double> result = _fsGround.Calculate(inputValues);
            _fsGround.DefuzzificationMethod = DefuzzificationMethod.Centroid;

            double outputValue = result[fvOutput];
            string outputType = ReturnLimitNearbyResults(rules, outputValue);

            return outputType;

                      
        }

        public bool CreateFuzzyRuleAndCompare(FetchRuleDTO rules, double inputValue1, double inputValue2, int ratio)
        {
            FuzzySystemResultDTO system = new FuzzySystemResultDTO();
            system = CreateSystem(rules, inputValue1);
            _fsGround = system.System;
            inputValue1 = system.InputValue;

            FuzzyVariable fvInput1 = _fsGround.InputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Input).visibleVariableName);
            FuzzyVariable fvOutput1 = _fsGround.OutputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Output).visibleVariableName);

            Dictionary<FuzzyVariable, double> inputValues1 = new Dictionary<FuzzyVariable, double>();
            inputValues1.Add(fvInput1, inputValue1);

            Dictionary<FuzzyVariable, double> result1 = _fsGround.Calculate(inputValues1);
            _fsGround.DefuzzificationMethod = DefuzzificationMethod.Bisector;

            double outputValue1 = result1[fvOutput1];

            _fsGround = null;
            system = CreateSystem(rules, inputValue2);
            _fsGround = system.System;
            inputValue2 = system.InputValue;

            FuzzyVariable fvInput2 = _fsGround.InputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Input).visibleVariableName);
            FuzzyVariable fvOutput2 = _fsGround.OutputByName(rules.VariableList.FirstOrDefault(d => d.variableTypeID == (byte)Enums.VariableType.Output).visibleVariableName);

            Dictionary<FuzzyVariable, double> inputValues2 = new Dictionary<FuzzyVariable, double>();
            inputValues2.Add(fvInput2, inputValue2);

            Dictionary<FuzzyVariable, double> result2 = _fsGround.Calculate(inputValues2);
            _fsGround.DefuzzificationMethod = DefuzzificationMethod.Centroid;

            double outputValue2 = result2[fvOutput2];

            var result = ReturnNearbyResults(outputValue1, outputValue2, ratio);

            return result;
        }

        private FuzzySystemResultDTO CreateSystem(FetchRuleDTO rules, double inputValue)
        {
            FuzzySystemResultDTO result = new FuzzySystemResultDTO();
            MamdaniFuzzySystem fsGround = new MamdaniFuzzySystem();

            foreach (var variable in rules.VariableList)
            {
                if (variable.variableTypeID == (byte)Enums.VariableType.Input)
                {

                    FuzzyVariable fvInput = new FuzzyVariable(variable.visibleVariableName, 0.0, 1000.0);
                    var variableItems = rules.VariableItemList.Where(k => k.variableID == variable.variableID).ToList();
                    for (int i = 0; i < variableItems.Count; i++)
                    {
                        if (inputValue == variableItems[i].minValue)
                        {
                            inputValue++;
                        }
                        double maxValue;
                        if (i != variableItems.Count - 1)
                        {
                            if (variableItems[i].maxValue == variableItems[i + 1].minValue)
                                maxValue = variableItems[i].maxValue - 1;
                            else
                                maxValue = variableItems[i].maxValue;
                        }
                        else
                            maxValue = variableItems[i].maxValue;

                        fvInput.Terms.Add(new FuzzyTerm(variableItems[i].variableItemVisibleName, new TriangularMembershipFunction(variableItems[i].minValue, (variableItems[i].minValue + variableItems[i].maxValue) / 2, maxValue)));
                    }
                    fsGround.Input.Add(fvInput);
                }
                else
                {
                    FuzzyVariable fvOutput = new FuzzyVariable(variable.visibleVariableName, 0.0, 1000.0);
                    var variableItems = rules.VariableItemList.Where(k => k.variableID == variable.variableID).ToList();
                    for (int i = 0; i < variableItems.Count; i++)
                    {
                        double maxValue;
                        if (i != variableItems.Count - 1)
                        {
                            if (variableItems[i].maxValue == variableItems[i + 1].minValue)
                                maxValue = variableItems[i].maxValue - 1;
                            else
                                maxValue = variableItems[i].maxValue;
                        }
                        else
                            maxValue = variableItems[i].maxValue;

                        fvOutput.Terms.Add(new FuzzyTerm(variableItems[i].variableItemVisibleName, new TriangularMembershipFunction(variableItems[i].minValue, (variableItems[i].minValue + variableItems[i].maxValue) / 2, maxValue)));
                    }
                    fsGround.Output.Add(fvOutput);
                }
            }

            foreach (var ruleText in rules.RuleListText)
            {
                MamdaniFuzzyRule rule = fsGround.ParseRule(ruleText.ruleText);
                fsGround.Rules.Add(rule);
            }

            result.System = fsGround;
            result.InputValue = inputValue;
            return result;
        }

        public void CreateRulesFLS(RuleSetDTO ruleSet)
        {
            
            throw new System.NotImplementedException();
        }

        public double Test(double value1, double value2, double value3)
        {

         
            return 0;
        }

        private List<VariableDTO> EditVisibleName(List<VariableDTO> variable)
        {
            string TrChar = "ığüşöçĞÜŞİÖÇ";
            string EnChar = "igusocGUSIOC";
            foreach (var item in variable)
            {
                item.VisibleName = item.VariableName.Replace(" ", "");
                for (int i = 0; i < TrChar.Length; i++)
                {
                    item.VisibleName = item.VisibleName.Replace(TrChar[i], EnChar[i]);
                }
            }
            return variable;
        }

        private string CreateRule(RuleListDTO ruleList)
        {
            var resistivity = TurkishChar(ruleList.FuzzyRule.Resistivity);
            var ground = TurkishChar(ruleList.FuzzyRule.Ground);

            return "if (Ozdirenc is " + resistivity + ") then (Toprak is " + ground + ")";

        }

        private string TurkishChar(string text)
        {
            string TrChar = "ığüşöçĞÜŞİÖÇ";
            string EnChar = "igusocGUSIOC";
            for (int i = 0; i < TrChar.Length; i++)
            {
                text = text.Replace(TrChar[i], EnChar[i]);
            }

            return text.Replace(" ", "");
        }

        public FetchRuleDTO FetchRule(long ruleID)
        {
            FetchRuleDTO fetchRule = new FetchRuleDTO();
            List<VariableItem> variableItemList = new List<VariableItem>();
            var rule = _ruleService.Queryable().FirstOrDefault(k => k.ruleID == ruleID && k.isActive == true);
            var ruleListText = rule.ruleListTexts.ToList();
            var variables = rule.variables.ToList();
            foreach (var item in variables)
            {
                var variableItems = _variableItemService.Queryable().Where(d => d.variableID == item.variableID).ToList();
                variableItemList.AddRange(variableItems);
            }

            fetchRule.FuzzyRule = rule;
            fetchRule.RuleListText = ruleListText;
            fetchRule.VariableList = variables;
            fetchRule.VariableItemList = variableItemList;

            return fetchRule;
        }

        private string ReturnLimitNearbyResults(FetchRuleDTO rules, double outputValue)
        {
            string result = "";

            var variableID = rules.VariableList.FirstOrDefault(dl => dl.variableTypeID == (byte)Enums.VariableType.Output).variableID;
            var OutputList = _variableItemService.Queryable().Where(d => d.variableID == variableID).ToList();

            for (int i = 0; i < OutputList.Count; i++)
            {
                if (i == OutputList.Count - 1)
                {
                    //if (outputValue >= OutputList[i].MinDeger && outputValue <= OutputList[i].MaxDeger)
                    //{
                    result = OutputList[i].variableItemName;
                    break;
                    //}                    
                }
                else
                {
                    if (OutputList[i].maxValue > OutputList[i + 1].minValue) //Bir sonraki tanım aralığı ile kesişimi var demektir
                    {
                        if (outputValue <= OutputList[i].maxValue && outputValue >= OutputList[i + 1].minValue)
                        {
                            result = Math.Abs(outputValue - OutputList[i].maxValue) > Math.Abs(outputValue - OutputList[i + 1].minValue) ? OutputList[i].variableItemName : OutputList[i + 1].variableItemName;
                            break;
                        }
                    }
                    else
                    {
                        if (outputValue >= OutputList[i].minValue && outputValue <= OutputList[i].maxValue)
                        {
                            result = OutputList[i].variableItemName;
                            break;
                        }
                    }
                }
            }


            return result;
        }

        private bool ReturnNearbyResults(double outputValue1, double outputValue2, int ratio)
        {
            if (outputValue1 > outputValue2)
            {
                if (outputValue1 * ratio / 100 > outputValue2)
                {
                    return false;
                }
            }
            else if (outputValue2 > outputValue1)
            {
                if (outputValue2 * ratio / 100 > outputValue1)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            return true;
        }
    }

    public interface IFuzzyManager : IBaseManager
    {
        double Test(double value1, double value2, double value3);

        void CreateRulesFLS(RuleSetDTO ruleSet);

        string CreateFuzzyRuleAndReturnResult(FetchRuleDTO rules, double inputValue);

        bool CreateFuzzyRuleAndCompare(FetchRuleDTO rules, double inputValue1, double inputValue2, int ratio);

        ResultDTO SaveSet(RuleSetDTO ruleSet);

        FetchRuleDTO FetchRule(long ruleID);
    }
}
