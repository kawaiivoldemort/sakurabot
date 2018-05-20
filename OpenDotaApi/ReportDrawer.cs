using System;
using System.Linq;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Collections.Generic;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace OpenDotaApi
{
    public class ReportDrawer
    {
        // Loaded Fonts
        private readonly Font verdana;
        private readonly Font verdanaBold;
        private readonly Font verdanaTiny;
        private readonly Font verdanaTinyBold;
        private readonly Font verdanaBig;
        private readonly Font verdanaBigBold;
        // LoadeFont Alignment Options
        private readonly TextGraphicsOptions centerText = new TextGraphicsOptions(true);
        private readonly TextGraphicsOptions centerMiddleText = new TextGraphicsOptions(true);
        private readonly TextGraphicsOptions leftText = new TextGraphicsOptions(true);
        private readonly TextGraphicsOptions leftMiddleText = new TextGraphicsOptions(true);
        private readonly TextGraphicsOptions rightText = new TextGraphicsOptions(true);
        private readonly TextGraphicsOptions rightMiddleText = new TextGraphicsOptions(true);
        // Assembly Statics
        private readonly Assembly assembly;
        public ReportDrawer(Assembly assembly)
        {
            this.assembly = assembly;
            FontCollection fonts = new FontCollection();
            fonts.Install(this.assembly.GetManifestResourceStream($"OpenDotaApi.ref.fonts.verdana.ttf"));
            centerText.HorizontalAlignment = HorizontalAlignment.Center;
            centerText.VerticalAlignment = VerticalAlignment.Top;
            centerMiddleText.HorizontalAlignment = HorizontalAlignment.Center;
            centerMiddleText.VerticalAlignment = VerticalAlignment.Center;
            leftText.HorizontalAlignment = HorizontalAlignment.Left;
            leftText.VerticalAlignment = VerticalAlignment.Top;
            leftMiddleText.HorizontalAlignment = HorizontalAlignment.Left;
            leftMiddleText.VerticalAlignment = VerticalAlignment.Center;
            rightText.HorizontalAlignment = HorizontalAlignment.Right;
            rightText.VerticalAlignment = VerticalAlignment.Top;
            rightMiddleText.HorizontalAlignment = HorizontalAlignment.Right;
            rightMiddleText.VerticalAlignment = VerticalAlignment.Center;
            var fontFamilies = fonts.Families;
            var fontfamily = fontFamilies.First();
            verdana = fontfamily.CreateFont(16, FontStyle.Regular);
            verdanaBold = fontfamily.CreateFont(16, FontStyle.Bold);
            verdanaTiny = fontfamily.CreateFont(12, FontStyle.Regular);
            verdanaTinyBold = fontfamily.CreateFont(12, FontStyle.Bold);
            verdanaBig = fontfamily.CreateFont(24, FontStyle.Regular);
            verdanaBigBold = fontfamily.CreateFont(24, FontStyle.Bold);
        }
        public List<byte[]> DrawReport(DotaMatch match)
        {
            var images = new List<byte[]>();
            using(var matchReport = Image.Load(this.assembly.GetManifestResourceStream($"OpenDotaApi.ref.assets.{match.winner}Win.png")))
            {
                matchReport.Mutate(
                    (imgContext) => {
                        // Background Fill
                        var boxColor = new Rgba32(32, 32, 32, 192);

                        int x = 20;
                        int y = 20;
                        // Draw Match Information
                        var matchDateTime = DateTimeOffset.FromUnixTimeSeconds(match.startTime).UtcDateTime;
                        var matchDuration = TimeSpan.FromSeconds(match.matchDuration);
                        imgContext.DrawText(
                            centerText,
                            $"Match {match.matchId}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(566, 20)
                        );
                        imgContext.DrawText(
                            centerText,
                            match.matchType.name.ToUpper(),
                            verdana,
                            Rgba32.White,
                            new PointF(566, 36)
                        );
                        imgContext.DrawText(
                            centerText,
$@"{match.server.name} Server

{matchDateTime.ToShortDateString()}
{matchDateTime.ToShortTimeString()}
{matchDuration.Hours}:{matchDuration.Minutes}:{matchDuration.Seconds}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(566, 58)
                        );
                        y = 170;
                        imgContext.DrawLines(Rgba32.White, 0.5f, new PointF[]{ new PointF(20, y - 20), new PointF(1112, y - 20) });
                        imgContext.DrawLines(Rgba32.White, 0.5f, new PointF[]{ new PointF(20, y + 520), new PointF(1112, y + 520) });
                        imgContext.DrawLines(Rgba32.White, 0.5f, new PointF[]{ new PointF(20, y + 1060), new PointF(1112, y + 1060) });
                        // Draw Team Information
                        for(var j = 0; j < 2; j++)
                        {
                            var thisTeamPlayers = match.players.Where(player => player.team == j);
                            imgContext.DrawLines(
                                boxColor,
                                220,
                                new PointF[] {
                                    new PointF(x + 110, y + j * 540),
                                    new PointF(x + 110, y + 500 + j * 540)
                                }
                            );
                            var team = (j == 0 ? "Radiant" : "Dire");
                            imgContext.DrawText(
                                centerText,
                                team.ToUpper(),
                                verdanaBig,
                                Rgba32.White,
                                new PointF(x + 110, y + 20 + j * 540)
                            );
                            imgContext.DrawText(
                                centerText,
                                (team.Equals(match.winner) ? "( winner )" : ""),
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(x + 110, y + 50 + j * 540)
                            );
                            imgContext.DrawText(
                                leftMiddleText,
@"KILLS

DEATHS

ASSISTS



LAST HITS

DENIES



HERO DMG

TOWER DMG

HEAL



TOTAL GOLD

TOTAL XP",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 20, y + 316 + j * 540)
                        );
                        imgContext.DrawText(
                            leftMiddleText,
$@":  {(j == 0 ? match.radiantKills : match.direKills)}

:  {(j == 0 ? match.radiantDeaths : match.direDeaths)}

:  {thisTeamPlayers.Select(player => (int) player.assists).Sum()}



:  {thisTeamPlayers.Select(player => (int) player.lastHits).Sum()}

:  {thisTeamPlayers.Select(player => (int) player.denies).Sum()}



:  {thisTeamPlayers.Select(player => (int) player.heroDamage).Sum()}

:  {thisTeamPlayers.Select(player => (int) player.towerDamage).Sum()}

:  {thisTeamPlayers.Select(player => (int) player.heroHealing).Sum()}



:  {thisTeamPlayers.Select(player => (int) player.netWorth).Sum()}

:  {thisTeamPlayers.Select(player => (int) player.experience).Sum()}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(x + 130, y + 316 + j * 540)
                            );
                        }
                        // Draw Player Data
                        x = 240;
                        imgContext.DrawText(
                            centerMiddleText,
                            "LVL",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 590, y - 40)
                        );
                        imgContext.DrawText(
                            centerMiddleText,
                            "NW",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 660, y - 40)
                        );
                        imgContext.DrawText(
                            centerMiddleText,
                            "K",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 730, y - 40)
                        );
                        imgContext.DrawText(
                            centerMiddleText,
                            "D",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 790, y - 40)
                        );
                        imgContext.DrawText(
                            centerMiddleText,
                            "A",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x + 850, y - 40)
                        );
                        for(var j = 0; j < match.players.Length; j++)
                        {
                            var player = match.players[j];
                            var playerTeam = (player.team == 0) ? "Radiant" : "Dire";
                            var winState = ((playerTeam == match.winner) ? "Winner" : "Loser");
                            var imgx = x;
                            var imgy = y + (int) (player.slot * 104) + (int) (player.team * 540);
                            // boxBorder = new Image<Rgba32>(270, 154);
                            // boxBorder.Mutate(img => img.Fill(Rgba32.DarkSlateGray));
                            // imgContext.DrawImage(
                            //     boxBorder,
                            //     1,
                            //     new Point((int) x - 2, (int) y - 2)
                            // );
                            Image<Rgba32> heroImage = null;
                            var heroImagePath = $@"./imgcache/heroes/{player.hero.id}.png";
                            if(File.Exists(heroImagePath))
                            {
                                heroImage = Image.Load(heroImagePath);
                                heroImage.Mutate(img => img.Resize(150, 84));
                            }
                            else
                            {
                                heroImage = new Image<Rgba32>(150, 84);
                                heroImage.Mutate(img => img.Fill(Rgba32.DarkSlateGray));
                            }
                            imgContext.DrawLines(
                                boxColor,
                                160,
                                new PointF[] {
                                    new PointF(imgx + 100, imgy - 2),
                                    new PointF(imgx + 100, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{(player.playerName == null ? "---" : player.playerName)}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 100, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                154,
                                new PointF[] {
                                    new PointF(imgx + 275, imgy - 2),
                                    new PointF(imgx + 275, imgy + 86)
                                }
                            );
                            imgContext.DrawImage(
                                heroImage,
                                1,
                                new Point(imgx + 200, imgy)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                184,
                                new PointF[] {
                                    new PointF(imgx + 460, imgy - 2),
                                    new PointF(imgx + 460, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.hero.name.ToUpper()}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 460, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                44,
                                new PointF[] {
                                    new PointF(imgx + 590, imgy - 2),
                                    new PointF(imgx + 590, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.level}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 590, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                64,
                                new PointF[] {
                                    new PointF(imgx + 660, imgy - 2),
                                    new PointF(imgx + 660, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.netWorth}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 660, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                44,
                                new PointF[] {
                                    new PointF(imgx + 730, imgy - 2),
                                    new PointF(imgx + 730, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.kills}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 730, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                44,
                                new PointF[] {
                                    new PointF(imgx + 790, imgy - 2),
                                    new PointF(imgx + 790, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.deaths}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 790, imgy + 40)
                            );
                            imgContext.DrawLines(
                                boxColor,
                                44,
                                new PointF[] {
                                    new PointF(imgx + 850, imgy - 2),
                                    new PointF(imgx + 850, imgy + 86)
                                }
                            );
                            imgContext.DrawText(
                                centerMiddleText,
                                $"{player.assists}",
                                verdanaTinyBold,
                                Rgba32.White,
                                new PointF(imgx + 850, imgy + 40)
                            );
                            images.Add(DrawPlayerReport(match, player));
                        }
                    }
                );
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    matchReport.SaveAsPng(memoryStream);
                    images.Insert(0, memoryStream.ToArray());
                }
            }
            return images;
        }
        // Draws a report picture for each Match Player
        public byte[] DrawPlayerReport(DotaMatch match, MatchPlayers player)
        {
            var playerTeam = (player.team == 0) ? "Radiant" : "Dire";
            var winState = ((playerTeam == match.winner) ? "Winner" : "Loser");
            byte[] playerReportData = null;
            // 306:234 is split pane point
            using(var playerReport = Image.Load(this.assembly.GetManifestResourceStream($"OpenDotaApi.ref.assets.{playerTeam}.png")))
            {
                playerReport.Mutate
                (
                    (imgContext) =>
                    {
                        // Background Color
                        // imgContext.Fill(Rgba32.Black);
                        Image<Rgba32> boxBorder;

                        // Drawing Pointers
                        var x = 0;
                        var y = 0;

                        // Draw Player Text
                        x = 20;
                        y = 20;
                        imgContext.DrawText(
                            leftText,
                            ((player.playerName != null) ? (player.playerName) : ""),
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x, y)
                        );
                        x = 286;
                        imgContext.DrawText(
                            rightText,
                            $"{playerTeam} ({winState})",
                            verdanaTiny,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );

                        // Draw Hero Image
                        x = 24;
                        y = 56;
                        boxBorder = new Image<Rgba32>(270, 154);
                        boxBorder.Mutate(img => img.Fill(Rgba32.DarkSlateGray));
                        imgContext.DrawImage(
                            boxBorder,
                            1,
                            new Point((int) x - 2, (int) y - 2)
                        );
                        Image<Rgba32> heroImage = null;
                        var heroImagePath = $@"./imgcache/heroes/{player.hero.id}.png";
                        if(File.Exists(heroImagePath))
                        {
                            heroImage = Image.Load(heroImagePath);
                            heroImage.Mutate(img => img.Resize(266, 150));
                        }
                        else
                        {
                            heroImage = new Image<Rgba32>(266, 150);
                            heroImage.Mutate(img => img.Fill(new Rgba32(32, 32, 32)));
                        }
                        imgContext.DrawImage(
                            heroImage,
                            1,
                            new Point(x, y)
                        );

                        // Draw Player Hero Name
                        x = 153;
                        y = 226;
                        imgContext.DrawText(
                            centerText,
                            $"{player.hero.name}",
                            verdana,
                            Rgba32.White,
                            new PointF(x, y)
                        );
                        
                        // Draw Player Stats
                        y = 262;
                        imgContext.DrawText(
                            centerText,
                            player.netWorth.ToString(),
                            verdanaBigBold,
                            Rgba32.White,
                            new PointF(x, y)
                        );
                        y = 306;
                        imgContext.DrawText(
                            centerText,
                            $@"KDA {player.kills} / {player.deaths} / {player.assists}",
                            verdana,
                            Rgba32.White,
                            new PointF(x, y)
                        );

                        // Draw Player Items
                        x = 20;
                        y = 346;
                        boxBorder = new Image<Rgba32>(266, 136);
                        boxBorder.Mutate(img => img.Fill(new Rgba32(64, 64, 64)));
                        imgContext.DrawImage(
                            boxBorder,
                            1,
                            new Point((int) x, (int) y)
                        );
                        for(var j = 0; j < 6; j++)
                        {
                            var item = player.items[j];
                            Image<Rgba32> itemImage = null;
                            string itemImagePath;
                            if(item != null)
                            {
                                itemImagePath = $@"./imgcache/items/{item.id}.png";
                                if(!File.Exists(itemImagePath))
                                {
                                    itemImage = new Image<Rgba32>(86, 65);
                                    itemImage.Mutate(img => img.Fill(Rgba32.DarkSlateGray));
                                }
                                else
                                {
                                    itemImage = Image.Load(itemImagePath);
                                    itemImage.Mutate(img => img.Resize(86, 65));
                                }
                            }
                            else
                            {
                                itemImage = new Image<Rgba32>(86, 65);
                                itemImage.Mutate(img => img.Fill(new Rgba32(32, 32, 32)));                      
                            }
                            imgContext.DrawImage(
                                itemImage,
                                1,
                                new Point((int) x + 2 + 88 * (j % 3), (int) y + 2 + 67 * (j / 3))
                            );
                        }
                        // Draw Backpack Items
                        x = 53;
                        y = 480;
                        boxBorder = new Image<Rgba32>(194, 49);
                        boxBorder.Mutate(img => img.Fill(new Rgba32(48, 48, 48)));
                        imgContext.DrawImage(
                            boxBorder,
                            1,
                            new Point((int) x, (int) y + 2)
                        );
                        for(var j = 0; j < 3; j++)
                        {
                            var item = player.items[j + 6];
                            Image<Rgba32> itemImage = null;
                            string itemImagePath;
                            if(item != null)
                            {
                                itemImagePath = $@"./imgcache/items/{item.id}.png";
                                if(!File.Exists(itemImagePath))
                                {
                                    itemImage = new Image<Rgba32>(62, 47);
                                    itemImage.Mutate(img => img.Fill(new Rgba32(20, 20, 20)));
                                }
                                else
                                {
                                    itemImage = Image.Load(itemImagePath);
                                    itemImage.Mutate(img => img.Resize(62, 47));
                                }
                            }
                            else
                            {
                                itemImage = new Image<Rgba32>(62, 47);
                                itemImage.Mutate(img => img.Fill(new Rgba32(20, 20, 20)));                                   
                            }
                            imgContext.DrawImage(
                                itemImage,
                                1,
                                new Point((int) x + 2 + 64 * (j % 3), (int) y + 2)
                            );
                        }

                        // Draw Permanent Buffs
                        x = 153;
                        y = 551;
                        imgContext.DrawText(
                            centerText,
                            "PERMANENT BUFFS",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        if(player.permanentBuffs != null)
                        {
                            x = 20;
                            y = 580;
                            for(var j = 0; j < player.permanentBuffs.Length; j++)
                            {
                                var buff = player.permanentBuffs[j];
                                Image<Rgba32> buffImage = null;
                                string buffImagePath;
                                if(buff.buff.orgin == "Item")
                                {
                                    buffImagePath = $@"./imgcache/items/{buff.buff.originId}.png";
                                }
                                else
                                {
                                    buffImagePath = $@"./imgcache/abilities/{buff.buff.originId}.png";
                                }                                    
                                if(!File.Exists(buffImagePath))
                                {
                                    buffImage = new Image<Rgba32>(42, 42);
                                    buffImage.Mutate(img => img.Fill(new Rgba32(32, 32, 32)));
                                }
                                else
                                {
                                    buffImage = Image.Load(buffImagePath);
                                    buffImage.Mutate(img => img.Crop(buffImage.Height, buffImage.Height).Resize(42, 42).Fill(new Rgba32(255, 255, 255, 64)));
                                }
                                var imgx = x + 3 + 44 * (j % 6);
                                boxBorder = new Image<Rgba32>(44, 44);
                                boxBorder.Mutate(img => img.Fill(Rgba32.Gold));
                                imgContext.DrawImage(
                                    boxBorder,
                                    1,
                                    new Point((int) imgx - 1, (int) y - 1)
                                );
                                imgContext.DrawImage(
                                    buffImage,
                                    1,
                                    new Point((int) imgx, (int) y)
                                );
                                imgContext.DrawText(
                                    centerMiddleText,
                                    buff.value.ToString(),
                                    verdanaTinyBold,
                                    Rgba32.Black,
                                    new Point((int) imgx + 21, (int) y + 21)
                                );
                            }
                        }
                        else
                        {
                            y = 580;
                            imgContext.DrawText(
                                centerMiddleText,
                                "---",
                                verdanaTinyBold,
                                Rgba32.White,
                                new Point((int) x, (int) y)
                            );
                        }

                        // Draw a Line through the Middle
                        x = 306;
                        imgContext.DrawLines(Rgba32.LightGray, 0.5f, new PointF[]{ new PointF(x, 20), new PointF(x, 624) });

                        // Draw More Player Stats
                        x = 326;
                        var x2 = 520;
                        y = 34;
                        imgContext.DrawText(
                            leftMiddleText,
                            "LEVEL",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.level}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 60;
                        imgContext.DrawLines(Rgba32.LightGray, 0.5f, new PointF[]{ new PointF(x, y), new PointF(x2, y) });
                        y = 80;
                        imgContext.DrawText(
                            leftMiddleText,
                            "LAST HITS",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.lastHits}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 108;
                        imgContext.DrawText(
                            leftMiddleText,
                            "DENIES",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.denies}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 134;
                        imgContext.DrawLines(Rgba32.LightGray, 0.5f, new PointF[]{ new PointF(x, y), new PointF(x2, y) });
                        y = 154;
                        imgContext.DrawText(
                            leftMiddleText,
                            "GOLD PER MINUTE",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.goldPerMinute}\n",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 182;
                        imgContext.DrawText(
                            leftMiddleText,
                            "XP PER MINUTE",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.experiencePerMinute}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 208;
                        imgContext.DrawLines(Rgba32.LightGray, 0.5f, new PointF[]{ new PointF(x, y), new PointF(x2, y) });
                        y = 228;
                        imgContext.DrawText(
                            leftMiddleText,
                            "HERO DAMAGE",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.heroDamage}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 256;
                        imgContext.DrawText(
                            leftMiddleText,
                            "TOWER DAMAGE",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.towerDamage}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );
                        y = 284;
                        imgContext.DrawText(
                            leftMiddleText,
                            "HERO HEAL",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        imgContext.DrawText(
                            rightMiddleText,
                            $"{player.heroHealing}",
                            verdanaTinyBold,
                            Rgba32.White,
                            new PointF(x2, y)
                        );

                        // Draw Skill Build
                        y = 310;
                        imgContext.DrawLines(Rgba32.LightGray, 0.5f, new PointF[]{ new PointF(x, y), new PointF(x2, y) });
                        x = 423;
                        y = 332;
                        imgContext.DrawText(
                            centerMiddleText,
                            "SKILL BUILD",
                            verdanaTinyBold,
                            Rgba32.LightGray,
                            new PointF(x, y)
                        );
                        x = 326;
                        y = 360;
                        var t = 1;
                        var skillLevel = 0;
                        var talentNumber = 0;
                        for(var j = 0; j < player.skillBuild.Length; j++)
                        {
                            var skill = player.skillBuild[j];
                            var imgx = x + 2 + 32 * (j % 6);
                            var imgy = y + 2 + 64 * (j / 6);
                            boxBorder = new Image<Rgba32>(34, 34);
                            boxBorder.Mutate(img => img.Fill(new Rgba32(48, 48, 48)));
                            imgContext.DrawImage(
                                boxBorder,
                                1,
                                new Point((int) imgx - 2, (int) imgy - 2)
                            );
                            Image<Rgba32> skillImage = null;
                            if(skill.codeName == "talent")
                            {
                                if(skillLevel < (9 + talentNumber * 5))
                                {
                                    skillLevel = 9 + talentNumber * 5;
                                }
                                skillImage = Image.Load(this.assembly.GetManifestResourceStream($"OpenDotaApi.ref.assets.Talent.png"));
                                skillImage.Mutate(img => img.Resize(30, 30));
                                talentNumber++;
                            }
                            else
                            {
                                string skillImagePath = $@"./imgcache/abilities/{skill.id}.png";
                                if(!File.Exists(skillImagePath))
                                {
                                    skillImage = new Image<Rgba32>(30, 30);
                                    skillImage.Mutate(img => img.Fill(new Rgba32(20, 20, 20)));
                                }
                                else
                                {
                                    skillImage = Image.Load(skillImagePath);
                                    skillImage.Mutate(img => img.Resize(30, 30));
                                }
                            }
                            imgContext.DrawImage(
                                skillImage,
                                1,
                                new Point((int) imgx, (int) imgy)
                            );
                            imgContext.DrawText(
                                centerText,
                                (skillLevel + 1).ToString(),
                                verdanaTinyBold,
                                Rgba32.White,
                                new Point((int) imgx + 16, (int) imgy + 34)
                            );
                            if(skillLevel < 25)
                            {
                                skillLevel++;
                            }
                        }
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            playerReport.SaveAsPng(memoryStream);
                            playerReportData = memoryStream.ToArray();
                        }
                    }
                );
            }
            return playerReportData;
        }
    }
}