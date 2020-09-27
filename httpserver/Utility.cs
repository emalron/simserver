using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace httpserver
{
    public class Utility
    {
        public static byte[] Parser(string path, Dictionary<string, string> patterns)
        {
            string text = getText(path);
            foreach(var p in patterns)
            {
                string pattern = string.Format(@"{0}.({1}){2}", "{{", p.Key, "}}");
                text = Regex.Replace(text, pattern, p.Value);
            }
            return Encoding.UTF8.GetBytes(text);
        }

        static string getText(string path)
        {
            string result = null;
            try
            {
                result = File.ReadAllText(path);
            } catch(Exception e)
            {
                return e.Message;
            }

            return result;
        }
    }
}
