//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//
//     Source: tags.txt
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Liquid.Ruby\writetest.rb
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Liquid.NET.Constants;
using NUnit.Framework;

namespace Liquid.NET.Tests.Ruby
{
    [TestFixture]
    public class TagTests {

        [Test]
        [TestCase(@"", @"")]
        [TestCase(@"{% if ""x"" == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if ""x"" == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if empty == ""x"" %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if 0 == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if '' == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"EMPTY")]
        [TestCase(@"{% assign myarray = """" |split: "","" %}{% if myarray == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"EMPTY")]
        [TestCase(@"{% assign myarray = ""1"" |split: "","" %}{% if myarray == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign x = """" %}{% if x == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"EMPTY")]
        [TestCase(@"{% assign x = undef %}{% if x == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign x = 0 %}{% if x == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign x = null %}{% if x == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign x = nil %}{% if x == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign x = nil %}{% if x == nil %}EMPTY{% else %}NOT EMPTY{% endif %}", @"EMPTY")]
        [TestCase(@"{% if "" "" == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if "" "" == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if null == empty %}EMPTY{% else %}NOT EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% assign myarray = ""1"" |split: "","" %}{% if x != empty %}NOT EMPTY{% else %}EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if "" "" != empty %}NOT EMPTY{% else %}EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if "" "" != empty %}NOT EMPTY{% else %}EMPTY{% endif %}", @"NOT EMPTY")]
        [TestCase(@"{% if null != empty %}NOT EMPTY{% else %}EMPTY{% endif %}", @"NOT EMPTY")]
        public void It_Should_Match_Ruby_Output(String input, String expected) {

            // Arrange
            ITemplateContext ctx = new TemplateContext().WithAllFilters();
            var template = LiquidTemplate.Create(input);
        
            // Act
            String result = template.Render(ctx);
        
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        
    }
}
