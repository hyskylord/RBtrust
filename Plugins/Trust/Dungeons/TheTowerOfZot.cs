using Clio.Utilities;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.Pathing.Avoidance;
using ff14bot.Enums;
using ff14bot.NeoProfiles;
using ff14bot.Behavior;
using ff14bot.Helpers;
using System;
using System.Linq;
using System.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;


namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 81 The Tower of Zot dungeon logic.
    /// </summary>
    ///
    
    public class TheTowerOfZot : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheTowerOfZot;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheTowerOfZot;

        private CapabilityManagerHandle TrustHandle1 = CapabilityManager.CreateNewHandle();
        private CapabilityManagerHandle TrustHandle2 = CapabilityManager.CreateNewHandle();
        private CapabilityManagerHandle TrustHandle3 = CapabilityManager.CreateNewHandle();
        private CapabilityManagerHandle TrustHandle4 = CapabilityManager.CreateNewHandle();
        private PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();
        private Stopwatch DAsw = new Stopwatch();
        private Stopwatch TMsw = new Stopwatch();
        private Stopwatch STsw = new Stopwatch();
        
        // BOSS MECHANIC SPELLIDS
        
        // B1
        //
        // Minduruva   Manusya Bio	    	25248
	    //	           Manusya Blizzard	    25234
	    //	           Manusya Fire III	    25233
	    //	           Manusya Thunder		25235
	    //	           Manusya Bio III		25236
	    //	           Transmute Fire III	25242
	    //             Manusya Fire		    25699
	    //         	   Dhrupad		    	25244
	    //	           Transmute Thunder	25372
	    //         	   Transmute Blizzard	25371
	    //         	   Transmute Bio III	25373
        
        // B2
        //
        // Sanduruva   Isitva Siddhi       25257
        //             Prapti Siddhi       25256
        //             Manusya Berserk     25249
        //             Explosive Force     25250
        //             Prakamya Siddhi     25251
        //             Manusya Stop        25255
        //             Manusya Confuse     25253
        
        // B3 (3 sisters at once)
        //
        // Cinduruva   Samsara             25273
        //             Manusya Reflect     25259
        // Sanduruva   Manusya Faith       25258
        //             Isitva Siddhi       25280
        //             Prapti Siddhi       25275
        // Minduruva   Dhrupad             25281
        //             Manusya Fire        25287
        //             Manusya Blizzard    25288
        //             Manusya Thunder     25289
        //             Delta Attack        25260
        //             Delta Attack        25261
        //             Delta Attack        25262
        //
        //  TRASH WITH NO OMEN
        // Zot Roader  Haywire             24145

        // GENERIC MECHANICS
        private HashSet<uint> stack = new HashSet<uint>()
            {
                25234, 25233, 25250, 24145, 25253
            };

        // B1

        private HashSet<uint> transmute = new HashSet<uint>()
            {
                25242, 25372, 25371, 25373
            };

        private HashSet<uint> deltaattack = new HashSet<uint>()
            {
                25260, 25261, 25262
            };

        

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            if (!Core.Me.InCombat)
            { 
                CapabilityManager.Update(TrustHandle1, CapabilityFlags.Movement, 0, "End Combat");
                CapabilityManager.Update(TrustHandle2, CapabilityFlags.Facing, 0, "End Combat");
                CapabilityManager.Update(TrustHandle3, CapabilityFlags.Movement, 0, "End Combat");
                CapabilityManager.Update(TrustHandle4, CapabilityFlags.Facing, 0, "End Combat");
                DAsw.Reset();
                TMsw.Reset();
                STsw.Reset();
            }


            if (stack.IsCasting())
            {

                if (!STsw.IsRunning)
                    STsw.Restart();

                               
                sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i=> i.CanRun);
                CapabilityManager.Update(TrustHandle1, CapabilityFlags.Movement, 1000, "Follow/Stack Mechanic In Progress");
                CapabilityManager.Update(TrustHandle2, CapabilityFlags.Facing, 1000, "Follow/Stack Mechanic In Progress");
                await MovementHelpers.GetClosestAlly.Follow();
                await Coroutine.Yield();


            }

            if (STsw.ElapsedMilliseconds > 3000)
                {
                    STsw.Reset();
                    sidestepPlugin.Enabled = true;
                }

            if (deltaattack.IsCasting() || (DAsw.IsRunning && DAsw.ElapsedMilliseconds < 24000))
            {
                if (!DAsw.IsRunning || DAsw.ElapsedMilliseconds >= 24000)
                {

                    CapabilityManager.Update(TrustHandle3, CapabilityFlags.Movement, 24000, "Delta Attack Avoid");
                    CapabilityManager.Update(TrustHandle4, CapabilityFlags.Facing, 24000, "Delta Attack Avoid");
                    DAsw.Restart();
                }

                if (DAsw.ElapsedMilliseconds < 24000)
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i=> i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow2(DAsw, 24000);
                    await Coroutine.Yield();
                }
                           
            }

            if (DAsw.ElapsedMilliseconds > 24000)
                sidestepPlugin.Enabled = true;

            //

            if ((transmute.IsCasting() || TMsw.IsRunning))
            {
                if (!TMsw.IsRunning)
                    {
                        CapabilityManager.Update(TrustHandle3, CapabilityFlags.Movement, 20000, "Transmute Avoid");
                        CapabilityManager.Update(TrustHandle4, CapabilityFlags.Facing, 20000, "Transmute Avoid");
                        TMsw.Restart();
                    }

                if (TMsw.ElapsedMilliseconds < 20000)
                    {
                        sidestepPlugin.Enabled = false;
                        AvoidanceManager.RemoveAllAvoids(i=> i.CanRun);
                        await MovementHelpers.GetClosestAlly.Follow2(TMsw, 20000);
                        await Coroutine.Yield();
                }
                                
                if (TMsw.ElapsedMilliseconds >= 20000)
                { 
                    sidestepPlugin.Enabled = true;
                    TMsw.Reset();
                }    
            }

            return false;
        }
    }
}
