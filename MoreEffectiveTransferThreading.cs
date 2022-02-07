﻿using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using ICities;
using MoreEffectiveTransfer.Util;

namespace MoreEffectiveTransfer
{
    public class MoreEffectiveTransferThreading : ThreadingExtensionBase
    {
        public static bool isFirstTime = true;
        public const int HarmonyPatchNumExpected = 1; //was: 8


        public override void OnBeforeSimulationFrame()
        {
            base.OnBeforeSimulationFrame();

            if (!isFirstTime)
                return;

            if (Loader.CurrentLoadMode == LoadMode.LoadGame || Loader.CurrentLoadMode == LoadMode.NewGame)
            {
                if (MoreEffectiveTransfer.IsEnabled)
                {
                    uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                    int num4 = (int)(currentFrameIndex & 255u);
                    if (num4 == 255)
                    {
                        MoreEffectiveTransfer.shipStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 200) * 0.005f;
                        MoreEffectiveTransfer.trainStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 200) * 0.005f;
                        MoreEffectiveTransfer.planeStationDistanceRandom = (float)Singleton<SimulationManager>.instance.m_randomizer.Int32(100, 200) * 0.005f;
                    }

                    CheckDetour();

                    // init transfer manager
                    if (!CustomManager.CustomTransferManager._init)
                        CustomManager.CustomTransferManager.Init();
                    
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: all inits completed.");
                }
            }
        }

        public void CheckDetour()
        {
            if (isFirstTime && Loader.DetourInited && Loader.HarmonyDetourInited)
            {
                isFirstTime = false;
                if (Loader.DetourInited)
                {
                    DebugLog.LogToFileOnly("ThreadingExtension.OnBeforeSimulationFrame: First frame detected. Checking detours.");
                    if (Loader.HarmonyDetourFailed)
                    {
                        string error = "MoreEffectiveTransfer HarmonyDetourInit is failed, Send MoreEffectiveTransfer.txt to Author.";
                        DebugLog.LogToFileOnly(error);
                        UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                    }
                    else
                    {
                        var harmony = new Harmony(HarmonyDetours.ID);
                        var methods = harmony.GetPatchedMethods();
                        int i = 0;
                        foreach (var method in methods)
                        {
                            var info = Harmony.GetPatchInfo(method);
                            if (info.Owners?.Contains(harmony.Id) == true)
                            {
                                DebugLog.LogToFileOnly($"Harmony patch method = {method.FullDescription()}");
                                if (info.Prefixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PreFix");
                                }
                                if (info.Postfixes.Count != 0)
                                {
                                    DebugLog.LogToFileOnly("Harmony patch method has PostFix");
                                }
                                i++;
                            }
                        }

                        if (i != HarmonyPatchNumExpected)
                        {
                            string error = $"MoreEffectiveTransfer HarmonyDetour Patch Num is {i}, expected: {HarmonyPatchNumExpected}. Send MoreEffectiveTransfer.txt to Author.";
                            DebugLog.LogToFileOnly(error);
                            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", error, true);
                        }
                    }

#if (PROFILE)
                    DebugLog.LogToFileOnly("PROFILING MODE - statistics will be output at end!");
#endif

                }
            }
        }
    }
}