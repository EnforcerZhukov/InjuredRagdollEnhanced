﻿using System;
using GTA;
using GTA.Native;
using System.Windows.Forms;

public class InjuredRagdoll : Script
{
    private ScriptSettings config;
    //Public strings, bools and ints
        private bool activatehealthragdoll, activatearmorragdoll, activatecoverhealthragdoll, activatecoverarmorragdoll, activatehealthvehicleragdoll, activatearmorvehicleragdoll, debug, antispamhealth, antispamarmor, randomizehealthtime, randomizearmortime, randomizehealthanim, randomizearmoranim, screamhealth, screamarmor, speechhealth, speecharmor, improved;
        private int ragdollvehiclecount, ragdolltimehealth, ragdolltimearmor, ragdolltypehealth, ragdolltypearmor, ragdolltimehe, ragdolltimear, antispamtimehe, antispamtimear, ragdollcount, minrandomhealthtime, maxrandomhealthtime, minrandomarmortime, maxrandomarmortime, screamtypehealth, screamtypearmor;
	    private string speechtypehealth, speechtypearmor, speechparamshealth, speechparamsarmor, speechtypehealthz, speechtypearmorz, speechparamshealthz, speechparamsarmorz;
        private Random r = new Random();

    public InjuredRagdoll()
    {
        Tick += OnTick;
        //Interval = 10;

        //Grab configs from the INI
            config = ScriptSettings.Load("scripts\\InjuredRagdoll.ini");

            activatehealthragdoll = config.GetValue<bool>("ACTIVATERAGDOLL", "HealthRagdoll", true);
            activatearmorragdoll = config.GetValue<bool>("ACTIVATERAGDOLL", "ArmorRagdoll", true);
            debug = config.GetValue<bool>("DEBUG", "Debug", false);
            improved = config.GetValue<bool>("ACTIVATERAGDOLL", "WeaponOnly", false);

            activatecoverhealthragdoll = config.GetValue<bool>("ACTIVATERAGDOLL", "InCoverHealthRagdoll", true);
            activatecoverarmorragdoll = config.GetValue<bool>("ACTIVATERAGDOLL", "InCoverArmorRagdoll", true);

            activatehealthvehicleragdoll = config.GetValue<bool>("RAGDOLLINVEHICLE", "RagdollInVehicleHealth", false);
            activatearmorvehicleragdoll = config.GetValue<bool>("RAGDOLLINVEHICLE", "RagdollInVehicleArmor", false);
            
            ragdolltimehealth = config.GetValue<int>("RAGDOLLTIME", "RagdollTimeHealth", 2);
            ragdolltimearmor = config.GetValue<int>("RAGDOLLTIME", "RagdollTimeArmor", 2);
            antispamhealth = config.GetValue<bool>("RAGDOLLTIME", "AntiSpamHealth", false);
            antispamarmor = config.GetValue<bool>("RAGDOLLTIME", "AntiSpamArmor", false);
            antispamtimehe = config.GetValue<int>("RAGDOLLTIME", "AntiSpamTimeHealth", 5);
            antispamtimear = config.GetValue<int>("RAGDOLLTIME", "AntiSpamTimeArmor", 5);

            randomizehealthtime = config.GetValue<bool>("RANDOMIZE", "RandomHealthTime", false);
            randomizearmortime = config.GetValue<bool>("RANDOMIZE", "RandomArmorTime", false);
            minrandomhealthtime = config.GetValue<int>("RANDOMIZE", "MinRandomHealthTime", 2);
            maxrandomhealthtime = config.GetValue<int>("RANDOMIZE", "MaxRandomHealthTime", 6);
            minrandomarmortime = config.GetValue<int>("RANDOMIZE", "MinRandomArmorTime", 2);
            maxrandomarmortime = config.GetValue<int>("RANDOMIZE", "MaxRandomArmorTime", 5);
            randomizehealthanim = config.GetValue<bool>("RANDOMIZE", "RandomHealthAnim", false);
            randomizearmoranim = config.GetValue<bool>("RANDOMIZE", "RandomArmorAnim", false);
            
            ragdolltypehealth = config.GetValue<int>("RAGDOLLTYPE", "RagdollTypeHealth", 0);
            ragdolltypearmor = config.GetValue<int>("RAGDOLLTYPE", "RagdollTypeArmor", 0);

            screamhealth = config.GetValue<bool>("SCREAMING", "screamHealth", true);
            screamarmor = config.GetValue<bool>("SCREAMING", "screamArmor", true);
            screamtypehealth = config.GetValue<int>("SCREAMING", "screamTypeHealth", 6);
            screamtypearmor = config.GetValue<int>("SCREAMING", "screamTypeArmor", 5);

            speechhealth = config.GetValue<bool>("ALTERNATIVESPEECH", "speechHealth", false);
            speecharmor = config.GetValue<bool>("ALTERNATIVESPEECH", "speechArmor", false);
            speechtypehealthz = config.GetValue<string>("ALTERNATIVESPEECH", "speechTypeHealth", "GENERIC_SHOCKED_HIGH");
            speechtypearmorz = config.GetValue<string>("ALTERNATIVESPEECH", "speechTypeArmor", "GENERIC_SHOCKED_HIGH");
            speechparamshealthz = config.GetValue<string>("ALTERNATIVESPEECH", "speechParamsHealth", "SPEECH_PARAMS_FORCE");
            speechparamsarmorz = config.GetValue<string>("ALTERNATIVESPEECH", "speechParamsArmor", "SPEECH_PARAMS_FORCE");

        //Time multipliers for ragdolling functions
            ragdolltimehe = ragdolltimehealth * 1000;
            ragdolltimear = ragdolltimearmor * 1000;

        //Strings names with quote marks
            if (speechhealth || speecharmor)
            {
                speechtypehealth = "\"" + speechtypehealthz + "\"";
                speechtypearmor = "\"" + speechtypearmorz + "\"";
                speechparamshealth = "\"" + speechparamshealthz + "\"";
                speechparamsarmor = "\"" + speechparamsarmorz + "\"";
            }

        //Debug when the script starts running
            if (debug)
            {
                UI.Notify("InjuredRagdoll running");
            }
    }

