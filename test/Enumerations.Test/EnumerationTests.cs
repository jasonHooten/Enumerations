using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Enumerations.Test
{
    public enum EmployeeTypeEnum
    {
        Manager = 0,
        Servant = 1,
        AssistantToTheRegionalManager = 2
    }

    public class EnumerationTests
    {
         public class EmployeeType : Enumeration
         {
             public static readonly EmployeeType Manager
                 = new EmployeeType((int) EmployeeTypeEnum.Manager, "Manager");
             public static readonly EmployeeType Servant
                 = new EmployeeType(1, "Servant");
             public static readonly EmployeeType AssistantToTheRegionalManager
                 = new EmployeeType(2, "Assistant to the Regional Manager");

             private EmployeeType(int value, string displayName) : base(value, displayName) { }
         }

        [Fact]
        public void Test_you_can_get_a_enumeration_by_vaue()
        {
            var employeeType0 = Enumeration.FromValue<EmployeeType>(0);
            var employeeType1 = Enumeration.FromValue<EmployeeType>(1);
            var employeeType2 = Enumeration.FromValue<EmployeeType>(2);

            Assert.Equal(EmployeeType.Manager, employeeType0);
            Assert.Equal(EmployeeType.Servant, employeeType1);
            Assert.Equal(EmployeeType.AssistantToTheRegionalManager, employeeType2);
        }
    }
}
