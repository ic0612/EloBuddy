﻿
using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EssentialMapHack.Utilities;
using SharpDX;
using Color = System.Drawing.Color;
using Sprite = EloBuddy.SDK.Rendering.Sprite;

namespace EssentialMapHack
{
    class Champ
    {
        private readonly int side;
        private readonly Text timer;
        private readonly int spriteSize;
        private readonly Sprite img;

        private Vector3 oldpos;
        private float oldhp;
        private float oldmana;

        private float health;
        private float teleStart;
        private float teleDuration;
        private bool teleporting;
        public Vector3 position;
        public readonly Vector3 spawn;

        public float invisTime;
        public readonly AIHeroClient hero;
        public int place;
        public bool visible;

        public Champ(AIHeroClient hero, int spriteSize = -1)
        {
            timer = new Text("30", new Font(FontFamily.GenericMonospace, 9, FontStyle.Regular)) { Color = Color.Red };
            if (spriteSize == -1)
                spriteSize = (int)(1700 * Util.MinimapMul);
            this.spriteSize = spriteSize / 2;
            this.hero = hero;
            img = Util.CreateChampTexture(hero.ChampionName, spriteSize);
            spawn = Util.GetSpawn(hero);
            position = spawn;
            side = spawn.X > 1000 ? -1 : 1;
            health = hero.MaxHealth;
            oldpos = hero.Position;
            oldhp = hero.Health;
            oldmana = hero.Mana;
            CheckVisible();
            
            Teleport.OnTeleport += Teleport_OnTeleport;
        }

        public void Update(float dt)
        {
            /*
            if (visible || hero.IsDead)
            {
                health = hero.Health;
                position = hero.IsDead ? spawn : hero.Position;
                invisTime = 0;
            }
            else
            {
                if (!teleporting)
                    invisTime += dt;
                if (health < hero.MaxHealth)
                    health += hero.HPRegenRate*dt;
            }*/
        }

        public void Draw()
        {

            if (invisTime <= 0) return;
            DrawIG();
            Vector2 pos = position.WorldToMinimap();
            int offset = position==spawn ? side * place * Globals.PortraitsOffset : 0;
            DrawSprite(pos+new Vector2(-spriteSize+offset, -spriteSize-offset));
            Vector2 of = new Vector2(offset, -offset);
            if (invisTime > Globals.HideCircleTime)
                DrawText(pos+of);
            else
                DrawRange(pos);
            DrawHpRecall(pos+of);
        }
        private void DrawSprite(Vector2 pos)
        {
            img.Draw(pos);
        }

        private void DrawRange(Vector2 pos)
        {
            Util.DrawCricleMinimap(pos, (hero.MoveSpeed>1?hero.MoveSpeed:320) * invisTime * Util.MinimapMul, Color.Red, Globals.CircleWidth, Globals.CircleQuality);
        }

        private void DrawHpRecall(Vector2 pos)
        {
            if (Globals.ShowHP)
                Util.DrawArc(pos, spriteSize + 1, Color.LawnGreen, 3.1415f, Util.PI2 * health / hero.MaxHealth, 2f,
                    Globals.CircleQuality / 3);

            if (teleporting && Globals.ShowRecalls && teleDuration > 1)
                Util.DrawArc(pos, spriteSize + 4, Color.Aqua, 3.1415f, Util.PI2 * ((Game.Time - teleStart) / teleDuration), 2f, Globals.CircleQuality);
        }
        private void DrawIG()
        {
            if (teleporting && Globals.ShowIG && teleDuration > 0.1 && health/hero.MaxHealth < .3)
                Drawing.DrawCircle(position, hero.MoveSpeed*invisTime, Color.Aqua);
        }

        private void DrawText(Vector2 pos)
        {
            timer.TextValue = Math.Floor(invisTime).ToString();
            pos.X -= 6;
            pos.Y -= 7;
            timer.Position = pos;
            timer.Draw();
        }

        public void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {

            if (sender.NetworkId != hero.NetworkId) return;
            if (args.Status == TeleportStatus.Start)
            {
                teleStart = Game.Time;
                teleDuration = args.Duration / 1000f;
                teleporting = true;
                if(invisTime>.8f)
                    invisTime -= .8f;
            }

            if (args.Status == TeleportStatus.Abort)
            {
                teleporting = false;
            }
            if (args.Status == TeleportStatus.Finish)
            {
                teleporting = false;
                invisTime = 0;
                position = spawn;
                health = hero.MaxHealth;

            }
        }

        public void Kill()
        {
            Teleport.OnTeleport -= Teleport_OnTeleport;
        }

        public void CheckVisible()
        {

            if (hero.Position == oldpos && hero.Mana == oldmana && hero.Health == oldhp)
            {
                if (!teleporting&&!hero.IsDead)
                    invisTime += 0.05f;
                if (health < hero.MaxHealth)
                    health += hero.HPRegenRate*.05F;
                    
                visible = false;

            }
            else
            {
                health = hero.Health;
                
                invisTime = 0;
                visible = true;
            }

            if(visible)
                position = hero.Position;
            if (hero.IsDead)
                position = spawn;
            oldpos = hero.Position;
            oldhp = hero.Health;
            oldmana = hero.Mana;
            Core.DelayAction(CheckVisible, visible ? 500 : 50);
        }
    }
}
