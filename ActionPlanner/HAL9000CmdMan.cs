using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Robotics.API;
using Robotics.Controls;

namespace ActionPlanner
{
	public enum JustinaCommands
	{
		BLK_alive, BLK_read_var, BLK_list_vars, BLK_read_vars, BLK_suscribevar, BLK_create_var, BLK_write_var,
		BLK_stat_var,

		ARMS_ra_abspos, ARMS_ra_artpos, ARMS_ra_goto, ARMS_ra_move, ARMS_ra_getabspos, ARMS_ra_reachable,
		ARMS_la_abspos, ARMS_la_artpos, ARMS_la_goto, ARMS_la_move, ARMS_la_reachable,
		ARMS_ra_orientation, ARMS_ra_opengrip, ARMS_ra_closegrip, ARMS_ra_getgripstatus, ARMS_ra_torque,
		ARMS_ra_servotorqueon, ARMS_ra_relpos, ARMS_la_opengrip, ARMS_la_closegrip, ARMS_goto,
		ARMS_ra_hand_move,
		

		HEAD_lookat, HEAD_show, HEAD_search, HEAD_stop, HEAD_lookatloop, HEAD_lookatrel, HEAD_lookatobject,
		HEAD_followskeleton, HEAD_stopfollowskeleton,

		MVN_PLN_pause, MVN_PLN_move, MVN_PLN_goto, MVN_PLN_addobject, MVN_PLN_obstacle, MVN_PLN_position,
		MVN_PLN_enablelaser, MVN_PLN_disablelaser, MVN_PLN_getclose, MVN_PLN_getclosef, MVN_PLN_go_to_room, MVN_PLN_go_to_region,
		MVN_PLN_mv, MVN_PLN_ic, MVN_PLN_stop, MVN_PLN_setspeeds, MVN_PLN_report, MVN_PLN_getpos, MVN_PLN_lectures,
		MVN_PLN_startfollowhuman, MVN_PLN_stopfollowhuman, MVN_PLN_selfterminate, MVN_PLN_fixhuman,
		MVN_PLN_sethumanpos, MVN_PLN_calculatepath, MVN_PLN_distancetogoal, MVN_PLN_updatelinelmarks, MVN_PLN_findlegs,
		MVN_PLN_infolaser, MVN_PLN_basebusy, MVN_PLN_addlocation, MVN_PLN_startsavingpath, MVN_PLN_stopsavingpath,

		OBJ_FNDT_find, OBJ_FNDT_findontable, OBJ_FNDT_removetable, OBJ_FNDT_findedge, OBJ_FNDT_findedgetuned, OBJ_FNDT_findhandgrip,
		OBJ_FNDT_sethumantracker, OBJ_FNDT_tracking, OBJ_FNDT_findhuman, OBJ_FNDT_findandtrackhuman,
		OBJ_FNDT_locatehuman, OBJ_FNDT_remember, OBJ_FNDT_findedgereloaded, OBJ_FNDT_trainshirt, OBJ_FNDT_testshirt,
		OBJ_FNDT_detectsmoke, OBJ_FNDT_takepicture,OBJ_FNDT_findedgereturns,OBJ_FNDT_findgolemhand,
		OBJ_FNDT_findedgefastandfurious, OBJ_FNDT_gestodi, OBJ_FNDT_findpaddle,

		VISION_findmarker,VISION_findmarkermultiplelanguages, VISION_findfall, VISION_findwaving,

		SENSORS_start, SENSORS_stop,

		SP_GEN_say, SP_GEN_asay, SP_GEN_read, SP_GEN_aread, SP_GEN_aplay, SP_GEN_playloop, SP_GEN_shutup,
		SP_GEN_play, SP_GEN_voice,

		SP_REC_na, SP_REC_status, SP_REC_grammar, SP_REC_words,

		PRS_FND_find, PRS_FND_remember, PRS_FND_auto, PRS_FND_sleep, PRS_FND_shutdown, PRS_FND_amnesia,PRS_FND_findhuman, PRS_FND_forgethuman, PRS_FND_knownnames,
		PRS_FND_rememberhuman, PRS_FND_resolution, PRS_FND_source, 

