using System;
using System.Text.RegularExpressions;

namespace Devil7.Android.SepolicyHelper.Utils
{
    public class Sepolicy
    {
        public static Models.SepolicyInfo GetSepolicy(string LogString)
        {
            Models.SepolicyInfo sepolicy = null;
            try
            {
                Models.SepolicyInfo sepolicyTmp = ReadLog(LogString);
                sepolicyTmp.Sepolicy = WriteSepolicy(sepolicyTmp);
                sepolicyTmp.Reference = LogString;
                if (!((sepolicyTmp.Source == "") | (sepolicyTmp.Target == "")))
                {
                    sepolicy = sepolicyTmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return sepolicy;
        }

        private static Models.SepolicyInfo ReadLog(string LogString)
        {
            Regex regex = new Regex(@".*: avc: denied \{ (?<action>.*) \} for .*scontext=.*:.*:(?<source>.*):s0.*tcontext=.*:.*:(?<target>.*):s0.*tclass=(?<class>.*) permissive=.*");
            GroupCollection groups = regex.Match(LogString).Groups;
            return new Models.SepolicyInfo(groups["action"].Value, groups["source"].Value, groups["target"].Value, groups["class"].Value);
        }

        private static string WriteSepolicy(Models.SepolicyInfo Info)
        {
            object[] args = new object[] { "{", "}", Info.Source, Info.Target, Info.TargetClass, Info.Action };
            return string.Format("allow {2} {3}:{4} {0} {5} {1};", args);
        }
    }
}