    void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;

        int armor = player.Armor;
        int health = player.Health;

        //Disable ragdolling when player is on cover, if it's activated
            bool ragdollhealthblock = false;
            bool ragdollarmorblock = false;
            bool playerincover = false;
            if (!activatecoverhealthragdoll || !activatecoverhealthragdoll)
            {
                playerincover = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_IN_COVER, player);
                if (!activatecoverhealthragdoll && playerincover)
                {
                    ragdollhealthblock = true;
                }
                if (!activatecoverarmorragdoll && playerincover)
                {
                    ragdollarmorblock = true;
                }
            }

        //Check if player is Franklin, Michael or Trevor, to disable screaming, if it's activated
            bool franklinmichaeltrevor = true;
            if (screamhealth || screamarmor)
            {
                bool isplayerzero = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_MODEL, player, "player_zero");
                bool isplayerone = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_MODEL, player, "player_one");
                bool isplayertwo = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_MODEL, player, "player_two");
                if (!isplayerzero && !isplayerone && !isplayertwo)
                {
                    franklinmichaeltrevor = false;
                }
            }

        //End of recopilation script & Start of Instant Variables recopilation

            if (activatehealthragdoll || activatearmorragdoll || activatehealthvehicleragdoll || activatearmorvehicleragdoll)
            {
            //Hit detection
                bool ishealthreduced = false;
                bool isarmorreduced = false;

            Wait(200);
            if (improved) { //Improved Detection
                if (health > player.Health && !GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, player, 0, 1)) {
                    ishealthreduced = true;
                    //GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, player);
                }
                if (armor > player.Armor && !GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, player, 0, 1)) {
                    isarmorreduced = true;
                    //GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, player);
                }
            } else { //Simple Detection
                if (health > player.Health) { ishealthreduced = true; }
                if (armor > player.Armor) { isarmorreduced = true; }
            }
            //Randomize ragdoll times
	            if (randomizehealthtime && ishealthreduced)
	            {
	                ragdolltimehealth = r.Next(minrandomhealthtime, maxrandomhealthtime);
	                ragdolltimehe = ragdolltimehealth * 1000;
	            }
	            if (randomizearmortime && isarmorreduced)
	            {
	                ragdolltimearmor = r.Next(minrandomarmortime, maxrandomarmortime);
	                ragdolltimear = ragdolltimearmor * 1000;
	            }

            //Randomize ragdoll animations
                if ((randomizehealthanim || randomizearmoranim) && (ishealthreduced || isarmorreduced))
                {
                    if (randomizehealthanim)
                    {
                        ragdolltypehealth = r.Next(0, 3);
                    }
                    if (randomizearmoranim)
                    {
                        ragdolltypearmor = r.Next(0, 3);
                    }
                }

        //End of instant variables recopilation & START of Ragdolling SCRIPT

            //Ragdoll-Case HEALTH
                if (!player.IsInVehicle() && ishealthreduced && activatehealthragdoll && !ragdollhealthblock)
                {
                    //GTA.Native.Function.Call(Hash.SET_PED_TO_RAGDOLL, player, ragdolltimehe, ragdolltimehelong, ragdolltypehealth, 1, 1, 1);
                    GTA.Native.Function.Call(Hash.SET_PED_TO_RAGDOLL, player, ragdolltimehe, ragdolltimehe, ragdolltypehealth, 1, 1, 1);
                    if (debug) //ragdoll debug for health
                    {
                        string debugishealthrandomized = "Time not randomized.";
                        if (randomizehealthtime)
                        {
                            debugishealthrandomized = "Randomized time.";
                        }
                        ragdollcount = ragdollcount + 1;
                        int debugragdolltimehealth = ragdolltimehe / 1000;
                        UI.Notify("Ragdoll by health decrease, number " + ragdollcount + ". Ragdoll time: " + debugragdolltimehealth + ". " + debugishealthrandomized);
                    }
                    if (screamhealth && !franklinmichaeltrevor && !speechhealth)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, player, screamtypehealth, 0, 0);
                    }
                    if (speechhealth && !screamhealth)
                    {
                        Wait(850);
                        GTA.Native.Function.Call(GTA.Native.Hash._PLAY_AMBIENT_SPEECH1, player, speechtypehealth, speechparamshealth);
                    }
                    if (speechhealth && !franklinmichaeltrevor && screamhealth)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, player, screamtypehealth, 0, 0);
                        Wait(1250);
                        GTA.Native.Function.Call(GTA.Native.Hash._PLAY_AMBIENT_SPEECH1, player, speechtypehealth, speechparamshealth);
                    }
                    if (antispamhealth)
                    {
                        Wait(antispamtimehe * 1000);
                    }
                }
                //Special ragdoll in Vehicle for health
                    if (activatehealthvehicleragdoll && ishealthreduced && player.IsInVehicle())
                    {
                        Vehicle playervehicle = GTA.Native.Function.Call<GTA.Vehicle>(GTA.Native.Hash.GET_VEHICLE_PED_IS_IN, player, 0);
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_FORWARD_SPEED, playervehicle, 30f);
                        if (debug) //debug message for health
                        {
                            ragdollvehiclecount = ragdollvehiclecount + 1;
                            UI.Notify("Character damaged (health reduced), vehicle-on-ragdoll (by Health). VehicleRagdoll number " + ragdollvehiclecount);
                        }
                    }

            //Case ARMOR
                if (!player.IsInVehicle() && isarmorreduced && activatearmorragdoll && !ragdollarmorblock)
                {
                    GTA.Native.Function.Call(Hash.SET_PED_TO_RAGDOLL, player, ragdolltimear, ragdolltimear, ragdolltypearmor, 1, 1, 1);
                    if (debug) //ragdoll debug for health
                    {
                        string debugisarmorrandomized = "Time not randomized.";
                        if (randomizearmortime)
                        {
                            debugisarmorrandomized = "Randomized time.";
                        }
                        ragdollcount = ragdollcount + 1;
                        int debugragdolltimearmor = ragdolltimear / 1000;
                        UI.Notify("Ragdoll by armor decrease, number " + ragdollcount + ". Ragdoll time: " + debugragdolltimearmor + ". " + debugisarmorrandomized);
                    }
                    if (screamarmor && !franklinmichaeltrevor && !speecharmor)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, player, screamtypearmor, 0, 0);
                    }
                    if (speecharmor && !screamarmor)
                    {
                        Wait(850);
                        GTA.Native.Function.Call(GTA.Native.Hash._PLAY_AMBIENT_SPEECH1, player, speechtypearmor, speechparamsarmor);
                    }
                    if (speecharmor && !franklinmichaeltrevor && screamarmor)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, player, screamtypehealth, 0, 0);
                        Wait(1250);
                        GTA.Native.Function.Call(GTA.Native.Hash._PLAY_AMBIENT_SPEECH1, player, speechtypearmor, speechparamsarmor);
                    }
                    if (antispamarmor)
                    {
                        Wait(antispamtimear * 1000);
                    }
                }
                //Special ragdoll in Vehicle for armor
                    if (activatearmorvehicleragdoll && isarmorreduced && player.IsInVehicle())
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_FORWARD_SPEED, Game.Player.LastVehicle, 30f);
                        if (debug) //debug message for armor
                        {
                            ragdollvehiclecount = ragdollvehiclecount + 1;
                            UI.Notify("Character damaged (armor reduced), vehicle-on-ragdoll (by Armor). VehicleRagdoll number " + ragdollvehiclecount);
                        }
                    }
            //Clear the Debug Count if the player is dead && it's activated
                if (debug && player.IsDead)
                {
                    ragdollcount = 0;
                    ragdollvehiclecount = 0;
                }
        }
    }
}