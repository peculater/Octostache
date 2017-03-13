﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Octostache.Tests
{
    public class FiltersFixture : BaseFixture
    {
        [Theory]
        [InlineData("#{foo | ToUpper}")]
        [InlineData("#{Foo.Bar | HtmlEscape}")]
        [InlineData("#{Foo.Bar | ToUpper}")]
        [InlineData("#{Foo.Bar | Markdown}")]
        public void UnmatchedSubstitutionsAreEchoed(string template)
        {
            string error;
            var result = new VariableDictionary().Evaluate(template, out error);
            result.Should().Be(template);
            error.Should().Be($"The following tokens were unable to be evaluated: '{template}'");
        }

        [Fact]
        public void UnknownFiltersAreEchoed()
        {
            var result = Evaluate("#{Foo | ToBazooka}", new Dictionary<string, string> { { "Foo", "Abc" } });
            result.Should().Be("#{Foo | ToBazooka}");
        }

        [Fact]
        public void UnknownFiltersWithOptionsAreEchoed()
        {
            var result = Evaluate("#{Foo | ToBazooka 6}", new Dictionary<string, string> { { "Foo", "Abc" } });
            result.Should().Be("#{Foo | ToBazooka 6}");
        }

        [Fact]
        public void FiltersAreApplied()
        {
            var result = Evaluate("#{Foo | ToUpper}", new Dictionary<string, string> { { "Foo", "Abc" } });
            result.Should().Be("ABC");
        }

        [Fact]
        public void HtmlIsEscaped()
        {
            var result = Evaluate("#{Foo | HtmlEscape}", new Dictionary<string, string> { { "Foo", "A&'bc" } });
            result.Should().Be("A&amp;&apos;bc");
        }

        [Fact]
        public void XmlIsEscaped()
        {
            var result = Evaluate("#{Foo | XmlEscape}", new Dictionary<string, string> { { "Foo", "A&'bc" } });
            result.Should().Be("A&amp;&apos;bc");
        }

        [Fact]
        public void JsonIsEscaped()
        {
            var result = Evaluate("#{Foo | JsonEscape}", new Dictionary<string, string> { { "Foo", "A&\"bc" } });
            result.Should().Be("A&\\\"bc");
        }

        [Fact]
        public void MarkdownIsProcessed()
        {
            var result = Evaluate("#{Foo | Markdown}", new Dictionary<string, string> { { "Foo", "_yeah!_" } });
            result.Trim().Should().Be("<p><em>yeah!</em></p>");
        }

        [Fact]
        public void MarkdownHttpLinkIsProcessed()
        {
            var result = Evaluate("#{Foo | Markdown}", new Dictionary<string, string> { { "Foo", "http://octopus.com" } });
            result.Trim().Should().Be("<p><a href=\"http://octopus.com\">http://octopus.com</a></p>");
        }

        [Fact]
        public void MarkdownTablesAreProcessed()
        {
            var dictionary = new Dictionary<string, string> { {"Foo", 
@"|Header1|Header2|
|-|-|
|Cell1|Cell2|" }};
            var result = Evaluate("#{Foo | Markdown}", dictionary);
            result.Trim().Should().Be("<table>\n<thead>\n<tr>\n<th>Header1</th>\n<th>Header2</th>\n</tr>\n</thead>\n<tbody>\n<tr>\n<td>Cell1</td>\n<td>Cell2</td>\n</tr>\n</tbody>\n</table>");
        }

        [Fact]
        public void DateIsFormatted()
        {
            var dict = new Dictionary<string, string> { { "Foo", "2030/05/22 09:05:00" } };

            var result = Evaluate("#{Foo | Format Date \"HH dd-MMM-yyy\"}", dict);
            result.Should().Be("09 22-May-2030");
        }

        [Fact]
        public void DateFormattingCanUseInnerVariable()
        {
            var dict = new Dictionary<string, string> { { "Foo", "2030/05/22 09:05:00" }, { "Format", "HH dd-MMM-yyyy" } };

            var result = Evaluate("#{Foo | Format Date #{Format}}", dict);
            result.Should().Be("09 22-May-2030");
        }

        [Fact]
        public void GenericConverterAcceptsDouble()
        {
            var dict = new Dictionary<string, string> { { "Cash", "23.4" }};

            var result = Evaluate("#{Cash | Format Double C}", dict);
            result.Should().Be(23.4.ToString("C"));
        }
        
        [Fact]
        public void GenericConverterAcceptsDate()
        {
            var dict = new Dictionary<string, string> { { "MyDate", "2030/05/22 09:05:00" }, { "Format", "HH dd-MMM-yyyy" } };
            var result = Evaluate("#{MyDate | Format DateTime \"HH dd-MMM-yyyy\" }", dict);
            result.Should().Be("09 22-May-2030");
        }

        [Fact]
        public void FormatFunctionDefaultAssumeDecimal()
        {
            var dict = new Dictionary<string, string> { { "Cash", "23.4" } };

            var result = Evaluate("#{Cash | Format C}", dict);
            result.Should().Be(23.4.ToString("C"));
        }

        [Fact]
        public void FormatFunctionWillTryDefaultDateTimeIfNotDecimal()
        {
            var dict = new Dictionary<string, string> { { "Date", "2030/05/22 09:05:00" } };
            var result = Evaluate("#{ Date | Format yyyy}", dict);
            result.Should().Be("2030");
        }

        [Fact]
        public void FormatFunctionWillReturnUnreplacedIfNoDefault()
        {
            var dict = new Dictionary<string, string> { { "Invalid", "hello World" } };

            var result = Evaluate("#{Invalid | Format yyyy}", dict);
            result.Should().Be("#{Invalid | Format yyyy}");
        }

        [Fact]
        public void NowDateReturnsNow()
        {
            var result = Evaluate("#{ | NowDate}", new Dictionary<string, string> ());
            DateTime.Parse(result).Should().BeCloseTo(DateTime.Now, 60000);
        }


        [Fact]
        public void NowDateCanBeFormatted()
        {
            var result = Evaluate("#{ | NowDate yyyy}", new Dictionary<string, string>());
            result.Should().Be(DateTime.Now.Year.ToString());
        }

        [Fact]
        public void NullJsonPropertyTreatedAsEmptyString()
        {
            var result = Evaluate("Alpha#{Foo.Bar | ToUpper}bet", new Dictionary<string, string> { { "Foo", "{Bar: null}" } });
            result.Should().Be("Alphabet");
        }

        [Fact]
        public void NowDateCanBeChained()
        {
            var result = Evaluate("#{ | NowDate | Format Date MMM}", new Dictionary<string, string>());
            result.Should().Be(DateTime.Now.ToString("MMM"));
        }

        [Fact]
        public void NowDateReturnsNowInUtc()
        {
            var result = Evaluate("#{ | NowDateUtc}", new Dictionary<string, string>());
            DateTimeOffset.Parse(result).Should().BeCloseTo(DateTimeOffset.UtcNow, 60000);
        }

        [Fact]
        public void NowDateUtcCanBeChained()
        {
            var result = Evaluate("#{ | NowDateUtc | Format DateTimeOffset zz}", new Dictionary<string, string>());
            result.Should().Be("+00");

            var result1 = Evaluate("#{ | NowDate | Format DateTimeOffset zz}", new Dictionary<string, string>());
            result1.Should().Be(DateTimeOffset.Now.ToString("zz"));
        }

        [Fact]
        public void FiltersAreAppliedInOrder()
        {
            var result = Evaluate("#{Foo|ToUpper|ToLower}", new Dictionary<string, string> { { "Foo", "Abc" } });
            result.Should().Be("abc");
        }
    }
}
