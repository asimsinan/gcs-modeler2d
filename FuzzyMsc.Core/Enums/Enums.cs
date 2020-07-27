namespace FuzzyMsc.Core.Enums
{
    public class Enums
    {
        public enum Roles
        {
            Admin = 1
        }

        public enum Operator
        {
            And = 1,
            Or = 2
        }

        public enum VariableType
        {
            Input = 1,
            Output = 2
        }

        public enum SheetType
        {
            Resistivity = 1,
            Seismic = 2,
            Drilling = 3
        }

        public enum Direction
        {
            Left = 1,
            Right = 2
        }

        public enum ExcelDataType
        {
            Real = 1,
            Artificial = 2
        }
    }
}
