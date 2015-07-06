using System.Linq;
using System;
using ActionPlanner.ComplexActions;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Robotics.Controls;
using Robotics.StateMachines;
using Robotics.Mathematics;
namespace ActionPlanner.Tests.WorkInProgressTests
{
    class Fllw1
    {
        #region Enums
        enum States
		{
            SubiendoMisBrazos,
            EsperandoPersonaEnFrente,
            SensandoPersona,
            PersiguiendoPersona,
            AntesDeTerminar,
            FinalState
        }
        public enum FinalStates
        {
            StillRunning,
            Ok,
            Failed
        }
        #endregion
        #region Variables
        private readonly HAL9000Brain brain;
        private readonly HAL9000CmdMan cmdMan;
        private FinalStates finalState;
        private FunctionBasedStateMachine SM;
		private int attemptCounter;
        private Vector3 hum = new Vector3();
        private double umbraldis = 0.25;
		private double AnguloEstable = 0.17453292;
        #endregion
        #region Constructor
        public Fllw1(HAL9000Brain brain, HAL9000CmdMan cmdman)
        {

			this.brain = brain;
			this.cmdMan = cmdman;
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Ejecutando prueba de seguimiento");
            this.brain.Status.TestBeingExecuted = "Fllw1";
            this.brain.OnStatusChanged(new HAL9000StatusArgs(this.brain.Status));
            this.finalState = FinalStates.StillRunning;
            attemptCounter = 0;
            SM = new FunctionBasedStateMachine();
            SM.AddState(new FunctionState((int)States.SubiendoMisBrazos, SubiendoMisBrazos));
            SM.AddState(new FunctionState((int)States.EsperandoPersonaEnFrente, EsperandoPersonaEnFrente));
            SM.AddState(new FunctionState((int)States.SensandoPersona, SensandoPersona));
            SM.AddState(new FunctionState((int)States.PersiguiendoPersona, PersiguiendoPersona));
            SM.AddState(new FunctionState((int)States.AntesDeTerminar, AntesDeTerminar, true));
            SM.AddState(new FunctionState((int)States.FinalState, FinalState, true));
            SM.SetFinalState((int)States.FinalState);
        }
        #endregion

        #region Class methods
        public FinalStates Execute()
        {
            while (this.brain.Status.IsRunning && this.brain.Status.IsExecutingPredefinedTask && !SM.Finished)
            {
                if (this.brain.Status.IsPaused)
                {
                    Thread.Sleep((int)this.brain.Status.BrainWaveType);
                    continue;
                }
                SM.RunNextStep();
            }
            return this.finalState;
        }
        #endregion

        #region SMFunctions
        private int SubiendoMisBrazos(int currentState, object o)
        {
            this.cmdMan.ARMS_goto("standby", 8000);
            //this.cmdMan.HEAD_lookat();
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Subiendo mis brazos");
            return (int)States.EsperandoPersonaEnFrente;
        }
        private int EsperandoPersonaEnFrente(int currentState, object o)
        {
			List<Vector3> pier;
			TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\EsperandoPersonaEnFrente.-> Esperando a que la persona se digne a ponerse en frente");
			do
			{
				Thread.Sleep(500);
				this.cmdMan.MVN_PLN_findlegs("15", out pier, 2000);
			} while ((pier.Count == 0));
			bool sal = false;
			for (int i = 0; i < pier.Count; i++)
			{
				if ((Math.Pow(pier[i].X, 2) + Math.Pow(pier[i].Y, 2)) < 2.25)
				{
					hum = pier[i];
					sal = true;
				}
			}
			if (sal)
			{
				Thread.Sleep(500);
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\EsperandoPersonaEnFrente.-> Por fin Aparece");
				brain.SayAsync("Hello human, I am the robot justina.");
				return (int)States.SensandoPersona;
			}
			else
			{
				TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000\\EsperandoPersonaEnFrente.-> Ups no era");
				return (int)States.EsperandoPersonaEnFrente;
			}

        }
        private int SensandoPersona(int currentState, object o)
        {
            List<Vector3> pier;
            double distan=-1,aux=0;
            Vector3 ten=new Vector3();
            ten.X = 0; ten.Y = 0; ten.Z = 0;
            this.cmdMan.MVN_PLN_findlegs("", out pier, 2000);
            for (int i = 0; i < pier.Count; i++)
            {
                aux = Math.Pow(pier[i].X - hum.X, 2) + Math.Pow(pier[i].Y - hum.Y, 2);
                if (aux < umbraldis)
                {
                    distan = aux;
                    ten = pier[i];
                }
            }
            if (distan == -1)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> perdi a la persona, deja vuelvo a intentar");
                return (int)States.SensandoPersona;
            }
            else
            {
                hum = ten;
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> Persona detectada");
                return (int)States.PersiguiendoPersona;
            }
        }
        private int PersiguiendoPersona(int currentState, object o)
        {
            double distanc =Math.Sqrt(Math.Pow(hum.X, 2) + Math.Pow(hum.Y, 2));
            if (distanc > 1 && distanc < 1.5)
            {
                distanc = 0;
            }
            else
            {
                distanc = distanc - 1.25;
            }
            double ang = 0;
            if (hum.X == 0)
            {
                if (hum.Y > 0)
                {
                    ang = Math.PI / 2;
                }
                else
                {
                    ang = -Math.PI / 2;
                }
            }
            else
            {
                ang = Math.Atan2(hum.Y, hum.X);
                if (Math.Abs(ang) < AnguloEstable)
                {
                    ang = 0;
                }
            }
            if (distanc < 0)
            {
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> por fin");
                return (int)States.AntesDeTerminar;
            }
            else
            {
                this.cmdMan.MVN_PLN_move(distanc, ang);
                TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> siguiendo a la persona");
                return (int)States.SensandoPersona;
            }
        }
        private int AntesDeTerminar(int currentState, object o)
        {
            brain.SayAsync("Bye, bye.");
            this.cmdMan.ARMS_goto("home", 8000);
            //this.cmdMan.HEAD_lookat();
            TextBoxStreamWriter.DefaultLog.WriteLine("HAL9000.-> bye");
            return (int)States.FinalState;
        }
        private int FinalState(int currentState, object o)
        {
            return currentState;
        }
        #endregion
    }
}
