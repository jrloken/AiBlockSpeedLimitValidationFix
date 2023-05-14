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
using System.Collections.Generic;
using System.Reflection.Emit;
using Sandbox.Game.EntityComponents;
using System.Linq;
using Torch.Managers.PatchManager;
using Torch.Utils;
using System.Reflection;
using Sandbox.Common.ObjectBuilders;
using Torch.Managers.PatchManager.MSIL;


namespace MyAutopilotComponentSpeedPatch
{

    [PatchShim]
    public static class MyAutopilotComponent_InitWithObjectBuilder_Patch
    {
        private static BindingFlags aiBlockFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly MethodInfo initWithOB_RemoteBlock = typeof(MyAutopilotComponent).GetMethod(nameof(MyAutopilotComponent.InitWithObjectBuilder), new Type[] { typeof(MyObjectBuilder_RemoteControl) });
        private static readonly MethodInfo initWithOB_AiBlock = typeof(MyAutopilotComponent).GetMethod(nameof(MyAutopilotComponent.InitWithObjectBuilder), aiBlockFlags, null, new Type[] { typeof(MyObjectBuilder_AutopilotComponent) }, null);

#pragma warning disable 649
        [ReflectedMethodInfo(typeof(MyAutopilotComponent_InitWithObjectBuilder_Patch), nameof(Transpiler))]
        private static readonly MethodInfo transpiler;

        [ReflectedFieldInfo(typeof(MyAutopilotComponent), "m_autopilotSpeedLimit")]
        private static readonly FieldInfo autopilotSpeedLimitField;
#pragma warning restore 649

        public static IEnumerable<MsilInstruction> Transpiler(IEnumerable<MsilInstruction> instructions)
        {
            var instructionList = new List<MsilInstruction>(instructions);

            for (var i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].OpCode == OpCodes.Ldfld &&
                    instructionList[i].Operand is MsilOperandInline<FieldInfo> field && field.Value == autopilotSpeedLimitField &&
                    instructionList[i + 1].OpCode == OpCodes.Ldc_R4 &&
                    instructionList[i + 1].Operand is MsilOperandInline<float> arg1 && arg1.Value == 0 &&
                    instructionList[i + 2].Operand is MsilOperandInline<float> arg2 && arg2.Value != 0 &&
                    instructionList[i + 3].OpCode == OpCodes.Call)
                {
                    instructionList[i + 2].InlineValue(10000f);
                    break;
                }
            }
            return instructionList.AsEnumerable();
        }

        public static void Patch(PatchContext ctx) {
            ctx.GetPattern(initWithOB_RemoteBlock).Transpilers.Add(transpiler);
            ctx.GetPattern(initWithOB_AiBlock).Transpilers.Add(transpiler);            
        }
    }

    public class MyAutopilotComponentSpeedPatch : TorchPluginBase
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
        }
    }
}
