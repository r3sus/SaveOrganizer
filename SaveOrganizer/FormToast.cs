﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaveOrganizer
{
    public partial class FormToast : Form
    {
        double Count = 0;
        int EventCounter = 0;
        int MilliSeconds;
        Point PLocation;
        ActionCenter Actions = new ActionCenter();

        public FormToast(string Toast, Point Locations, double Seconds)
        {
            InitializeComponent();
            LblToast.Text = Toast;
            MilliSeconds = Convert.ToInt32(Seconds * 10);
            PLocation = Locations;
        }

        private void ToastForm_Load(object sender, EventArgs e)
        {
            this.Height = 95;
            SetDesktopLocation(PLocation.X, PLocation.Y);
            FadeIn();
        }

        private void FadeIn()
        {
            TimerStart.Start();
        }

        private void FadeOut()
        {
            TimerStop.Start();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }

        private void ToastForm_Paint(object sender, PaintEventArgs e)
        {
            GraphicsPath graphicpath = new GraphicsPath();
            graphicpath.StartFigure();
            graphicpath.AddArc(0, 0, 25, 25, 180, 90);
            graphicpath.AddLine(25, 0, this.Width - 25, 0);
            graphicpath.AddArc(this.Width - 25, 0, 25, 25, 270, 90);
            graphicpath.AddLine(this.Width, 25, this.Width, this.Height - 25);
            graphicpath.AddArc(this.Width - 25, this.Height - 25, 25, 25, 0, 90);
            graphicpath.AddLine(this.Width - 25, this.Height, 25, this.Height);
            graphicpath.AddArc(0, this.Height - 25, 25, 25, 90, 90);
            graphicpath.CloseFigure();
            this.Region = new Region(graphicpath);
        }

        private void TimerStart_Tick(object sender, EventArgs e)
        {
            Count = Count + 0.1;
            Opacity = Count;
            if (Count >= 1)
            {
                TimerStart.Stop();
                EventsTimer.Start();
            }
        }

        private void EventsTimer_Tick(object sender, EventArgs e)
        {
            EventCounter++;
            if (EventCounter == MilliSeconds)
            {
                EventsTimer.Stop();
                FadeOut();
            }
        }

        private void TimerStop_Tick(object sender, EventArgs e)
        {
            Count = Count - 0.1;
            Opacity = Count;
            if (Count <= 0)
            {
                this.Close();
            }
        }

    }
}
