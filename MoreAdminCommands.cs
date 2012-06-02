using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Threading;

namespace MoreAdminCommands
{
    [APIVersion(1, 12)]
    public class MoreAdminCommands : TerrariaPlugin
    {
        public static SqlTableEditor SQLEditor;
        public static SqlTableCreator SQLWriter;
        public static bool timeFrozen = false;
        public static double timeToFreezeAt = 1000;
        public static bool freezeDayTime = true;
        public static bool[] isGhost = new bool[256];
        public static bool cansend = false;
        public static bool[] isHeal = new bool[256];
        public static bool[] flyMode = new bool[256];
        public static List<List<Vector2>> carpetPoints = new List<List<Vector2>>();
        public static int[] carpetY = new int[256];
        public static bool[] upPressed = new bool[256];
        public static List<List<int>> buffsUsed = new List<List<int>>();
        public static List<int> allBuffsUsed = new List<int>();
        public static Dictionary<string, bool> regionMow = new Dictionary<string, bool>();
        public static bool[] muted = new bool[256];
        public static int[] muteTime = new int[256];
        public static bool[] muteAllFree = new bool[256];
        public static bool muteAll = false;
        public static Dictionary<string, List<int>> buffsUsedGroup = new Dictionary<string, List<int>>();
        public static string defaultMuteAllReason = "Listen to find out";
        public static string muteAllReason = "Listen to find out";
        public static int maxDamage = 500;
        public static bool maxDamageIgnore = false;
        public static bool maxDamageKick = false;
        public static bool maxDamageBan = false;
        public static Dictionary<string, List<int>> buffGroups = new Dictionary<string, List<int>>();
        public static Dictionary<string, Dictionary<NPC, int>> spawnGroups = new Dictionary<string, Dictionary<NPC, int>>();
        public static bool[] viewAll = new bool[256];
        public static int viewAllTeam = 4;
        public static string redPass = "";
        public static string bluePass = "";
        public static string greenPass = "";
        public static string yellowPass = "";
        public static bool[] accessRed = new bool[256];
        public static bool[] accessBlue = new bool[256];
        public static bool[] accessGreen = new bool[256];
        public static bool[] accessYellow = new bool[256];
        public static bool[] autoKill = new bool[256];
        public static bool forcePVP = false;
        public static bool cannotChangePVP = false;
        public static bool[] tpOff = new bool[256];
        public override string Name
        {
            get { return "MoreAdminCommands"; }
        }
        public override string Author
        {
            get { return "Created by DaGamesta"; }
        }
        public override string Description
        {
            get { return ""; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            GameHooks.Update += OnUpdate;
            ServerHooks.Chat += OnChat;
            NetHooks.SendData += OnSendData;
            ServerHooks.Join += OnJoin;
            ServerHooks.Leave += OnLeave;
            NetHooks.GetData += OnGetData;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                GameHooks.Update -= OnUpdate;
                ServerHooks.Chat -= OnChat;
                NetHooks.SendData -= OnSendData;
                ServerHooks.Join -= OnJoin;
                ServerHooks.Leave -= OnLeave;
                NetHooks.GetData -= OnGetData;
            }

            base.Dispose(disposing);
        }

        public MoreAdminCommands(Main game)
            : base(game)
        {
            Order = -1;
        }

        public void OnInitialize()
        {
            Main.buffName[1] = "Obsidian Skin";
            Main.buffName[2] = "Regeneration";
            Main.buffName[3] = "Swiftness";
            Main.buffName[4] = "Gills";
            Main.buffName[5] = "Ironskin";
            Main.buffName[6] = "Mana Regeneration";
            Main.buffName[7] = "Magic Power";
            Main.buffName[8] = "Featherfall";
            Main.buffName[9] = "Spelunker";
            Main.buffName[10] = "Invisibility";
            Main.buffName[11] = "Shine";
            Main.buffName[12] = "Night Owl";
            Main.buffName[13] = "Battle";
            Main.buffName[14] = "Thorns";
            Main.buffName[15] = "Water Walking";
            Main.buffName[0x10] = "Archery";
            Main.buffName[0x11] = "Hunter";
            Main.buffName[0x12] = "Gravitation";
            Main.buffName[0x13] = "Orb of Light";
            Main.buffName[20] = "Poisoned";
            Main.buffName[0x15] = "Potion Sickness";
            Main.buffName[0x16] = "Darkness";
            Main.buffName[0x17] = "Cursed";
            Main.buffName[0x18] = "On Fire!";
            Main.buffName[0x19] = "Tipsy";
            Main.buffName[0x1a] = "Well Fed";
            Main.buffName[0x1b] = "Fairy";
            Main.buffName[0x1c] = "Werewolf";
            Main.buffName[0x1d] = "Clairvoyance";
            Main.buffName[0x1e] = "Bleeding";
            Main.buffName[0x1f] = "Confused";
            Main.buffName[0x20] = "Slow";
            Main.buffName[0x21] = "Weak";
            Main.buffName[0x22] = "Merfolk";
            Main.buffName[0x23] = "Silenced";
            Main.buffName[0x24] = "Broken Armor";
            Main.buffName[0x25] = "Horrified";
            Main.buffName[0x26] = "The Tongue";
            Main.buffName[0x27] = "Cursed Inferno";
            Main.buffName[0x28] = "Bunny";

            SQLEditor = new SqlTableEditor(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            var table = new SqlTable("regionMow",
                        new SqlColumn("Name", MySqlDbType.Text),
                        new SqlColumn("Mow", MySqlDbType.Int32));
            SQLWriter.EnsureExists(table);
            var table2 = new SqlTable("muteList",
                        new SqlColumn("Name", MySqlDbType.Text),
                        new SqlColumn("IP", MySqlDbType.Text));
            SQLWriter.EnsureExists(table2);
            var readTableName = SQLEditor.ReadColumn("regionMow", "Name", new List<SqlValue>());
            var readTableBool = SQLEditor.ReadColumn("regionMow", "Mow", new List<SqlValue>());
            for (int i = 0; i < readTableName.Count; i++)
            {

                try
                {
                    regionMow.Add(readTableName[i].ToString(), Convert.ToBoolean(readTableBool[i]));
                }
                catch (Exception) { }

            }
            reload();
            List<string> permlist = new List<string>();
            permlist.Add("ghostmode");
            permlist.Add("fly");
            permlist.Add("flymisc");
            permlist.Add("mute");
            permlist.Add("reloadmore");
            permlist.Add("permabuff");
            TShock.Groups.AddPermissions("trustedadmin", permlist);
            for (int i = 0; i < 256; i++)
            {

                isGhost[i] = false;
                isHeal[i] = false;
                flyMode[i] = false;
                upPressed[i] = false;
                carpetPoints.Add(new List<Vector2>());
                buffsUsed.Add(new List<int>());
                muted[i] = false;
                muteTime[i] = -1;
                muteAllFree[i] = false;
                viewAll[i] = false;
                accessRed[i] = (redPass == "");
                accessBlue[i] = (bluePass == "");
                accessGreen[i] = (greenPass == "");
                accessYellow[i] = (yellowPass == "");
                autoKill[i] = false;
                tpOff[i] = false;

            }
            Commands.ChatCommands.Add(new Command("ghostmode", Ghost, "ghost"));
            Commands.ChatCommands.Add(new Command("freezetime", FreezeTime, "freezetime"));
            Commands.ChatCommands.Add(new Command("spawnmob", SpawnMobPlayer, "spawnmobplayer"));
            Commands.ChatCommands.Add(new Command("spawnmob", SpawnAll, "spawnall"));
            Commands.ChatCommands.Add(new Command("spawnmob", SpawnGroup, "spawngroup"));
            Commands.ChatCommands.Add(new Command("autoheal", AutoHeal, "autoheal"));
            Commands.ChatCommands.Add(new Command("fly", Fly, "fly"));
            Commands.ChatCommands.Add(new Command("flymisc", Fetch, "fetch"));
            //Commands.ChatCommands.Add(new Command("flymisc", CarpetBody, "carpetbody"));
            //Commands.ChatCommands.Add(new Command("flymisc", CarpetSides, "carpetsides"));
            Commands.ChatCommands.Add(new Command("editspawn", Mow, "mow"));
            Commands.ChatCommands.Add(new Command("permabuff", permaBuff, "permabuff"));
            Commands.ChatCommands.Add(new Command("permabuff", permaBuffAll, "permabuffall"));
            Commands.ChatCommands.Add(new Command("permabuff", permaBuffGroup, "permabuffgroup"));
            Commands.ChatCommands.Add(new Command("forcegive", ForceGive, "forcegive"));
            Commands.ChatCommands.Add(new Command("killall", KillAll, "killall"));
            Commands.ChatCommands.Add(new Command("mute", Mute, "mute"));
            Commands.ChatCommands.Add(new Command("mute", PermaMute, "permamute"));
            Commands.ChatCommands.Add(new Command("muteall", MuteAll, "muteall"));
            Commands.ChatCommands.Add(new Command("butcher", ButcherAll, "butcherall"));
            Commands.ChatCommands.Add(new Command("butcher", ButcherFriendly, "butcherfriendly"));
            Commands.ChatCommands.Add(new Command("butcher", ButcherNPC, "butchernpc"));
            Commands.ChatCommands.Add(new Command("butcher", ButcherNear, "butchernear"));
            Commands.ChatCommands.Add(new Command("spawnmob", SpawnByMe, "spawnbyme"));
            //Commands.ChatCommands.Add(new Command("clearitems", ClearItems, "clearitems"));
            Commands.ChatCommands.Add(new Command("reloadmore", ReloadMore, "reloadmore"));
            Commands.ChatCommands.Add(new Command("viewall", ViewAll, "viewall"));
            Commands.ChatCommands.Add(new Command(null, TeamUnlock, "teamunlock"));
            Commands.ChatCommands.Add(new Command("autokill", AutoKill, "autokill"));
            Commands.ChatCommands.Add(new Command("forcepvp", ForcePvP, "forcepvp"));
            Commands.ChatCommands.Add(new Command("forcepvp", CanOffPvP, "canoffpvp"));
            Commands.ChatCommands.Add(new Command("antitp", TPOff, "tpoff"));
            Commands.ChatCommands.Add(new Command("moonphase", MoonPhase, "moonphase"));

        }

        private DateTime LastCheck = DateTime.UtcNow;
        private DateTime OtherLastCheck = DateTime.UtcNow;

        public static void MoonPhase(CommandArgs args)
        {

            try
            {

                Main.moonPhase = Convert.ToInt32(args.Parameters[0]);
                NetMessage.SendData((int)PacketTypes.TimeSet, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
                args.Player.SendMessage("The moon phase has been changed!", Color.Red);

            }
            catch (Exception) { args.Player.SendMessage("Invalid phase number!", Color.Red); }

        }

        public static void TPOff(CommandArgs args)
        {

            tpOff[args.Player.Index] = !tpOff[args.Player.Index];
            if (tpOff[args.Player.Index])
                args.Player.SendMessage("Players can no longer TP to you.");
            else
                args.Player.SendMessage("Players can now tp to you.");

        }

        public static void ForcePvP(CommandArgs args)
        {

            /*if (cannotChangePVP)
            {
               forcePVP = !forcePVP;
                if (forcePVP)
                    TShockAPI.TShock.Utils.Broadcast("Forced PVP is now on.");
                else
                    TShockAPI.TShock.Utils.Broadcast("PVP is no longer forced.");
            }
            else
            {*/
                args.Player.SendMessage("Set PvP is not allowed by the server any more.", Color.Red);
                /*foreach (TSPlayer tsply in TShock.Players)
                {
                    args.Player.SendMessage("Set PvP is not allowed by the server any more.", Color.Red);
                }
                TShockAPI.TShock.Utils.Broadcast("Everyone has had PVP activated.");

            }
            */
        }

        public static void CanOffPvP(CommandArgs args)
        {

            cannotChangePVP = !cannotChangePVP;
            if (cannotChangePVP)
                args.Player.SendMessage("PVP will now be forced when you use the /forcepvp command.");
            else
            {
                TShockAPI.TShock.Utils.Broadcast("PVP is no longer forced.");
                forcePVP = false;
            }

        }

        public static void AutoKill(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                List<TSPlayer> plyList = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);
                if (plyList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (plyList.Count < 1)
                {

                    args.Player.SendMessage(plyList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    if (!plyList[0].Group.HasPermission("autokill") || args.Player == plyList[0])
                    {
                        autoKill[plyList[0].Index] = !autoKill[plyList[0].Index];
                        if (autoKill[plyList[0].Index])
                        {
                            args.Player.SendMessage(plyList[0].Name + " is now being auto-killed.");
                            plyList[0].SendMessage("You are now being auto-killed.  Beg for mercy, that you may be spared.");
                        }
                        else
                        {
                            args.Player.SendMessage(plyList[0].Name + " is no longer being auto-killed.");
                            plyList[0].SendMessage("You have been pardoned.");
                        }
                    }
                    else
                    {

                        args.Player.SendMessage("You cannot autokill someone with the autokill permission.", Color.Red);

                    }

                }

            } else {

                args.Player.SendMessage("Invalid syntax.  Proper Syntax: /autokill playername", Color.Red);

            }

        }

        public static void TeamUnlock(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                string str = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                
                switch (args.Parameters[0].ToLower())
                {

                    case "red": if (str == redPass)
                        {

                            accessRed[args.Player.Index] = true;
                            args.Player.SendMessage("The red team has been unlocked.");

                        }
                        else
                        {

                            args.Player.SendMessage("That is not the password.", Color.Red);

                        }
                        break;
                    case "blue": if (str == bluePass)
                        {

                            accessBlue[args.Player.Index] = true;
                            args.Player.SendMessage("The blue team has been unlocked.");

                        }
                        else
                        {

                            args.Player.SendMessage("That is not the password.", Color.Red);

                        }
                        break;
                    case "green": if (str == greenPass)
                        {

                            accessGreen[args.Player.Index] = true;
                            args.Player.SendMessage("The green team has been unlocked.");

                        }
                        else
                        {

                            args.Player.SendMessage("That is not the password.", Color.Red);

                        }
                        break;
                    case "yellow": if (str == yellowPass)
                        {

                            accessYellow[args.Player.Index] = true;
                            args.Player.SendMessage("The yellow team has been unlocked.");

                        }
                        else
                        {

                            args.Player.SendMessage("That is not the password.", Color.Red);

                        }
                        break;
                    default: args.Player.SendMessage("That is not a valid team color.", Color.Red); break;

                }
            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /teamunlock teamcolor password", Color.Red);

            }

        }

        public static void ViewAll(CommandArgs args)
        {

            viewAll[args.Player.Index] = !viewAll[args.Player.Index];
            if (viewAll[args.Player.Index])
                args.Player.SendMessage("View All mode has been turned on.");
            else
            {
                args.Player.SetTeam(Main.player[args.Player.Index].team);
                foreach (TSPlayer tply in TShock.Players)
                {

                    try
                    {

                        NetMessage.SendData((int)PacketTypes.PlayerTeam, args.Player.Index, -1, "", tply.Index);

                    }
                    catch (Exception) { }

                }
                args.Player.SendMessage("View All mode has been turned off.");
            }

        }

        public static void SpawnGroup(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                try
                {

                    Dictionary<NPC, int> groupSpawn = GetSpawnBuffByName(args.Parameters[0]);
                    if (groupSpawn.Count < 1)
                    {

                        args.Player.SendMessage("Invalid Spawn Group name.", Color.Red);

                    }
                    else
                    {

                        if (args.Parameters.Count > 1)
                        {

                            try
                            {

                                double multiplier = Convert.ToDouble(args.Parameters[1]);
                                foreach (KeyValuePair<NPC, int> entry in groupSpawn)
                                {

                                    int amount = (int)(entry.Value * multiplier);
                                    if (amount > 1000)
                                    {

                                        amount = 1000;

                                    }
                                    TSPlayer.Server.SpawnNPC(entry.Key.type, entry.Key.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);
                                    TShockAPI.TShock.Utils.Broadcast(entry.Key.name + " was spawned " + amount.ToString() + " times.");

                                }

                            }
                            catch (Exception) { args.Player.SendMessage("Invalid Syntax.  Proper Syntax: /spawngroup spawngroupname [multiplier]", Color.Red); }

                        }
                        else
                        {

                            foreach (KeyValuePair<NPC, int> entry in groupSpawn)
                            {

                                TSPlayer.Server.SpawnNPC(entry.Key.type, entry.Key.name, entry.Value, args.Player.TileX, args.Player.TileY, 50, 20);
                                TShockAPI.TShock.Utils.Broadcast(entry.Key.name + " was spawned " + entry.Value.ToString() + " times.");

                            }

                        }

                    }

                }
                catch (Exception) { args.Player.SendMessage("Invalid spawn group name.", Color.Red); }

            }
            else
            {

                args.Player.SendMessage("Invalid Syntax.  Proper Syntax: /spawngroup spawngroupname [multiplier]");

            }

        }

