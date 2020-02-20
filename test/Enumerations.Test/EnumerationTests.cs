using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Enumerations.Test
{
    [TestFixture]
    public class EnumerationTests
    {
        [Test]
        public void Test_that_GetAll_returns_all_enumerations_for_a_type()
        {
            //setup
            // see Examples.cs

            //run
            var simpleItems = Enumeration.GetAll<SimpleEnumeration>();
            var colorItems = Enumeration.GetAll<ColorsEnumeration>();
            var weaponItems = Enumeration.GetAll<WeaponEnumeration>();

            //test
            Assert.That(simpleItems.Count, Is.EqualTo(2));
            Assert.That(colorItems.Count, Is.EqualTo(4));
            Assert.That(weaponItems.Count, Is.EqualTo(3));
        }


    }
}
