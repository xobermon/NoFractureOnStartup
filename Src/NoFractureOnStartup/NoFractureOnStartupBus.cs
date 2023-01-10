using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

namespace NoFractureOnStartup
{
    // Create a Bus here - Busses are unsaved objects for communicating between the game and the mod. Great for things like Harmony patches that can be safely removed from a save without consequences.
    class NoFractureOnStartupBus : OctModBus
    {
        // Prefixes are executed before the original function
        // out XXX __state is a special parameter that will retain its value between prefix and postfix calls
        // ref ___XXXX is a special parameter that will contain the value of that field (even if private!) when entering the original call
        public static void PropManagerInitializePrefix(out Dictionary<PropBaseBehavior, PropType> __state, ref Dictionary<PropBaseBehavior, PropType> ___demolitionSetupQueue)
        {
            // store the existing demolition setup queue (so that it isn't processed in the Initialize method)
            __state = ___demolitionSetupQueue;

            // and replace it with an empty one
            ___demolitionSetupQueue = new Dictionary<PropBaseBehavior, PropType>();
        }

        // Postfixes are executed after the original function (and any prefixes) runs
        public static void PropManagerInitializePostfix(Dictionary<PropBaseBehavior, PropType> __state, ref Dictionary<PropBaseBehavior, PropType> ___demolitionSetupQueue)
        {
            // restore the demolition queue so that the props can still be factured, just on-demand instead of all at once on startup
            ___demolitionSetupQueue = __state;
        }

        // do our patching in the loaded event of our bus - this will happen before octdats are loaded or managers are initialized
        public override void Loaded()
        {
            // register ourselves with Harmony - make sure you use a unique name
            var harmony = new Harmony("NoFractureOnStartup");

            // enable to get debug info about the harmony patching process
            //Harmony.DEBUG = true;

            // find the original method that we want to patch - PropManager.Initialize
            if (typeof(PropManager).GetMethod(nameof(PropManager.Initialize)) is MethodInfo original)
            {
                // locate our Prefix
                if (typeof(NoFractureOnStartupBus).GetMethod(nameof(NoFractureOnStartupBus.PropManagerInitializePrefix)) is MethodInfo prefix)
                {
                    // locate our Postfix
                    if (typeof(NoFractureOnStartupBus).GetMethod(nameof(NoFractureOnStartupBus.PropManagerInitializePostfix)) is MethodInfo postfix)
                    {
                        // patch!
                        harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                    }
                }
            }
        }
    }
}
