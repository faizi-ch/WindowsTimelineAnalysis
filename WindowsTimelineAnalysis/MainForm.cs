using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using WindowsTimelineAnalysis;

namespace WindowsTimelineAnalysis
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        // XtraUserControl employeesUserControl;
        //XtraUserControl customersUserControl;
        TimelineControl timelineControl = null;

        private TimelineControl PrefetchTimeline;
        private TimelineControl JumplistsTimeline;
        private TimelineControl LnkTimeline;
        private TimelineControl ShellbagsTimeline;
        private TimelineControl ChromeTimeline;
        private TimelineControl FirefoxTimeline;

        public MainForm()
        {
            InitializeComponent();
            //employeesUserControl = CreateUserControl("Prefetch");
            //customersUserControl = CreateUserControl("Jumplists");

            PrefetchTimeline=new TimelineControl("Prefetch");

            JumplistsTimeline=new TimelineControl("Jumplists");

            LnkTimeline = new TimelineControl("Lnk");
            //accordionControl.SelectedElement = employeesAccordionControlElement;
            ShellbagsTimeline = new TimelineControl("Shellbags");

            ChromeTimeline = new TimelineControl("Google Chrome");

            FirefoxTimeline = new TimelineControl("Firefox");

        }
 
        void accordionControl_SelectedElementChanged(object sender, SelectedElementChangedEventArgs e)
        {

            
            if (e.Element == null) return;
            
            if (e.Element.Text == "Prefetch")
            {
                timelineControl = PrefetchTimeline;

                splashScreenManager1.ShowWaitForm();
                timelineControl.Analyize("Prefetch");
                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
            }
            else if (e.Element.Text == "Jumplists")
            {
                timelineControl = JumplistsTimeline;
                splashScreenManager1.ShowWaitForm();

                timelineControl.Analyize("Jumplist");

                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
                //timelineControl.Set("j");
            }
            else if (e.Element.Text == "Lnk")
            {
                timelineControl = LnkTimeline;
                splashScreenManager1.ShowWaitForm();
                timelineControl.Analyize("Lnk");
                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
            }

            else if (e.Element.Text == "Shellbags")
            {
                timelineControl = ShellbagsTimeline;
                splashScreenManager1.ShowWaitForm();
                timelineControl.Analyize("Shellbags");
                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
            }
            else if (e.Element.Text == "Google Chrome")
            {
                
                timelineControl = ChromeTimeline;
                splashScreenManager1.ShowWaitForm();
                timelineControl.Analyize("Google Chrome");
                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
            }
            else if (e.Element.Text == "Firefox")
            {
                timelineControl = FirefoxTimeline;
                splashScreenManager1.ShowWaitForm();
                timelineControl.Analyize("Firefox");
                if (splashScreenManager1.IsSplashFormVisible)
                {
                    splashScreenManager1.CloseWaitForm();
                }
            }
           
            //XtraUserControl userControl = e.Element.Text == "Employees" ? employeesUserControl : customersUserControl;
            tabbedView.AddDocument(timelineControl);
            tabbedView.ActivateDocument(timelineControl);
        }
        void barButtonNavigation_ItemClick(object sender, ItemClickEventArgs e)
        {
            int barItemIndex = barSubItemNavigation.ItemLinks.IndexOf(e.Link);
            accordionControl.SelectedElement = mainAccordionGroup.Elements[barItemIndex];
        }
        void tabbedView_DocumentClosed(object sender, DocumentEventArgs e)
        {
            RecreateUserControls(e);
            SetAccordionSelectedElement(e);
        }
        void SetAccordionSelectedElement(DocumentEventArgs e)
        {
            if (tabbedView.Documents.Count != 0)
            {
                //tabbedView.FloatDocuments.
                if (e.Document.Caption == "Prefetch")
                    accordionControl.SelectedElement = jumplistsAccordionControlElement;
                else if (e.Document.Caption == "Jumplists")
                {
                    accordionControl.SelectedElement = prefetchAccordionControlElement;
                }
                else if (e.Document.Caption == "Lnk")
                {
                    //accordionControl.SelectedElement = lnkAccordionControlElement;
                }
                else if (e.Document.Caption == "Shellbags")
                {
                   // accordionControl.SelectedElement =  
                }
                
            }
            else
            {
                //accordionControl.SelectedElement = null;
            }
        }
        void RecreateUserControls(DocumentEventArgs e)
        {

            splashScreenManager1.ShowWaitForm();

            if (e.Document.Caption == "Prefetch")
                PrefetchTimeline = new TimelineControl("Prefetch");
            else if (e.Document.Caption == "Jumplists")
            {
                JumplistsTimeline = new TimelineControl("Jumplists");
            }
            else if (e.Document.Caption == "Lnk")
            {
                LnkTimeline = new TimelineControl("Lnk");
            }         
            else if (e.Document.Caption == "Shellbags")
            {
                ShellbagsTimeline = new TimelineControl("Shellbags");
            }
            else if (e.Document.Caption == "Google Chrome")
            {
                ChromeTimeline = new TimelineControl("Google Chrome");
            }
            else if (e.Document.Caption == "Firefox")
            {
                FirefoxTimeline = new TimelineControl("Firefox");
            }

            if (splashScreenManager1.IsSplashFormVisible)
            {
                splashScreenManager1.CloseWaitForm();
            }
            
        }
    }
}