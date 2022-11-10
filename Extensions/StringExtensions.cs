using System.Globalization;
using System.Text.RegularExpressions;

namespace StarLink_Blog.Extensions
{
    public static class StringExtensions
    {
        public static string Slugify(this string phrase)
        {
            // remove all accents and make the string lower case 
            string output = phrase.RemoveAccents().ToLower();

            //Remove Special chars
            output = Regex.Replace(output, @"[^A-Za-z0-9\s]", "");

            //remove all additional spaces in favour of one 
            output = Regex.Replace(output, @"\s+", " ").Trim();

            // replace all spaces with the hyphen 

            output = Regex.Replace(output, @"\s", "-");

            // return the slug.
            return output;
        }

        private static string RemoveAccents(this string phrase)
        {

            if(string.IsNullOrWhiteSpace(phrase))
            {
                return phrase;
            }

            // convert for unicode 
            phrase = phrase.Normalize(System.Text.NormalizationForm.FormD);

            // format unicode/ascii
            char[] chars = phrase.Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                                      != UnicodeCategory.NonSpacingMark).ToArray();

            //convert and return the new phrase 
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
