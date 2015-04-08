//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//
//     Source: filters.txt
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
    public class FilterTests {

        [Test]
        [TestCase(@"", @"")]
        [TestCase(@"{{ ""1"" | divided_by: ""0"" }}", @"Liquid error: divided by 0")]
        [TestCase(@"{{ ""x"" | divided_by: ""1"" }}", @"0")]
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
