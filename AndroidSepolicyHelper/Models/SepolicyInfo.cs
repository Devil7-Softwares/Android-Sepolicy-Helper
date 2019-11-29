using System.ComponentModel;

namespace Devil7.Android.SepolicyHelper.Models
{
    public class SepolicyInfo
    {
        #region Constructor
        public SepolicyInfo(string Action, string Source, string Target, string TargetClass)
        {
            this.Action = Action;
            this.Source = Source;
            this.Target = Target;
            this.TargetClass = TargetClass;
        }
        #endregion

        #region Properties
        [Browsable(false)]
        public string Action { get; set; }

        [Browsable(false)]
        public string Reference { get; set; }

        public string Sepolicy { get; set; }

        [Browsable(false)]
        public string Source { get; set; }

        [Browsable(false)]
        public string Target { get; set; }

        [Browsable(false)]
        public string TargetClass { get; set; }
        #endregion
    }
}

