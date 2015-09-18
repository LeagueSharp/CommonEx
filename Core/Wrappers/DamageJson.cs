﻿// <copyright file="DamageJson.cs" company="LeagueSharp">
//    Copyright (c) 2015 LeagueSharp.
// 
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
// 
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
// 
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace LeagueSharp.SDK.Core.Wrappers
{
    using System.Collections.Generic;

    /// <summary>
    ///     Damage wrapper class, contains functions to calculate estimated damage to a unit and also provides damage details.
    /// </summary>
    public static partial class Damage
    {
        #region Enums

        /// <summary>
        ///     The Damage Scaling Target Type enumeration.
        /// </summary>
        public enum DamageScalingTarget
        {
            /// <summary>
            ///     The Source target type.
            /// </summary>
            Source,

            /// <summary>
            ///     The Target target type.
            /// </summary>
            Target
        }

        /// <summary>
        ///     The Damage Scaling Type enumeration.
        /// </summary>
        public enum DamageScalingType
        {
            /// <summary>
            ///     The Bonus Attack Points scaling type.
            /// </summary>
            BonusAttackPoints,

            /// <summary>
            ///     The Ability Points scaling type.
            /// </summary>
            AbilityPoints,

            /// <summary>
            ///     The Attack Points scaling type.
            /// </summary>
            AttackPoints,

            /// <summary>
            ///     The Max Health scaling type.
            /// </summary>
            MaxHealth,

            /// <summary>
            ///     The Current Health scaling type.
            /// </summary>
            CurrentHealth,

            /// <summary>
            ///     The Missing Health scaling type.
            /// </summary>
            MissingHealth,

            /// <summary>
            ///     The Armor scaling type.
            /// </summary>
            Armor,

            /// <summary>
            ///     The <c>Mana</c> scaling type.
            /// </summary>
            MaxMana
        }

        /// <summary>
        ///     The Damage Stage enumeration.
        /// </summary>
        public enum DamageStage
        {
            /// <summary>
            ///     The Default stage.
            /// </summary>
            Default,

            /// <summary>
            ///     The WayBack stage.
            /// </summary>
            WayBack,

            /// <summary>
            ///     The Detonation stage.
            /// </summary>
            Detonation,

            /// <summary>
            ///     The Damage Per Second stage.
            /// </summary>
            DamagePerSecond,

            /// <summary>
            ///     The Second Form stage.
            /// </summary>
            SecondForm,

            /// <summary>
            ///     The Second Cast stage.
            /// </summary>
            SecondCast
        }

        #endregion

        /// <summary>
        ///     The Champion Damage class container.
        /// </summary>
        public class ChampionDamage
        {
            #region Public Properties

            /// <summary>
            ///     Gets the 'E' spell damage classes.
            /// </summary>
            public List<ChampionDamageSpell> E { get; internal set; }

            /// <summary>
            ///     Gets the 'Q' spell damage classes.
            /// </summary>
            public List<ChampionDamageSpell> Q { get; internal set; }

            /// <summary>
            ///     Gets the 'R' spell damage classes.
            /// </summary>
            public List<ChampionDamageSpell> R { get; internal set; }

            /// <summary>
            ///     Gets the 'W' spell damage classes.
            /// </summary>
            public List<ChampionDamageSpell> W { get; internal set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Resolves the spell damage classes entry through the SpellSlot component.
            /// </summary>
            /// <param name="slot">
            ///     The SpellSlot.
            /// </param>
            /// <returns>
            ///     The spell damage classes of the requested Spell Slot.
            /// </returns>
            public List<ChampionDamageSpell> GetSlot(SpellSlot slot)
            {
                switch (slot)
                {
                    case SpellSlot.Q:
                        return this.Q;
                    case SpellSlot.W:
                        return this.W;
                    case SpellSlot.E:
                        return this.E;
                    case SpellSlot.R:
                        return this.R;
                }

                return null;
            }

            #endregion
        }

        /// <summary>
        ///     The Champion Damage Spell class container.
        /// </summary>
        public class ChampionDamageSpell
        {
            #region Public Properties

            /// <summary>
            ///     Gets the Spell Data.
            /// </summary>
            public ChampionDamageSpellData SpellData { get; internal set; }

            /// <summary>
            ///     Gets the Spell Stage.
            /// </summary>
            public DamageStage Stage { get; internal set; }

            #endregion
        }

        /// <summary>
        ///     The Champion Damage Spell Bonus class container.
        /// </summary>
        public class ChampionDamageSpellBonus
        {
            #region Public Properties

            /// <summary>
            ///     Gets the Damage Percentages.
            /// </summary>
            public List<double> DamagePercentages { get; internal set; }

            /// <summary>
            ///     Gets the Damage Type.
            /// </summary>
            public DamageType DamageType { get; internal set; }

            /// <summary>
            ///     Gets the Scaling Target Type.
            /// </summary>
            public DamageScalingTarget ScalingTarget { get; internal set; }

            /// <summary>
            ///     Gets the Scaling Type.
            /// </summary>
            public DamageScalingType ScalingType { get; internal set; }

            #endregion
        }

        /// <summary>
        ///     The Champion Damage Spell Data class container.
        /// </summary>
        public class ChampionDamageSpellData
        {
            #region Public Properties

            /// <summary>
            ///     Gets the Bonus Damages.
            /// </summary>
            public List<ChampionDamageSpellBonus> BonusDamages { get; internal set; }

            /// <summary>
            ///     Gets the Main Damages.
            /// </summary>
            public List<int> Damages { get; internal set; }

            /// <summary>
            ///     Gets the Damage Type.
            /// </summary>
            public DamageType DamageType { get; internal set; }

            #endregion
        }
    }
}