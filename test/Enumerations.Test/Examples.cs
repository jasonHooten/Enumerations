using System;
using System.Collections.Generic;
using System.Linq;

namespace Enumerations.Test
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Super simple example of a very straight forward use of Enumeration as a replacement for an Enum class,
    /// the display name can be anything you want it to be as the same with the value
    /// </summary>
    public class SimpleEnumeration : Enumeration
    {
        public static SimpleEnumeration
            Example1 = new SimpleEnumeration(1, "Example 1"),
            Example2 = new SimpleEnumeration(2, "Example 2");

        private SimpleEnumeration(int value, string displayName) : base(value, displayName)
        {
        }
    }



    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This is a simple example of how you can take an exist enum and depreciate it but still retain the previous enum values for backwards comparability
    /// </summary>
    /// <example>
    ///     if(ColorsEnumeration.Blue.DepreciatedColorEnum == Colors.Blue) ...
    /// </example>
    public class ColorsEnumeration : Enumeration
    {
        public static ColorsEnumeration
            Blue = new ColorsEnumeration(0, "Blue", Colors.Blue),
            Red = new ColorsEnumeration(1, "Red", Colors.Red),
            Yellow = new ColorsEnumeration(2, "Yellow", Colors.Yellow),
            Green = new ColorsEnumeration(3, "Green", Colors.Green);

        public Colors DepreciatedColorEnum { get; }
        
        public enum Colors
        {
            Blue = 22,
            Red = 33,
            Yellow = 2,
            Green = 3
        }

        private ColorsEnumeration(int value, string displayName, Colors color) : base(value, displayName)
        {
            DepreciatedColorEnum = color;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An example of a much more complex example with subtyping. The subtyping is used if you have properties or methods that are different across groups of types of enums
    /// </summary>
    public abstract class WeaponEnumeration : Enumeration
    {
        private readonly int _damage;

        public static WeaponEnumeration
            GreatSword = new SharpWeaponEnumeration(0, "Great Sword", 10, true),
            Sword = new SharpWeaponEnumeration(1, "Sword + Shield", 5, false),
            Mace = new BluntWeaponEnumeration(2, "Mace", 5, false)
            ;


        public bool NeedsTwoHands { get; }

        public abstract bool IsSharp { get; }
        public virtual int CalculateDamage(Orc orc) => NeedsTwoHands ? _damage * 2 : _damage;
        
        private WeaponEnumeration(int value, string displayName, int damage, bool needsTwoHands) : base(value, displayName)
        {
            _damage = damage;
            NeedsTwoHands = needsTwoHands;
        }


        /// <summary>
        /// An example of a sharp weapon class that will do double damage if the orc is wearing no armor but 90% less damage if he is
        /// </summary>
        private class SharpWeaponEnumeration : WeaponEnumeration
        {
            public SharpWeaponEnumeration(int value, string displayName, int damage, bool needsTwoHands) : base(value, displayName, damage, needsTwoHands)
            {
            }

            public override bool IsSharp => true;

            public override int CalculateDamage(Orc orc) =>
                       orc.Armored
                            ? (int) Math.Round(base.CalculateDamage(orc) * .1)
                            : base.CalculateDamage(orc) * 2;
        }



        /// <summary>
        /// An example of a blunt weapon class that will always at least do 3 damage and isn't that affected by armor (only reduced by 20%)
        /// </summary>
        private class BluntWeaponEnumeration : WeaponEnumeration
        {
            public BluntWeaponEnumeration(int value, string displayName, int damage, bool needsTwoHands) : base(value, displayName, damage, needsTwoHands)
            {
            }

            private int _impactDamage = 3;

            public override bool IsSharp => false;

            public override int CalculateDamage(Orc orc) =>
                        _impactDamage + (orc.Armored
                                                ? (int) Math.Round(base.CalculateDamage(orc) * .8)
                                                : base.CalculateDamage(orc));
        }


        // you can also have static methods as adapters for the enums (filtering, sorting etc....)
        public static IEnumerable<WeaponEnumeration> GetAllSingleHandledWeapons() =>
            GetAll<WeaponEnumeration>().Where(x => x.NeedsTwoHands == false);
        public static IEnumerable<WeaponEnumeration> GetAllTwoHandledWeapons() =>
            GetAll<WeaponEnumeration>().Where(x => x.NeedsTwoHands == true);
    }


    public class Orc
    {
        public Orc(int hp, bool armored)
        {
            HP = hp;
            Armored = armored;
        }


        public int HP { get; }
        public bool Armored { get;  }
    }

}
