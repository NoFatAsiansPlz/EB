﻿namespace Olaf
{
    using System;

    using EloBuddy;
    using EloBuddy.SDK.Events;

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != Champion.Olaf.ToString())
            {
                return;
            }

            Spells.LoadSpells();
            Config.CallMenu();
            Mainframe.Init();
        }

        #endregion
    }
}