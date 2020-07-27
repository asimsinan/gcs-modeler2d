using FuzzyMsc.Bll.Interface;
using FuzzyMsc.Dto.FuzzyDTOS;
using System;
using System.Collections.Generic;

namespace FuzzyMsc.Bll
{
    public class InputManager : IInputManager
    {
        public List<VariableDTO> Resistance
        {
            get
            {
                var list = new List<VariableDTO>();
                list.Add(new VariableDTO { VariableName = "CokGevsek", VisibleName = "Çok Gevşek", MinValue = 0, MaxValue = 200 });
                list.Add(new VariableDTO { VariableName = "Gevsek", VisibleName = "Gevşek", MinValue = 200, MaxValue = 300 });
                list.Add(new VariableDTO { VariableName = "Orta", VisibleName = "Orta", MinValue = 300, MaxValue = 500 });
                list.Add(new VariableDTO { VariableName = "Siki", VisibleName = "Sıkı", MinValue = 500, MaxValue = 700 });
                list.Add(new VariableDTO { VariableName = "Kaya", VisibleName = "Kaya", MinValue = 700, MaxValue = Double.MaxValue });
                return list;
            }
            set { }
        }
        public List<VariableDTO> Saturation
        {
            get
            {
                var list = new List<VariableDTO>();
                list.Add(new VariableDTO { VariableName = "GazaDoygun", VisibleName = "Gaza Doygun", MinValue = 0, MaxValue = 2 });
                list.Add(new VariableDTO { VariableName = "GazaVeSuyaDoygun", VisibleName = "Gaza Ve Suya Doygun", MinValue = 2, MaxValue = 4 });
                list.Add(new VariableDTO { VariableName = "SuyaDoygun", VisibleName = "SuyaDoygun", MinValue = 4, MaxValue = Double.MaxValue });
                return list;
            }
            set { }
        }

        public List<ConstantDTO> OperatorList {
            get {
                var list = new List<ConstantDTO>();
                list.Add(new ConstantDTO { Text = " and ", Value = 1 });
                list.Add(new ConstantDTO { Text = " or ", Value = 2 });
                return list;
            } set { }
        }
        public List<ConstantDTO> ResistanceList {
            get
            {
                var list = new List<ConstantDTO>();
                list.Add(new ConstantDTO { Text = "CokGevsek", Value = 1 });
                list.Add(new ConstantDTO { Text = "Gevsek", Value = 2 });
                list.Add(new ConstantDTO { Text = "Orta", Value = 3 });
                list.Add(new ConstantDTO { Text = "Siki", Value = 4 });
                list.Add(new ConstantDTO { Text = "Kaya", Value = 5 });
                return list;
            }
            set { }
        }
        public List<ConstantDTO> SaturationList {
            get
            {
                var list = new List<ConstantDTO>();
                list.Add(new ConstantDTO { Text = "SuyaDoygun", Value = 1 });
                list.Add(new ConstantDTO { Text = "SuyaVeGazaDoygun", Value = 2 });
                list.Add(new ConstantDTO { Text = "GazaDoygun", Value = 3 });
                return list;
            }
            set { }
        }
        public List<ConstantDTO> EqualityList {
            get
            {
                var list = new List<ConstantDTO>();
                list.Add(new ConstantDTO { Text = " is ", Value = 1 });
                list.Add(new ConstantDTO { Text = " is not ", Value = 2 });
                return list;
            }
            set { }
        }

        public byte[] StringToByteArray(string value)
        {
            char[] charArr = value.ToCharArray();
            byte[] bytes = new byte[charArr.Length];
            for (int i = 0; i < charArr.Length; i++)
            {
                byte current = Convert.ToByte(charArr[i]);
                bytes[i] = current;
            }

            return bytes;
        }
    }

    public interface IInputManager : IBaseManager
    {
        List<VariableDTO> Resistance { get; set; }
        List<VariableDTO> Saturation { get; set; }
        
        List<ConstantDTO> OperatorList { get; set; }
        List<ConstantDTO> ResistanceList { get; set; }
        List<ConstantDTO> SaturationList { get; set; }
        List<ConstantDTO> EqualityList { get; set; }

        byte[] StringToByteArray(string value);
    }
}