        public static void SpawnAll(CommandArgs args)
        {

            int amount = 1;
            if (args.Parameters.Count > 0)
            {

                try
                {

                    amount = Convert.ToInt32(args.Parameters[0]);

                }
                catch (Exception) { args.Player.SendMessage("Improper Syntax.  Proper Syntax: /spawnall [amount]", Color.Red); return; }

            }
            for (int i = 0; i < Main.maxNPCTypes; i++)
            {

                var npc = TShockAPI.TShock.Utils.GetNPCById(i);
                if (!npc.name.ToLower().StartsWith("dungeon guar"))
                TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);

            }
            if (amount > 1000 / Main.maxNPCTypes)
            {

                amount = 1000 / Main.maxNPCTypes;

            }
            TShockAPI.TShock.Utils.Broadcast(args.Player.Name + " has spawned every npc " + amount.ToString() + " times!");

        }

        public static void ClearItems(CommandArgs args) {

            int radius = 50;
            if (args.Parameters.Count > 0)
            {

                if (args.Parameters[0].ToLower() == "all")
                {

                    radius = Int32.MaxValue / 16;

                }
                else
                {

                    try
                    {

                        radius = Convert.ToInt32(args.Parameters[0]);

                    }
                    catch (Exception) { args.Player.SendMessage("Please either enter the keyword \"all\", or the block radius you wish to delete all items from.", Color.Red); return; }

                }

            }
            int count = 0;
            for (int i = 0; i < 200; i++)
            {

                if ((distance(new Vector2(Main.item[i].position.X, Main.item[i].position.Y), new Point((int)args.Player.X, (int)args.Player.Y)) < radius * 16) && (Main.item[i].active))
                {

                    Main.item[i].active = false;
                    NetMessage.SendData(0x15, -1, -1, "", i, 0f, 0f, 0f, 0);
                    count++;
                }

            }
            args.Player.SendMessage("All " + count.ToString() + " items within a radius of " + radius.ToString() + " have been deleted.");

        }

        public static void ReloadMore(CommandArgs args)
        {

            reload();
            args.Player.SendMessage("More Admin Commands Config file successfully reloaded.");

        }

