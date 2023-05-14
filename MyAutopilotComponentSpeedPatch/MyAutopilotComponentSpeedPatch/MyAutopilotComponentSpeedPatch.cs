using NLog;
using System;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Sandbox.Game.EntityComponents;
using System.Linq;
using Torch.Managers.PatchManager;

namespace MyAutopilotComponentSpeedPatch
{
    public static class MyAutopilotComponent_InitWithObjectBuilder_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = new List<CodeInstruction>(instructions);

            for (var i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i + 1].opcode == OpCodes.Ldc_R4 && (float)instructionList[i + 1].operand == 0 && (float)instructionList[i + 2].operand != 0 && instructionList[i + 3].opcode == OpCodes.Call)
                {
                    instructionList[i + 2].operand = 1000f;
                    break;
                }
            }
            return instructionList.AsEnumerable();
        }
    }

    public class MyAutopilotComponentSpeedPatch : TorchPluginBase
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            var harmony = new Harmony("com.jasper.macsp");
            var acMethod = AccessTools.Method(typeof(MyAutopilotComponent), nameof(MyAutopilotComponent.InitWithObjectBuilder), new Type[] { typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_AutopilotComponent) });
            var rcMethod = AccessTools.Method(typeof(MyAutopilotComponent), nameof(MyAutopilotComponent.InitWithObjectBuilder), new Type[] { typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_RemoteControl) });
            var transpiler = AccessTools.Method(typeof(MyAutopilotComponent_InitWithObjectBuilder_Patch), "Transpiler");
            harmony.Patch(acMethod, transpiler: new HarmonyMethod(transpiler));
            harmony.Patch(rcMethod, transpiler: new HarmonyMethod(transpiler));            
        }
    }
}
