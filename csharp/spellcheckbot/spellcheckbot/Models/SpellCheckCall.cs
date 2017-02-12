using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace seesharpbot.Models
{

    public class SpellCheck
    {
        public string offset
        {
            get;
            set;
        }
        public string token
        {
            get;
            set;
        }
        public string suggestion
        {
            get;
            set;
        }
    }
    public class SpellCol
    {
        public SpellCheck spellcol
        {
            get;
            set;
        }
    }

}