        public static void permaBuffGroup(CommandArgs args) {

            if (args.Parameters.Count() > 1)
            {

                string str = args.Parameters[0].ToLower();
                if (TShock.Groups.GroupExists(str))
                {

                    List<int> buffs = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[1]);
                    List<int> buffs2 = GetGroupBuffByName(args.Parameters[1]);
                    bool isGroup = false;
                    if (args.Parameters[1].ToLower() == "off")
                    {

                        if (!buffsUsedGroup.ContainsKey(str))
                        {

                            args.Player.SendMessage("There are no permabuffs currently applied to this group.", Color.Red);

                        }
                        else
                        {
                            try
                            {

                                buffsUsedGroup[str].Clear();

                            }
                            catch (Exception) { }
                            args.Player.SendMessage("The " + str + " group has had all buffs removed.");
                        }
                        return;

                    }
                    if (buffs.Count + buffs2.Count < 1) {

                        args.Player.SendMessage("No buffs by that name can be found.", Color.Red);

                    }
                    else if (buffs.Count + buffs2.Count > 1)
                    {

                        args.Player.SendMessage("More than one buff matched.", Color.Red);

                    }
                    else
                    {

                        if (buffs2.Count == 1)
                        {

                            isGroup = true;

                        }
                        if (!buffsUsedGroup.ContainsKey(str))
                        {

                            if (!isGroup)
                            {
                                List<int> tempList = new List<int>();
                                tempList.Add(buffs[0]);
                                buffsUsedGroup.Add(str, tempList);
                                args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff added.");
                            }
                            else
                            {

                                foreach (int id in buffGroups.Values.ToArray()[buffs2[0]])
                                {
                                    List<int> tempList = new List<int>();
                                    tempList.Add(id);
                                    List<int> tempList2 = new List<int>();
                                    if (!buffsUsedGroup.Keys.ToArray().Contains(str))
                                    {
                                        buffsUsedGroup.Add(str, tempList);
                                    }
                                    else
                                    {
                                        buffsUsedGroup.TryGetValue(str, out tempList2);
                                        buffsUsedGroup.Remove(str);
                                        tempList2.Add(id);
                                        buffsUsedGroup.Add(str, tempList2);

                                    }
                                    args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff added.");
                                }

                            }

                        }
                        else
                        {

                            try
                            {
                                List<int> tempChangeList;
                                buffsUsedGroup.TryGetValue(str, out tempChangeList);
                                if (!isGroup)
                                {
                                    if (!tempChangeList.Contains(buffs[0]))
                                    {

                                        tempChangeList.Add(buffs[0]);
                                        args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff added.");

                                    }
                                    else
                                    {

                                        tempChangeList.Remove(buffs[0]);
                                        args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(buffs[0]) + " permabuff removed.");

                                    }
                                }
                                else
                                {

                                    foreach (int id in buffGroups.Values.ToArray()[buffs2[0]])
                                    {

                                        if (!tempChangeList.Contains(id))
                                        {

                                            tempChangeList.Add(id);
                                            args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff added.");

                                        }
                                        else
                                        {

                                            tempChangeList.Remove(id);
                                            args.Player.SendMessage("The " + str + " group has had the " + TShockAPI.TShock.Utils.GetBuffName(id) + " permabuff removed.");

                                        }

                                    }

                                }
                                buffsUsedGroup.Remove(str);
                                buffsUsedGroup.Add(str, tempChangeList);

                            }
                            catch (Exception) { args.Player.SendMessage("There was an error with the command, please report this to the plugin developer.", Color.Red); }

                        }

                    }

                }
                else
                {

                    args.Player.SendMessage("The specified group does not exist.", Color.Red);

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /permabuffgroup groupname buffname", Color.Red);

            }

        }

        public static void MuteAll(CommandArgs args)
        {

            muteAll = !muteAll;
            if (muteAll)
            {
                muteAllReason = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    muteAllReason += args.Parameters[i];
                    if (i < args.Parameters.Count - 1)
                    {

                        muteAllReason += " ";

                    }

                }
                if (muteAllReason == "")
                {

                    muteAllReason = defaultMuteAllReason;

                }
                TShockAPI.TShock.Utils.Broadcast(args.Player.Name + " has muted everyone.");
                args.Player.SendMessage("You have muted everyone without the mute permission.  They will remain muted until you use /muteall again.");
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {

                    muteAllFree[i] = false;

                }
                TShockAPI.TShock.Utils.Broadcast(args.Player.Name + " has unmuted everyone, except perhaps those muted before everyone was muted.");
            }
            

        }

        public static void SpawnByMe(CommandArgs args)
        {

            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnbyme <mob name/id> [amount]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing mob name/id", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count >= 2 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnbyme <mob name/id> [amount]", Color.Red);
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) mob matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 2, 3);
                    TShockAPI.TShock.Utils.Broadcast(string.Format("{0} was spawned {1} time(s).", npc.name, amount));
                }
                else
                    args.Player.SendMessage("Invalid mob type!", Color.Red);
            }

        }

        public static void ButcherNear(CommandArgs args)
        {

            int nearby = 50;
            if (args.Parameters.Count > 0)
            {
                try
                {
                    
                    nearby = Convert.ToInt32(args.Parameters[0]);

                }
                catch (Exception) { args.Player.SendMessage("Improper Syntax. Proper Syntax: /butchernear [distance]"); return; }
            }
            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if ((Main.npc[i].active) && (distance(new Vector2(Main.item[i].position.X, Main.item[i].position.Y), new Point((int)Main.player[args.Player.Index].position.X, (int)Main.player[args.Player.Index].position.Y)) < nearby * 16))
                {
                    
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TShockAPI.TShock.Utils.Broadcast(string.Format("Killed {0} NPCs within a radius of " + nearby.ToString() + " blocks.", killcount));

        }

        public static void ButcherAll(CommandArgs args)
        {

            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TShockAPI.TShock.Utils.Broadcast(string.Format("Killed {0} NPCs.", killcount));

        }

        public static void ButcherFriendly(CommandArgs args)
        {

            int killcount = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].active && Main.npc[i].townNPC)
                {
                    TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                    killcount++;
                }
            }
            TShockAPI.TShock.Utils.Broadcast(string.Format("Killed {0} friendly NPCs.", killcount));

        }

        public static void ButcherNPC(CommandArgs args)
        {

            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /butchernpc <npc name/id>", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing npc name/id", Color.Red);
                return;
            }
            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid npc type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) npc matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    int killcount = 0;
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == npc.type)
                        {
                            TSPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
                            killcount++;
                        }
                    }
                    TShockAPI.TShock.Utils.Broadcast(string.Format("Killed {0} " + npc.name + "(s).", killcount));
                }
                else
                    args.Player.SendMessage("Invalid npc type!", Color.Red);
            }

        }

        public static void Mute(CommandArgs args)
        {

            if (args.Parameters.Count() > 0)
            {

                List<TSPlayer> tply = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);
                if (tply.Count() > 1)
                {

                    args.Player.SendMessage("More than 1 player matched.", Color.Red);

                }
                else if (tply.Count() < 1)
                {

                    args.Player.SendMessage("No players found under that name.", Color.Red);

                }
                else
                {

                    if (!tply[0].Group.HasPermission("mute"))
                    {
                        if (!muteAll)
                        {
                            if (args.Parameters.Count() > 1)
                            {

                                try
                                {

                                    int secs = Convert.ToInt32(args.Parameters[1]);
                                    muteTime[tply[0].Index] = secs;
                                    muted[tply[0].Index] = true;
                                    args.Player.SendMessage(tply[0].Name + " has been muted for " + secs.ToString() + " seconds.");
                                    tply[0].SendMessage("You have been muted by an admin for " + secs.ToString() + " seconds.", Color.Red);

                                }
                                catch (Exception) { args.Player.SendMessage("Please enter a valid integer for the seconds you wish to mute.", Color.Red); }

                            }
                            else
                            {

                                muteTime[tply[0].Index] = -1;
                                if (!muted[tply[0].Index])
                                {

                                    muted[tply[0].Index] = true;
                                    args.Player.SendMessage(tply[0].Name + " has been muted until he/she either leaves, or you use /mute again to unmute.");
                                    tply[0].SendMessage("You have been muted by an admin.", Color.Red);

                                }
                                else
                                {

                                    muted[tply[0].Index] = false;
                                    var readTableIP = SQLEditor.ReadColumn("muteList", "IP", new List<SqlValue>());
                                    if (!readTableIP.Contains(tply[0].IP))
                                    {
                                        args.Player.SendMessage(tply[0].Name + " has been unmuted.");
                                    }
                                    else
                                    {
                                        args.Player.SendMessage(tply[0].Name + " has been unmuted, but will still be muted upon entry until taken off of the perma-mute list.");
                                    }
                                    tply[0].SendMessage("You have been unmuted.");

                                }

                            }
                        }
                        else
                        {

                            if (args.Parameters.Count() > 1)
                            {

                                try
                                {

                                    int secs = Convert.ToInt32(args.Parameters[1]);
                                    muteTime[tply[0].Index] = secs;
                                    muted[tply[0].Index] = true;
                                    args.Player.SendMessage(tply[0].Name + " has been muted for " + secs.ToString() + " seconds.");
                                    tply[0].SendMessage("You have been muted by an admin for " + secs.ToString() + " seconds.", Color.Red);

                                }
                                catch (Exception) { args.Player.SendMessage("Please enter a valid integer for the seconds you wish to mute.", Color.Red); }

                            }
                            else
                            {

                                muteTime[tply[0].Index] = -1;
                                if (muteAllFree[tply[0].Index])
                                {

                                    muteAllFree[tply[0].Index] = false;
                                    args.Player.SendMessage(tply[0].Name + " has been muted with everyone else of the muteall variety.");
                                    tply[0].SendMessage("You have been muted by an admin.", Color.Red);

                                }
                                else
                                {

                                    muted[tply[0].Index] = false;
                                    muteAllFree[tply[0].Index] = true;
                                    args.Player.SendMessage(tply[0].Name + " has been unmuted.");
                                    tply[0].SendMessage("You have been unmuted.");

                                }

                            }

                        }

                    }
                    else
                    {

                        args.Player.SendMessage("You cannot mute this player.", Color.Red);

                    }

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /mute player [seconds]", Color.Red);

            }

        }

        public static void PermaMute(CommandArgs args)
        {

            if (args.Parameters.Count() > 0)
            {

                List<TSPlayer> tply = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[0]);
                var readTableName = SQLEditor.ReadColumn("muteList", "Name", new List<SqlValue>());
                var readTableIP = SQLEditor.ReadColumn("muteList", "IP", new List<SqlValue>());
                if (tply.Count() > 1)
                {

                    args.Player.SendMessage("More than 1 player matched.", Color.Red);

                }
                else if (tply.Count() < 1)
                {

                    if (readTableName.Contains(args.Parameters[0].ToLower()))
                    {

                        List<SqlValue> theList = new List<SqlValue>();
                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("Name", "'" + args.Parameters[0].ToLower() + "'"));
                        SQLWriter.DeleteRow("muteList", where);
                        args.Player.SendMessage(args.Parameters[0] + " has been successfully been removed from the perma-mute list.");

                    }
                    else
                    {

                        args.Player.SendMessage("No players found under that name on the server or in the perma-mute list.", Color.Red);

                    }

                }
                else
                {

                    muteTime[tply[0].Index] = -1;
                    string str = tply[0].Name.ToLower();
                    int index = SearchTable(SQLEditor.ReadColumn("muteList", "Name", new List<SqlValue>()), str);
                    if (index == -1)
                    {

                        List<SqlValue> theList = new List<SqlValue>();
                        theList.Add(new SqlValue("Name", "'" + str + "'"));
                        theList.Add(new SqlValue("IP", "'" + tply[0].IP + "'"));
                        SQLEditor.InsertValues("muteList", theList);
                        muted[tply[0].Index] = true;
                        args.Player.SendMessage(tply[0].Name + " has been permamuted by his/her IP Address.");
                        tply[0].SendMessage("You have been muted by an admin.", Color.Red);

                    }
                    else
                    {

                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("IP", "'" + tply[0].IP + "'"));
                        SQLWriter.DeleteRow("muteList", where);
                        muted[tply[0].Index] = false;
                        args.Player.SendMessage(tply[0].Name + " has been taken off the perma-mute list, and is now un-muted.");
                        tply[0].SendMessage("You have been unmuted.");

                    }

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /permamute player", Color.Red);

            }

        }

        private void OnJoin(int ply, HandledEventArgs e)
        {

            var player = new TSPlayer(ply);
            var readTableIP = SQLEditor.ReadColumn("muteList", "IP", new List<SqlValue>());
            if (readTableIP.Contains(player.IP))
            {

                muted[ply] = true;
                muteTime[ply] = -1;
                for (int i = 0; i < 256; i++)
                {

                    try
                    {

                        TSPlayer tsplr = TShock.Players[i];
                        if ((tsplr.Group.HasPermission("mute")) || (tsplr.Group.Name == "superadmin"))
                        {

                            tsplr.SendMessage("A player that is on the perma-mute list is about to enter the server, and has been muted.");

                        }

                    }
                    catch (Exception) { }

                }

            }
            else
            {

                muteTime[ply] = -1;
                muted[ply] = false;

            }

        }

        private void OnLeave(int ply)
        {
            isGhost[ply] = false;
            isHeal[ply] = false;
            flyMode[ply] = false;
            muted[ply] = false;
            muteAllFree[ply] = false;
            viewAll[ply] = false;
            foreach (Vector2 entry in carpetPoints[ply])
            {

                Main.tile[(int)entry.X, (int)entry.Y].active = false;

            }
            carpetPoints[ply] = new List<Vector2>();
            buffsUsed[ply] = new List<int>();
            accessRed[ply] = (redPass == "");
            accessBlue[ply] = (bluePass == "");
            accessGreen[ply] = (greenPass == "");
            accessYellow[ply] = (yellowPass == "");
            autoKill[ply] = false;
            tpOff[ply] = false;
        }

        void OnGetData(GetDataEventArgs e)
        {
            
            if (e.MsgID == PacketTypes.PlayerHp)
            {

                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var playerID = reader.ReadByte();
                    var theHP = reader.ReadInt16();
                    var theMaxHP = reader.ReadInt16();
                    if (isHeal[playerID])
                    {

                        Item heart = TShockAPI.TShock.Utils.GetItemById(58);
                        Item star = TShockAPI.TShock.Utils.GetItemById(184);
                        if (theHP <= theMaxHP / 2)
                        {

                            for (int i = 0; i < 20; i++)
                                TShock.Players[playerID].GiveItem(heart.type, heart.name, heart.width, heart.height, heart.maxStack);
                            for (int i = 0; i < 10; i++)
                                TShock.Players[playerID].GiveItem(star.type, star.name, star.width, star.height, star.maxStack);
                            TShock.Players[playerID].SendMessage("You just got healed!");
                        }

                    }

                }

            }
            else if (e.MsgID == PacketTypes.PlayerMana)
            {

                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var playerID = reader.ReadByte();
                    var theMana = reader.ReadInt16();
                    var theMaxMana = reader.ReadInt16();
                    if (isHeal[playerID])
                    {

                        Item heart = TShockAPI.TShock.Utils.GetItemById(58);
                        Item star = TShockAPI.TShock.Utils.GetItemById(184);
                        if (theMana <= theMaxMana / 2)
                        {

                            for (int i = 0; i < 20; i++)
                                TShock.Players[playerID].GiveItem(heart.type, heart.name, heart.width, heart.height, heart.maxStack);
                            for (int i = 0; i < 10; i++)
                                TShock.Players[playerID].GiveItem(star.type, star.name, star.width, star.height, star.maxStack);
                            TShock.Players[playerID].SendMessage("You just got healed!");
                        }

                    }

                }

            }
            else if (e.MsgID == PacketTypes.PlayerDamage)
            {

                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var ply = reader.ReadByte();
                    var hitDirection = reader.ReadByte();
                    var damage = reader.ReadInt16();
                    if ((damage > maxDamage || damage < 0) && !TShock.Players[e.Msg.whoAmI].Group.HasPermission("ignorecheatdetection") && e.Msg.whoAmI != ply)
                    {

                        if (maxDamageBan)
                        {

                            TShockAPI.TShock.Utils.Ban(TShock.Players[e.Msg.whoAmI], "You have exceeded the max damage limit.");

                        }
                        else if (maxDamageKick)
                        {

                            TShockAPI.TShock.Utils.Kick(TShock.Players[e.Msg.whoAmI], "You have exceeded the max damage limit.");

                        }
                        if (maxDamageIgnore)
                        {

                            e.Handled = true;

                        }

                    }
                    if (viewAll[ply])
                    {

                        e.Handled = true;

                    }

                }

            }
            else if (e.MsgID == PacketTypes.NpcStrike)
            {
                
                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var npcID = reader.ReadInt16();
                    var damage = reader.ReadInt16();
                    if ((damage > maxDamage || damage < 0) && !TShock.Players[e.Msg.whoAmI].Group.HasPermission("ignorecheatdetection"))
                    {

                        if (maxDamageBan)
                        {

                            TShockAPI.TShock.Utils.Ban(TShock.Players[e.Msg.whoAmI], "You have exceeded the max damage limit.");

                        }
                        else if (maxDamageKick)
                        {

                            TShockAPI.TShock.Utils.Kick(TShock.Players[e.Msg.whoAmI], "You have exceeded the max damage limit.");

                        }
                        if (maxDamageIgnore)
                        {

                            e.Handled = true;

                        }

                    }

                }

            }
            else if (e.MsgID == PacketTypes.DoorUse)
            {

                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var Closed = reader.ReadBoolean();
                    var TileX = reader.ReadInt32();
                    var TileY = reader.ReadInt32();
                    if (!Closed)
                    {

                        if (Main.tile[TileX - 1, TileY].type == 10 || Main.tile[TileX + 1, TileY].type == 10)
                        {

                            e.Handled = true;

                        }

                    }

                }

            }
            else if (e.MsgID == PacketTypes.PlayerTeam)
            {

                using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                {
                    var reader = new BinaryReader(data);
                    var ply = reader.ReadByte();
                    var team = reader.ReadByte();
                    switch (team)
                    {

                        case 1: if ((!accessRed[ply]) && (TShock.Players[ply].Group.Name != "superadmin"))
                            {

                                e.Handled = true;
                                TShock.Players[ply].SendMessage("This team is locked, use /teamunlock red password to access it.", Color.Red);
                                TShock.Players[ply].SetTeam(TShock.Players[ply].Team);

                            } break;
                        case 2: if ((!accessGreen[ply]) && (TShock.Players[ply].Group.Name != "superadmin"))
                            {

                                e.Handled = true;
                                TShock.Players[ply].SendMessage("This team is locked, use /teamunlock green password to access it.", Color.Red);
                                TShock.Players[ply].SetTeam(TShock.Players[ply].Team);

                            } break;
                        case 3: if ((!accessBlue[ply]) && (TShock.Players[ply].Group.Name != "superadmin"))
                            {

                                e.Handled = true;
                                TShock.Players[ply].SendMessage("This team is locked, use /teamunlock blue password to access it.", Color.Red);
                                TShock.Players[ply].SetTeam(TShock.Players[ply].Team);

                            } break;
                        case 4: if ((!accessYellow[ply]) && (TShock.Players[ply].Group.Name != "superadmin"))
                            {

                                e.Handled = true;
                                TShock.Players[ply].SendMessage("This team is locked, use /teamunlock yellow password to access it.", Color.Red);
                                TShock.Players[ply].SetTeam(TShock.Players[ply].Team);

                            } break;

                    }

                }

            }
        }

        public void OnSendData(SendDataEventArgs e)
        {
            try
            {
                List<int> ghostIDs = new List<int>();
                for (int i = 0; i < 256; i++)
                {

                    if (isGhost[i])
                    {

                        ghostIDs.Add(i);

                    }

                }
                switch (e.MsgID)
                {
                    case PacketTypes.DoorUse:
                    case PacketTypes.EffectHeal:
                    case PacketTypes.EffectMana:
                    case PacketTypes.PlayerDamage:
                    case PacketTypes.Zones:
                    case PacketTypes.PlayerAnimation:
                    case PacketTypes.PlayerTeam:
                    case PacketTypes.PlayerSpawn:
                        if ((ghostIDs.Contains(e.number)) && (isGhost[e.number]))
                            e.Handled = true;
                        break;
                    case PacketTypes.ProjectileNew:
                    case PacketTypes.ProjectileDestroy:
                        if ((ghostIDs.Contains(e.ignoreClient)) && (isGhost[e.ignoreClient]))
                            e.Handled = true;
                        break;
                    default: break;

                }
                if ((e.number >= 0) && (e.number <= 255) && (isGhost[e.number]))
                {

                    if ((!cansend) && (e.MsgID == PacketTypes.PlayerUpdate))
                    {

                        e.Handled = true;

                    }
                }
            }
            catch (Exception) { }

        }

        private static void KillAll(CommandArgs args)
        {
            foreach (TSPlayer plr in TShock.Players)
            {
                try
                {
                    if (plr != args.Player)
                    {
                        plr.DamagePlayer(999999);
                        plr.SendMessage(string.Format("{0} just killed you! (along with everyone else)", args.Player.Name));
                    }
                }
                catch (Exception) { }
            }
            args.Player.SendMessage(string.Format("You just killed everyone!"));
        }

        private static void ForceGive(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /forcegive <item type/id> <player> [item amount]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing item name/id", Color.Red);
                return;
            }
            if (args.Parameters[1].Length == 0)
            {
                args.Player.SendMessage("Missing player name", Color.Red);
                return;
            }
            int itemAmount = 0;
            var items = TShockAPI.TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
            args.Parameters.RemoveAt(0);
            string plStr = args.Parameters[0];
            args.Parameters.RemoveAt(0);
            if (args.Parameters.Count > 0)
                int.TryParse(args.Parameters[args.Parameters.Count - 1], out itemAmount);


            if (items.Count == 0)
            {
                args.Player.SendMessage("Invalid item type!", Color.Red);
            }
            else if (items.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) item matched!", items.Count), Color.Red);
            }
            else
            {
                var item = items[0];
                if (item.type >= 1 && item.type < Main.maxItemTypes)
                {
                    var players = TShockAPI.TShock.Utils.FindPlayer(plStr);
                    if (players.Count == 0)
                    {
                        args.Player.SendMessage("Invalid player!", Color.Red);
                    }
                    else if (players.Count > 1)
                    {
                        args.Player.SendMessage("More than one player matched!", Color.Red);
                    }
                    else
                    {
                        var plr = players[0];
                        int stacks = 1;
                        if (itemAmount == 0)
                            itemAmount = item.maxStack;
                        if (itemAmount > item.maxStack)
                            stacks = itemAmount / item.maxStack + 1;
                        for (int i = 1; i < stacks; i++)
                            plr.GiveItem(item.type, item.name, item.width, item.height, item.maxStack);
                        if (itemAmount - (itemAmount / item.maxStack) * item.maxStack != 0)
                            plr.GiveItem(item.type, item.name, item.width, item.height, itemAmount - (itemAmount / item.maxStack) * item.maxStack);
                        args.Player.SendMessage(string.Format("Gave {0} {1} {2}(s).", plr.Name, itemAmount, item.name));
                        plr.SendMessage(string.Format("{0} gave you {1} {2}(s).", args.Player.Name, itemAmount, item.name));
                    }
                }
                else
                {
                    args.Player.SendMessage("Invalid item type!", Color.Red);
                }
            }
        }

        public static void permaBuff(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {

                args.Player.SendMessage("Improper Syntax! Proper Syntax: /permabuff buff [player]", Color.Red);

            }
            else if (args.Parameters.Count == 1)
            {

                int id = 0;
                bool isGroup = false;
                if (!int.TryParse(args.Parameters[0], out id))
                {
                    var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                    List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                    if (args.Parameters[0].ToLower() == "off")
                    {

                        if (buffsUsed[args.Player.Index].Count() > 0)
                        {

                            buffsUsed[args.Player.Index].Clear();
                            args.Player.SendMessage("You have had all permabuffs removed.");

                        }
                        else
                        {

                            args.Player.SendMessage("You do not currently have any permabuffs applied (solely) to yourself.");

                        }
                        return;

                    }
                    if (found.Count + found2.Count == 0)
                    {
                        args.Player.SendMessage("Invalid buff name!", Color.Red);
                        return;
                    }
                    else if (found.Count + found2.Count > 1)
                    {
                        args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                        return;
                    }
                    if (found.Count == 1)
                    {

                        id = found[0];

                    }
                    else if (found2.Count == 1)
                    {

                        id = found2[0];
                        isGroup = true;

                    }
                    else
                    {

                        return;

                    }
                }
                if (!isGroup)
                {
                    if (id > 0 && id < Main.maxBuffs)
                    {
                        if (!buffsUsed[args.Player.Index].Contains(id))
                        {
                            args.Player.SetBuff(id, short.MaxValue);
                            buffsUsed[args.Player.Index].Add(id);
                            args.Player.SendMessage(string.Format("You have permabuffed yourself with {0}({1})!",
                                TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                        else
                        {
                            buffsUsed[args.Player.Index].Remove(id);
                            args.Player.SendMessage(string.Format("You have removed your {0} permabuff.",
                                TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                        }
                    }
                }
                else
                {

                    foreach (int id2 in buffGroups.Values.ToArray()[id])
                    {
                            args.Player.SetBuff(id2, short.MaxValue);
                            if (!buffsUsed[args.Player.Index].Contains(id2))
                            buffsUsed[args.Player.Index].Add(id2);
                            args.Player.SendMessage(string.Format("You have permabuffed yourself with {0}({1})!",
                                TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id2)), Color.Green);
                    }

                }

            }
            else
            {

                string str = "";
                for (int i = 1; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    int id = 0;
                    bool isGroup = false;
                    if (!int.TryParse(args.Parameters[0], out id))
                    {
                        var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                        List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                        if (args.Parameters[0].ToLower() == "off")
                        {

                            if (buffsUsed[thePlayer.Index].Count() > 0)
                            {

                                buffsUsed[thePlayer.Index].Clear();
                                args.Player.SendMessage("You have had all permabuffs removed from " + thePlayer.Name + ".");
                                TShock.Players[thePlayer.Index].SendMessage("You have had all permabuffs removed.");

                            }
                            else
                            {

                                args.Player.SendMessage("You do not currently have any permabuffs applied (solely) to " + thePlayer.Name + ".");

                            }
                            return;

                        }
                        if (found.Count + found2.Count == 0)
                        {
                            args.Player.SendMessage("Invalid buff name!", Color.Red);
                            return;
                        }
                        else if (found.Count + found2.Count > 1)
                        {
                            args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                            return;
                        }
                        if (found.Count == 1)
                        {

                            id = found[0];

                        }
                        else if (found2.Count == 1)
                        {

                            id = found2[0];
                            isGroup = true;

                        }
                        else
                        {

                            return;

                        }
                    }
                    if (!isGroup)
                    {
                        if (id > 0 && id < Main.maxBuffs)
                        {
                            if (!buffsUsed[thePlayer.Index].Contains(id))
                            {
                                thePlayer.SetBuff(id, short.MaxValue);
                                buffsUsed[thePlayer.Index].Add(id);
                                args.Player.SendMessage(string.Format("You have permabuffed " + thePlayer.Name + " with {0}",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);
                                thePlayer.SendMessage(string.Format("You have been permabuffed with {0}({1})!",
                                 TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                            }
                            else
                            {
                                buffsUsed[thePlayer.Index].Remove(id);
                                args.Player.SendMessage(string.Format("You have removed " + thePlayer.Name + "'s {0} permabuff.",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);
                                thePlayer.SendMessage(string.Format("Your {0} permabuff has been removed.",
                                    TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                            }
                        }
                    }
                    else
                    {
                        foreach (int id2 in buffGroups.Values.ToArray()[id])
                        {
                            thePlayer.SetBuff(id2, short.MaxValue);
                            if (!buffsUsed[thePlayer.Index].Contains(id2))
                            buffsUsed[thePlayer.Index].Add(id2);
                            args.Player.SendMessage(string.Format("You have permabuffed " + thePlayer.Name + " with {0}",
                                TShockAPI.TShock.Utils.GetBuffName(id2)), Color.Green);
                            thePlayer.SendMessage(string.Format("You have been permabuffed with {0}({1})!",
                             TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                    }

                }

            }

        }

        public static void permaBuffAll(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {

                args.Player.SendMessage("Improper Syntax! Proper Syntax: /permabuffall buff [player]", Color.Red);

            }
            else if (args.Parameters.Count == 1)
            {

                int id = 0;
                bool isGroup = false;
                if (!int.TryParse(args.Parameters[0], out id))
                {
                    var found = TShockAPI.TShock.Utils.GetBuffByName(args.Parameters[0]);
                    List<int> found2 = GetGroupBuffByName(args.Parameters[0]);
                    if (args.Parameters[0].ToLower() == "off")
                    {

                        if (allBuffsUsed.Count() > 0)
                        {

                            allBuffsUsed.Clear();
                            TShockAPI.TShock.Utils.Broadcast("All Global permabuffs have been deactivated.");

                        }
                        else
                        {

                            args.Player.SendMessage("There are currently no global permabuffs active.");

                        }
                        return;

                    }
                    if (found.Count + found2.Count == 0)
                    {
                        args.Player.SendMessage("Invalid buff name!", Color.Red);
                        return;
                    }
                    else if (found.Count + found2.Count > 1)
                    {
                        args.Player.SendMessage(string.Format("More than one ({0}) buff matched!", found.Count), Color.Red);
                        return;
                    }
                    if (found.Count == 1)
                    {

                        id = found[0];

                    }
                    else if (found2.Count == 1)
                    {

                        id = found2[0];
                        isGroup = true;

                    }
                    else
                    {

                        return;

                    }
                }
                if (!isGroup)
                {
                    if (id > 0 && id < Main.maxBuffs)
                    {
                        if (!allBuffsUsed.Contains(id))
                        {
                            TSPlayer.All.SetBuff(id, short.MaxValue);
                            allBuffsUsed.Add(id);
                            TShockAPI.TShock.Utils.Broadcast(string.Format("Everyone has been permabuffed with {0}({1})!",
                                TShockAPI.TShock.Utils.GetBuffName(id), TShockAPI.TShock.Utils.GetBuffDescription(id)), Color.Green);
                        }
                        else
                        {
                            allBuffsUsed.Remove(id);
                            TShockAPI.TShock.Utils.Broadcast(string.Format("Everyone has had the {0} permabuff removed.",
                                TShockAPI.TShock.Utils.GetBuffName(id)), Color.Green);

                        }
                    }
                }
                else
                {

                    foreach (int id2 in buffGroups.Values.ToArray()[id])
                    {
                        TSPlayer.All.SetBuff(id2, short.MaxValue);
                        if (!allBuffsUsed.Contains(id2))
                            allBuffsUsed.Add(id2);
                        TShockAPI.TShock.Utils.Broadcast(string.Format("Everyone has been permabuffed with {0}({1})!",
                            TShockAPI.TShock.Utils.GetBuffName(id2), TShockAPI.TShock.Utils.GetBuffDescription(id2)), Color.Green);
                    }

                }

            }

        }

        public static void Fly(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {
                flyMode[args.Player.Index] = !flyMode[args.Player.Index];
                carpetY[args.Player.Index] = args.Player.TileY;
                if (flyMode[args.Player.Index])
                {

                    args.Player.SendMessage("Flying carpet activated.");

                }
                else
                {

                    foreach (Vector2 entry in carpetPoints[args.Player.Index])
                    {

                        Main.tile[(int)entry.X, (int)entry.Y].active = false;
                        TSPlayer.All.SendTileSquare((int)entry.X, (int)entry.Y, 1);
                        //carpetPoints.Remove(entry);

                    }
                    args.Player.SendMessage("Flying carpet deactivated.");

                }
            }
            else
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    flyMode[thePlayer.Index] = !flyMode[thePlayer.Index];
                    carpetY[thePlayer.Index] = thePlayer.TileY;
                    if (flyMode[thePlayer.Index])
                    {

                        args.Player.SendMessage("Flying carpet activated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You have been given the flying carpet!");

                    }
                    else
                    {

                        foreach (Vector2 entry in carpetPoints[thePlayer.Index])
                        {

                            Main.tile[(int)entry.X, (int)entry.Y].active = false;
                            TSPlayer.All.SendTileSquare((int)entry.X, (int)entry.Y, 1);
                            //carpetPoints.Remove(entry);

                        }
                        args.Player.SendMessage("Flying carpet deactivated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("Flying carpet deactivated.");

                    }

                }

            }

        }

        public static void Fetch(CommandArgs args)
        {
            
            if (flyMode[args.Player.Index])
            {

                List<Vector2> tilesToUpdate = new List<Vector2>();
                foreach (Vector2 entry in carpetPoints[args.Player.Index])
                {

                    Main.tile[(int)entry.X, (int)entry.Y].active = false;
                    tilesToUpdate.Add(new Vector2(entry.X, entry.Y));
                    carpetY[args.Player.Index] = args.Player.TileY;

                }
                foreach (Vector2 entry in tilesToUpdate)
                {

                    TSPlayer.All.SendTileSquare((int)entry.X, (int)entry.Y, 3);
                    carpetPoints[args.Player.Index].Remove(entry);

                }
                args.Player.SendMessage("Carpet Fetched.");

            }
            else
            {

                args.Player.SendMessage("You have no flying carpet activated.", Color.Red);

            }

        }

        public static void CarpetBody(CommandArgs args)
        {



        }

        public static void CarpetSides(CommandArgs args)
        {



        }
        public static void Mow(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                TShockAPI.DB.Region theRegion = TShock.Regions.GetRegionByName(str);
                if (theRegion != default(TShockAPI.DB.Region))
                {
                    try
                    {
                        int index = SearchTable(SQLEditor.ReadColumn("regionMow", "Name", new List<SqlValue>()), str);
                        if (index == -1)
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            theList.Add(new SqlValue("Name", "'" + str + "'"));
                            theList.Add(new SqlValue("Mow", 1));
                            SQLEditor.InsertValues("regionMow", theList);
                            regionMow.Add(str, true);
                            args.Player.SendMessage(str + " is now set to auto-mow.");

                        }
                        else if (Convert.ToBoolean(SQLEditor.ReadColumn("regionMow", "Mow", new List<SqlValue>())[index]))
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            List<SqlValue> where = new List<SqlValue>();
                            theList.Add(new SqlValue("Mow", 0));
                            where.Add(new SqlValue("Name", "'" + str + "'"));
                            SQLEditor.UpdateValues("regionMow", theList, where);
                            regionMow.Remove(str);
                            regionMow.Add(str, false);
                            args.Player.SendMessage(str + " now has auto-mow turned off.");

                        }
                        else
                        {

                            List<SqlValue> theList = new List<SqlValue>();
                            List<SqlValue> where = new List<SqlValue>();
                            theList.Add(new SqlValue("Mow", 1));
                            where.Add(new SqlValue("Name", "'" + str + "'"));
                            SQLEditor.UpdateValues("regionMow", theList, where);
                            regionMow.Remove(str);
                            regionMow.Add(str, true);
                            args.Player.SendMessage(str + " is now set to auto-mow.");

                        }
                    }
                    catch (Exception) { args.Player.SendMessage("An error occurred when writing to the DataBase.", Color.Red); }

                }
                else
                {

                    args.Player.SendMessage("The specified region does not exist.");

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /mow regionname", Color.Red);

            }

        }

        public static void AutoHeal(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                isHeal[args.Player.Index] = !isHeal[args.Player.Index];
                if (isHeal[args.Player.Index])
                {

                    args.Player.SendMessage("Auto Heal Mode is now on.");

                }
                else
                {

                    args.Player.SendMessage("Auto Heal Mode is now off.");

                }
            }
            else
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    isHeal[thePlayer.Index] = !isHeal[thePlayer.Index];
                    if (isHeal[thePlayer.Index])
                    {

                        args.Player.SendMessage("You have activated auto-heal for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You have been given regenerative powers!");

                    }
                    else
                    {

                        args.Player.SendMessage("You have deactivated auto-heal for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You now have the healing powers of an average human.");

                    }

                }

            }

        }

        public static void Ghost(CommandArgs args)
        {

            if (args.Parameters.Count == 0)
            {
                int tempTeam = args.Player.TPlayer.team;
                args.Player.TPlayer.team = 0;
                NetMessage.SendData(45, -1, -1, "", args.Player.Index);
                args.Player.TPlayer.team = tempTeam;
                if (!isGhost[args.Player.Index])
                {

                    args.Player.SendMessage("Ghost Mode activated!");
                    TShockAPI.TShock.Utils.Broadcast(args.Player.Name + " has left.", Color.Yellow);

                }
                else
                {

                    args.Player.SendMessage("Ghost Mode deactivated!");
                    TShockAPI.TShock.Utils.Broadcast(args.Player.Name + " has joined.", Color.Yellow);

                }
                isGhost[args.Player.Index] = !isGhost[args.Player.Index];
                args.Player.TPlayer.position.X = 0;
                args.Player.TPlayer.position.Y = 0;
                cansend = true;
                NetMessage.SendData(13, -1, -1, "", args.Player.Index);
                cansend = false;
            }
            else
            {

                string str = "";
                for (int i = 0; i < args.Parameters.Count; i++)
                {

                    if (i != args.Parameters.Count - 1)
                    {

                        str += args.Parameters[i] + " ";

                    }
                    else
                    {

                        str += args.Parameters[i];

                    }

                }
                List<TShockAPI.TSPlayer> playerList = TShockAPI.TShock.Utils.FindPlayer(str);
                if (playerList.Count > 1)
                {

                    args.Player.SendMessage("Player does not exist.", Color.Red);

                }
                else if (playerList.Count < 1)
                {

                    args.Player.SendMessage(playerList.Count.ToString() + " players matched.", Color.Red);

                }
                else
                {

                    TShockAPI.TSPlayer thePlayer = playerList[0];
                    int tempTeam = thePlayer.TPlayer.team;
                    thePlayer.TPlayer.team = 0;
                    NetMessage.SendData(45, -1, -1, "", thePlayer.Index);
                    thePlayer.TPlayer.team = tempTeam;
                    if (!isGhost[thePlayer.Index])
                    {

                        args.Player.SendMessage("Ghost Mode activated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You have become a stealthy ninja!");

                    }
                    else
                    {

                        args.Player.SendMessage("Ghost Mode deactivated for " + thePlayer.Name + ".");
                        thePlayer.SendMessage("You no longer have the stealth of a ninja.");

                    }
                    isGhost[thePlayer.Index] = !isGhost[thePlayer.Index];
                    thePlayer.TPlayer.position.X = 0;
                    thePlayer.TPlayer.position.Y = 0;
                    cansend = true;
                    NetMessage.SendData(13, -1, -1, "", thePlayer.Index);
                    cansend = false;

                }

            }

        }

        private void OnUpdate()
        {

            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)
            {
                
                LastCheck = DateTime.UtcNow;
                if (timeFrozen)
                {

                    if (Main.dayTime != freezeDayTime)
                    {

                        if (timeToFreezeAt > 10000)
                        {

                            timeToFreezeAt -= 100;

                        }
                        else
                        {

                            timeToFreezeAt += 100;

                        }

                    }
                    TSPlayer.Server.SetTime(freezeDayTime, timeToFreezeAt);

                }
                foreach (int buffID in allBuffsUsed)
                {

                    foreach (TSPlayer tsplr in TShock.Players)
                    {

                        try
                        {

                            tsplr.SetBuff(buffID, short.MaxValue);

                        }
                        catch (Exception) { }

                    }

                }
                for (int i = 0; i < 256; i++)
                {
                    if (autoKill[i])
                    {

                        TShock.Players[i].DamagePlayer(9999);

                    }
                    if (forcePVP && cannotChangePVP)
                    {

                        try
                        {
                            // TerrariaServer can not set the pvp mode of clients.
                            /*if (TShock.Players[i].Group.Name != "superadmin")
                            {
                                TShock.Players[i].SetPvP(true);
                            }*/

                        }
                        catch (Exception) { }

                    }
                    if (viewAll[i])
                    {

                        foreach (TSPlayer tply in TShock.Players)
                        {

                            try
                            {

                                int prevTeam = Main.player[tply.Index].team;
                                Main.player[tply.Index].team = viewAllTeam;
                                NetMessage.SendData((int)PacketTypes.PlayerTeam, i, -1, "", tply.Index);
                                Main.player[tply.Index].team = prevTeam;

                            }
                            catch (Exception) { }

                        }

                    }
                    foreach (int buffID in buffsUsed[i])
                    {

                        TShock.Players[i].SetBuff(buffID, short.MaxValue);

                    }
                    if (muted[i])
                    {

                        if (muteTime[i] > 0)
                        {

                            muteTime[i] -= 1;
                            if (muteTime[i] <= 0)
                            {

                                muted[i] = false;
                                muteTime[i] = -1;
                                try
                                {
                                    TShock.Players[i].SendMessage("Your time is up, you're free to speak again.");
                                }
                                catch (Exception) { }

                            }

                        }

                    }

                }
                foreach (KeyValuePair<string, List<int>> buffUsedGroup in buffsUsedGroup)
                {

                    for (int i = 0; i < 256; i++)
                    {

                        try
                        {

                            if (TShock.Players[i].Group.Name.ToLower() == buffUsedGroup.Key)
                            {

                                for (int j = 0; j < buffUsedGroup.Value.Count; j++)
                                {

                                    TShock.Players[i].SetBuff(buffUsedGroup.Value[j], short.MaxValue);

                                }

                            }

                        }
                        catch (Exception) { }

                    }

                }
                foreach (KeyValuePair<string, bool> entry in regionMow)
                {

                    if (entry.Value)
                    {

                        TShockAPI.DB.Region theRegion = TShock.Regions.GetRegionByName(entry.Key);
                        if (theRegion != default(TShockAPI.DB.Region))
                        {

                            for (int i = 0; i <= theRegion.Area.Height; i++)
                            {

                                for (int j = 0; j <= theRegion.Area.Width; j++)
                                {

                                    if (Main.tile[theRegion.Area.X + j, theRegion.Area.Y + i].active)
                                    {
                                        switch (Main.tile[theRegion.Area.X + j, theRegion.Area.Y + i].type)
                                        {

                                            case 3:
                                            case 20:
                                            case 24:
                                            case 32:
                                            case 52:
                                            case 61:
                                            case 62:
                                            case 69:
                                            case 70:
                                            case 73:
                                            case 74:
                                            case 82:
                                            case 83:
                                            case 84:
                                                Main.tile[theRegion.Area.X + j, theRegion.Area.Y + i].active = false;
                                                TSPlayer.All.SendTileSquare(theRegion.Area.X + j, theRegion.Area.Y + i, 3);
                                                break;

                                        }
                                    }

                                }

                            }

                        }

                    }

                }
            }
            for (int i = 0; i < 256; i++)
            {
                if (flyMode[i])
                {

                    try
                    {

                        List<Vector2> tilesToUpdate = new List<Vector2>();
                        if ((TShock.Players[i].TileY < carpetY[i] - 9) || ((TShock.Players[i].TileY > carpetY[i]) && (TShock.Players[i].TPlayer.velocity.Y == 0)))
                        {

                            foreach (Vector2 entry in carpetPoints[i])
                            {

                                Main.tile[(int)entry.X, (int)entry.Y].active = false;
                                tilesToUpdate.Add(new Vector2(entry.X, entry.Y));
                                carpetY[i] = TShock.Players[i].TileY + 3;

                            }

                        }
                        foreach (Vector2 entry in carpetPoints[i])
                        {

                            if ((Main.tile[(int)entry.X, (int)entry.Y].type == 54) || (Main.tile[(int)entry.X, (int)entry.Y].type == 30))
                            {
                                if ((entry.Y < TShock.Players[i].TileY + 3) || (entry.Y != carpetY[i] + 3) || (Math.Abs(entry.X - TShock.Players[i].TileX) > 5))
                                {

                                    if ((entry.Y != carpetY[i] + 2) || (Math.Abs(entry.X - TShock.Players[i].TileX) > 6) || (Math.Abs(entry.X - TShock.Players[i].TileX) < 6))
                                    {
                                        Main.tile[(int)entry.X, (int)entry.Y].active = false;
                                        tilesToUpdate.Add(new Vector2(entry.X, entry.Y));
                                    }

                                }
                            }
                            else if ((entry.Y == TShock.Players[i].TileY + 3) && (TShock.Players[i].TPlayer.velocity.Y == 0))
                            {

                                carpetY[i] = TShock.Players[i].TileY;
                                Main.tile[(int)entry.X, (int)entry.Y].type = 54;
                                tilesToUpdate.Add(new Vector2(entry.X, entry.Y));

                            }
                            else if ((entry.X < TShock.Players[i].TileX - 1) || (entry.X > TShock.Players[i].TileX + 2) || (entry.Y != carpetY[i] - 1))
                            {

                                Main.tile[(int)entry.X, (int)entry.Y].active = false;
                                tilesToUpdate.Add(new Vector2(entry.X, entry.Y));

                            }

                        }
                        if (TShock.Players[i].TileY >= carpetY[i])
                        {
                            if (TShock.Players[i].TPlayer.controlDown)
                            {

                                carpetY[i] += 4;

                            }
                        }
                        for (int j = -5; j <= 5; j++)
                        {

                            if (!Main.tile[TShock.Players[i].TileX + j, carpetY[i] + 3].active)
                            {

                                Main.tile[TShock.Players[i].TileX + j, carpetY[i] + 3].type = 54;
                                Main.tile[TShock.Players[i].TileX + j, carpetY[i] + 3].active = true;
                                tilesToUpdate.Add(new Vector2(TShock.Players[i].TileX + j, carpetY[i] + 3));
                                carpetPoints[i].Add(new Vector2(TShock.Players[i].TileX + j, carpetY[i] + 3));

                            }

                        }
                        if (!Main.tile[TShock.Players[i].TileX + 6, carpetY[i] + 2].active)
                        {

                            Main.tile[TShock.Players[i].TileX + 6, carpetY[i] + 2].type = 30;
                            Main.tile[TShock.Players[i].TileX + 6, carpetY[i] + 2].active = true;
                            tilesToUpdate.Add(new Vector2(TShock.Players[i].TileX + 6, carpetY[i] + 2));
                            carpetPoints[i].Add(new Vector2(TShock.Players[i].TileX + 6, carpetY[i] + 2));

                        }
                        if (!Main.tile[TShock.Players[i].TileX - 6, carpetY[i] + 2].active)
                        {

                            Main.tile[TShock.Players[i].TileX - 6, carpetY[i] + 2].type = 30;
                            Main.tile[TShock.Players[i].TileX - 6, carpetY[i] + 2].active = true;
                            tilesToUpdate.Add(new Vector2(TShock.Players[i].TileX - 6, carpetY[i] + 2));
                            carpetPoints[i].Add(new Vector2(TShock.Players[i].TileX - 6, carpetY[i] + 2));

                        }
                        for (int j = -1; j <= 2; j++)
                        {

                            if (!Main.tile[TShock.Players[i].TileX + j, carpetY[i] - 1].active)
                            {

                                Main.tile[TShock.Players[i].TileX + j, carpetY[i] - 1].type = 19;
                                Main.tile[TShock.Players[i].TileX + j, carpetY[i] - 1].active = true;
                                tilesToUpdate.Add(new Vector2(TShock.Players[i].TileX + j, carpetY[i] - 1));
                                carpetPoints[i].Add(new Vector2(TShock.Players[i].TileX + j, carpetY[i] - 1));

                            }

                        }
                        foreach (Vector2 entry in tilesToUpdate)
                        {

                            TSPlayer.All.SendTileSquare((int)entry.X, (int)entry.Y, 3);
                            if (!Main.tile[(int)entry.X, (int)entry.Y].active)
                                carpetPoints[i].Remove(entry);

                        }

                    }
                    catch (Exception) { }

                }

            }

        }

        public void FreezeTime(CommandArgs args)
        {

            timeFrozen = !timeFrozen;
            freezeDayTime = Main.dayTime;
            timeToFreezeAt = Main.time;
            if (timeFrozen)
            {

                TShockAPI.TShock.Utils.Broadcast(args.Player.Name.ToString() + " froze time.");

            }
            else
            {

                TShockAPI.TShock.Utils.Broadcast(args.Player.Name.ToString() + " unfroze time.");

            }

        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {

            if (findIfPlayingCommand(text) && !TShock.Players[ply].Group.HasPermission("ghostmode"))
            {

                string sb = "";
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player != null && player.Active && !isGhost[player.Index])
                    {
                        if (sb.Length != 0)
                        {
                            sb += ", ";
                        }
                        sb += player.Name;
                    }
                }
                TShock.Players[ply].SendMessage(string.Format("Current players: {0}.", sb), 255, 240, 20);
                e.Handled = true;

            }
            if (((muted[ply]) && (findIfMeCommand(text))) || ((muteAll) && (!TShock.Players[ply].Group.HasPermission("mute"))))
            {

                TShock.Players[ply].SendMessage("You cannot use the /me command, you are muted.", Color.Red);
                e.Handled = true;
                return;

            }
            if (text.StartsWith("/tp "))
            {

                string tempText = text;
                tempText = tempText.Remove(0, 1);
                parseParameters(tempText);

            }
            if ((muted[ply] || muteAll) && !TShock.Players[ply].Group.HasPermission("mute"))
            {

                var tsplr = TShock.Players[msg.whoAmI];
                if (text.StartsWith("/"))
                {
                    try
                    {
                        Commands.HandleCommand(tsplr, text);
                    }
                    catch (Exception ex)
                    {
                        Log.ConsoleError("Command exception");
                        Log.Error(ex.ToString());
                    }
                }
                else
                {

                    if (!muteAll)
                    {
                        if (muteTime[tsplr.Index] <= 0)
                        {
                            tsplr.SendMessage("You have been muted by an admin.", Color.Red);
                        }
                        else
                        {
                            tsplr.SendMessage("You have " + muteTime[tsplr.Index].ToString() + " seconds left of muting.", Color.Red);
                        }
                    }
                    else
                    {

                        tsplr.SendMessage("The server is now muted for this reason: " + muteAllReason, Color.Red);

                    }

                }
                e.Handled = true;

            }

        }

        public static void SpawnMobPlayer(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 3)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount] [username]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing mob name/id", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 3 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount] [username]", Color.Red);
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TShockAPI.TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            var players = TShockAPI.TShock.Utils.FindPlayer(args.Parameters[2]);
            if (players.Count == 0)
            {
                args.Player.SendMessage("Invalid player!", Color.Red);
            }
            else if (players.Count > 1)
            {
                args.Player.SendMessage("More than one player matched!", Color.Red);
            }
            else if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) mob matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes)
                {
                    TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, players[0].TileX, players[0].TileY, 50, 20);
                    TShockAPI.TShock.Utils.Broadcast(string.Format("{0} was spawned {1} time(s) nearby {2}.", npc.name, amount, players[0].Name));
                }
                else
                    args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
        }
        public static int SearchTable(List<object> Table, string Query)
        {

            for (int i = 0; i < Table.Count; i++)
            {

                try
                {
                    if (Query == Table[i].ToString())
                    {

                        return (i);

                    }
                }
                catch (Exception) { }

            }
            return (-1);

        }
        public static int distance(Vector2 point1, Point point2)
        {

            return (Convert.ToInt32(Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2))));

        }
        public static void reload()
        {
            buffGroups = new Dictionary<string, List<int>>();
            spawnGroups = new Dictionary<string, Dictionary<NPC, int>>();
            if (!File.Exists(@"tshock/MoreCommandsConfig.txt"))
            {

                File.WriteAllText(@"tshock/MoreCommandsConfig.txt", ";;Maximum damage a person can do, and whether it should be ignored, if the person should be kicked for it, and if the person should be banned for it." + Environment.NewLine +
                    "maxDamage:500" + Environment.NewLine +
                    "maxDamageIgnore:false" + Environment.NewLine +
                    "maxDamageKick:false" + Environment.NewLine +
                    "maxDamageBan:false" + Environment.NewLine +
                    ";;Default reason shown for muting everyone, it the [reason] field in /muteall [reason] is left blank." + Environment.NewLine +
                    "defaultMuteAllMessage:Listen to find out" + Environment.NewLine +
                    ";;Permabuff multiple buffs at once with buff groups." + Environment.NewLine +
                    "buffGroup:movement=(swift,grav,feather,water)" + Environment.NewLine +
                    "buffGroup:defense=(iron,thorn,obsidian)" + Environment.NewLine +
                    ";;Spawn preset enemy groups." + Environment.NewLine +
                    "spawnGroup:night=((zombie,20),(demon eye,10))" + Environment.NewLine +
                    "spawnGroup:day=((Green Slime,20),(Blue Slime,10))" + Environment.NewLine +
                    ";;Add default passwords to teams on startup.  Leave blank for no password necessary." + Environment.NewLine +
                    "redPass:" + Environment.NewLine +
                    "bluePass:" + Environment.NewLine +
                    "greenPass:" + Environment.NewLine +
                    "yellowPass:");
                List<int> tempGroupList = new List<int>();
                try
                {
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("swift")[0]);
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("grav")[0]);
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("feather")[0]);
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("water")[0]);
                }
                catch (Exception) { }
                buffGroups.Add("movement", tempGroupList);
                tempGroupList = new List<int>();
                try
                {
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("iron")[0]);
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("thorn")[0]);
                    tempGroupList.Add(TShockAPI.TShock.Utils.GetBuffByName("obsidian")[0]);
                }
                catch (Exception) { }
                buffGroups.Add("defense", tempGroupList);
                Dictionary<NPC, int> tempSpawnGroupList = new Dictionary<NPC, int>();
                try
                {
                    tempSpawnGroupList.Add(TShockAPI.TShock.Utils.GetNPCByName("zombie")[0], 20);
                    tempSpawnGroupList.Add(TShockAPI.TShock.Utils.GetNPCByName("demon eye")[0], 10);
                }
                catch (Exception) { }
                spawnGroups.Add("night", tempSpawnGroupList);
                tempSpawnGroupList = new Dictionary<NPC, int>();
                try
                {
                    tempSpawnGroupList.Add(TShockAPI.TShock.Utils.GetNPCByName("Green Slime")[0], 20);
                    tempSpawnGroupList.Add(TShockAPI.TShock.Utils.GetNPCByName("Blue Slime")[0], 10);
                }
                catch (Exception) { }
                spawnGroups.Add("day", tempSpawnGroupList);
                redPass = "";
                bluePass = "";
                greenPass = "";
                yellowPass = "";

            }
            else
            {

                using (StreamReader file = new StreamReader(@"tshock/MoreCommandsConfig.txt", true))
                {
                    string currentLine = null;
                    while ((currentLine = file.ReadLine()) != null)
                    {
                        if (currentLine.StartsWith("defaultMuteAllMessage:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 22);
                            defaultMuteAllReason = tempLine;
                        }
                        else if (currentLine.StartsWith("redPass:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 8);
                            redPass = tempLine;
                        }
                        else if (currentLine.StartsWith("bluePass:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 9);
                            bluePass = tempLine;
                        }
                        else if (currentLine.StartsWith("greenPass:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 10);
                            greenPass = tempLine;
                        }
                        else if (currentLine.StartsWith("yellowPass:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 11);
                            yellowPass = tempLine;
                        }
                        else if (currentLine.StartsWith("maxDamage:"))
                        {
                            currentLine.Remove(0, 10);
                            try
                            {

                                maxDamage = Convert.ToInt32(currentLine);

                            }
                            catch (Exception) { }
                        }
                        else if (currentLine.StartsWith("maxDamageIgnore:"))
                        {
                            if (currentLine.ToLower().Contains("true"))
                                maxDamageIgnore = true;
                            else
                                maxDamageIgnore = false;
                        }
                        else if (currentLine.StartsWith("maxDamageKick:"))
                        {
                            if (currentLine.ToLower().Contains("true"))
                                maxDamageKick = true;
                            else
                                maxDamageKick = false;
                        }
                        else if (currentLine.StartsWith("maxDamageBan:"))
                        {
                            if (currentLine.ToLower().Contains("true"))
                                maxDamageBan = true;
                            else
                                maxDamageBan = false;
                        }
                        else if (currentLine.StartsWith("buffGroup:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 10);
                            string buffName = tempLine.Substring(0, tempLine.IndexOf('='));
                            tempLine = tempLine.Remove(0, tempLine.IndexOf('=') + 2);
                            List<int> theBuffs = new List<int>();
                            while (true)
                            {

                                try
                                {
                                    if (tempLine.IndexOf(',') != -1)
                                    {

                                        theBuffs.Add(TShockAPI.TShock.Utils.GetBuffByName(tempLine.Substring(0, tempLine.IndexOf(',')))[0]);
                                        tempLine = tempLine.Remove(0, tempLine.IndexOf(',') + 1);
                                    }
                                    else
                                    {
                                        theBuffs.Add(TShockAPI.TShock.Utils.GetBuffByName(tempLine.Substring(0, tempLine.IndexOf(')')))[0]);
                                        break;
                                    }
                                }
                                catch (Exception ex) { break; }

                            }
                            buffGroups.Add(buffName, theBuffs);
                        }
                        else if (currentLine.StartsWith("spawnGroup:"))
                        {
                            string tempLine = currentLine;
                            tempLine = tempLine.Remove(0, 11);
                            string userName = tempLine.Substring(0, tempLine.IndexOf('='));
                            tempLine = tempLine.Remove(0, tempLine.IndexOf('=') + 3);
                            Dictionary<NPC, int> theBuffs = new Dictionary<NPC, int>();
                            while (true)
                            {

                                try
                                {
                                    theBuffs.Add(TShockAPI.TShock.Utils.GetNPCByName(tempLine.Substring(0, tempLine.IndexOf(',')))[0], Convert.ToInt32(tempLine.Substring(tempLine.IndexOf(',') + 1, tempLine.IndexOf(')') - (tempLine.IndexOf(',') + 1))));
                                    if (tempLine.IndexOf('(') < 0)
                                        break;
                                    tempLine = tempLine.Remove(0, tempLine.IndexOf('(') + 1);
                                }
                                catch (Exception ex) { Console.Write(ex.Message); break; }

                            }
                            spawnGroups.Add(userName, theBuffs);
                        }

                    }
                }

            }

        }
        public static List<int> GetGroupBuffByName(string theString)
        {
            List<int> theList = new List<int>();
            theString = theString.ToLower();
            for (int i = 0; i < buffGroups.Keys.ToArray().Count(); i++)
            {
                if (buffGroups.Keys.ToArray()[i].ToLower() == theString)
                {

                    theList.Add(i);
                    return (theList);

                }
                else if (buffGroups.Keys.ToArray()[i].ToLower().StartsWith(theString))
                {

                    theList.Add(i);

                }

            }
            return (theList);

        }
        public static Dictionary<NPC, int> GetSpawnBuffByName(string theString)
        {
            Dictionary<NPC, int> theList = new Dictionary<NPC, int>();
            theString = theString.ToLower();
            for (int i = 0; i < spawnGroups.Keys.ToArray().Count(); i++)
            {
                if (spawnGroups.Keys.ToArray()[i].ToLower() == theString)
                {

                    theList = spawnGroups.Values.ToArray()[i];
                    return (theList);

                }
                else if (spawnGroups.Keys.ToArray()[i].ToLower().StartsWith(theString))
                {

                    theList.Concat(spawnGroups.Values.ToArray()[i]);

                }

            }
            return (theList);

        }
        public static bool findIfPlayingCommand(string text)
        {

            if (text.StartsWith("/playing"))
            {

                if (text.Length == 8)
                    return true;
                else if (text[8] == ' ')
                    return true;
                else
                    return false;

            }
            else if (text.StartsWith("/who"))
            {

                if (text.Length == 4)
                    return true;
                else if (text[4] == ' ')
                    return true;
                else
                    return false;

            }
            else
                return false;

        }
        public static bool findIfMeCommand(string text)
        {

            if (text.StartsWith("/me"))
            {

                if (text.Length == 3)
                    return true;
                else if (text[3] == ' ')
                    return true;
                else
                    return false;

            }
            else
                return false;

        }
        public static List<String> parseParameters(string str) {

            var ret = new List<string>();
            string sb = "";
            bool instr = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (instr)
                {
                    if (c == '\\')
                    {
                        if (i + 1 >= str.Length)
                            break;
                        c = GetEscape(str[++i]);
                    }
                    else if (c == '"')
                    {
                        ret.Add(sb);
                        sb = "";
                        instr = false;
                        continue;
                    }
                    sb += c;
                }
                else
                {
                    if (IsWhiteSpace(c))
                    {
                        if (sb.Length > 0)
                        {
                            ret.Add(sb.ToString());
                            sb = "";
                        }
                    }
                    else if (c == '"')
                    {
                        if (sb.Length > 0)
                        {
                            ret.Add(sb.ToString());
                            sb = "";
                        }
                        instr = true;
                    }
                    else
                    {
                        sb += c;
                    }
                }
            }
            if (sb.Length > 0)
                ret.Add(sb.ToString());

            return ret;

        }
        private static char GetEscape(char c)
        {
            switch (c)
            {
                case '\\':
                    return '\\';
                case '"':
                    return '"';
                case 't':
                    return '\t';
                default:
                    return c;
            }
        }
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }
    }
}