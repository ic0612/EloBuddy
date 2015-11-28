﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using AutoBuddy.Humanizers;
using AutoBuddy.MainLogics;
using AutoBuddy.MyChampLogic;
using AutoBuddy.Utilities;
using AutoBuddy.Utilities.AutoShop;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Version = System.Version;

namespace AutoBuddy
{
    internal class Program
    {
        private static Menu menu;
        private static IChampLogic myChamp;
        public static SkillLevelUp LevelUp { get; private set; }
        //private static EasyShop easyShop;
        public static LogicSelector Logic { get; private set; }

        public static void Main()
        {
            Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }


        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            createFS();
            Chat.Print("AutoBuddy will start in 5 seconds. ");
            Core.DelayAction(Start, 5000);
            menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            menu.Add("autolvl", new CheckBox("Disable built-in skill leveler(press f5 after)", false));
            menu.Add("sep1", new Separator(1));
            CheckBox c =
                new CheckBox("Try to go mid, will leave if other player stays on mid(works only with auto lane)", true);

            PropertyInfo property2 = typeof (CheckBox).GetProperty("Size");

            property2.GetSetMethod(true).Invoke(c, new object[] {new Vector2(500, 20)});
            menu.Add("mid", c);

            Slider s = menu.Add("lane", new Slider(" ", 1, 1, 4));
            string[] lanes =
            {
                "", "Selected lane: Auto", "Selected lane: Top", "Selected lane: Mid",
                "Selected lane: Bot"
            };
            s.DisplayName = lanes[s.CurrentValue];
            s.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = lanes[changeArgs.NewValue];
                };


            menu.Add("sep2", new Separator(250));
            menu.Add("reselectlane", new CheckBox("Reselect lane", false));
            menu.Add("debuginfo", new CheckBox("Draw debug info(press f5 after)", true));
            menu.Add("l1", new Label("By Christian Brutal Sniper"));
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            menu.Add("l2",
                new Label("Version " + v.Major + "." + v.Minor + " Build time: " + v.Build%100 + " " +
                          CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(v.Build/100) + " " +
                          (v.Revision/100).ToString().PadLeft(2, '0') + ":" +
                          (v.Revision%100).ToString().PadLeft(2, '0')));
        }


        private static void Start()
        {
            RandGen.Start();
            bool generic = false;
            switch (ObjectManager.Player.Hero)
            {
                case Champion.Ashe:
                    myChamp = new Ashe();
                    break;
                case Champion.Caitlyn:
                    myChamp = new Caitlyn();
                    break;
                default:
                    generic = true;
                    myChamp = new Generic();
                    break;
                case Champion.Ezreal:
                    myChamp = new Ezreal();
                    break;
            }
            if (!generic)
            {
                BuildCreator bc = new BuildCreator(menu, Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "AutoBuddy\\Builds"), myChamp.ShopSequence);
            }


            else
            {
                myChamp = new Generic();
                if (MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName) != null &&
                    MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName).Get<Label>("shopSequence") != null)
                {
                    Chat.Print("Autobuddy: Loaded shop plugin for " + ObjectManager.Player.ChampionName);
                    //easyShop = new EasyShop(
                    //
                    BuildCreator bc = new BuildCreator(menu, Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "AutoBuddy\\Builds"),
                        MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName)
                            .Get<Label>("shopSequence")
                            .DisplayName);
                }
                else
                {
                    BuildCreator bc = new BuildCreator(menu, Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "AutoBuddy\\Builds"), myChamp.ShopSequence);
                }
            }
            if (!menu.Get<CheckBox>("autolvl").CurrentValue)
                LevelUp = new SkillLevelUp(myChamp);
            Logic = new LogicSelector(myChamp);
        }

        private static void createFS()
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "AutoBuddy"));
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "AutoBuddy\\Builds"));
        }
    }
}