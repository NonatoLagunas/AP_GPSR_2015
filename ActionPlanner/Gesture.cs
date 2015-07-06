using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robotics.Controls;

namespace ActionPlanner
{
    public class Gesture
    {
        private string gesture;
        private string id;

        public Gesture()
        {
            this.gesture = "";
            this.id = "";
        }

        public Gesture(string gesture, string id)
        {
            this.gesture = gesture;
            this.id = id;
        }

        public string GestureName
        {
            get { return this.gesture; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    TextBoxStreamWriter.DefaultLog.WriteLine("Gesture: Invalid gesture name");
                else this.gesture = value;
            }
        }

        public string ID
        {
            get { return this.id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    TextBoxStreamWriter.DefaultLog.WriteLine("Gesture: Invalid gesture id");
                else this.id = value;
            }
        }
    }
}
