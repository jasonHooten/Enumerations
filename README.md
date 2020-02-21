# Enumerations
a simple enumeration class repository based on [a blog by Jimmy Bogard](https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/)

The idea is to solve the a couple issues that arise in solutions making heavy use on Enums in domain logic.
* Behavior related to the enumeration gets scattered around the application
* New enumeration values requires editing logic in many places of the code
* Enumerations don’t follow the [Open-Closed Principle](https://stackify.com/solid-design-open-closed-principle/)

Clode that looks like this...

```c#
public class Employee
{
    public EmployeeType Type { get; }
    public decimal Commission { get; }
}


public enum EmployeeType
{
    Manager,
    Servant,
    AssistantToTheRegionalManager
}


// and somewhere in the code...
public decimal ProcessBonus(Employee employee)
{
    var bonus = 0;
    switch(employee.Type)
    {
        case EmployeeType.Manager:
            bonus = 1000m;
            break;
        case EmployeeType.Servant:
            bonus = 0.01m;
            break;
        case EmployeeType.AssistantToTheRegionalManager:
            bonus = 1.0m;
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
    return employee.Commission * bonus;
}
```

becomes...

```c#
public class Employee
{
    public EmployeeType Type { get; }
    public decimal Commission { get; }
}


public class EmployeeType : Enumeration
{
    public static readonly EmployeeType
        Manager = new EmployeeType(0, "Manager", 1000m),
        Servant = new EmployeeType(1, "Servant", 0.01m),
        AssistantToTheRegionalManager = new EmployeeType(2, "Assistant To The Regional Manager", 1.0m);

    private decimal _bonus { get; }

    public decimal ProcessBonus(Employee employee)
    {
        return employee.Commission * _bonus;
    }

    private EmployeeType(int value, string displayName, decimal bonus) : base(value, displayName)
    {
        _bonus = bonus
    }
}
```