		ST_PLN_alignhuman, ST_PLN_cleanplane, ST_PLN_comewith, ST_PLN_dopresentation, ST_PLN_drop, ST_PLN_findhuman,
		ST_PLN_findhumanobject, ST_PLN_findobject, ST_PLN_findtable, ST_PLN_grab, ST_PLN_greethuman, ST_PLN_release,
		ST_PLN_rememberhuman, ST_PLN_searchobject, ST_PLN_seehand, ST_PLN_seeobject, ST_PLN_shutdown, ST_PLN_take,
		ST_PLN_deliverobject, ST_PLN_startlearn, ST_PLN_stoplearn, ST_PLN_takehandover, ST_PLN_pointatobject,
		ST_PLN_fashion_findobject, ST_PLN_taketable, ST_PLN_takexyz, ST_PLN_pickbox, ST_PLN_dropbox, ST_PLN_shakehand,
        ST_PLN_alignedge,

		ERT_addlocation, ERT_addfire, ERT_addperson, ERT_buildreport,
	}

	public enum RegionType { Room, Region, Location }

	public partial class HAL9000CmdMan : CommandManager
	{
		private const int TotalExistingCommands = 250;
		private JustinaCmdAndResp[] justinaCmdAndResp;
		private SortedList<string, JustinaCmdAndResp> sortedCmdAndResp;
		private HAL9000Status status;

		public HAL9000CmdMan(HAL9000Status status)
			: base()
		{
			this.status = status;
			this.ResponseReceived += new ResponseReceivedEventHandler(HAL9000CmdMan_ResponseReceived);
			this.justinaCmdAndResp = new JustinaCmdAndResp[TotalExistingCommands];
			this.sortedCmdAndResp = new SortedList<string, JustinaCmdAndResp>();
			this.LoadJustinaCommands();
		}

		private void HAL9000CmdMan_ResponseReceived(Response response)
		{
			this.ParseResponse(response);
		}

		protected override void ParseResponse(Response response)
		{
			base.ParseResponse(response);

			this.sortedCmdAndResp[response.CommandName].Response = response;
			this.sortedCmdAndResp[response.CommandName].IsResponseReceived = true;
			TextBoxStreamWriter.DefaultLog.WriteLine(2, "HAL9000CmdMan: Response Received: " +
				response.StringToSend);
		}

