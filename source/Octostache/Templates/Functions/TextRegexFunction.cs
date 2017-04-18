using System;
using System.Text.RegularExpressions;

namespace Octostache.Templates.Functions
{
    internal class TextRegexFunction
    {
        public static string Replace(string argument, string[] options)
        {
            if (argument == null)
                return null;

            if (options.Length <= 1)
                return null;

            var inputString = options[0];
            var replacementString = options[1];

            try
            {
                if (inputString == null || replacementString == null)
                {
                    return null;
                }

                if (options.Length == 2)
                {
                    return Regex.Replace(argument, inputString, replacementString);
                }
            }
            catch (ArgumentNullException) { }
            catch (RegexMatchTimeoutException) { }
            return null;
        }
    }
}
