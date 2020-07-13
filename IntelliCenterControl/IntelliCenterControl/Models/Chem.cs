using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using IntelliCenterControl.Annotations;

namespace IntelliCenterControl.Models
{
    public class Chem : Circuit<IntelliCenterConnection>
    {
        public const string ChemKeys = "[\"SALT\"]";

        public enum ChemType
        {
            [Display(Name = "IntelliChem")]
            ICHEM,
            [Display(Name = "IntelliChlor")]
            ICHLOR
        }

        private ChemType _type;

        public ChemType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        private string _salt = "-";

        public string Salt
        {
            get => _salt;
            set
            {
                _salt = value;
                OnPropertyChanged();
            }
        }



        public Chem(string name, ChemType chemType) : base(name, CircuitType.CHEM)
        {
            Type = chemType;
        }

    }
}