		private void LoadJustinaCommands()
		{
			#region Commands for BLK
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_alive] = new JustinaCmdAndResp("alive");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_create_var] = new JustinaCmdAndResp("create_var");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_list_vars] = new JustinaCmdAndResp("list_vars");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_read_var] = new JustinaCmdAndResp("read_var");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_read_vars] = new JustinaCmdAndResp("read_vars");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_suscribevar] = new JustinaCmdAndResp("subscribe_var");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_write_var] = new JustinaCmdAndResp("write_var");
			this.justinaCmdAndResp[(int)JustinaCommands.BLK_stat_var] = new JustinaCmdAndResp("stat_var");
			#endregion
			#region Commands for ARMS
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_abspos] = new JustinaCmdAndResp("ra_abspos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_artpos] = new JustinaCmdAndResp("ra_artpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_goto] = new JustinaCmdAndResp("ra_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_move] = new JustinaCmdAndResp("ra_move");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_closegrip] = new JustinaCmdAndResp("ra_closegrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_opengrip] = new JustinaCmdAndResp("ra_opengrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_reachable] = new JustinaCmdAndResp("ra_reachable");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_abspos] = new JustinaCmdAndResp("la_abspos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_artpos] = new JustinaCmdAndResp("la_artpos");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_goto] = new JustinaCmdAndResp("la_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_move] = new JustinaCmdAndResp("la_move");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_closegrip] = new JustinaCmdAndResp("la_closegrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_opengrip] = new JustinaCmdAndResp("la_opengrip");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_la_reachable] = new JustinaCmdAndResp("la_reachable");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_goto] = new JustinaCmdAndResp("arms_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.ARMS_ra_hand_move] = new JustinaCmdAndResp("ra_hand_move");
			#endregion
			#region Commands for HEAD
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_followskeleton] = new JustinaCmdAndResp("hd_followskeleton");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_stopfollowskeleton] = new JustinaCmdAndResp("hd_stopfollowskeleton");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookat] = new JustinaCmdAndResp("hd_lookat");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_lookatrel] = new JustinaCmdAndResp("hd_lookatrel");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_search] = new JustinaCmdAndResp("hd_search");
			this.justinaCmdAndResp[(int)JustinaCommands.HEAD_show] = new JustinaCmdAndResp("hd_show");
			#endregion
			#region Commands for MVN-PLN
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_fixhuman] = new JustinaCmdAndResp("mp_fixhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclose] = new JustinaCmdAndResp("mp_getclose");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_getclosef] = new JustinaCmdAndResp("mp_getclosef");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_goto] = new JustinaCmdAndResp("mp_goto");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_move] = new JustinaCmdAndResp("mp_move");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_obstacle] = new JustinaCmdAndResp("mp_obstacle");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_position] = new JustinaCmdAndResp("mp_position");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_report] = new JustinaCmdAndResp("mp_report");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_setspeeds] = new JustinaCmdAndResp("mp_setspeeds");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startfollowhuman] = new JustinaCmdAndResp("mp_startfollowhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopfollowhuman] = new JustinaCmdAndResp("mp_stopfollowhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_sethumanpos] = new JustinaCmdAndResp("mp_sethumanpos");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_calculatepath] = new JustinaCmdAndResp("mp_calculatepath");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_distancetogoal] = new JustinaCmdAndResp("mp_distancetogoal");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_updatelinelmarks] = new JustinaCmdAndResp("mp_updatelinelmarks");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_findlegs] = new JustinaCmdAndResp("mp_findlegs");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_infolaser] = new JustinaCmdAndResp("mp_infolaser");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_basebusy] = new JustinaCmdAndResp("mp_basebusy");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_addlocation] = new JustinaCmdAndResp("mp_addlocation");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_startsavingpath] = new JustinaCmdAndResp("mp_startsavingpath");
			this.justinaCmdAndResp[(int)JustinaCommands.MVN_PLN_stopsavingpath] = new JustinaCmdAndResp("mp_stopsavingpath");
						
			#endregion
			#region Commands for OBJ_FNDT
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_find] = new JustinaCmdAndResp("oft_find");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findontable] = new JustinaCmdAndResp("oft_findontable");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedge] = new JustinaCmdAndResp("oft_findedge");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgetuned] = new JustinaCmdAndResp("oft_findedgetuned");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereloaded] = new JustinaCmdAndResp("oft_findedgereloaded");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgefastandfurious] = new JustinaCmdAndResp("oft_findedgefastandfurious");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findedgereturns] = new JustinaCmdAndResp("oft_findedgereturns");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findhandgrip] = new JustinaCmdAndResp("oft_findhandgrip");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_sethumantracker] = new JustinaCmdAndResp("oft_sethumantracker");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_tracking] = new JustinaCmdAndResp("oft_tracking");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findhuman] = new JustinaCmdAndResp("oft_findhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findandtrackhuman] = new JustinaCmdAndResp("oft_findandtrackhuman");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_locatehuman] = new JustinaCmdAndResp("oft_locatehuman");
            this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_remember] = new JustinaCmdAndResp("oft_remember");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_trainshirt] = new JustinaCmdAndResp("oft_trainshirt");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_testshirt] = new JustinaCmdAndResp("oft_testshirt");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_detectsmoke] = new JustinaCmdAndResp("oft_detectsmoke");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_takepicture] = new JustinaCmdAndResp("oft_takepicture");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findgolemhand] = new JustinaCmdAndResp("oft_findgolemhand");
			this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_gestodi] = new JustinaCmdAndResp("oft_gestodi");
            this.justinaCmdAndResp[(int)JustinaCommands.OBJ_FNDT_findpaddle] = new JustinaCmdAndResp("oft_findpaddle");            

			#endregion
			#region Commands for VSN_VISION
			this.justinaCmdAndResp[(int)JustinaCommands.VISION_findmarker] = new JustinaCmdAndResp("vsn_findmarker");
			this.justinaCmdAndResp[(int)JustinaCommands.VISION_findmarkermultiplelanguages] = new JustinaCmdAndResp("vsn_findmarkermultiplelanguages");
			this.justinaCmdAndResp[(int)JustinaCommands.VISION_findfall] = new JustinaCmdAndResp("vsn_findfall");
			this.justinaCmdAndResp[(int)JustinaCommands.VISION_findwaving] = new JustinaCmdAndResp("vsn_findwaving");	
			
			#endregion
			#region Commands for PRS-FND
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_amnesia] = new JustinaCmdAndResp("pf_amnesia");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_findhuman] = new JustinaCmdAndResp("pf_find");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_forgethuman] = new JustinaCmdAndResp("pf_forget");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_knownnames] = new JustinaCmdAndResp("pf_knownnames");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_rememberhuman] = new JustinaCmdAndResp("pf_remember");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_resolution] = new JustinaCmdAndResp("pf_resolution");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_shutdown] = new JustinaCmdAndResp("pf_shutdown");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_sleep] = new JustinaCmdAndResp("pf_sleep");
			this.justinaCmdAndResp[(int)JustinaCommands.PRS_FND_source] = new JustinaCmdAndResp("pf_source");
			#endregion
			#region Commands for SPG-GEN
			//this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_aread] = new JustinaCmdAndResp("spg_aread");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_asay] = new JustinaCmdAndResp("spg_asay");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_aplay] = new JustinaCmdAndResp("spg_aplay");
			//this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_read] = new JustinaCmdAndResp("spg_read");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_say] = new JustinaCmdAndResp("spg_say");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_shutup] = new JustinaCmdAndResp("spg_shutup");
			this.justinaCmdAndResp[(int)JustinaCommands.SP_GEN_voice] = new JustinaCmdAndResp("spg_voice");
			#endregion
			#region Commands for SP-REC
			this.justinaCmdAndResp[(int)JustinaCommands.SP_REC_grammar] = new JustinaCmdAndResp("spr_grammar");
			#endregion
			#region Commands for ST-PLN
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_alignhuman] = new JustinaCmdAndResp("align_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_cleanplane] = new JustinaCmdAndResp("clean_plane");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_comewith] = new JustinaCmdAndResp("come_with");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_dopresentation] = new JustinaCmdAndResp("dopresentation");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_drop] = new JustinaCmdAndResp("drop");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhuman] = new JustinaCmdAndResp("find_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findhumanobject] = new JustinaCmdAndResp("find_human_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_fashion_findobject] = new JustinaCmdAndResp("fashionfind_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findobject] = new JustinaCmdAndResp("find_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_findtable] = new JustinaCmdAndResp("find_table");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_grab] = new JustinaCmdAndResp("grab");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_greethuman] = new JustinaCmdAndResp("greethuman");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_release] = new JustinaCmdAndResp("release");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_rememberhuman] = new JustinaCmdAndResp("remember_human");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_searchobject] = new JustinaCmdAndResp("search_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_seehand] = new JustinaCmdAndResp("see_hand");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_seeobject] = new JustinaCmdAndResp("see_object");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_shutdown] = new JustinaCmdAndResp("shutdown");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_take] = new JustinaCmdAndResp("take");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_deliverobject] = new JustinaCmdAndResp("deliverobject");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_startlearn] = new JustinaCmdAndResp("startlearn");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_stoplearn] = new JustinaCmdAndResp("stoplearn");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takehandover] = new JustinaCmdAndResp("takehandover");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_taketable] = new JustinaCmdAndResp("taketable");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_pointatobject] = new JustinaCmdAndResp("pointatobject");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_takexyz] = new JustinaCmdAndResp("takexyz");
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_pickbox] = new JustinaCmdAndResp("pickbox");
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_dropbox] = new JustinaCmdAndResp("dropbox");
			this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_shakehand] = new JustinaCmdAndResp("shakehand");
            this.justinaCmdAndResp[(int)JustinaCommands.ST_PLN_alignedge] = new JustinaCmdAndResp("aligneedge");			
            			
			#endregion
			#region Commands for ERT_Report
			this.justinaCmdAndResp[(int)JustinaCommands.ERT_addlocation] = new JustinaCmdAndResp("ert_addlocation");
			this.justinaCmdAndResp[(int)JustinaCommands.ERT_addfire] = new JustinaCmdAndResp("ert_addfire");
			this.justinaCmdAndResp[(int)JustinaCommands.ERT_addperson] = new JustinaCmdAndResp("ert_addperson");
			this.justinaCmdAndResp[(int)JustinaCommands.ERT_buildreport] = new JustinaCmdAndResp("ert_buildreport");
			#endregion
			

			foreach (JustinaCmdAndResp jcar in this.justinaCmdAndResp)
				if (jcar != null)
					this.sortedCmdAndResp.Add(jcar.Command.CommandName, jcar);
		}

		private void TerminateProcess(string processName, int waitTime_ms)
		{
			Process[] processesToTerminate = Process.GetProcessesByName(processName);
			foreach (Process p in processesToTerminate)
			{
				try
				{
					if (!p.HasExited && p.Responding)
						p.Close();
				}
				catch { }
			}
			Thread.Sleep(waitTime_ms);
			foreach (Process p in processesToTerminate)
			{
				try
				{
					p.Kill();
				}
				catch { }
			}
		}

		private void RunProcess(string path, string args)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = path;
			psi.Arguments = args;
			psi.WindowStyle = ProcessWindowStyle.Minimized;
			Process.Start(psi);
		}

		private void SetupAndSendCommand(JustinaCommands justinaCmd, string parameters)
		{
			this.justinaCmdAndResp[(int)justinaCmd].IsResponseReceived = false;
			this.justinaCmdAndResp[(int)justinaCmd].Command.Parameters = parameters;
			if (!this.SendCommand(this.justinaCmdAndResp[(int)justinaCmd].Command))
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000cmdman: Can't send command");
			else TextBoxStreamWriter.DefaultLog.WriteLine(2, "HAL9000CmdMan: Send command: " +
				this.justinaCmdAndResp[(int)justinaCmd].Command.StringToSend);
		}

		public bool WaitForResponse(JustinaCommands expectedCmdResp, int timeOut_ms)
		{
			int sleepsNumber = (int)(((double)timeOut_ms) / ((double)this.status.BrainWaveType));

			while (!this.IsResponseReceived(expectedCmdResp) && this.status.IsRunning &&
				this.status.IsExecutingPredefinedTask && sleepsNumber-- > 0)
				Thread.Sleep((int)this.status.BrainWaveType);

			if (sleepsNumber == 0)
				TextBoxStreamWriter.DefaultLog.WriteLine(2, "HAL9000CmdMan: " + expectedCmdResp.ToString() + ": Time out");

			if (this.IsResponseReceived(expectedCmdResp))
				return this.justinaCmdAndResp[(int)expectedCmdResp].Response.Success;
			else return false;
		}

		/// <summary>
		/// Checks if a response for a given command is already received
		/// </summary>
		/// <param name="waitedCommandResponse">CommandType whose response is expected</param>
		/// <returns></returns>
		public bool IsResponseReceived(JustinaCommands expectedCmdResp)
		{
			if (this.justinaCmdAndResp[(int)expectedCmdResp].IsResponseReceived)
				return true;
			else return false;
		}

		public bool IsResponseReceived(JustinaCommands expectedCmdResp, out Response receivedResp)
		{
			receivedResp = null;
			if (this.IsResponseReceived(expectedCmdResp))
			{
				receivedResp = this.justinaCmdAndResp[(int)expectedCmdResp].Response;
				return true;
			}
			else return false;
		}

		public bool IsResponseSuccesfully(JustinaCommands cmdToCheck)
		{
			return this.justinaCmdAndResp[(int)cmdToCheck].Response.Success;
		}

		


						

		


		

		

		

		public JustinaCmdAndResp[] JustinaCmdAndResps
		{
			get { return this.justinaCmdAndResp; }
		}

		public bool UseFloorLaser { get; set; }


		
	}
}
